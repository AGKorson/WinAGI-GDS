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
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.ViewUndo.ActionType;
using WinAGI.Common;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using EnvDTE;
using System.Numerics;

namespace WinAGI.Editor {
    public partial class frmViewEdit : ClipboardMonitor, IMessageFilter {
        #region Enums
        private enum ViewEditToolType {
            Edit,
            Select,
            Draw,
            Line,
            Rectangle,
            BoxFill,
            Paint,
            Erase,
        }

        private enum ViewEditOperation {
            opNone,
            opSetSelection,
            opMoveSelection,
            opDraw,
            opChangeWidth,
            opChangeHeight,
            opChangeBoth,
        }

        private enum ViewCursor {
            NoOp,
            Edit,
            Cross,
            Select,
            Move,
            Draw,
            Erase,
            Paint,
            DragSurface,
            ResizeHeight,
            ResizeWidth,
            ResizeBoth,
        }
        #endregion

        #region Structures
        public struct PixelInfo {
            public Point Location;
            public byte Value;
        }

        public struct SelectionInfo {
            public Rectangle Bounds;
            public byte[,] Data;
            public byte[,] UnderData;
        }
        #endregion

        #region Constants
        private const int MAXLOOPS = 254;
        private const int MAXCELS = 254;
        #endregion

        #region Members
        public int ViewNumber;
        public Engine.View EditView;
        private TreeNode ViewNode;
        internal bool InGame;
        internal bool IsChanged;
        private bool closing = false;
        private Stack<ViewUndo> UndoCol = [];
        private Bitmap PlayImage, StopImage;
        private Bitmap AddCelImage, AddLoopImage;
        public int ChangingHeight, ChangingWidth;

        // selection and editing variables
        private ViewEditMode ViewMode;
        public int SelectedLoop;
        private int SelectedCel;
        private ViewEditToolType SelectedTool;
        private int mX, mY;
        private Point AnchorPt, ViewPt;
        private Point DragPT, CelOffset;
        private SelectionInfo Selection;
        private ViewCursor CurCursor, ToolCursor;
        private bool blnDragging;
        private ViewEditOperation CurrentOperation;
        private bool SelectionMoved, SelectionVisible;
        private AGIColorIndex LeftColor, RightColor;
        private bool TransSel = false;
        private List<PixelInfo> PixelData;
        private AGIColorIndex DrawCol;

        // display variables
        float ScaleFactor;
        private bool ShowGrid;
        internal EGAColors EditPalette = DefaultPalette.Clone();
        private int dashdistance = 6;
        readonly Pen dash1 = new(Color.Black), dash2 = new(Color.White);
        private const int VE_MARGIN = 3;
        private readonly Color GRID_COLOR = Color.FromArgb(0xB4, 0xB4, 0xB4);

        // view preview
        private bool ShowPreview, CyclePreview;
        private int PreviewCel;
        private float PreviewScale;
        private int lngVAlign, lngHAlign;
        private int lngMotion = 0;
        private bool TransparentPreview = true;

        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spTool;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCurX;
        internal ToolStripStatusLabel spCurY;
        #endregion

        public frmViewEdit() {
            InitializeComponent();
            Application.AddMessageFilter(this);
            Disposed += (sender, e) => Application.RemoveMessageFilter(this);
            InitFonts();

            MdiParent = MDIMain;
            // setu toolbars and other conrols
            tsbTool.DropDown.AutoSize = false;
            tsbTool.DropDown.Width = 34;
            tspAlignH.DropDown.AutoSize = false;
            tspAlignH.DropDown.Width = 26;
            tspAlignV.DropDown.AutoSize = false;
            tspAlignV.DropDown.Width = 26;
            trkSpeed.Top = toolStrip3.Top + 4;
            tspMode.Text = "normal";
            picCel.MouseWheel += picCel_MouseWheel;
            picPreview.MouseWheel += preview_MouseWheel;

            // other initializations:
            dash1.DashPattern = [3, 3];
            dash2.DashPattern = [3, 3];
            dash2.DashOffset = 3;

            // get default settings
            ScaleFactor = WinAGISettings.ViewScaleEdit.Value;
            // validate
            if (ScaleFactor < 1) {
                ScaleFactor = 1;
            }
            else if (ScaleFactor <= 3) {
                ScaleFactor = (int)(ScaleFactor * 4) / 4f;
            }
            else if (ScaleFactor < 8) {
                ScaleFactor = (int)(ScaleFactor * 2) / 2f;
            }
            else if (ScaleFactor < 20) {
                ScaleFactor = (int)ScaleFactor;
            }
            else {
                ScaleFactor = 20;
            }
            LeftColor = (AGIColorIndex)WinAGISettings.DefVColor1.Value;
            RightColor = (AGIColorIndex)WinAGISettings.DefVColor2.Value;
            ShowGrid = WinAGISettings.ShowGrid.Value;
            ShowPreview = WinAGISettings.ShowVEPreview.Value;
            if (!ShowPreview) {
                tmrMotion.Enabled = false;
                splitCanvas.Panel2Collapsed = true;
            }
            // get default preview values
            PreviewScale = WinAGISettings.ViewScaleEdit.Value;
            CyclePreview = WinAGISettings.DefPrevPlay.Value;
            lngHAlign = WinAGISettings.ViewAlignH.Value;
            tspAlignH.Image = tspAlignH.DropDownItems[lngHAlign].Image;
            lngVAlign = WinAGISettings.ViewAlignV.Value;
            tspAlignV.Image = tspAlignV.DropDownItems[lngVAlign].Image;
            ViewNode = tvwView.Nodes[0];
            // set initial mode
            ViewMode = ViewEditMode.Cel;
            // start with no tool
            SelectedTool = ViewEditToolType.Edit;
            ToolCursor = ViewCursor.Edit;
            SetCursor(ToolCursor);

            byte[] obj = (byte[])EditorResources.ResourceManager.GetObject("btn_play");
            Stream stream = new MemoryStream(obj);
            PlayImage = (Bitmap)Image.FromStream(stream);
            obj = (byte[])EditorResources.ResourceManager.GetObject("btn_stop");
            stream = new MemoryStream(obj);
            StopImage = (Bitmap)Image.FromStream(stream);
            obj = (byte[])EditorResources.ResourceManager.GetObject("btn_addcel");
            stream = new MemoryStream(obj);
            AddCelImage = (Bitmap)Image.FromStream(stream);
            obj = (byte[])EditorResources.ResourceManager.GetObject("btn_addloop");
            stream = new MemoryStream(obj);
            AddLoopImage = (Bitmap)Image.FromStream(stream);
            // setup the status bar
            InitStatusStrip();
        }

        #region Form Event Handlers
        protected override void OnClipboardChanged() {
            base.OnClipboardChanged();
            switch (ViewMode) {
            case ViewEditMode.View:
                // paste is never available
                tsbPaste.Enabled = false;
                break;
            case ViewEditMode.Loop:
            case ViewEditMode.EndLoop:
                tsbPaste.Enabled = ClipboardHasLoop();
                break;
            case ViewEditMode.Cel:
                if (SelectionVisible) {
                    tsbPaste.Enabled = Clipboard.ContainsImage();
                }
                else {
                    tsbPaste.Enabled = ClipboardHasCel() || Clipboard.ContainsImage();
                }
                break;
            case ViewEditMode.EndCel:
                tsbPaste.Enabled = ClipboardHasCel();
                break;
            }
        }

        /// <summary>
        /// Pre-filters the message to catch mouse wheel events.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool PreFilterMessage(ref Message m) {
            // in splitcontainers, mousewheel auto-scrolls the vertical scrollbar
            // only way to stop it is to catch the mousewheel message
            // and handle it manually
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_MOUSEWHEEL) {
                //if (Control.FromHandle(m.HWnd) is Control control && InSplitContainer(control)) {
                if (Control.FromHandle(m.HWnd) is Control control) {
                    if (control == picCel) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000) >> 16;
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000) >> 16;
                        picCel_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
                    }
                    else if (control == picPreview) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000);
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000);
                        preview_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
                    }
                    else {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            return false;
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
            // dispose of graphics objects
            dash1?.Dispose();
            dash2?.Dispose();
        }

        private void frmViewEdit_Resize(object sender, EventArgs e) {
            if (picPalette.Visible) {
                picPalette.Refresh();
            }
        }

        private void frmViewEdit_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Modifiers) {
            case Keys.None:
                switch (e.KeyCode) {
                case Keys.Oemplus:
                    // '+'
                    // zoom in
                    ChangeScale(1);
                    e.Handled = true;
                    break;
                case Keys.OemMinus:
                    // '-'
                    // zoom out
                    ChangeScale(-1);
                    e.Handled = true;
                    break;
                case Keys.Space:
                    // ' ' (spacebar)
                    // start/stop animation
                    if (ShowPreview && tspCycle.Enabled) {
                        TogglePreviewMotion();
                    }
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    if (SelectionVisible) {
                        HideSelection();
                    }
                    break;
                }
                break;
            case Keys.Control:
                switch (e.KeyCode) {
                case Keys.Oemplus:
                    // '+'
                    // zoom preview in
                    if (ShowPreview) {
                        ChangePreviewScale(1);
                    }
                    e.Handled = true;
                    break;
                case Keys.OemMinus:
                    // '-'
                    // zoom out
                    if (ShowPreview) {
                        ChangePreviewScale(-1);
                    }
                    e.Handled = true;
                    break;
                }
                break;
            case Keys.Shift:
                switch (e.KeyCode) {
                case Keys.T:
                    if (ShowPreview) {
                        TogglePreviewTransparency();
                    }
                    break;
                }
                break;
            case Keys.Alt:
                break;
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

        /// <summary>
        /// Dynamic function to reset the resource menu.
        /// </summary>
        public void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRExportLoopGIF.Enabled = true;
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                // save the view
                SaveView();
            }
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportView();
        }

        public void mnuRInGame_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                ToggleInGame();
            }
        }

        private void mnuRRenumber_Click(object sender, EventArgs e) {
            if (InGame) {
                RenumberView();
            }
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditViewProperties(1);
        }

        private void mnuRExportLoopGIF_Click(object sender, EventArgs e) {
            if (SelectedLoop >= 0 && SelectedLoop < EditView.Loops.Count) {
                ExportLoopGIF(EditView, SelectedLoop);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            if (picCel.Focused) {
                if (CurrentOperation != ViewEditOperation.opNone) {
                    CurrentOperation = ViewEditOperation.opNone;
                }
            }
            SetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange([mnuUndo, toolStripSeparator6,
            mnuCut, mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuInsert, mnuSelectAll, toolStripSeparator7,
            mnuFlipH, mnuFlipV, toolStripSeparator8,
            mnuTogglePreview, mnuToggleGrid, mnuToggleSelectionMode]);
            SetEditMenu();
        }

        private void SetEditMenu() {
            // set the edit menu items
            mnuUndo.Enabled = UndoCol.Count > 0;
            mnuUndo.Text = SetUndoText();
            switch (ViewMode) {
            case ViewEditMode.View:
                mnuCut.Visible = false;
                mnuCopy.Visible = true;
                mnuCopy.Enabled = true;
                mnuCopy.Text = "Copy View";
                mnuPaste.Visible = false;
                mnuDelete.Visible = false;
                mnuClear.Visible = true;
                mnuClear.Enabled = true;
                mnuClear.Text = "Clear View";
                mnuInsert.Visible = false;
                mnuSelectAll.Visible = false;
                toolStripSeparator7.Visible = false;
                mnuFlipH.Visible = false;
                mnuFlipV.Visible = false;
                break;
            case ViewEditMode.Loop:
                mnuCut.Visible = true;
                mnuCut.Enabled = EditView.Loops.Count > 1;
                mnuCut.Text = "Cut Loop";
                mnuCopy.Visible = true;
                mnuCopy.Enabled = true;
                mnuCopy.Text = "Copy Loop";
                mnuPaste.Visible = true;
                mnuPaste.Enabled = ClipboardHasLoop();
                mnuPaste.Text = "Paste Loop";
                mnuDelete.Visible = true;
                mnuDelete.Enabled = EditView.Loops.Count > 1;
                mnuDelete.Text = "Delete Loop";
                mnuClear.Visible = true;
                mnuClear.Enabled = true;
                mnuClear.Text = "Clear Loop";
                mnuInsert.Visible = true;
                mnuInsert.Enabled = true;
                mnuInsert.Text = "Insert New Loop";
                mnuSelectAll.Visible = false;
                toolStripSeparator7.Visible = true;
                mnuFlipH.Visible = true;
                mnuFlipH.Enabled = true;
                mnuFlipH.Text = "Flip Loop Horizontally";
                mnuFlipV.Visible = true;
                mnuFlipV.Enabled = true;
                mnuFlipV.Text = "Flip Loop Vertically";
                break;
            case ViewEditMode.EndLoop:
                mnuCut.Visible = true;
                mnuCut.Enabled = false;
                mnuCut.Text = "Cut Loop";
                mnuCopy.Visible = true;
                mnuCopy.Enabled = false;
                mnuCopy.Text = "Copy Loop";
                mnuPaste.Visible = true;
                mnuPaste.Enabled = ClipboardHasLoop();
                mnuPaste.Text = "Paste Loop";
                mnuDelete.Visible = true;
                mnuDelete.Enabled = false;
                mnuDelete.Text = "Delete Loop";
                mnuClear.Visible = true;
                mnuClear.Enabled = false;
                mnuClear.Text = "Clear Loop";
                mnuInsert.Visible = true;
                mnuInsert.Enabled = true;
                mnuInsert.Text = "Insert New Loop";
                mnuSelectAll.Visible = false;
                toolStripSeparator7.Visible = false;
                mnuFlipH.Visible = false;
                mnuFlipV.Visible = false;
                break;
            case ViewEditMode.Cel:
                mnuCut.Visible = true;
                mnuCopy.Visible = true;
                mnuPaste.Visible = true;
                mnuDelete.Visible = true;
                mnuSelectAll.Visible = true;
                mnuSelectAll.Enabled = true;
                mnuSelectAll.Text = "Select All";
                mnuFlipH.Visible = true;
                mnuFlipV.Visible = true;
                if (SelectionVisible) {
                    mnuCut.Enabled = true;
                    mnuCut.Text = "Cut";
                    mnuCopy.Enabled = true;
                    mnuCopy.Text = "Copy";
                    mnuPaste.Enabled = Clipboard.ContainsImage();
                    mnuPaste.Text = "Paste";
                    mnuDelete.Enabled = true;
                    mnuDelete.Text = "Delete";
                    mnuClear.Visible = false;
                    mnuInsert.Visible = false;
                    toolStripSeparator7.Visible = false;
                    mnuFlipH.Enabled = Selection.Bounds.Width > 1;
                    mnuFlipH.Text = "Flip Selection Horizontally";
                    mnuFlipV.Enabled = Selection.Bounds.Height > 1;
                    mnuFlipV.Text = "Flip Selection Vertically";
                }
                else {
                    mnuCut.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                    mnuCut.Text = "Cut Cel";
                    mnuCopy.Enabled = true;
                    mnuCopy.Text = "Copy Cel";
                    if (ClipboardHasCel()) {
                        mnuPaste.Enabled = true;
                        mnuPaste.Text = "Paste Cel";
                    }
                    else if (Clipboard.ContainsImage()) {
                        mnuPaste.Enabled = true;
                        mnuPaste.Text = "Paste";
                    }
                    else {
                        mnuPaste.Enabled = false;
                        mnuPaste.Text = "Paste";
                    }
                    mnuDelete.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                    mnuDelete.Text = "Delete Cel";
                    mnuClear.Visible = true;
                    mnuClear.Enabled = true;
                    mnuClear.Text = "Clear Cel";
                    mnuInsert.Visible = true;
                    mnuInsert.Enabled = true;
                    mnuInsert.Text = "Insert New Cel";
                    toolStripSeparator7.Visible = true;
                    mnuFlipH.Enabled = EditView[SelectedLoop][SelectedCel].Width > 1;
                    mnuFlipH.Text = "Flip Cel Horizontally";
                    mnuFlipV.Enabled = EditView[SelectedLoop][SelectedCel].Height > 1;
                    mnuFlipV.Text = "Flip Cel Vertically";
                }
                break;
            case ViewEditMode.EndCel:
                mnuCut.Visible = true;
                mnuCut.Enabled = false;
                mnuCut.Text = "Cut Cel";
                mnuCopy.Visible = true;
                mnuCopy.Enabled = false;
                mnuCopy.Text = "Copy Cel";
                mnuPaste.Visible = true;
                mnuPaste.Enabled = ClipboardHasCel();
                mnuPaste.Text = "Paste";
                mnuDelete.Visible = true;
                mnuDelete.Enabled = false;
                mnuDelete.Text = "Delete Cel";
                mnuClear.Visible = true;
                mnuClear.Enabled = false;
                mnuClear.Text = "Clear Cel";
                mnuInsert.Visible = true;
                mnuInsert.Enabled = true;
                mnuInsert.Text = "Insert New Cel";
                mnuSelectAll.Visible = true;
                mnuSelectAll.Enabled = false;
                mnuSelectAll.Text = "Select All";
                toolStripSeparator7.Visible = false;
                mnuFlipH.Visible = false;
                mnuFlipV.Visible = false;
                break;
            }
            mnuTogglePreview.Enabled = true;
            mnuTogglePreview.Checked = ShowPreview;

            mnuToggleGrid.Enabled = true;
            mnuToggleGrid.Checked = ShowGrid;

            mnuToggleSelectionMode.Enabled = true;
            mnuToggleSelectionMode.Checked = TransSel;
        }

        /// <summary>
        /// Set the undo menu item text to match the 
        /// type of the top undo item on the undocol stack
        /// </summary>
        /// <returns></returns>
        private string SetUndoText() {
            if (UndoCol.Count == 0) {
                return "Undo"; 
            }
            switch (UndoCol.Peek().UDAction) {
            case AddLoop:
                return "Undo Add Loop";
            case PasteLoop:
                return "Undo Paste Loop";
            case AddCel:
                return "Undo Add Cel";
            case PasteCel:
                return "Undo Paste Cel";
            case DelLoop:
                return "Undo Delete Loop";
            case CutLoop:
                return "Undo Cut Loop";
            case DelCel:
                return "Undo Delete Cel";
            case CutCel:
                return "Undo Cut Cel";
            case IncHeight:
                return "Undo Increase Height";
            case IncWidth:
                return "Undo Increase Width";
            case DecHeight:
                return "Undo Decrease Height";
            case DecWidth:
                return "Undo Decrease Width";
            case Mirror:
                return "Undo Mirror";
            case ChangeTransCol:
                return "Undo Change Transparent Color";
            case ViewUndo.ActionType.ClearLoop:
                return "Undo Clear Loop";
            case ViewUndo.ActionType.ClearCel:
                return "Undo Clear Cel";
            case ChangeVDesc:
                return "Undo Change View Description";
            case FlipLoopH:
                return "Undo Flip Loop Horizontally";
            case FlipLoopV:
                return "Undo Flip Loop Vertically";
            case ViewUndo.ActionType.FlipCelH:
                return "Undo Flip Cel Horizontally";
            case ViewUndo.ActionType.FlipCelV:
                return "Undo Flip Cel Vertically";
            case ViewUndo.ActionType.FlipSelectionH:
                return "Undo Flip Selection Horizontally";
            case ViewUndo.ActionType.FlipSelectionV:
                return "Undo Flip Selection Vertically";
            case PaintFill:
                return "Undo Paint Fill";
            case Line:
                return "Undo Draw Line";
            case Box:
                return "Undo Draw Box";
            case BoxFill:
                return "Undo Box Fill";
            case Draw:
                return "Undo Draw";
            case Erase:
                return "Undo Erase";
            case CutSelection:
                return "Undo Cut Selection";
            case DelSelection:
                return "Undo Delete Selection";
            case ViewUndo.ActionType.PasteSelection:
                return "Undo Paste Selection";
            case ViewUndo.ActionType.MoveSelection:
                return "Undo Move Selection";
            case ClearView:
                return "Undo Clear View";
            default:
                return "Undo";
            }
        }

        private void ConfigureToolbar() {
            //tsbUndo - handled inline
            switch (ViewMode) {
            case ViewEditMode.View:
                tsbCut.Enabled = false;
                tsbCut.ToolTipText = "Cut";
                tsbCopy.Enabled = true;
                tsbCopy.ToolTipText = "Copy View";
                tsbPaste.Enabled = false;
                tsbPaste.ToolTipText = "Paste";
                tsbDelete.Enabled = false;
                tsbDelete.ToolTipText = "Delete";
                tsbInsert.Enabled = false;
                tsbInsert.ToolTipText = "Insert";
                tsbInsert.Image = AddLoopImage;
                tsbFlipH.Enabled = false;
                tsbFlipH.ToolTipText = "Flip";
                tsbFlipV.Enabled = false;
                tsbFlipV.ToolTipText = "Flip";
                break;
            case ViewEditMode.Loop:
                tsbCut.Enabled = EditView.Loops.Count > 1;
                tsbCut.ToolTipText = "Cut Loop";
                tsbCopy.Enabled = true;
                tsbCopy.ToolTipText = "Copy Loop";
                tsbPaste.Enabled = ClipboardHasLoop();
                tsbPaste.ToolTipText = "Paste Loop";
                tsbDelete.Enabled = EditView.Loops.Count > 1;
                tsbDelete.ToolTipText = "Delete Loop";
                tsbInsert.Enabled = true;
                tsbInsert.ToolTipText = "Insert Loop";
                tsbInsert.Image = AddLoopImage;
                tsbFlipH.Enabled = true;
                tsbFlipH.ToolTipText = "Flip Horizontal";
                tsbFlipV.Enabled = true;
                tsbFlipV.ToolTipText = "Flip Vertical";
                break;
            case ViewEditMode.EndLoop:
                tsbCut.Enabled = false;
                tsbCut.ToolTipText = "Cut Loop";
                tsbCopy.Enabled = false;
                tsbCopy.ToolTipText = "Copy Loop";
                tsbPaste.Enabled = ClipboardHasLoop();
                tsbPaste.ToolTipText = "Paste Loop";
                tsbDelete.Enabled = false;
                tsbDelete.ToolTipText = "Delete Loop";
                tsbInsert.Enabled = true;
                tsbInsert.ToolTipText = "Insert Loop";
                tsbInsert.Image = AddLoopImage;
                tsbFlipH.Enabled = false;
                tsbFlipH.ToolTipText = "Flip";
                tsbFlipV.Enabled = false;
                tsbFlipV.ToolTipText = "Flip";
                break;
            case ViewEditMode.Cel:
                tsbCopy.Enabled = true;
                if (SelectionVisible) {
                    tsbCut.Enabled = true;
                    tsbCut.ToolTipText = "Cut Selection";
                    tsbCopy.ToolTipText = "Copy Selection";
                    if (Clipboard.ContainsImage()) {
                        tsbPaste.Enabled = true;
                        tsbPaste.ToolTipText = "Paste Selection";
                    }
                    else if (ClipboardHasCel()) {
                        tsbPaste.Enabled = true;
                        tsbPaste.ToolTipText = "Paste Cel";
                    }
                    else {
                        tsbPaste.Enabled = false;
                        tsbPaste.ToolTipText = "Paste";
                    }
                    tsbDelete.Enabled = true;
                    tsbDelete.ToolTipText = "Delete";
                    tsbInsert.Enabled = false;
                    tsbInsert.ToolTipText = "Insert";
                    tsbInsert.Image = AddCelImage;
                    tsbFlipH.Enabled = Selection.Bounds.Width > 1;
                    tsbFlipH.ToolTipText = "Flip Horizontal";
                    tsbFlipV.Enabled = Selection.Bounds.Height > 1;
                    tsbFlipV.ToolTipText = "Flip Vertical";
                }
                else {
                    tsbCut.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                    tsbCut.ToolTipText = "Cut Cel";
                    tsbCopy.ToolTipText = "Copy Cel";
                    if (ClipboardHasCel()) {
                        tsbPaste.Enabled = true;
                        tsbPaste.ToolTipText = "Paste Cel";
                    }
                    else if (Clipboard.ContainsImage()) {
                        tsbPaste.Enabled = true;
                        tsbPaste.ToolTipText = "Paste";
                    }
                    else {
                        tsbPaste.Enabled = false;
                        tsbPaste.ToolTipText = "Paste";
                    }
                    tsbDelete.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                    tsbDelete.ToolTipText = "Delete Cel";
                    tsbInsert.Enabled = true;
                    tsbInsert.ToolTipText = "Insert Cel";
                    tsbInsert.Image = AddCelImage;
                    tsbFlipH.Enabled = EditView[SelectedLoop][SelectedCel].Width > 1;
                    tsbFlipH.ToolTipText = "Flip Horizontal";
                    tsbFlipV.Enabled = EditView[SelectedLoop][SelectedCel].Height > 1;
                    tsbFlipV.ToolTipText = "Flip Vertical";
                }
                break;
            case ViewEditMode.EndCel:
                tsbCut.Enabled = false;
                tsbCut.ToolTipText = "Cut Cel";
                tsbCopy.Enabled = false;
                tsbCopy.ToolTipText = "Copy Cel";
                tsbPaste.Enabled = ClipboardHasCel();
                tsbPaste.ToolTipText = "Paste";
                tsbDelete.Enabled = false;
                tsbDelete.ToolTipText = "Delete Cel";
                tsbInsert.Enabled = true;
                tsbInsert.ToolTipText = "Insert Cel";
                tsbInsert.Image = AddCelImage;
                tsbFlipH.Enabled = false;
                tsbFlipH.ToolTipText = "Flip";
                tsbFlipV.Enabled = false;
                tsbFlipV.ToolTipText = "Flip";
                break;
            }
        }

        private void ResetEditMenu() {
            mnuUndo.Enabled = true;
            mnuCut.Enabled = true;
            mnuCopy.Enabled = true;
            mnuPaste.Enabled = true;
            mnuDelete.Enabled = true;
            mnuClear.Enabled = true;
            mnuInsert.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuFlipH.Enabled = true;
            mnuFlipV.Enabled = true;
            mnuTogglePreview.Enabled = true;
            mnuToggleGrid.Enabled = true;
            mnuToggleSelectionMode.Enabled = true;
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([mnuUndo, toolStripSeparator6,
            mnuCut, mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuInsert, mnuSelectAll, toolStripSeparator7,
            mnuFlipH, mnuFlipV, toolStripSeparator8,
            mnuTogglePreview, mnuToggleGrid, mnuToggleSelectionMode]);
            ResetEditMenu();
        }

        private void mnuUndo_Click(object sender, EventArgs e) {
            int i, j;
            byte CelWidth, CelHeight;
            //byte CelStartX, CelStartY, CelEndX, CelEndY;
            int startx, starty, endx, endy;

            if (UndoCol.Count == 0) {
                return;
            }
            ViewUndo NextUndo = UndoCol.Pop();
            tsbUndo.Enabled = (UndoCol.Count > 0);

            switch (NextUndo.UDAction) {
            case AddLoop or PasteLoop:
                // get loop to delete
                SelectedLoop = NextUndo.UDLoopNo;
                // delete loop
                DeleteLoop((byte)SelectedLoop, true);
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                break;
            case AddCel or PasteCel:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // delete cel
                DeleteCel((byte)SelectedLoop, (byte)SelectedCel, true);
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                break;
            case DelLoop or CutLoop:
                SelectedLoop = NextUndo.UDLoopNo;
                // add a new loop here
                EditView.Loops.Add(SelectedLoop);
                // if the loop was mirrored
                if (NextUndo.UndoData[0] != -1) {
                    // reset mirror for this loop
                    EditView.SetMirror((byte)SelectedLoop, (byte)NextUndo.UndoData[1]);
                }
                else {
                    // copy from undo
                    EditView[SelectedLoop].CloneFrom(NextUndo.UndoLoop);
                }
                UpdateTree();
                // select and display first cel in this loop
                SelectCel(SelectedLoop, 0, true);
                break;
            case DelCel or CutCel:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // add a cel
                EditView[SelectedLoop].Cels.Add(SelectedCel);
                // copy cel from undo
                EditView[SelectedLoop][SelectedCel].CloneFrom(NextUndo.UndoCel);
                UpdateTree();
                // select and display this cel
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case IncHeight:
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo);
                ChangeHeight(NextUndo.UndoData[0], true);
                break;
            case IncWidth:
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo);
                ChangeWidth(NextUndo.UndoData[0], true);
                break;
            case DecHeight:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                CelWidth = EditView[SelectedLoop][SelectedCel].Width;
                CelHeight = EditView[SelectedLoop][SelectedCel].Height;
                // set new height
                EditView[SelectedLoop][SelectedCel].Height = (byte)NextUndo.UndoData[0];
                // recover data
                for (j = CelHeight; j < NextUndo.UndoData[0] - 1; j++) {
                    for (i = 0; i < CelWidth; i++) {
                        // add this cel pixel's color
                        EditView[SelectedLoop][SelectedCel].AllCelData[i, j] = NextUndo.CelData[i, j - CelHeight];
                    }
                }
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case DecWidth:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                CelWidth = EditView[SelectedLoop][SelectedCel].Width;
                CelHeight = EditView[SelectedLoop][SelectedCel].Height;
                // set new width
                EditView[SelectedLoop][SelectedCel].Width = (byte)NextUndo.UndoData[0];
                // recover data
                for (i = CelWidth; i < NextUndo.UndoData[0] - 1; i++) {
                    for (j = 0; j < CelHeight; j++) {
                        // get this cel pixel's color
                        EditView[SelectedLoop][SelectedCel].AllCelData[i, j] = NextUndo.CelData[i - CelWidth, j];
                    }
                }
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case Mirror:
                SelectLoop(NextUndo.UDLoopNo);
                // select first cel
                SelectedCel = 0;
                // unmirror loop
                EditView.Loops[SelectedLoop].UnMirror();
                // if old mirror another loop
                if (NextUndo.UndoData[0] != -1) {
                    EditView.SetMirror((byte)SelectedLoop, (byte)NextUndo.UndoData[0]);
                }
                else {
                    // restore old loop
                    EditView[SelectedLoop].CloneFrom(NextUndo.UndoLoop);
                }
                // rebuild tree (since cel counts might be different)
                UpdateTree();
                SelectLoop(SelectedLoop);
                break;
            case ChangeTransCol:
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo);
                ChangeTransColor((AGIColorIndex)NextUndo.UndoData[0], true);
                break;
            case ViewUndo.ActionType.ClearLoop:
                SelectedLoop = NextUndo.UDLoopNo;
                EditView[SelectedLoop].CloneFrom(NextUndo.UndoLoop);
                // if loop was mirrored
                if (NextUndo.UndoData[0] != -1) {
                    // need to reset mirror
                    EditView.SetMirror((byte)NextUndo.UndoData[0], (byte)SelectedLoop);
                }
                UpdateTree();
                // select first cel
                SelectCel(SelectedLoop, 0);
                break;
            case ViewUndo.ActionType.ClearCel:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // restore cel
                EditView[SelectedLoop][SelectedCel].CloneFrom(NextUndo.UndoCel);
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case ChangeVDesc:
                ChangeViewDesc(NextUndo.OldText, true);
                if (ViewMode == ViewEditMode.View) {
                    propertyGrid1.Refresh();
                }
                break;
            case FlipLoopH:
                for (i = 0; i < EditView[NextUndo.UDLoopNo].Cels.Count; i++) {
                    FlipCelH(NextUndo.UDLoopNo, i, true);
                }
                SelectLoop(NextUndo.UDLoopNo);
                break;
            case FlipLoopV:
                for (i = 0; i < EditView[NextUndo.UDLoopNo].Cels.Count; i++) {
                    FlipCelV(NextUndo.UDLoopNo, i, true);
                }
                SelectLoop(NextUndo.UDLoopNo);
                break;
            case ViewUndo.ActionType.FlipCelH:
                FlipCelH(NextUndo.UDLoopNo, NextUndo.UDCelNo, true);
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo, true);
                break;
            case ViewUndo.ActionType.FlipCelV:
                FlipCelV(NextUndo.UDLoopNo, NextUndo.UDCelNo, true);
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo, true);
                break;
            case ViewUndo.ActionType.FlipSelectionH:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                Selection = NextUndo.UDSelection;
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                // re-flip
                FlipSelectionH(true);
                SelectCel(SelectedLoop, SelectedCel, true);
                // re-enable selection
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                if (SelectedTool != ViewEditToolType.Select) {
                    SelectedTool = ViewEditToolType.Select;
                    tsbTool.Image = tstSelect.Image;
                    spTool.Text = SelectedTool.ToString();
                    SetCursor(ViewCursor.Select);
                }
                break;
            case ViewUndo.ActionType.FlipSelectionV:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                Selection = NextUndo.UDSelection;
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                // re-flip
                FlipSelectionV(true);
                SelectCel(SelectedLoop, SelectedCel, true);
                // re-enable selection
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                if (SelectedTool != ViewEditToolType.Select) {
                    SelectedTool = ViewEditToolType.Select;
                    tsbTool.Image = tstSelect.Image;
                    spTool.Text = SelectedTool.ToString();
                    SetCursor(ViewCursor.Select);
                }
                break;
            case PaintFill:
            case Line:
            case Box:
                // these all store undo data in the same format - a
                // list of pixels with the original data
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                for (i = 0; i < NextUndo.PixelData.Count; i++) {
                    PixelInfo px = NextUndo.PixelData[i];
                    EditView[SelectedLoop][SelectedCel][px.Location.X, px.Location.Y] = px.Value;
                }
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case BoxFill:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // get start and end points
                for (i = NextUndo.UndoData[0]; i <= NextUndo.UndoData[2]; i++) {
                    for (j = NextUndo.UndoData[1]; j <= NextUndo.UndoData[3]; j++) {
                        EditView[SelectedLoop][SelectedCel][i, j] = NextUndo.CelData[i - NextUndo.UndoData[0], j - NextUndo.UndoData[1]];
                    }
                }
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case Draw or Erase:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // undo pixels by stepping backward through pixel list
                for (i = NextUndo.PixelData.Count - 1; i >= 0; i--) {
                    EditView[SelectedLoop][SelectedCel][NextUndo.PixelData[i].Location.X, NextUndo.PixelData[i].Location.Y] = NextUndo.PixelData[i].Value;
                }
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case CutSelection or DelSelection:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                Selection = NextUndo.UDSelection;
                // restore data
                startx = Selection.Bounds.X;
                starty = Selection.Bounds.Y;
                endx = Selection.Bounds.Right;
                endy = Selection.Bounds.Bottom;
                for (i = startx; i < endx; i++) {
                    for (j = starty; j < endy; j++) {
                        if (i >= 0 && j >= 0 &&
                            i < EditView[SelectedLoop][SelectedCel].Width &&
                            j < EditView[SelectedLoop][SelectedCel].Height) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                        }
                    }
                }
                // select the cel
                SelectCel(SelectedLoop, SelectedCel, true);
                // re-enable selection
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                if (SelectedTool != ViewEditToolType.Select) {
                    SelectedTool = ViewEditToolType.Select;
                    tsbTool.Image = tstSelect.Image;
                    spTool.Text = SelectedTool.ToString();
                    SetCursor(ViewCursor.Select);
                }
                ConfigureToolbar();
                break;
            case ViewUndo.ActionType.PasteSelection:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // restore data under
                startx = Selection.Bounds.X;
                starty = Selection.Bounds.Y;
                endx = Selection.Bounds.Right;
                endy = Selection.Bounds.Bottom;
                for (i = startx; i < endx; i++) {
                    for (j = starty; j < endy; j++) {
                        if (i >= 0 && j >= 0 &&
                            i < EditView[SelectedLoop][SelectedCel].Width &&
                            j < EditView[SelectedLoop][SelectedCel].Height) {
                            if (!TransSel || Selection.Data[i - startx, j - starty] != (byte)EditView[SelectedLoop][SelectedCel].TransColor) {
                                EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                            }
                        }
                    }
                }
                // select the cel
                SelectCel(SelectedLoop, SelectedCel, true);
                // no selection
                HideSelection();
                ConfigureToolbar();
                break;
            case ViewUndo.ActionType.MoveSelection:
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                Selection = NextUndo.UDSelection;
                // use move to put the selection back
                Point oldpos = new(NextUndo.UndoData[0], NextUndo.UndoData[1]);
                MoveSelection(oldpos);
                SelectCel(SelectedLoop, SelectedCel, true);
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                ConfigureToolbar();
                break;
            }
        }

        private void mnuCut_Click(object sender, EventArgs e) {
            int undocount = UndoCol.Count;
            mnuCopy_Click(sender, e);
            mnuDelete_Click(sender, e);
            if (undocount != UndoCol.Count) {
                // undo was added
                switch (UndoCol.Peek().UDAction) {
                case DelLoop:
                    UndoCol.Peek().UDAction = CutLoop;
                    break;
                case DelCel:
                    UndoCol.Peek().UDAction = CutCel;
                    break;
                case DelSelection:
                    UndoCol.Peek().UDAction = CutSelection;
                    break;
                }
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.View:
                // 'copying' a view just makes a duplicate of the entire
                // view, and no data is stored on the clipboard

                // copy view to a new blank view
                frmViewEdit frmNew = new();
                ViewEditors.Add(frmNew);
                frmNew.LoadView(EditView);
                // reset name
                frmNew.EditView.ID = "Copy of " + frmNew.EditView.ID;
                // and caption
                frmNew.Text = sVIEWED + frmNew.EditView.ID;
                // select first cel
                SelectCel(0, 0);
                // switch to new form
                frmNew.Show();
                // force changed status
                frmNew.MarkAsChanged();
                frmNew.Select();
                return;
            case ViewEditMode.Loop:
                // copy loop
                ViewClipboardData viewCB = new(ViewClipboardMode.Loop);
                viewCB.Loop.CloneFrom(EditView[SelectedLoop]);
                DataObject cbData = new(VIEW_CB_FMT, viewCB);
                Clipboard.SetDataObject(cbData, true);
                break;
            case ViewEditMode.Cel:
                if (SelectionVisible) {
                    CopySelection();
                }
                else {
                    // copy cel
                    viewCB = new(ViewClipboardMode.Cel);
                    viewCB.Cel.CloneFrom(EditView[SelectedLoop][SelectedCel]);
                    cbData = new(VIEW_CB_FMT, viewCB);
                    Clipboard.SetDataObject(cbData, true);
                }
                break;
            }
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.View:
                break;
            case ViewEditMode.Loop:
            case ViewEditMode.EndLoop:
                if (ClipboardHasLoop()) {
                    ViewClipboardData vcb = Clipboard.GetData(VIEW_CB_FMT) as ViewClipboardData;
                    if (InsertLoop(SelectedLoop, vcb.Loop)) {
                        // force re-select first cel of new loop
                        SelectCel(SelectedLoop, 0, true);
                    }
                }
                break;
            case ViewEditMode.Cel:
            case ViewEditMode.EndCel:
                if (SelectionVisible) {
                    if (Clipboard.ContainsImage()) {
                        PasteSelection(Selection.Bounds.Location);
                    }
                }
                else {
                    if (ClipboardHasCel()) {
                        ViewClipboardData vcb = Clipboard.GetData(VIEW_CB_FMT) as ViewClipboardData;
                        if (InsertCel(SelectedLoop, SelectedCel, vcb.Cel)) {
                            // select this cel
                            SelectCel(SelectedLoop, SelectedCel);
                        }
                    }
                    else if (Clipboard.ContainsImage()) {
                        PasteSelection(new(0, 0));
                    }
                }
                break;
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.Loop:
                // delete loop
                DeleteLoop((byte)SelectedLoop);
                break;
            case ViewEditMode.Cel:
                if (SelectionVisible) {
                    DeleteSelection();
                }
                else {
                    // delete cel
                    DeleteCel((byte)SelectedLoop, (byte)SelectedCel);
                }
                break;
            }
        }

        private void mnuClear_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.View:
                // reset view to a single loop with a single cel
                // with height and width of one with black transcolor
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ClearView;
                NextUndo.View = EditView.Clone();
                // clearing mirrored loop not working...
                AddUndo(NextUndo);
                EditView.Clear();
                UpdateTree();
                SelectCel(0, 0);
                break;
            case ViewEditMode.Loop:
                ClearLoop(SelectedLoop);
                UpdateTree();
                SelectCel(SelectedLoop, 0, true);
                break;
            case ViewEditMode.Cel:
                ClearCel(SelectedLoop, SelectedCel);
                // force selection to show cleared cel
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            }
        }

        private void mnuInsert_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.View:
                break;
            case ViewEditMode.Loop:
            case ViewEditMode.EndLoop:
                if (InsertLoop(SelectedLoop)) {
                    // invalidate loop so it gets re-selected correctly
                    int newloop = SelectedLoop;
                    SelectedLoop = -1;
                    // select first cel of new loop
                    SelectCel(newloop, 0);
                }
                break;
            case ViewEditMode.Cel:
            case ViewEditMode.EndCel:
                if (InsertCel(SelectedLoop, SelectedCel)) {
                    SelectCel(SelectedLoop, SelectedCel);
                }
                break;
            }
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            if (CurrentOperation == ViewEditOperation.opNone) {
                SelectAll();
            }
        }

        private void mnuFlipH_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.Loop:
                // flip all cels in the loop
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ViewUndo.ActionType.FlipLoopH;
                NextUndo.UDLoopNo = SelectedLoop;
                AddUndo(NextUndo);
                for (int i = 0; i < EditView[SelectedLoop].Cels.Count; i++) {
                    FlipCelH(SelectedLoop, i, true);
                }
                SelectLoop(SelectedLoop);
                break;
            case ViewEditMode.Cel:
                if (SelectionVisible) {
                    FlipSelectionH();
                    DisplayCel();
                    if (ShowPreview) {
                        DisplayPrevLoop();
                    }
                }
                else {
                    FlipCelH(SelectedLoop, SelectedCel);
                    SelectCel(SelectedLoop, SelectedCel, true);
                }
                break;
            }
        }

        private void mnuFlipV_Click(object sender, EventArgs e) {
            switch (ViewMode) {
            case ViewEditMode.Loop:
                // flip all cels in the loop
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ViewUndo.ActionType.FlipLoopV;
                NextUndo.UDLoopNo = SelectedLoop;
                AddUndo(NextUndo);
                for (int i = 0; i < EditView[SelectedLoop].Cels.Count; i++) {
                    FlipCelV(SelectedLoop, i, true);
                }
                SelectLoop(SelectedLoop);
                break;
            case ViewEditMode.Cel:
                if (SelectionVisible) {
                    FlipSelectionV();
                    DisplayCel();
                    if (ShowPreview) {
                        DisplayPrevLoop();
                    }
                }
                else {
                    FlipCelV(SelectedLoop, SelectedCel);
                    SelectCel(SelectedLoop, SelectedCel, true);
                }
                break;
            }
        }

        private void mnuTogglePreview_Click(object sender, EventArgs e) {
            if (ShowPreview) {
                ShowPreview = false;
                tmrMotion.Enabled = false;
                splitCanvas.Panel2Collapsed = true;
            }
            else {
                ShowPreview = true;
                splitCanvas.Panel2Collapsed = true;
                DisplayPrevLoop();
            }
        }

        private void mnuToggleGrid_Click(object sender, EventArgs e) {
            ShowGrid = !ShowGrid;
            switch (ViewMode) {
            case ViewEditMode.View:
            case ViewEditMode.Loop:
                break;
            case ViewEditMode.Cel:
                DisplayCel();
                splitCanvas.Panel1.Invalidate();
                break;
            }
        }

        private void mnuToggleSelectionMode_Click(object sender, EventArgs e) {
            TransSel = !TransSel;
            if  (ViewMode == ViewEditMode.Cel && SelectionVisible) {
                // redraw selection, updating cel data
                int startx = Selection.Bounds.X;
                int starty = Selection.Bounds.Y;
                int endx = Selection.Bounds.Right;
                int endy = Selection.Bounds.Bottom;
                for (int i = startx; i < endx; i++) {
                    for (int j = starty; j < endy; j++) {
                        if (i >= 0 && j >= 0 &&
                            i < EditView[SelectedLoop][SelectedCel].Width &&
                            j < EditView[SelectedLoop][SelectedCel].Height) {
                            if (TransSel) {
                                if (Selection.Data[i - startx, j - starty] == (byte)EditView[SelectedLoop][SelectedCel].TransColor) {
                                    EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                                }
                                else {
                                    EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                                }
                            }
                            else {
                                EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                            }
                        }
                    }
                }
                DisplayCel();
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
            }
        }
        #endregion

        #region ToolStrip Event Handlers
        private void tsbTool_DropDownOpening(object sender, EventArgs e) {
            foreach (ToolStripItem item in tsbTool.DropDownItems) {
                if (item is ToolStripMenuItem menuItem) {
                    menuItem.Checked = false;
                }
            }
            ((ToolStripMenuItem)tsbTool.DropDownItems[(int)SelectedTool]).Checked = true;
        }

        private void tsbTool_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            tsbTool.Image = e.ClickedItem.Image;
            SelectedTool = (ViewEditToolType)e.ClickedItem.Tag;
            spTool.Text = SelectedTool.ToString();
            switch (SelectedTool) {
            case ViewEditToolType.Edit:
                ToolCursor = ViewCursor.Edit;
                break;
            case ViewEditToolType.Select:
                ToolCursor = ViewCursor.Select;
                break;
            case ViewEditToolType.Draw:
                ToolCursor = ViewCursor.Draw;
                break;
            case ViewEditToolType.Line:
                ToolCursor = ViewCursor.Cross;
                break;
            case ViewEditToolType.Rectangle:
                ToolCursor = ViewCursor.Cross;
                break;
            case ViewEditToolType.BoxFill:
                ToolCursor = ViewCursor.Cross;
                break;
            case ViewEditToolType.Paint:
                ToolCursor = ViewCursor.Paint;
                break;
            case ViewEditToolType.Erase:
                ToolCursor = ViewCursor.Erase;
                break;
            }
            SetCursor(ToolCursor);
        }

        private void tsbZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void tsbZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }

        private void tspAlignH_DropDownOpening(object sender, EventArgs e) {
            foreach (ToolStripItem item in tspAlignH.DropDownItems) {
                if (item is ToolStripMenuItem menuItem) {
                    menuItem.Checked = false;
                }
            }
            ((ToolStripMenuItem)tspAlignH.DropDownItems[lngHAlign]).Checked = true;
        }

        private void tspAlignV_DropDownOpening(object sender, EventArgs e) {
            foreach (ToolStripItem item in tspAlignV.DropDownItems) {
                if (item is ToolStripMenuItem menuItem) {
                    menuItem.Checked = false;
                }
            }
            ((ToolStripMenuItem)tspAlignV.DropDownItems[lngVAlign]).Checked = true;
        }

        private void tspAlignH_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            tspAlignH.Image = e.ClickedItem.Image;
            lngHAlign = (int)e.ClickedItem.Tag;
            switch (ViewMode) {
            case ViewEditMode.View:
                break;
            case ViewEditMode.Loop:
            case ViewEditMode.Cel:
                DisplayPrevCel();
                break;
            }
        }

        private void tspAlignV_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            tspAlignV.Image = e.ClickedItem.Image;
            lngVAlign = (int)e.ClickedItem.Tag;
            switch (ViewMode) {
            case ViewEditMode.View:
                break;
            case ViewEditMode.Loop:
            case ViewEditMode.Cel:
                DisplayPrevCel();
                break;
            }
        }

        private void tspMode_SelectedIndexChanged(object sender, EventArgs e) {
            lngMotion = tspMode.SelectedIndex;
            picPreview.Select();
        }

        private void tspCycle_Click(object sender, EventArgs e) {
            TogglePreviewMotion();
        }

        private void tspTransparency_Click(object sender, EventArgs e) {
            TogglePreviewTransparency();
        }

        private void tspZoomIn_Click(object sender, EventArgs e) {
            ChangePreviewScale(1);
        }

        private void tspZoomOut_Click(object sender, EventArgs e) {
            ChangePreviewScale(-1);
        }
        #endregion

        #region Control Event Handlers
        private void tvwView_MouseDown(object sender, MouseEventArgs e) {
            // force selection to change BEFORE context menu is shown
            if (e.Button == MouseButtons.Right) {
                TreeNode node = tvwView.GetNodeAt(e.X, e.Y);
                if (node != null) {
                    tvwView.SelectedNode = node;
                }
            }
        }

        private void tvwView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            switch (e.Node.Level) {
            case 0:
                // root
                SelectView();
                break;
            case 1:
                // loop
                SelectLoop(e.Node.Index);
                break;
            case 2:
                // cel
                SelectCel(e.Node.Parent.Index, e.Node.Index);
                break;
            }
        }

        private void tvwView_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void tvwView_KeyUp(object sender, KeyEventArgs e) {
            // when using keyboard to move the selection, the 
            // displayed loop/cel also need to be updated
            switch (e.KeyCode) {
            case Keys.Down:
            case Keys.Up:
            case Keys.Right:
            case Keys.Left:
            case Keys.PageDown:
            case Keys.PageUp:
            case Keys.Home:
            case Keys.End:
                switch (tvwView.SelectedNode.Level) {
                case 0:
                    // root
                    SelectView();
                    break;
                case 1:
                    // loop
                    SelectLoop(tvwView.SelectedNode.Index);
                    break;
                case 2:
                    // cel
                    SelectCel(tvwView.SelectedNode.Parent.Index, tvwView.SelectedNode.Index);
                    break;
                }
                break;
            }
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void tmrMotion_Tick(object sender, EventArgs e) {
            if (PreviewCel > EditView[SelectedLoop].Cels.Count - 1) {
                PreviewCel = 0;
            }

            // advance to next cel, depending on mode
            switch (lngMotion) {
            case 0:
                // normal
                if (PreviewCel == EditView[SelectedLoop].Cels.Count - 1) {
                    PreviewCel = 0;
                }
                else {
                    PreviewCel++;
                }
                break;
            case 1:
                // reverse
                if (PreviewCel == 0) {
                    PreviewCel = EditView[SelectedLoop].Cels.Count - 1;
                }
                else {
                    PreviewCel--;
                }
                break;
            case 2:
                // end of loop
                if (PreviewCel == EditView[SelectedLoop].Cels.Count - 1) {
                    tmrMotion.Enabled = false;
                    tspCycle.Image = PlayImage;
                }
                else {
                    PreviewCel++;
                }
                break;
            case 3:
                // reverse loop
                if (PreviewCel == 0) {
                    tmrMotion.Enabled = false;
                    tspCycle.Image = StopImage;
                }
                else {
                    PreviewCel--;
                }
                break;
            }
            DisplayPrevCel();
        }

        private void picCel_MouseDown(object sender, MouseEventArgs e) {

            // left button + shift key overrides all modes and
            // starts a drag so check for drag first
            if (e.Button == MouseButtons.Left && (SelectedTool == ViewEditToolType.Edit || ModifierKeys == Keys.Shift)) {
                // when edit tool is active, click to drag; otherwise,
                // shift-click to drag
                if (hsbCel.Visible || vsbCel.Visible) {
                    DragPT = e.Location;
                    blnDragging = true;
                    SetCursor(ViewCursor.DragSurface);
                    return;
                }
            }

            // no other operations use key modifiers
            if (ModifierKeys != Keys.None) {
                return;
            }
            // set color based on button pressed
            switch (e.Button) {
            case MouseButtons.Left:
                DrawCol = LeftColor;
                break;
            case MouseButtons.Right:
                DrawCol = RightColor;
                break;
            default:
                return;
            }
            if (SelectedTool == ViewEditToolType.Erase) {
                // override color
                DrawCol = EditView[SelectedLoop][SelectedCel].TransColor;
            }
            // calculate the AGI pixel position
            ViewPt.X = (int)(e.Location.X / (2 * ScaleFactor));
            ViewPt.Y = (int)(e.Location.Y / ScaleFactor);

            // drawing only happens if drawtool selected so check
            // for tool status first

            if (SelectedTool == ViewEditToolType.Edit || SelectedTool == ViewEditToolType.Select) {
                if (e.Button == MouseButtons.Left) {
                    // left:
                    if (SelectedTool == ViewEditToolType.Select) {
                        // select or drag select:
                        // if within selection region
                        if (SelectionVisible && Selection.Bounds.Contains(ViewPt)) {
                            BeginMoveSelection(ViewPt);
                        }
                        else {
                            // set operation to setsel
                            CurrentOperation = ViewEditOperation.opSetSelection;
                            HideSelection();
                            mX = e.X;
                            mY = e.Y;
                            // set anchor to current pixel
                            AnchorPt = ViewPt;
                        }
                    }
                    else {
                        // resize cel
                        if (e.Location.Y > picCel.Height - 3) {
                            // at bottom edge - change height
                            ChangingHeight = EditView[SelectedLoop][SelectedCel].Height;
                            CurrentOperation = ViewEditOperation.opChangeHeight;
                        }
                        if (e.Location.X > picCel.Width - 3) {
                            // at right edge - change width
                            // (or both, if height is also being changed)
                            ChangingWidth = EditView[SelectedLoop][SelectedCel].Width;
                            if (CurrentOperation == ViewEditOperation.opChangeHeight) {
                                CurrentOperation = ViewEditOperation.opChangeBoth;
                            }
                            else {
                                CurrentOperation = ViewEditOperation.opChangeWidth;
                            }
                        }
                    }
                }
                else {
                    //right:
                    // context menu
                }
            }
            else {
                // drawing
                switch (SelectedTool) {
                case ViewEditToolType.Draw:
                case ViewEditToolType.Erase:
                    // start a new draw op
                    PixelData = new();
                    PixelInfo pixel = new();
                    pixel.Location = ViewPt;
                    pixel.Value = EditView[SelectedLoop][SelectedCel][ViewPt.X, ViewPt.Y];
                    PixelData.Add(pixel);
                    // draw the new pixel
                    EditView[SelectedLoop][SelectedCel][ViewPt.X, ViewPt.Y] = (byte)DrawCol;
                    // set operation
                    CurrentOperation = ViewEditOperation.opDraw;
                    DisplayCel();
                    if (ShowPreview && PreviewCel == SelectedCel) {
                        picPreview.Invalidate();
                    }
                    break;
                case ViewEditToolType.Line:
                case ViewEditToolType.Rectangle:
                case ViewEditToolType.BoxFill:
                    // start draw operation
                    CurrentOperation = ViewEditOperation.opDraw;
                    AnchorPt = ViewPt;
                    DisplayCel();
                    break;
                case ViewEditToolType.Paint:
                    FloodFill(ViewPt, DrawCol);
                    break;
                }
            }
        }

        private void picCel_MouseMove(object sender, MouseEventArgs e) {
            Point tmpPt = new(0, 0);
            // check for dragging of cel
            if (blnDragging) {
                if (hsbCel.Visible) {
                    int newL = picCel.Left + e.X - DragPT.X;
                    if (newL < -(hsbCel.Maximum - hsbCel.LargeChange + 1)) {
                        newL = -(hsbCel.Maximum - hsbCel.LargeChange + 1);
                    }
                    if (newL > VE_MARGIN) {
                        newL = VE_MARGIN;
                    }
                    picCel.Left = newL;
                    hsbCel.Value = -newL;
                }
                if (vsbCel.Visible) {
                    int newT = picCel.Top + e.Y - DragPT.Y;
                    if (newT < -(vsbCel.Maximum - vsbCel.LargeChange + 1)) {
                        newT = -(vsbCel.Maximum - vsbCel.LargeChange + 1);
                    }
                    if (newT > VE_MARGIN) {
                        newT = VE_MARGIN;
                    }
                    picCel.Top = newT;
                    vsbCel.Value = -newT;
                }
                return;
            }

            // check for on-edge (to set cursor as needed)
            // this has to happen before the pixel value is set
            if (SelectedTool == ViewEditToolType.Edit) { // || SelectedTool == ViewEditToolType.Select) {
                if (CurrentOperation == ViewEditOperation.opNone) {
                    if (e.Location.X > picCel.Width - 3 || e.Location.Y > picCel.Height - 3) {
                        // check for corner
                        if (e.Location.X > picCel.Width - 3 && e.Location.Y > picCel.Height - 3) {
                            SetCursor(ViewCursor.ResizeBoth);
                        }
                        else if (e.Location.X > picCel.Width - 3) {
                            SetCursor(ViewCursor.ResizeWidth);
                        }
                        else {
                            SetCursor(ViewCursor.ResizeHeight);
                        }
                    }
                    else {
                        if (SelectedTool == ViewEditToolType.Edit) {
                            // use regular cursor
                            SetCursor(ViewCursor.Edit);
                        }
                    }
                }
            }

            // before selecting a single pixel there has to 
            // some mouse movement
            if (CurrentOperation == ViewEditOperation.opSetSelection && !SelectionVisible) {
                if (mX != e.X || mY != e.Y) {
                    SelectionVisible = true;
                    tmrSelect.Enabled = true;
                    SelectRegion(AnchorPt);
                }
            }
            // calculate agi coordinates
            tmpPt.X = (int)(e.X / (2 * ScaleFactor));
            tmpPt.Y = (int)(e.Y / ScaleFactor);
            // limit coordinates based on current op
            switch (CurrentOperation) {
            case ViewEditOperation.opMoveSelection:
                // restrict movement to keep at leat one
                // pixel on the image
                // selection must overlap by at least one pixel
                if (tmpPt.X - CelOffset.X + Selection.Bounds.Width < 1) {
                    tmpPt.X = 1 + CelOffset.X - Selection.Bounds.Width;
                }
                if (tmpPt.Y - CelOffset.Y + Selection.Bounds.Height < 1) {
                    tmpPt.Y = 1 + CelOffset.Y - Selection.Bounds.Height;
                }
                if (tmpPt.X - CelOffset.X > EditView[SelectedLoop][SelectedCel].Width - 1) {
                    tmpPt.X = CelOffset.X + EditView[SelectedLoop][SelectedCel].Width - 1;
                }
                if (tmpPt.Y - CelOffset.Y > EditView[SelectedLoop][SelectedCel].Height - 1) {
                    tmpPt.Y = CelOffset.Y + EditView[SelectedLoop][SelectedCel].Height - 1;
                }
                break;
            case ViewEditOperation.opChangeWidth:
            case ViewEditOperation.opChangeHeight:
            case ViewEditOperation.opChangeBoth:
                // allow height and width to exceed cel dimensions, 
                // but not less than one (new dimension is pt val + 1)
                if (tmpPt.X < 0) {
                    tmpPt.X = 0;
                }
                if (tmpPt.Y < 0) {
                    tmpPt.Y = 0;
                }
                break;
            default:
                // must be with in the cel dimensions
                if (tmpPt.X < 0) {
                    tmpPt.X = 0;
                }
                else if (tmpPt.X >= EditView[SelectedLoop][SelectedCel].Width) {
                    tmpPt.X = EditView[SelectedLoop][SelectedCel].Width - 1;
                }
                if (tmpPt.Y < 0) {
                    tmpPt.Y = 0;
                }
                else if (tmpPt.Y >= EditView[SelectedLoop][SelectedCel].Height) {
                    tmpPt.Y = EditView[SelectedLoop][SelectedCel].Height - 1;
                }
                break;
            }
            // if no change, do nothing
            if (tmpPt == ViewPt) {
                return;
            }
            // save cursor position
            ViewPt = tmpPt;
            // when moving mouse, always update the status bar
            spCurX.Text = "X: " + ViewPt.X;
            spCurY.Text = "Y: " + ViewPt.Y;
            // set cursor to provide feedback on current operation
            switch (CurrentOperation) {
            case ViewEditOperation.opNone:
                switch (SelectedTool) {
                case ViewEditToolType.Edit:
                    break;
                case ViewEditToolType.Select:
                    // if inside selection box
                    if (SelectionVisible && Selection.Bounds.Contains(ViewPt)) {
                        SetCursor(ViewCursor.Move);
                    }
                    else {
                        SetCursor(ViewCursor.Select);
                    }
                    break;
                }
                break;
            case ViewEditOperation.opSetSelection:
                // select region defined by current position and anchor
                SelectRegion(ViewPt);
                break;
            case ViewEditOperation.opMoveSelection:
                MoveSelection(new(ViewPt.X - CelOffset.X, ViewPt.Y - CelOffset.Y));
                break;
            case ViewEditOperation.opDraw:
                switch (SelectedTool) {
                case ViewEditToolType.Draw:
                case ViewEditToolType.Erase:
                    // save pixel data
                    PixelInfo pixel = new();
                    pixel.Location = ViewPt;
                    pixel.Value = EditView[SelectedLoop][SelectedCel][ViewPt.X, ViewPt.Y];
                    PixelData.Add(pixel);
                    // update cel data
                    EditView[SelectedLoop][SelectedCel][ViewPt.X, ViewPt.Y] = (byte)DrawCol;
                    DisplayCel();
                    if (ShowPreview && PreviewCel == SelectedCel) {
                        picPreview.Invalidate();
                    }
                    break;
                case ViewEditToolType.Line:
                    picCel.Invalidate();
                    break;
                case ViewEditToolType.Rectangle:
                    picCel.Invalidate();
                    break;
                case ViewEditToolType.BoxFill:
                    picCel.Invalidate();
                    break;
                }
                break;
            case ViewEditOperation.opChangeWidth:
                // set width
                picCel.Width = (int)((ViewPt.X + 1) * 2 * ScaleFactor);
                ChangingWidth = ViewPt.X + 1;
                propertyGrid1.Refresh();
                break;
            case ViewEditOperation.opChangeHeight:
                // set height
                picCel.Height = (int)((ViewPt.Y + 1) * ScaleFactor);
                ChangingHeight = ViewPt.Y + 1;
                propertyGrid1.Refresh();
                break;
            case ViewEditOperation.opChangeBoth:
                // set both
                picCel.Width = (int)((ViewPt.X + 1) * 2 * ScaleFactor);
                picCel.Height = (int)((ViewPt.Y + 1) * ScaleFactor);
                ChangingWidth = ViewPt.X + 1;
                ChangingHeight = ViewPt.Y + 1;
                propertyGrid1.Refresh();
                break;
            }
        }

        private void picCel_MouseUp(object sender, MouseEventArgs e) {
            if (blnDragging) {
                blnDragging = false;
                SetCursor(ToolCursor);
                return;
            }
            ViewUndo NextUndo;

            ViewPt.X = (int)(e.Location.X / (2 * ScaleFactor));
            ViewPt.Y = (int)(e.Location.Y / ScaleFactor);
            if (CurrentOperation != ViewEditOperation.opMoveSelection) {
                // force lower limit to zero
                if (ViewPt.X < 0) {
                    ViewPt.X = 0;
                }
                if (ViewPt.Y < 0) {
                    ViewPt.Y = 0;
                }
                // force upper limit to cel width/height
                if (ViewPt.X > EditView[SelectedLoop][SelectedCel].Width - 1) {
                    ViewPt.X = EditView[SelectedLoop][SelectedCel].Width - 1;
                }
                if (ViewPt.Y > EditView[SelectedLoop][SelectedCel].Height - 1) {
                    ViewPt.Y = EditView[SelectedLoop][SelectedCel].Height - 1;
                }
            }
            switch (CurrentOperation) {
            case ViewEditOperation.opSetSelection:
                // if a selection is visible
                if (SelectionVisible) {
                    // set the selection
                    SetSelection();
                }
                break;
            case ViewEditOperation.opMoveSelection:
                if (SelectionMoved) {
                    EndMoveSelection(ViewPt.X - CelOffset.X, ViewPt.Y - CelOffset.Y);
                }
                break;
            case ViewEditOperation.opDraw:
                // set undo object
                NextUndo = new();
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                // if line or box
                if (SelectedTool == ViewEditToolType.Rectangle || SelectedTool == ViewEditToolType.BoxFill) {
                    // swap so first variable is always lowest
                    if (Selection.Bounds.X > ViewPt.X) {
                        (ViewPt.X, Selection.Bounds.X) = (Selection.Bounds.X, ViewPt.X);
                    }
                    if (Selection.Bounds.Y > ViewPt.Y) {
                        (ViewPt.Y, Selection.Bounds.Y) = (Selection.Bounds.Y, ViewPt.Y);
                    }
                }
                switch (SelectedTool) {
                case ViewEditToolType.Draw:
                case ViewEditToolType.Erase:
                    // if erasing
                    if (SelectedTool == ViewEditToolType.Erase) {
                        NextUndo.UDAction = Erase;
                    }
                    else {
                        NextUndo.UDAction = Draw;
                    }
                    // add pixel data
                    NextUndo.PixelData = PixelData;
                    break;
                case ViewEditToolType.Line:
                    NextUndo.UDAction = Line;
                    NextUndo.PixelData = new();
                    // draw the line
                    DrawLineOnCel(AnchorPt, ViewPt, NextUndo);
                    break;
                case ViewEditToolType.Rectangle:
                    NextUndo.UDAction = Box;
                    DrawBoxOnCel(DrawCol, AnchorPt, ViewPt, false, NextUndo);
                    break;
                case ViewEditToolType.BoxFill:
                    NextUndo.UDAction = BoxFill;
                    DrawBoxOnCel(DrawCol, AnchorPt, ViewPt, true, NextUndo);
                    break;
                }
                AddUndo(NextUndo);
                MarkAsChanged();
                DisplayCel();
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                break;
            case ViewEditOperation.opChangeWidth:
            case ViewEditOperation.opChangeHeight:
            case ViewEditOperation.opChangeBoth:
                // save height of picture box
                // (because changewidth call will reset
                // it to previous Value)
                ViewPt.Y = (int)(picCel.Height / ScaleFactor);
                // if width was being adjusted

                if (CurrentOperation == ViewEditOperation.opChangeWidth || CurrentOperation == ViewEditOperation.opChangeBoth) {
                    // if width has changed,
                    if (EditView[SelectedLoop][SelectedCel].Width != picCel.Width / (ScaleFactor * 2)) {
                        // set new width
                        ChangeWidth((int)(picCel.Width / (ScaleFactor * 2)));
                    }
                }
                // if height was being adjusted
                if (CurrentOperation == ViewEditOperation.opChangeHeight || CurrentOperation == ViewEditOperation.opChangeBoth) {
                    // if height has changed
                    if (EditView[SelectedLoop][SelectedCel].Height != ViewPt.Y) {
                        // set new height
                        ChangeHeight(ViewPt.Y);
                    }
                }
                // reset operation so property can be updated
                CurrentOperation = ViewEditOperation.opNone;
                propertyGrid1.Refresh();
                // reset cursor
                SetCursor(ViewCursor.Select);

                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                break;
            }
            // always reset operation
            CurrentOperation = ViewEditOperation.opNone;
        }

        private void picCel_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point panelPt = splitCanvas.Panel1.PointToClient(Cursor.Position);
            if (!splitCanvas.Panel1.ClientRectangle.Contains(panelPt)) {
                return;
            }
            ChangeScale(e.Delta, true);
        }

        private void picCel_MouseLeave(object sender, EventArgs e) {
            // hide cursor location information when mouse is not over
            // the draw surface
            spCurX.Text = "";
            spCurY.Text = "";
        }

        private void picCel_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (CurrentOperation == ViewEditOperation.opNone) {
                SelectAll();
            }
        }

        private void picCel_Paint(object sender, PaintEventArgs e) {
            Graphics cg = e.Graphics;
            cg.InterpolationMode = InterpolationMode.NearestNeighbor;
            cg.PixelOffsetMode = PixelOffsetMode.Half;
            // draw temp features based on current draw operation
            switch (CurrentOperation) {
            case ViewEditOperation.opDraw:
                switch (SelectedTool) {
                case ViewEditToolType.Line:
                    DrawLineOnImage(cg, DrawCol, AnchorPt, ViewPt);
                    break;
                case ViewEditToolType.Rectangle:
                case ViewEditToolType.BoxFill:
                    DrawBoxOnImage(cg, DrawCol, AnchorPt, ViewPt, SelectedTool == ViewEditToolType.BoxFill);
                    break;
                }
                break;
            }
            if (ScaleFactor > 3 && ShowGrid) {
                // draw grid over the cel
                DrawCelGrid(cg);
            }
            // draw selected area bounds, if a selection is active
            if (SelectionVisible && Selection.Bounds.Width > 0 && Selection.Bounds.Height > 0) {
                Rectangle rgn = new((int)(Selection.Bounds.X * ScaleFactor * 2) + 1,
                    (int)(Selection.Bounds.Y * ScaleFactor) + 1,
                    (int)(Selection.Bounds.Width * ScaleFactor * 2) - 1,
                    (int)(Selection.Bounds.Height * ScaleFactor) - 1);
                cg.DrawRectangle(dash1, rgn);
                cg.DrawRectangle(dash2, rgn);
            }
        }

        private void preview_MouseDown(object sender, MouseEventArgs e) {
            if (hsbPreview.Visible || vsbPreview.Visible) {
                // picPreview movement needs to be translated to the panel
                if (sender == picPreview) {
                    DragPT = pnlPreview.PointToClient(picPreview.PointToScreen(e.Location));
                }
                else {
                    DragPT = e.Location;
                }
                blnDragging = true;
                MemoryStream msCursor = new(EditorResources.EPC_MOVE);
                picPreview.Cursor = new Cursor(msCursor);
                pnlPreview.Cursor = new Cursor(msCursor);
            }
        }

        private void preview_MouseMove(object sender, MouseEventArgs e) {
            if (blnDragging) {
                Point pnlPT;
                if (sender == picPreview) {
                    pnlPT = pnlPreview.PointToClient(picPreview.PointToScreen(e.Location));
                }
                else {
                    pnlPT = e.Location;
                }
                if (hsbPreview.Visible) {
                    int newL = pnlPreview.Left + pnlPT.X - DragPT.X;
                    if (newL < -(hsbPreview.Maximum - hsbPreview.LargeChange + 1)) {
                        newL = -(hsbPreview.Maximum - hsbPreview.LargeChange + 1);
                    }
                    if (newL > VE_MARGIN) {
                        newL = VE_MARGIN;
                    }
                    pnlPreview.Left = newL;
                    hsbPreview.Value = -newL;
                }
                if (vsbPreview.Visible) {
                    int newT = pnlPreview.Top + pnlPT.Y - DragPT.Y;
                    if (newT < -(vsbPreview.Maximum - vsbPreview.LargeChange + 1)) {
                        newT = -(vsbPreview.Maximum - vsbPreview.LargeChange + 1);
                    }
                    if (newT > VE_MARGIN) {
                        newT = VE_MARGIN;
                    }
                    //pnlPreview.Top = -vsbPreview.Value + toolStrip2.Bottom;
                    pnlPreview.Top = newT + toolStrip2.Bottom;
                    vsbPreview.Value = -newT;
                    Debug.Print("drag newT: " + newT);
                }
                return;
            }
        }

        private void preview_MouseUp(object sender, MouseEventArgs e) {
            if (blnDragging) {
                blnDragging = false;
                picPreview.Cursor = Cursors.Default;
                pnlPreview.Cursor = Cursors.Default;
            }
        }

        private void preview_DoubleClick(object sender, EventArgs e) {
            // let user change background color of preview panel
            frmPalette NewPalette = new(1);
            if (NewPalette.ShowDialog(MDIMain) == DialogResult.OK) {
                splitCanvas.Panel2.BackColor = NewPalette.SelColor;
                if (TransparentPreview) {
                    DrawTransGrid(splitCanvas.Panel2, pnlPreview.Left % 10, pnlPreview.Top % 10);
                }
            }
            NewPalette.Dispose();
            //force redraw of preview cel
            if (pnlPreview.Visible) {
                DisplayCel();
            }
        }

        private void preview_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point panelPt = splitCanvas.Panel2.PointToClient(Cursor.Position);
            if (!splitCanvas.Panel2.ClientRectangle.Contains(panelPt)) {
                return;
            }
            ChangePreviewScale(e.Delta, true);
        }

        private void splitForm_MouseUp(object sender, MouseEventArgs e) {
            picCel.Select();
        }

        private void splitCanvas_MouseUp(object sender, MouseEventArgs e) {
            picCel.Select();
        }

        private void splitCanvas_Panel1_Resize(object sender, EventArgs e) {
            if (!Visible) {
                return;
            }
            SetEScrollbars();
        }

        private void splitCanvas_Panel2_Resize(object sender, EventArgs e) {
            if (!Visible) {
                return;
            }
            SetPScrollbars();
            if (TransparentPreview) {
                DrawTransGrid(splitCanvas.Panel2, pnlPreview.Left % 10, pnlPreview.Top % 10);
            }
        }

        private void tmrSelect_Tick(object sender, EventArgs e) {
            if (SelectionVisible && Selection.Bounds.Width > 0 && Selection.Bounds.Height > 0) {
                // decrement for clockwise movement, increment for 
                // counterclockwise movement
                dashdistance -= 1;
                if (dashdistance == 0) dashdistance = 6;
                dash1.DashOffset = dashdistance;
                dash2.DashOffset = dashdistance - 3;

                //force refresh
                picCel.Invalidate();
            }
        }

        private void hsbCel_Scroll(object sender, ScrollEventArgs e) {
            picCel.Left = -hsbCel.Value;
        }

        private void vsbCel_Scroll(object sender, ScrollEventArgs e) {
            picCel.Top = -vsbCel.Value;
        }

        private void hsbPreview_Scroll(object sender, ScrollEventArgs e) {
            pnlPreview.Left = -hsbPreview.Value;
            if (TransparentPreview) {
                DrawTransGrid(splitCanvas.Panel2, pnlPreview.Left % 10, pnlPreview.Top % 10);
            }
        }

        private void vsbPreview_Scroll(object sender, ScrollEventArgs e) {
            pnlPreview.Top = -vsbPreview.Value + toolStrip2.Bottom;
            if (TransparentPreview) {
                DrawTransGrid(splitCanvas.Panel2, pnlPreview.Left % 10, pnlPreview.Top % 10);
            }
        }

        private void trkSpeed_ValueChanged(object sender, EventArgs e) {
            //  tmrMotion.Interval = 600 / trkSpeed.Value - 45;
            //  tmrMotion.Interval = 750 / trkSpeed.Value - 45;
            tmrMotion.Interval = 900 / trkSpeed.Value - 50;
        }

        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            // determine color from x,Y position
            AGIColorIndex bytNewCol = (AGIColorIndex)(8 * (e.Y / 16) + (e.X / (picPalette.Width / 8)));

            switch (e.Button) {
            case MouseButtons.Left:
                switch (ModifierKeys) {
                case Keys.None:
                    LeftColor = bytNewCol;
                    break;
                case Keys.Control:
                    // transcolor- if a cel is selected
                    if (SelectedLoop >= 0 && SelectedCel >= 0 && SelectedCel != EditView[SelectedLoop].Cels.Count) {
                        if (bytNewCol != EditView[SelectedLoop][SelectedCel].TransColor) {
                            ChangeTransColor(bytNewCol);
                        }
                    }
                    break;
                }
                break;
            case MouseButtons.Right:
                if (ModifierKeys == Keys.None) {
                    RightColor = bytNewCol;
                }
                break;
            }
            picPalette.Refresh();
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            // update the palette to show available colors, as well as the current
            // right/left pen colors and cel transparency color (if a cel is selected)

            float dblWidth = picPalette.Width / 8;
            float dblHeight = picPalette.Height / 2;
            Graphics g = e.Graphics;

            // color area
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 8; j++) {
                    Color color = EditPalette[i * 8 + j];
                    g.FillRectangle(new SolidBrush(color), j * dblWidth, i * dblHeight, dblWidth, dblHeight);
                }
            }
            Color textcolor;
            Font font = new Font("Arial", 10, FontStyle.Bold);

            // add 'L' for left button color
            if ((int)LeftColor > 9) {
                textcolor = Color.Black;
            }
            else {
                textcolor = Color.White;
            }
            g.DrawString("L", font, new SolidBrush(textcolor), dblWidth * ((int)LeftColor % 8) + 3, 17 * ((int)LeftColor / 8));

            // add 'R' for right-button color
            if ((int)RightColor > 9) {
                textcolor = Color.Black;
            }
            else {
                textcolor = Color.White;
            }
            g.DrawString("R", font, new SolidBrush(textcolor), dblWidth * (((int)RightColor % 8) + 1) - 13, 17 * ((int)RightColor / 8));

            // if a cel is selected, add a 'T' in center of current transparency color
            if (SelectedLoop >= 0 && SelectedCel >= 0) {
                // make sure it's not an end-of - loop or end-of - cel
                if (SelectedLoop == EditView.Loops.Count ||
                          SelectedCel == EditView[SelectedLoop].Cels.Count) {
                    return;
                }
                AGIColorIndex lngTransCol = EditView[SelectedLoop][SelectedCel].TransColor;
                if ((int)lngTransCol > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                g.DrawString("T", font, new SolidBrush(textcolor),
                    dblWidth * ((int)lngTransCol % 8) + dblWidth / 2 - 3,
                    17 * ((int)lngTransCol / 8));
            }
        }

        private void spScale_MouseDown(object sender, MouseEventArgs e) {
            switch (e.Button) {
            case MouseButtons.Left:
                // zoom in
                ChangeScale(1);
                break;
            case MouseButtons.Right:
                // zoom out
                ChangeScale(-1);
                break;
            }
        }

        #endregion

        #region temp code
        void tmpviewform() {
            /*
      public void MenuClickHelp() {
        // help
        Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\winagi\View_Editor.htm");
      return;
      }
            */
        }
        #endregion

        #region Methods
        public void InitFonts() {
            tvwView.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
        }

        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spTool = new ToolStripStatusLabel();
            spCurX = new ToolStripStatusLabel();
            spCurY = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;

            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(70, 18);
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
            spScale.MouseDown += spScale_MouseDown;
            // 
            // spTool
            // 
            spTool.AutoSize = false;
            spTool.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spTool.BorderStyle = Border3DStyle.SunkenInner;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(70, 18);
            spTool.Text = SelectedTool.ToString();
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
        }

        public bool LoadView(Engine.View loadview, int StartLoop = 0, int StartCel = 0) {
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

            UpdateTree();
            SelectedLoop = StartLoop;
            SelectedCel = StartCel;
            (tvwView.SelectedNode = ViewNode.Nodes[SelectedLoop].Nodes[SelectedCel]).EnsureVisible();

            // select start cel
            SelectCel(SelectedLoop, SelectedCel, true);
            if (ShowPreview) {
                DisplayPrevLoop();
            }
            else {
                splitCanvas.Panel2Collapsed = true;
                tmrMotion.Enabled = false;
            }
            tsbPaste.Enabled = ClipboardHasCel() || Clipboard.ContainsImage();
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
            UpdateTree();
            SelectedLoop = 0;
            SelectedCel = 0;
            (tvwView.SelectedNode = ViewNode.Nodes[SelectedLoop].Nodes[SelectedCel]).EnsureVisible();
            if (ShowPreview) {
                DisplayPrevLoop();
            }
            else {
                // hide preview
                splitCanvas.Panel2Collapsed = true;
                tmrMotion.Enabled = false;
            }
            // display start cel
            DisplayCel();
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

        private void SetCursor(ViewCursor NewCursor) {
            MemoryStream msCursor;

            if (CurCursor == NewCursor) {
                return;
            }
            CurCursor = NewCursor;
            switch (NewCursor) {
            case ViewCursor.NoOp:
                picCel.Cursor = Cursors.No;
                break;
            case ViewCursor.Edit:
                picCel.Cursor = Cursors.Arrow;
                break;
            case ViewCursor.Cross:
                picCel.Cursor = Cursors.Cross;
                break;
            case ViewCursor.Select:
                msCursor = new(EditorResources.EVC_EDITSEL);
                picCel.Cursor = new Cursor(msCursor);
                break;
            case ViewCursor.Move:
                picCel.Cursor = Cursors.SizeAll;
                break;
            case ViewCursor.Draw:
                msCursor = new(EditorResources.EVC_DRAW);
                picCel.Cursor = new Cursor(msCursor);
                break;
            case ViewCursor.Erase:
                msCursor = new(EditorResources.EVC_ERASE);
                picCel.Cursor = new Cursor(msCursor);
                break;
            case ViewCursor.Paint:
                msCursor = new(EditorResources.EVC_FILL);
                picCel.Cursor = new Cursor(msCursor);
                break;
            case ViewCursor.DragSurface:
                msCursor = new(EditorResources.EPC_MOVE);
                picCel.Cursor = new Cursor(msCursor);
                break;
            case ViewCursor.ResizeHeight:
                picCel.Cursor = Cursors.SizeNS;
                break;
            case ViewCursor.ResizeWidth:
                picCel.Cursor = Cursors.SizeWE;
                break;
            case ViewCursor.ResizeBoth:
                picCel.Cursor = Cursors.SizeNWSE;
                break;
            }
        }

        private void UpdateTree() {
            // loads the tree view with the view structure
            // by comparing the current tree with the view
            // structure, and adding or removing nodes as needed
            // add view as root

            // count up loops until all are added/removed from
            // the tree
            for (int i = 0; i < 256; i++) {
                if (i <= EditView.Loops.Count) {
                    if (i >= ViewNode.Nodes.Count) {
                        // not in tree, so add it
                        if (i == EditView.Loops.Count) {
                            ViewNode.Nodes.Add("L" + i, "End", 3, 3);
                        }
                        else {
                            ViewNode.Nodes.Add("L" + i, "Loop " + i, 1, 1);
                        }
                    }
                    else {
                        // already in tree, make sure name/icon is correct
                        if (i == EditView.Loops.Count) {
                            // end loop
                            if (ViewNode.Nodes[i].Text != "End") {
                                ViewNode.Nodes[i].Text = "End";
                                ViewNode.Nodes[i].ImageIndex = 3;
                                ViewNode.Nodes[i].SelectedImageIndex = 3;
                            }
                        }
                        else {
                            // loop
                            if (ViewNode.Nodes[i].Text != "Loop " + i) {
                                ViewNode.Nodes[i].Text = "Loop " + i;
                                ViewNode.Nodes[i].ImageIndex = 1;
                                ViewNode.Nodes[i].SelectedImageIndex = 1;
                            }
                        }
                    }
                }
                else if (i < ViewNode.Nodes.Count) {
                    // the rest of the nodes are not needed
                    // remove them
                    do {
                        ViewNode.Nodes.RemoveAt(i);
                    } while (i != ViewNode.Nodes.Count);
                    break;
                }
                else {
                    // done
                    break;
                }
            }
            // now step through each loop, and add/remove cels
            // to the tree as needed
            for (int i = 0; i < EditView.Loops.Count; i++) {
                for (int j = 0; j < 256; j++) {
                    if (j <= EditView.Loops[i].Cels.Count) {
                        if (j >= ViewNode.Nodes[i].Nodes.Count) {
                            // not in tree, so add it
                            if (j == EditView.Loops[i].Cels.Count) {
                                ViewNode.Nodes[i].Nodes.Add("L" + i + "C" + j, "End", 3, 3);
                            }
                            else {
                                ViewNode.Nodes[i].Nodes.Add("L" + i + "C" + j, "Cel " + j, 2, 2);
                            }
                        }
                        else {
                            // already in tree, make sure name/icon is correct
                            if (j == EditView.Loops[i].Cels.Count) {
                                // end cel
                                if (ViewNode.Nodes[i].Nodes[j].Text != "End") {
                                    ViewNode.Nodes[i].Nodes[j].Text = "End";
                                    ViewNode.Nodes[i].Nodes[j].ImageIndex = 3;
                                    ViewNode.Nodes[i].Nodes[j].SelectedImageIndex = 3;
                                }
                            }
                            else {
                                // cel
                                if (ViewNode.Nodes[i].Nodes[j].Text != "Cel " + j) {
                                    ViewNode.Nodes[i].Nodes[j].Text = "Cel " + j;
                                    ViewNode.Nodes[i].Nodes[j].ImageIndex = 2;
                                    ViewNode.Nodes[i].Nodes[j].SelectedImageIndex = 2;
                                }
                            }
                        }
                    }
                    else if (j < ViewNode.Nodes[i].Nodes.Count) {
                        // the rest of the nodes are not needed
                        // remove them
                        do {
                            ViewNode.Nodes[i].Nodes.RemoveAt(j);
                        } while (j != ViewNode.Nodes[i].Nodes.Count);
                        break;
                    }
                    else {
                        // done
                        break;
                    }
                }
            }
            // last node (endloop) needs to be empty
            ViewNode.Nodes[EditView.Loops.Count].Nodes.Clear();
        }

        private void SelectView() {
            // root
            SelectedLoop = -1;
            SelectedCel = -1;
            ViewMode = ViewEditMode.View;
            propertyGrid1.SelectedObject = new ViewEditViewProperties(this);
            if (picCel.Visible && ShowGrid) {
                splitCanvas.Panel1.Invalidate();
            }
            picCel.Visible = false;
            if (ShowPreview) {
                DisablePreview();
            }
            if (!ViewNode.IsSelected) {
                tvwView.SelectedNode = ViewNode;
                ViewNode.EnsureVisible();
            }
            HideSelection();
            ConfigureToolbar();
            // update palette
            picPalette.Invalidate();
        }

        private void SelectLoop(int loopnum) {
            SelectedLoop = loopnum;
            SelectedCel = -1;
            if (loopnum < ViewNode.Nodes.Count - 1) {
                ViewMode = ViewEditMode.Loop;
                if (InGame && EditGame.InterpreterVersion == "2.089") {
                    propertyGrid1.SelectedObject = new ViewEditLoop2089Properties(this, EditView[SelectedLoop]);
                }
                else {
                    propertyGrid1.SelectedObject = new ViewEditLoopProperties(this, EditView[SelectedLoop]);
                }
                CyclePreview = WinAGISettings.DefPrevPlay.Value;
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
            }
            else {
                ViewMode = ViewEditMode.EndLoop;
                propertyGrid1.SelectedObject = null;
                if (ShowPreview) {
                    DisablePreview();
                }
            }
            if (picCel.Visible && ShowGrid) {
                splitCanvas.Panel1.Invalidate();
            }
            picCel.Visible = false;
            if (!ViewNode.Nodes[SelectedLoop].IsSelected) {
                tvwView.SelectedNode = ViewNode.Nodes[SelectedLoop];
                ViewNode.Nodes[SelectedLoop].EnsureVisible();
            }
            HideSelection();
            ConfigureToolbar();
            // update palette
            picPalette.Invalidate();
        }

        private void SelectCel(int loopnum, int celnum, bool forceloop = false) {
            if (loopnum != SelectedLoop || forceloop) {
                SelectedLoop = loopnum;
                CyclePreview = WinAGISettings.DefPrevPlay.Value;
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
            }
            SelectedLoop = loopnum;
            SelectedCel = celnum;
            if (celnum < ViewNode.Nodes[SelectedLoop].Nodes.Count - 1) {
                ViewMode = ViewEditMode.Cel;
                DisplayCel(true);
                propertyGrid1.SelectedObject = new ViewEditCelProperties(this, EditView[SelectedLoop][SelectedCel]);
                if (ShowPreview) {
                    if (!pnlPreview.Visible) {
                        EnablePreview();
                    }
                    if (!tmrMotion.Enabled) {
                        PreviewCel = celnum;
                    }
                    // validate preview cel, in case loop changed
                    if (PreviewCel >= EditView[SelectedLoop].Cels.Count) {
                        PreviewCel = EditView[SelectedLoop].Cels.Count - 1;
                    }
                    DisplayPrevCel();
                }
            }
            else {
                ViewMode = ViewEditMode.EndCel;
                picCel.Visible = false;
                propertyGrid1.SelectedObject = null;
                if (ShowPreview) {
                    if (!tmrMotion.Enabled) {
                        DisablePreview();
                    }
                }
            }
            if (!ViewNode.Nodes[SelectedLoop].Nodes[SelectedCel].IsSelected) {
                tvwView.SelectedNode = ViewNode.Nodes[SelectedLoop].Nodes[SelectedCel];
                ViewNode.Nodes[SelectedLoop].Nodes[SelectedCel].EnsureVisible();
            }
            HideSelection();
            ConfigureToolbar();
            // update palette
            picPalette.Invalidate();
        }

        private void DisplayCel(bool ResetPosition = false) {
            picCel.Visible = true;
            if (ResetPosition) {
                picCel.Location = new(VE_MARGIN, VE_MARGIN);
            }
            Cel currentcel = EditView[SelectedLoop][SelectedCel];
            // set transparent color
            picCel.BackColor = EditPalette[(int)currentcel.TransColor];
            // set height/width of edit area
            picCel.Width = (int)(currentcel.Width * 2 * ScaleFactor);
            picCel.Height = (int)(currentcel.Height * ScaleFactor);

            // draw cel bitmap on pixel cel
            picCel.Image = new Bitmap(picCel.Width, picCel.Height);
            Graphics gc = Graphics.FromImage(picCel.Image);
            // now draw the cel, with scaling mode set to give crisp
            // pixel edges
            gc.InterpolationMode = InterpolationMode.NearestNeighbor;
            gc.PixelOffsetMode = PixelOffsetMode.Half;
            gc.DrawImage(currentcel.CelImage, 0, 0, picCel.Width, picCel.Height);
            picCel.Refresh();
            // update scrollbars
            SetEScrollbars();
            if (ViewMode != ViewEditMode.Cel) {
                // reset mode to cel
                ViewMode = ViewEditMode.Cel;
                HideSelection();
            }
            // force redraw of property grid
            propertyGrid1.Refresh();
        }

        private void DrawCelGrid(Graphics gc) {

            // draw grid over the cel
            int sngH = picCel.Height;
            int sngW = picCel.Width;
            gc.InterpolationMode = InterpolationMode.NearestNeighbor;
            gc.PixelOffsetMode = PixelOffsetMode.Half;

            Pen gridPen = new Pen(GRID_COLOR);
            float i = ScaleFactor * 2;
            do {
                gc.DrawLine(gridPen, i, 0, i, sngH);
                i += ScaleFactor * 2;
            } while (i <= sngW);
            i = ScaleFactor;
            do {
                gc.DrawLine(gridPen, 0, i, sngW, i);
                i += ScaleFactor;
            } while (i <= sngH);
        }

        public void ChangeScale(int Dir, bool useanchor = false) {
            // valid scale factors are:
            // 100%, 125%, 150%, 175%, 200%, 225%, 250%, 275%, 300%,
            // 400%, 500%, 600%, 700%, 800%, 900%, 1000%, 1100%, 1200%, 
            // 1300%, 1400%, 1500%, 1600%, 1700%, 1800%, 1900%, 2000%
            float oldscale = 0;
            if (useanchor) {
                oldscale = ScaleFactor;
            }
            switch (Dir) {
            case > 0:
                if (ScaleFactor < 3) {
                    ScaleFactor += 0.25f;
                }
                else if (ScaleFactor < 20) {
                    ScaleFactor += 1;
                }
                break;
            case < 0:
                if (ScaleFactor > 3) {
                    ScaleFactor -= 1;
                }
                else if (ScaleFactor > 1) {
                    ScaleFactor -= 0.25f;
                }
                break;
            }
            if (oldscale == ScaleFactor) {
                return;
            }
            if (ViewMode == ViewEditMode.Cel) {
                _ = SendMessage(splitCanvas.Panel1.Handle, WM_SETREDRAW, false, 0);
                _ = SendMessage(picCel.Handle, WM_SETREDRAW, false, 0);
                // resize image
                picCel.Width = (int)(EditView[SelectedLoop][SelectedCel].Width * ScaleFactor * 2);
                picCel.Height = (int)(EditView[SelectedLoop][SelectedCel].Height * ScaleFactor);
                // then set the scrollbars
                SetEScrollbars(oldscale);
                // redraw cel at new scale
                DisplayCel();
                _ = SendMessage(splitCanvas.Panel1.Handle, WM_SETREDRAW, true, 0);
                _ = SendMessage(picCel.Handle, WM_SETREDRAW, true, 0);
                splitCanvas.Refresh();
            }
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
        }

        private void SetEScrollbars(float oldscale = 0) {
            bool showHSB = false, showVSB = false;
            Point anchorpt = new(-1, -1);

            // determine if scrollbars are necessary
            showHSB = picCel.Width > (splitCanvas.Panel1.ClientSize.Width - 2 * VE_MARGIN);
            showVSB = picCel.Height > (splitCanvas.Panel1.ClientSize.Height - 2 * VE_MARGIN - (showHSB ? hsbCel.Height : 0));
            // check horizontal again(in case addition of vert scrollbar forces it to be shown)
            showHSB = picCel.Width > (splitCanvas.Panel1.ClientSize.Width - 2 * VE_MARGIN - (showVSB ? vsbCel.Width : 0));
            // initial positions
            hsbCel.Width = splitCanvas.Panel1.ClientSize.Width;
            hsbCel.Top = splitCanvas.Panel1.ClientSize.Height - hsbCel.Height;
            vsbCel.Height = splitCanvas.Panel1.ClientSize.Height;
            vsbCel.Left = splitCanvas.Panel1.ClientSize.Width - vsbCel.Width;
            if (showHSB && showVSB) {
                // allow for corner
                picCelCorner.Left = vsbCel.Left;
                picCelCorner.Top = hsbCel.Top;
                picCelCorner.Visible = true;
                vsbCel.Height -= hsbCel.Height;
                hsbCel.Width -= vsbCel.Width;
            }
            else {
                picCelCorner.Visible = false;
            }
            hsbCel.Visible = showHSB;
            vsbCel.Visible = showVSB;
            if (oldscale > 0) {
                Point cp = splitCanvas.Panel1.PointToClient(Cursor.Position);
                if (splitCanvas.Panel1.ClientRectangle.Contains(cp)) {
                    // use this anchor
                    anchorpt = cp;
                }
                else {
                    // not a valid anchor
                    oldscale = 0;
                }
            }
            // now adjust all scrollbar parameters as needed
            AdjustScrollbars(ScaleFactor, oldscale, anchorpt, hsbCel, vsbCel, picCel, splitCanvas.Panel1.ClientSize);
        }

        private void AdjustScrollbars(double newscale, double oldscale, Point anchor, HScrollBar hsb, VScrollBar vsb, Control image, Size panelsize) {
            // Scrollbar math:
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
            // when including margins, the calculations are modified to:
            //      ACT_SZ = MGN + IMG_SZ + MGN
            //      SB_MIN = -MGN
            //      SV_MAX = ACT_SZ - WIN_SZ + SB_MIN
            //             = MGN + IMG_SZ + MGN + SB_MIN - WIN_SZ
            //             = MGN + IMG_SZ + MGN - MGN - WIN_SZ
            //      SV_MAX = IMG_SZ - WIN_SZ + MGN

            if (hsb.Visible) {
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                hsb.Maximum = image.Width;
                hsb.LargeChange = (int)(panelsize.Width * LG_SCROLL);
                hsb.SmallChange = (int)(panelsize.Width * SM_SCROLL);
                // calculate actual max (when image is fully scrolled to right)
                int SV_MAX = image.Width - (panelsize.Width - (vsb.Visible ? vsb.Width : 0)) + VE_MARGIN;
                // control MAX value equals actual Max + LargeChange - 1
                hsb.Maximum = SV_MAX + hsb.LargeChange - 1;
                int newscroll;
                if (oldscale > 0) {
                    // if cursor is over the image, use cursor pos as anchor point
                    // the correct algebra to make this work is:
                    //         SB1 = SB0 + (SB0 + WAN - MGN) * (SF1 / SF0 - 1)
                    // SB = scrollbar value
                    // WAN = panel client window anchor point (get from cursor pos)
                    // MGN is the left/top margin
                    // SF = scale factor (as calculated above)
                    // -0 = previous values
                    // -1 = new (desired) values
                    newscroll = (int)(hsb.Value + (hsb.Value + anchor.X - VE_MARGIN) * (newscale / oldscale - 1));
                }
                else {
                    newscroll = hsb.Value;
                }
                if (newscroll < -VE_MARGIN) {
                    hsb.Value = -VE_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    hsb.Value = SV_MAX;
                }
                else {
                    hsb.Value = newscroll;
                }
            }
            else {
                // reset to default
                hsb.Value = -VE_MARGIN;
            }
            // readjust picture position
            image.Left = -hsb.Value;

            // repeat for vertical scrollbar
            if (vsb.Visible) {
                vsb.Maximum = image.ClientSize.Height;
                vsb.LargeChange = (int)(panelsize.Height * LG_SCROLL);
                vsb.SmallChange = (int)(panelsize.Height * SM_SCROLL);
                int SV_MAX = image.Height - (panelsize.Height - (hsb.Visible ? hsb.Height : 0)) + VE_MARGIN;
                vsb.Maximum = SV_MAX + vsb.LargeChange - 1;
                int newscroll;
                if (oldscale > 0) {
                    newscroll = (int)(vsb.Value + (vsb.Value + anchor.Y - VE_MARGIN) * (newscale / oldscale - 1));
                }
                else {
                    newscroll = vsb.Value;
                }
                if (newscroll < -VE_MARGIN) {
                    vsb.Value = -VE_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    vsb.Value = SV_MAX;
                }
                else {
                    vsb.Value = newscroll;
                }
            }
            else {
                vsb.Value = -VE_MARGIN;
            }
            image.Top = -vsb.Value + vsb.Top;
        }

        internal void ChangeHeight(int NewCelHeight, bool DontUndo = false) {
            // height should be validated before
            // calling this, but just in case:
            // validate Height
            if (NewCelHeight < 1) {
                NewCelHeight = 1;
            }
            if (NewCelHeight > 167) {
                NewCelHeight = 167;
            }
            // local copy of current height
            int CelHeight = EditView[SelectedLoop][SelectedCel].Height;
            if (NewCelHeight == CelHeight) {
                // if no change just exit
                return;
            }

            if (!DontUndo) {
                int CelWidth = EditView[SelectedLoop][SelectedCel].Width;
                //set undo properties
                ViewUndo NextUndo = new();
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UndoData = [CelHeight];
                if (NewCelHeight > CelHeight) {
                    NextUndo.UDAction = IncHeight;
                }
                else {
                    NextUndo.UDAction = DecHeight;
                    // store data being eliminated
                    byte[,] tmpCelData = new byte[CelWidth, CelHeight - NewCelHeight];
                    for (int j = NewCelHeight; j < CelHeight; j++) {
                        for (int i = 0; i < CelWidth; i++) {
                            //add this cel pixel's color
                            tmpCelData[i, j - NewCelHeight] = EditView[SelectedLoop][SelectedCel][i, j];
                        }
                    }
                    NextUndo.CelData = tmpCelData;
                }
                AddUndo(NextUndo);
            }
            // set new height
            EditView[SelectedLoop][SelectedCel].Height = (byte)NewCelHeight;
            // redraw cel
            DisplayCel();
            if (ShowPreview) {
                DisplayPrevLoop();
            }
        }

        internal void ChangeWidth(int NewCelWidth, bool DontUndo = false) {
            // width should be validated before
            // calling this, but just in case:
            if (NewCelWidth < 1) {
                NewCelWidth = 1;
            }
            if (NewCelWidth > 159) {
                NewCelWidth = 159;
            }
            // local copy of current width
            int CelWidth = EditView[SelectedLoop][SelectedCel].Width;
            if (NewCelWidth == CelWidth) {
                // if no change just exit
                return;
            }
            if (!DontUndo) {
                // local copy of height
                int CelHeight = EditView[SelectedLoop][SelectedCel].Height;
                ViewUndo NextUndo = new();
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UndoData = [CelWidth];
                // if new width is greater than old width
                if (NewCelWidth > CelWidth) {
                    NextUndo.UDAction = IncWidth;
                }
                else {
                    NextUndo.UDAction = DecWidth;
                    // store data being eliminated
                    byte[,] tmpCelData = new byte[CelWidth - NewCelWidth, CelHeight];
                    for (int i = NewCelWidth; i < CelWidth; i++) {
                        for (int j = 0; j < CelHeight; j++) {
                            // add this cel pixel's color
                            tmpCelData[i - NewCelWidth, j] = EditView[SelectedLoop][SelectedCel][i, j];
                        }
                    }
                    NextUndo.CelData = tmpCelData;
                }
                AddUndo(NextUndo);
            }
            // set new width
            EditView[SelectedLoop][SelectedCel].Width = (byte)NewCelWidth;
            // redraw cel
            DisplayCel();
            if (ShowPreview) {
                DisplayPrevLoop();
            }
        }

        internal void ChangeTransColor(AGIColorIndex newcolor, bool DontUndo = false) {
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ChangeTransCol;
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UndoData = [(byte)EditView[SelectedLoop][SelectedCel].TransColor];
                AddUndo(NextUndo);
            }
            // change transcolor
            EditView[SelectedLoop][SelectedCel].TransColor = newcolor;
            // change background to match color
            picCel.BackColor = EditPalette[(int)newcolor];
            // redraw cel
            DisplayCel();
            if (ShowPreview) {
                DisplayPrevCel();
            }
        }

        internal void ChangeMirror(MirrorLoopOptions newmirror, bool DontUndo = false) {
            // determine if a change was made:
            if ((int)newmirror == EditView[SelectedLoop].MirrorLoop) {
                return;
            }
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = Mirror;
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UndoData = [EditView[SelectedLoop].MirrorLoop];

                if (!EditView[SelectedLoop].Mirrored) {
                    // there wasn't a mirror, and there is now so store old loop in undo
                    NextUndo.UndoLoop = new();
                    NextUndo.UndoLoop.CloneFrom(EditView[SelectedLoop]);
                }
                AddUndo(NextUndo);
            }
            // always unmirror the loop first
            EditView[SelectedLoop].UnMirror();
            // if unmirror was NOT the choice
            if (newmirror != MirrorLoopOptions.None) {
                // set mirror
                EditView.SetMirror((byte)SelectedLoop, (byte)newmirror);
                // update tree
                UpdateTree();
            }
            // select and expand current loop
            (tvwView.SelectedNode = ViewNode.Nodes[SelectedLoop]).EnsureVisible();
        }

        internal void ChangeViewDesc(string newtext, bool DontUndo = false) {
            // replace crlf with single linefeed
            newtext = newtext.Replace("\r\n", "\n");
            if (newtext == EditView.ViewDescription) {
                // no change
                return;
            }
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ChangeVDesc;
                NextUndo.OldText = EditView.ViewDescription;
                AddUndo(NextUndo);
            }
            EditView.ViewDescription = newtext;
            MarkAsChanged();
        }

        private bool ClipboardHasLoop() {
            if (Clipboard.ContainsData(VIEW_CB_FMT)) {
                ViewClipboardData viewCBData = Clipboard.GetData(VIEW_CB_FMT) as ViewClipboardData;
                if (viewCBData == null) {
                    return false;
                }
                return viewCBData.Mode == ViewClipboardMode.Loop;
            }
            return false;
        }

        private bool ClipboardHasCel() {
            if (Clipboard.ContainsData(VIEW_CB_FMT)) {
                ViewClipboardData viewCBData = Clipboard.GetData(VIEW_CB_FMT) as ViewClipboardData;
                if (viewCBData == null) {
                    return false;
                }
                return viewCBData.Mode == ViewClipboardMode.Cel;
            }
            return false;
        }

        private void DeleteLoop(byte loopnum, bool DontUndo = false) {
            // delete the loop
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = DelLoop;
                NextUndo.UDLoopNo = loopnum;
                NextUndo.UndoLoop = new();
                if (EditView[loopnum].Mirrored) {
                    // only need the mirror loop for undo
                    NextUndo.UndoData = [EditView[loopnum].MirrorLoop];
                }
                else {
                    // if not mirrored, save the loop for undo
                    NextUndo.UndoLoop.CloneFrom(EditView[loopnum]);
                }
                AddUndo(NextUndo);
            }
            EditView.Loops.Remove(loopnum);
            UpdateTree();
            if (loopnum == EditView.Loops.Count) {
                // if last loop was deleted, select the previous one
                loopnum--;
            }
            SelectLoop(loopnum);
        }

        private void DeleteCel(byte loopnum, byte celnum, bool DontUndo = false) {
            // delete the cel
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = DelCel;
                NextUndo.UDLoopNo = loopnum;
                NextUndo.UDCelNo = celnum;
                NextUndo.UndoCel = EditView[loopnum][celnum].Clone();
                AddUndo(NextUndo);
            }
            EditView[loopnum].Cels.Remove((byte)SelectedCel);
            UpdateTree();
            if (SelectedCel == EditView[loopnum].Cels.Count) {
                // if last cel was deleted, select the previous one
                SelectedCel--;
            }
            SelectCel(loopnum, SelectedCel);
        }

        private bool InsertLoop(int insertpos, Loop insertloop = null, bool DontUndo = false) {
            // inserts a new loop into the view; if insertloop is
            // null, the added loop will contain a single cel with 
            // default width and height. 

            // rare, but check for max number of loops
            if (EditView.Loops.Count == MAXLOOPS) {
                MessageBox.Show(MDIMain,
                    "Can't exceed " + MAXLOOPS + " loops per view.",
                    "Can't Insert Loop",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\views.htm#loop");
                return false;
            }
            if (insertloop == null) {
                // create a new loop
                insertloop = new Loop();
                insertloop.Cels.Add(0, WinAGISettings.DefCelW.Value, WinAGISettings.DefCelH.Value, AGIColorIndex.Black);
            }
            EditView.Loops.Add(insertpos);
            EditView[insertpos].CloneFrom(insertloop);

            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = AddLoop;
                NextUndo.UDLoopNo = insertpos;
                AddUndo(NextUndo);
            }
            UpdateTree();
            return true;
        }

        private bool InsertCel(int loopnum, int insertpos, Cel insertcel = null, bool DontUndo = false) {
            // adds a cel to selected loop
            byte newW, newH;
            AGIColorIndex newT;

            // rare, but check for max cel count
            if (EditView[SelectedLoop].Cels.Count == MAXCELS) {
                MessageBox.Show(MDIMain,
                    "Can't exceed " + MAXCELS + " cels per loop.",
                    "Can't Insert Cel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\views.htm#cel");
                return false;
            }

            if (insertcel == null) {
                insertcel = new();
                if (insertpos == EditView[SelectedLoop].Cels.Count) {
                    newW = EditView[SelectedLoop][insertpos - 1].Width;
                    newH = EditView[SelectedLoop][insertpos - 1].Height;
                    newT = EditView[SelectedLoop][insertpos - 1].TransColor;
                }
                else {
                    newH = WinAGISettings.DefCelH.Value;
                    newW = WinAGISettings.DefCelW.Value;
                    newT = AGIColorIndex.Black;
                }
                EditView[SelectedLoop].Cels.Add(insertpos, newW, newH, AGIColorIndex.Black);
            }
            else {
                EditView[SelectedLoop].Cels.Add(insertpos);
                EditView[SelectedLoop][insertpos].CloneFrom(insertcel);
            }
            UpdateTree();
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = AddCel;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UDLoopNo = SelectedLoop;
                AddUndo(NextUndo);
            }
            return true;
        }

        private void ClearLoop(int loopnum) {
            // deletes all cels in selected loop except one and sets
            // the remaining cel to one by one with black transcolor

            ViewUndo NextUndo = new();
            NextUndo.UDAction = ViewUndo.ActionType.ClearLoop;
            NextUndo.UDLoopNo = loopnum;
            NextUndo.UndoLoop.CloneFrom(EditView[loopnum]);
            // need to know if the loop is mirrored
            NextUndo.UndoData = [EditView[loopnum].MirrorLoop];
            AddUndo(NextUndo);

            if (EditView[loopnum].Cels.Count > 1) {
                // delete 'em
                int j = EditView[loopnum].Cels.Count - 1;
                for (int i = j; i > 0; i--) {
                    EditView[loopnum].Cels.Remove(i);
                }
            }
            // clear the remaining cel
            EditView[loopnum][0].Clear();
        }

        private void ClearCel(int loopnum, int celnum) {
            ViewUndo NextUndo = new();
            NextUndo.UDAction = ViewUndo.ActionType.ClearCel;
            NextUndo.UDLoopNo = loopnum;
            NextUndo.UDCelNo = celnum;
            NextUndo.UndoCel = EditView[loopnum][celnum].Clone();
            AddUndo(NextUndo);
            // clear the selected cel
            EditView[loopnum][celnum].Clear();
        }

        private void FlipCelH(int loopnum, int celnum, bool DontUndo = false) {
            byte CelWidth = EditView[loopnum][celnum].Width;

            // flip celdata
            for (byte j = 0; j < EditView[loopnum][celnum].Height; j++) {
                for (byte i = 0; i <= CelWidth / 2 - 1; i++) {
                    (EditView[loopnum][celnum][CelWidth - 1 - i, j],
                        EditView[loopnum][celnum][i, j]) =
                        (EditView[loopnum][celnum][i, j],
                        EditView[loopnum][celnum][CelWidth - 1 - i, j]);
                }
            }
            if (!DontUndo) {
                ViewUndo NextUndo = new() {
                    UDAction = ViewUndo.ActionType.FlipCelH,
                    UDLoopNo = loopnum,
                    UDCelNo = celnum
                };
                AddUndo(NextUndo);
            }
        }

        private void FlipCelV(int loopnum, int celnum, bool DontUndo = false) {
            byte CelHeight = EditView[loopnum][celnum].Height;

            // flip celdata
            for (byte i = 0; i < EditView[loopnum][celnum].Width; i++) {
                for (byte j = 0; j <= CelHeight / 2 - 1; j++) {
                    (EditView[loopnum][celnum][i, CelHeight - 1 - j],
                        EditView[loopnum][celnum][i, j]) =
                        (EditView[loopnum][celnum][i, j],
                        EditView[loopnum][celnum][i, CelHeight - 1 - j]);
                }
            }
            if (!DontUndo) {
                ViewUndo NextUndo = new() {
                    UDAction = ViewUndo.ActionType.FlipCelV,
                    UDLoopNo = loopnum,
                    UDCelNo = celnum
                };
                AddUndo(NextUndo);
            }
        }

        private void FlipSelectionH(bool DontUndo = false) {
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ViewUndo.ActionType.FlipSelectionH;
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UDSelection = Selection;
                AddUndo(NextUndo);
            }

            int startx = Selection.Bounds.X;
            int starty = Selection.Bounds.Y;
            int endx = Selection.Bounds.Right;
            int endy = Selection.Bounds.Bottom;
            // flip the selection data
            for (int i = startx; i < startx + Selection.Bounds.Width / 2; i++) {
                for (int j = starty; j < endy; j++) {
                    (Selection.Data[i - startx, j - starty],
                     Selection.Data[endx - i - 1, j - starty]) =
                    (Selection.Data[endx - i - 1, j - starty],
                     Selection.Data[i - startx, j - starty]);
                }
            }
            // update cel data
            for (int i = startx; i < endx; i++) {
                for (int j = starty; j < endy; j++) {
                    if (i >= 0 && j >= 0 &&
                        i < EditView[SelectedLoop][SelectedCel].Width &&
                        j < EditView[SelectedLoop][SelectedCel].Height) {
                        if (!TransSel || Selection.Data[i - startx, j - starty] != (byte)EditView[SelectedLoop][SelectedCel].TransColor) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                        }
                        else {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                        }
                    }
                }
            }
        }

        private void FlipSelectionV(bool DontUndo = false) {
            if (!DontUndo) {
                ViewUndo NextUndo = new();
                NextUndo.UDAction = ViewUndo.ActionType.FlipSelectionV;
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UDSelection = Selection;
                AddUndo(NextUndo);
            }

            int startx = Selection.Bounds.X;
            int starty = Selection.Bounds.Y;
            int endx = Selection.Bounds.Right;
            int endy = Selection.Bounds.Bottom;
            // flip the selection data
            for (int i = startx; i < endx; i++) {
                for (int j = starty; j < starty + Selection.Bounds.Height / 2; j++) {
                    (Selection.Data[i - startx, j - starty],
                     Selection.Data[i - startx, endy - j - 1]) =
                    (Selection.Data[i - startx, endy - j - 1],
                     Selection.Data[i - startx, j - starty]);
                }
            }
            // update cel data
            for (int i = startx; i < endx; i++) {
                for (int j = starty; j < endy; j++) {
                    if (i >= 0 && j >= 0 &&
                        i < EditView[SelectedLoop][SelectedCel].Width &&
                        j < EditView[SelectedLoop][SelectedCel].Height) {
                        if (!TransSel || Selection.Data[i - startx, j - starty] != (byte)EditView[SelectedLoop][SelectedCel].TransColor) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                        }
                        else {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                        }
                    }
                }
            }
        }

        private void SelectAll() {
            if (ViewMode == ViewEditMode.Cel) {
                if (SelectedTool != ViewEditToolType.Select) {
                    SelectedTool = ViewEditToolType.Select;
                    tsbTool.Image = tstSelect.Image;
                    spTool.Text = SelectedTool.ToString();
                    SetCursor(ViewCursor.Select);
                }
                // select entire Image
                SelectionVisible = true;
                tmrSelect.Enabled = true;
                AnchorPt = new(0, 0);
                SelectRegion(new(EditView[SelectedLoop][SelectedCel].Width - 1, EditView[SelectedLoop][SelectedCel].Height - 1));
                //set the selection
                SetSelection();
                // always reset operation
                CurrentOperation = ViewEditOperation.opNone;
            }
        }

        private void SelectRegion(Point endpt) {
            // adjust Selection.Bounds rectangle to upper-left
            // format based on anchor and current end point
            // and adjust to be one pixel outside selection
            // area
            if (endpt.X < AnchorPt.X) {
                Selection.Bounds.X = endpt.X;
            }
            else {
                Selection.Bounds.X = AnchorPt.X;
            }
            if (endpt.Y < AnchorPt.Y) {
                Selection.Bounds.Y = endpt.Y;
            }
            else {
                Selection.Bounds.Y = AnchorPt.Y;
            }
            Selection.Bounds.Width = Math.Abs(AnchorPt.X - endpt.X) + 1;
            Selection.Bounds.Height = Math.Abs(AnchorPt.Y - endpt.Y) + 1;
        }

        private void SetSelection() {
            // copy data and under-data (for a selection, it wil be blank)
            Selection.Data = new byte[Selection.Bounds.Width, Selection.Bounds.Height];
            Selection.UnderData = new byte[Selection.Bounds.Width, Selection.Bounds.Height];
            byte tc = (byte)EditView[SelectedLoop][SelectedCel].TransColor;
            for (int i = 0; i < Selection.Bounds.Width; i++) {
                for (int j = 0; j < Selection.Bounds.Height; j++) {
                    Selection.Data[i, j] = EditView[SelectedLoop][SelectedCel][i + Selection.Bounds.X, j + Selection.Bounds.Y];
                    Selection.UnderData[i, j] = tc;
                }
            }
            ConfigureToolbar();
        }

        private void HideSelection() {
            SelectionVisible = false;
            tmrSelect.Enabled = false;
            picCel.Invalidate();
        }

        private void BeginMoveSelection(Point anchorpt) {
            // set operation to move selection
            CurrentOperation = ViewEditOperation.opMoveSelection;
            AnchorPt = Selection.Bounds.Location;
            SelectionMoved = false;
            // get offset to upper left corner of selection from cel where pointer is at:
            CelOffset.X = anchorpt.X - Selection.Bounds.Location.X;
            CelOffset.Y = anchorpt.Y - Selection.Bounds.Location.Y;
        }

        private void MoveSelection(Point movept, bool pasting = false) {
            // restore data under
            int startx , starty, endx, endy;

            if (!pasting) {
                startx = Selection.Bounds.X;
                starty = Selection.Bounds.Y;
                endx = Selection.Bounds.Right;
                endy = Selection.Bounds.Bottom;
                // put under data back in cel
                for (int i = startx; i < endx; i++) {
                    for (int j = starty; j < endy; j++) {
                        if (i >= 0 && j >= 0 &&
                            i < EditView[SelectedLoop][SelectedCel].Width &&
                            j < EditView[SelectedLoop][SelectedCel].Height) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                        }
                    }
                }
            }
            // move selection
            Selection.Bounds.X = movept.X;
            Selection.Bounds.Y = movept.Y;

            // add selection and update data under
            startx = Selection.Bounds.X;
            starty = Selection.Bounds.Y;
            endx = Selection.Bounds.Right;
            endy = Selection.Bounds.Bottom;
            for (int i = startx; i < endx; i++) {
                for (int j = starty; j < endy; j++) {
                    if (i >= 0 && j >= 0 &&
                        i < EditView[SelectedLoop][SelectedCel].Width &&
                        j < EditView[SelectedLoop][SelectedCel].Height) {
                        Selection.UnderData[i - startx, j - starty] = EditView[SelectedLoop][SelectedCel][i, j];
                        if (!TransSel || Selection.Data[i - startx, j - starty] != (byte)EditView[SelectedLoop][SelectedCel].TransColor) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.Data[i - startx, j - starty];
                        }
                    }
                }
            }
            DisplayCel();
            if (ShowPreview) {
                DisplayPrevLoop();
            }

            // has it moved from starting point?
            SelectionMoved = Selection.Bounds.Location != AnchorPt;
        }

        private void EndMoveSelection(int movex, int movey) {

            Selection.Bounds.X = movex;
            Selection.Bounds.Y = movey;
            // save undo info
            ViewUndo NextUndo = new();
            NextUndo.UDAction = ViewUndo.ActionType.MoveSelection;
            NextUndo.UDLoopNo = SelectedLoop;
            NextUndo.UDCelNo = SelectedCel;
            NextUndo.UDSelection = Selection;
            NextUndo.UndoData = new int[2];
            // original location
            NextUndo.UndoData[0] = AnchorPt.X;
            NextUndo.UndoData[1] = AnchorPt.Y;
            AddUndo(NextUndo);
            ConfigureToolbar();
        }

        private void CopySelection() {
            if (SelectionVisible) {
                Bitmap selimage = new(Selection.Bounds.Width, Selection.Bounds.Height);
                using (Graphics g = Graphics.FromImage(selimage)) {
                    g.DrawImage(EditView[SelectedLoop][SelectedCel].CelImage,
                        new Rectangle(0, 0, Selection.Bounds.Width, Selection.Bounds.Height),
                        Selection.Bounds,
                        GraphicsUnit.Pixel);
                }
                Clipboard.SetImage(selimage);
            }
        }

        private void PasteSelection(Point pastepos) {
            if (!Clipboard.ContainsImage()) {
                // no data to paste
                return;
            }
            Bitmap pasteimage;
            byte[,] pixeldata;
            int width, height;
            try {
                pasteimage = (Bitmap)Clipboard.GetImage();
                // limit to 160 x 168
                width = pasteimage.Width;
                height = pasteimage.Height;
                if (width > 160) {
                    width = 160;
                }
                if (height > 167) {
                    height = 167;
                }
                // crop the bitmap if necessary
                if (width != pasteimage.Width || height != pasteimage.Height) {
                    Rectangle croprect = new Rectangle(0, 0, width, height);
                    pasteimage = pasteimage.Clone(croprect, pasteimage.PixelFormat);
                }
                // convert colors to nearest AGIIndex values
                pixeldata = new byte[width, height];
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        pixeldata[i, j] = NearestAGIColor(pasteimage.GetPixel(i, j));
                        pasteimage.SetPixel(i, j, EditPalette[pixeldata[i, j]]);
                    }
                }
            }
            catch (Exception ex) {
                // ignore if bitmap can't be retrieved
                return;
            }
            // if there is an existing Selection, replace it
            // otherwise, put the pasted selection at 0,0
            if (!SelectionVisible) {
                Selection = new();
            }
            Selection.Bounds.Location = pastepos;
            // reset the image size
            Selection.Bounds.Width = width;
            Selection.Bounds.Height = height;
            // assign data
            Selection.Data = pixeldata;
            Selection.UnderData = new byte[width, height];
            // enable selection
            SelectionVisible = true;
            tmrSelect.Enabled = true;
            // use move and endselectionmove to add pasted data, and to copy data under selection
            MoveSelection(Selection.Bounds.Location, true);
            EndMoveSelection(Selection.Bounds.Left, Selection.Bounds.Top);
            // change last Undo to undo-paste
            UndoCol.Peek().UDAction = ViewUndo.ActionType.PasteSelection;
            // force tool to 'select'
            SelectedTool = ViewEditToolType.Select;
            SetCursor(ViewCursor.Select);
        }

        private void DeleteSelection() {
            if (SelectionVisible) {
                // save undo data
                ViewUndo NextUndo = new();
                NextUndo.UDAction = DelSelection;
                NextUndo.UDLoopNo = SelectedLoop;
                NextUndo.UDCelNo = SelectedCel;
                NextUndo.UDSelection = Selection;
                AddUndo(NextUndo);
                // delete data (put under data back in cel)
                int startx = Selection.Bounds.X;
                int starty = Selection.Bounds.Y;
                int endx = Selection.Bounds.Right;
                int endy = Selection.Bounds.Bottom;
                // 
                for (int i = startx; i < endx; i++) {
                    for (int j = starty; j < endy; j++) {
                        if (i >= 0 && j >= 0 &&
                            i < EditView[SelectedLoop][SelectedCel].Width &&
                            j < EditView[SelectedLoop][SelectedCel].Height) {
                            EditView[SelectedLoop][SelectedCel][i, j] = Selection.UnderData[i - startx, j - starty];
                        }
                    }
                }
                DisplayCel();
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                HideSelection();
            }
        }

        private void DrawBoxOnImage(Graphics g, AGIColorIndex fillcolor, Point startpt, Point endpt, bool BoxFill) {
            // convert line to top-bottom/left-right format
            if (startpt.X > endpt.X) {
                (startpt.X, endpt.X) = (endpt.X, startpt.X);
            }
            if (startpt.Y > endpt.Y) {
                (startpt.Y, endpt.Y) = (endpt.Y, startpt.Y);
            }
            // convert to scale
            startpt.X *= (int)(2 * ScaleFactor);
            startpt.Y *= (int)ScaleFactor;
            //p2.X++;
            endpt.X *= (int)(2 * ScaleFactor);
            //p2.Y++;
            endpt.Y *= (int)ScaleFactor;

            if (BoxFill) {
                SolidBrush bc = new(EditPalette[(int)fillcolor]);
                if (ScaleFactor == 1) {
                    g.FillRectangle(bc, startpt.X, startpt.Y, endpt.X - startpt.X, endpt.Y - startpt.Y);
                }
                else {
                    g.FillRectangle(bc, startpt.X, startpt.Y, endpt.X - startpt.X + ScaleFactor * 2, endpt.Y - startpt.Y + ScaleFactor);
                }
            }
            else {
                if (ScaleFactor == 1) {
                    Pen lc = new(EditPalette[(int)fillcolor]);
                    g.DrawLine(lc, startpt.X, startpt.Y, endpt.X, startpt.Y);
                    g.DrawLine(lc, startpt.X, endpt.Y, endpt.X, endpt.Y);
                    g.DrawLine(lc, startpt.X, startpt.Y, startpt.X, endpt.Y);
                    g.DrawLine(lc, endpt.X, startpt.Y, endpt.X, endpt.Y);
                }
                else {
                    SolidBrush bc = new(EditPalette[(int)fillcolor]);
                    g.FillRectangle(bc, startpt.X, startpt.Y, endpt.X - startpt.X + ScaleFactor * 2, ScaleFactor);
                    g.FillRectangle(bc, startpt.X, endpt.Y, endpt.X - startpt.X + ScaleFactor * 2, ScaleFactor);
                    g.FillRectangle(bc, startpt.X, startpt.Y, ScaleFactor * 2, endpt.Y - startpt.Y + ScaleFactor);
                    g.FillRectangle(bc, endpt.X, startpt.Y, ScaleFactor * 2, endpt.Y - startpt.Y + ScaleFactor);
                }
            }
            picCel.Refresh();
        }

        private void DrawBoxOnCel(AGIColorIndex fillcolor, Point startpt, Point endpt, bool BoxFill, ViewUndo NextUndo, bool DontUndo = false) {
            // draws the box on the cel data

            // convert coordinates to top-bottom/left-right format
            if (endpt.X < startpt.X) {
                (endpt.X, startpt.X) = (startpt.X, endpt.X);
            }
            if (endpt.Y < startpt.Y) {
                (endpt.Y, startpt.Y) = (startpt.Y, endpt.Y);
            }

            if (!DontUndo) {
                // save pixel data under the box
                if (BoxFill) {
                    NextUndo.UndoData = [startpt.X, startpt.Y, endpt.X, endpt.Y];
                    NextUndo.CelData = new byte[endpt.X - startpt.X + 1, endpt.Y - startpt.Y + 1];
                    for (int i = startpt.X; i <= endpt.X; i++) {
                        for (int j = startpt.Y; j <= endpt.Y; j++) {
                            PixelInfo px = new();
                            px.Location = new(i, j);
                            NextUndo.CelData[i - startpt.X, j - startpt.Y] = EditView[SelectedLoop][SelectedCel][i, j];
                            EditView[SelectedLoop][SelectedCel][i, j] = (byte)fillcolor;
                        }
                    }
                }
                else {
                    NextUndo.UndoData = [startpt.X, startpt.Y, endpt.X, endpt.Y];
                    NextUndo.PixelData = new();
                    for (int i = startpt.X; i <= endpt.X; i++) {
                        PixelInfo px = new();
                        px.Location = new(i, startpt.Y);
                        px.Value = EditView[SelectedLoop][SelectedCel][i, startpt.Y];
                        EditView[SelectedLoop][SelectedCel][i, startpt.Y] = (byte)fillcolor;
                        NextUndo.PixelData.Add(px);
                        if (startpt.Y != endpt.Y) {
                            px = new();
                            px.Location = new(i, endpt.Y);
                            px.Value = EditView[SelectedLoop][SelectedCel][i, endpt.Y];
                            EditView[SelectedLoop][SelectedCel][i, endpt.Y] = (byte)fillcolor;
                            NextUndo.PixelData.Add(px);
                        }
                    }
                    for (int j = startpt.Y + 1; j < endpt.Y; j++) {
                        PixelInfo px = new();
                        px.Location = new(startpt.X, j);
                        px.Value = EditView[SelectedLoop][SelectedCel][startpt.X, j];
                        EditView[SelectedLoop][SelectedCel][startpt.X, j] = (byte)fillcolor;
                        NextUndo.PixelData.Add(px);
                        if (startpt.X != endpt.X) {
                            px = new();
                            px.Location = new(endpt.X, j);
                            px.Value = EditView[SelectedLoop][SelectedCel][endpt.X, j];
                            EditView[SelectedLoop][SelectedCel][endpt.X, j] = (byte)fillcolor;
                            NextUndo.PixelData.Add(px);
                        }
                    }
                }
            }
        }

        private void DrawLineOnImage(Graphics g, AGIColorIndex linecolor, Point p1, Point p2) {
            int xPos, yPos, XC, YC, MaxDelta;
            Pen lc = new(EditPalette[(int)linecolor]);
            SolidBrush lb = new(EditPalette[(int)linecolor]);

            // determine height/width  
            int DY = p2.Y - p1.Y;
            int vDir = Math.Sign(DY);
            int DX = p2.X - p1.X;
            int hDir = Math.Sign(DX);
            // check for a point, vertical line, or horizontal line  
            if (DY == 0 || DX == 0) {
                // convert line to top-bottom/left-right format so graphics methods  
                // work correctly  
                if (p2.Y < p1.Y) {
                    // swap  
                    yPos = p1.Y;
                    p1.Y = p2.Y;
                    p2.Y = yPos;
                    DY *= -1;
                }
                if (p2.X < p1.X) {
                    xPos = p1.X;
                    p1.X = p2.X;
                    p2.X = xPos;
                    DX *= -1;
                }
                if (ScaleFactor == 1) {
                    g.DrawLine(lc, p1.X * 2, p1.Y, p2.X * 2, p2.Y);
                }
                else {
                    g.FillRectangle(lb, p1.X * ScaleFactor * 2, p1.Y * ScaleFactor, (DX + 1) * ScaleFactor * 2, (DY + 1) * ScaleFactor);
                }
            }
            else {
                // this line drawing function EXACTLY matches the Sierra drawing function  

                // set the starting point  
                if (ScaleFactor == 1) {
                    g.DrawLine(lc, p1.X * 2, p1.Y, p1.X * 2 + 1, p1.Y);
                }
                else {
                    g.FillRectangle(lb, p1.X * ScaleFactor * 2, p1.Y * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                }
                xPos = p1.X;
                yPos = p1.Y;

                // invert DX and DY if they are negative  
                if (DY < 0) {
                    DY *= -1;
                }
                if ((DX < 0)) {
                    DX *= -1;
                }
                // set up the loop, depending on which direction is largest  
                if (DX >= DY) {
                    MaxDelta = DX;
                    YC = DX / 2;
                    XC = 0;
                }
                else {
                    MaxDelta = DY;
                    XC = DY / 2;
                    YC = 0;
                }
                for (int i = 1; i <= MaxDelta; i++) {
                    YC += DY;
                    if (YC >= MaxDelta) {
                        YC -= MaxDelta;
                        yPos += vDir;
                    }
                    XC += DX;
                    if (XC >= MaxDelta) {
                        XC -= MaxDelta;
                        xPos += hDir;
                    }
                    if (ScaleFactor == 1) {
                        g.DrawLine(lc, xPos * 2, yPos, xPos * 2 + 2, yPos);
                    }
                    else {
                        g.FillRectangle(lb, xPos * ScaleFactor * 2, yPos * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                    }
                }
            }
        }

        private void DrawLineOnCel(Point startpt, Point endpt, ViewUndo NextUndo) {
            // this method mirrors the AGI MSDOS draw function so 
            // lines are guaranteed to be an exact match.

            // determine deltaX/deltaY and direction
            int DY = endpt.Y - startpt.Y;
            int vDir = Math.Sign(DY);
            int DX = endpt.X - startpt.X;
            int hDir = Math.Sign(DX);
            if (DY == 0 && DX == 0) {
                DrawPixel(startpt.X, startpt.Y);
            }
            else if (DY == 0) {
                for (int i = startpt.X; i != endpt.X; i += hDir) {
                    DrawPixel(i, startpt.Y);
                }
                DrawPixel(endpt.X, startpt.Y);
            }
            else if (DX == 0) {
                for (int i = startpt.Y; i != endpt.Y; i += vDir) {
                    DrawPixel(startpt.X, i);
                }
                DrawPixel(startpt.X, endpt.Y);
            }
            else {
                int xPos, yPos;
                int XC, YC, MaxDelta;
                DrawPixel(startpt.X, startpt.Y);
                xPos = startpt.X;
                yPos = startpt.Y;
                // invert DX and DY if they are negative
                if (DY < 0) {
                    DY *= -1;
                }
                if ((DX < 0)) {
                    DX *= -1;
                }
                // set up the loop, depending on which direction is largest
                if (DX >= DY) {
                    MaxDelta = DX;
                    YC = DX / 2;
                    XC = 0;
                }
                else {
                    MaxDelta = DY;
                    XC = DY / 2;
                    YC = 0;
                }
                for (int i = 1; i <= MaxDelta; i++) {
                    YC += DY;
                    if (YC >= MaxDelta) {
                        YC -= MaxDelta;
                        yPos += vDir;
                    }
                    XC += DX;
                    if (XC >= MaxDelta) {
                        XC -= MaxDelta;
                        xPos += hDir;
                    }
                    DrawPixel(xPos, yPos);
                }
            }
            void DrawPixel(int x, int y) {
                // save color
                PixelInfo px = new();
                px.Location = new(x, y);
                px.Value = EditView[SelectedLoop][SelectedCel][x, y];
                NextUndo.PixelData.Add(px);
                // set linecolor in celdata
                EditView[SelectedLoop][SelectedCel][x, y] = (byte)DrawCol;
            }
        }

        private void FloodFill(Point startpt, AGIColorIndex fillcolor) {
            // paints an area with fill color

            // Get the starting color
            byte targetColor = EditView[SelectedLoop][SelectedCel][startpt];

            // ignore if no color change
            if (targetColor == (byte)fillcolor) {
                return;
            }
            // create undo
            ViewUndo NextUndo = new();
            NextUndo.UDAction = PaintFill;
            NextUndo.UDLoopNo = SelectedLoop;
            NextUndo.UDCelNo = SelectedCel;
            NextUndo.PixelData = [];

            // Get the dimensions of the cel
            int width = EditView[SelectedLoop][SelectedCel].Width;
            int height = EditView[SelectedLoop][SelectedCel].Height;

            // Create a queue to store points to process
            Queue<Point> queue = new();
            queue.Enqueue(startpt);

            // Perform the flood-fill
            while (queue.Count > 0) {
                Point current = queue.Dequeue();
                int x = current.X;
                int y = current.Y;

                // Check if the current point is within bounds and matches the target color
                if (x < 0 || x >= width || y < 0 || y >= height || EditView[SelectedLoop][SelectedCel][x, y] != targetColor) {
                    continue;
                }
                // save pixel to undo
                PixelInfo px = new();
                px.Location = new(x, y);
                px.Value = EditView[SelectedLoop][SelectedCel][x, y];
                NextUndo.PixelData.Add(px);
                // Set the current pixel to the fill color
                EditView[SelectedLoop][SelectedCel][x, y] = (byte)fillcolor;
                // Add orthogonally adjacent points to the queue
                queue.Enqueue(new Point(x + 1, y)); // Right
                queue.Enqueue(new Point(x - 1, y)); // Left
                queue.Enqueue(new Point(x, y + 1)); // Down
                queue.Enqueue(new Point(x, y - 1)); // Up
            }
            AddUndo(NextUndo);
            // Refresh the display to show the updated cel
            DisplayCel();
            if (ShowPreview) {
                DisplayPrevCel();
            }
        }

        private void EnablePreview() {
            // enable timer if cycling and more than 1 cel
            tmrMotion.Enabled = CyclePreview && EditView[SelectedLoop].Cels.Count > 1;
            // enable play/stop if more than one cel
            tspCycle.Enabled = EditView[SelectedLoop].Cels.Count > 1;
            // if cycling is active and more than 1 cel, show stop
            // icon, otherwise show play icon
            if (tmrMotion.Enabled) {
                tspCycle.Image = StopImage;
            }
            else {
                tspCycle.Image = PlayImage;
            }
            pnlPreview.Visible = true;
        }

        private void DisablePreview() {
            tmrMotion.Enabled = false;
            tspCycle.Image = PlayImage;
            pnlPreview.Visible = false;
            hsbPreview.Visible = false;
            vsbPreview.Visible = false;
        }

        private void DisplayPrevLoop() {
            switch (ViewMode) {
            case ViewEditMode.Loop:
            case ViewEditMode.Cel:
            case ViewEditMode.EndCel:
                // if endcel, only enable if cycling
                if (ViewMode != ViewEditMode.EndCel || CyclePreview) {
                    EnablePreview();
                    // start with current selection
                    PreviewCel = SelectedCel;
                    if (PreviewCel < 0) {
                        PreviewCel = 0;
                    }
                    else if (PreviewCel > EditView[SelectedLoop].Cels.Count - 1) {
                        PreviewCel = EditView[SelectedLoop].Cels.Count - 1;
                    }
                    // adjust panel size to hold all cels
                    ResizePreviewPanel();
                    //set scroll bars everytime loop is changed
                    SetPScrollbars();
                    // display the preview cel
                    DisplayPrevCel();
                }
                else {
                    DisablePreview();
                }
                    break;
            default:
                DisablePreview();
                break;
            }
        }

        private void ResizePreviewPanel() {
            // determine size of holding pic
            int CelFrameW = 0;
            int CelFrameH = 0;
            for (int i = 0; i <= EditView[SelectedLoop].Cels.Count - 1; i++) {
                if (EditView[SelectedLoop][i].Width > CelFrameW) {
                    CelFrameW = EditView[SelectedLoop][i].Width;
                }
                if (EditView[SelectedLoop][i].Height > CelFrameH) {
                    CelFrameH = EditView[SelectedLoop][i].Height;
                }
            }
            Debug.Assert(CelFrameW > 0 && CelFrameH > 0);
            pnlPreview.Width = (int)(CelFrameW * 2 * PreviewScale);
            pnlPreview.Height = (int)(CelFrameH * PreviewScale);
        }

        private void DisplayPrevCel() {
            int tgtW = (int)(EditView[SelectedLoop][PreviewCel].Width * 2 * PreviewScale);
            int tgtH = (int)(EditView[SelectedLoop][PreviewCel].Height * PreviewScale);
            int tgtX = 0, tgtY = 0;
            switch (lngHAlign) {
            case 0:
                tgtX = 0;
                break;
            case 1:
                tgtX = (pnlPreview.Width - tgtW) / 2;
                break;
            case 2:
                tgtX = pnlPreview.Width - tgtW;
                break;
            }
            switch (lngVAlign) {
            case 0:
                tgtY = 0;
                break;
            case 1:
                tgtY = (pnlPreview.Height - tgtH) / 2;
                break;
            case 2:
                tgtY = pnlPreview.Height - tgtH;
                break;
            }
            picPreview.Left = tgtX;
            picPreview.Top = tgtY;
            picPreview.Width = tgtW;
            picPreview.Height = tgtH;
            picPreview.Image = new Bitmap(picPreview.Width, picPreview.Height);
            using (Graphics gc = Graphics.FromImage(picPreview.Image)) {
                // now draw the cel, with scaling mode set to give crisp
                // pixel edges
                gc.InterpolationMode = InterpolationMode.NearestNeighbor;
                gc.PixelOffsetMode = PixelOffsetMode.Half;
                gc.DrawImage(TransparentPreview ? EditView[SelectedLoop][PreviewCel].TransImage : EditView[SelectedLoop][PreviewCel].CelImage, 0, 0, picPreview.Width, picPreview.Height);
                if (TransparentPreview) {
                    // draw single pixel dots spaced 10 pixels apart over transparent pixels only
                    Bitmap b = new(picPreview.Image);
                    int ofX = (10 - (picPreview.Left) % 10) % 10;
                    int ofY = (10 - (picPreview.Top) % 10) % 10;
                    for (int i = ofX; i < picPreview.Width; i += 10) {
                        for (int j = ofY; j < picPreview.Height; j += 10) {
                            if (b.GetPixel(i, j).A == 0) {
                                gc.FillRectangle(Brushes.Black, new Rectangle(i, j, 1, 1));
                            }
                        }
                    }
                }
            }
            picPreview.Refresh();
        }

        private void SetPScrollbars(float oldscale = 0) {
            bool showHSB = false, showVSB = false;
            Point anchorpt = new(-1, -1);

            // determine if scrollbars are necessary
            showHSB = pnlPreview.Width > (splitCanvas.Panel2.ClientSize.Width - 2 * VE_MARGIN);
            // don't forget to allow for the toolstrip height
            showVSB = pnlPreview.Height > (splitCanvas.Panel2.ClientSize.Height - 2 * VE_MARGIN - 2 * toolStrip2.Height);
            // check horizontal again(in case addition of vert scrollbar forces it to be shown)
            showHSB = pnlPreview.Width > (splitCanvas.Panel2.ClientSize.Width - 2 * VE_MARGIN - (showVSB ? vsbCel.Width : 0));
            // initial positions
            hsbPreview.Width = splitCanvas.Panel2.ClientSize.Width;
            hsbPreview.Top = toolStrip3.Top - hsbPreview.Height;
            vsbPreview.Height = splitCanvas.Panel2.ClientSize.Height - 2 * toolStrip2.Height;
            vsbPreview.Left = splitCanvas.Panel2.ClientSize.Width - vsbPreview.Width;
            if (showHSB && showVSB) {
                // allow for corner
                picPreviewCorner.Left = vsbPreview.Left;
                picPreviewCorner.Top = hsbPreview.Top;
                picPreviewCorner.Visible = true;
                vsbPreview.Height -= hsbPreview.Height;
                hsbPreview.Width -= vsbPreview.Width;
            }
            else {
                picCelCorner.Visible = false;
            }
            hsbPreview.Visible = showHSB;
            vsbPreview.Visible = showVSB;
            if (oldscale > 0) {
                Point cp = splitCanvas.Panel2.PointToClient(Cursor.Position);
                if (splitCanvas.Panel2.ClientRectangle.Contains(cp)) {
                    // use this anchor
                    anchorpt = cp;
                }
                else {
                    // not a valid anchor
                    oldscale = 0;
                }
            }
            // now adjust all scrollbar parameters as needed
            AdjustScrollbars(PreviewScale, oldscale, anchorpt, hsbPreview, vsbPreview, pnlPreview, new(splitCanvas.Panel2.ClientSize.Width, splitCanvas.Panel2.ClientSize.Height - 2 * toolStrip3.Height));
        }

        public void ChangePreviewScale(int Dir, bool useanchor = false) {
            // valid scale factors are:
            // 100%, 125%, 150%, 175%, 200%, 225%, 250%, 275%, 300%,
            // 400%, 500%, 600%, 700%, 800%, 900%, 1000%, 1100%, 1200%, 
            // 1300%, 1400%, 1500%, 1600%, 1700%, 1800%, 1900%, 2000%
            float oldscale = 0;
            if (useanchor) {
                oldscale = PreviewScale;
            }
            switch (Dir) {
            case > 0:
                if (PreviewScale < 3) {
                    PreviewScale += 0.25f;
                }
                else if (PreviewScale < 20) {
                    PreviewScale += 1;
                }
                break;
            case < 0:
                if (PreviewScale > 3) {
                    PreviewScale -= 1;
                }
                else if (PreviewScale > 1) {
                    PreviewScale -= 0.25f;
                }
                break;
            }
            if (oldscale == PreviewScale) {
                return;
            }
            if (pnlPreview.Visible) {
                _ = SendMessage(pnlPreview.Handle, WM_SETREDRAW, false, 0);
                _ = SendMessage(picPreview.Handle, WM_SETREDRAW, false, 0);
                // resize the holding panel
                ResizePreviewPanel();
                // resize image
                picPreview.Width = (int)(EditView[SelectedLoop][PreviewCel].Width * ScaleFactor * 2);
                picPreview.Height = (int)(EditView[SelectedLoop][PreviewCel].Height * ScaleFactor);
                // then set the scrollbars
                SetPScrollbars(oldscale);
                // redraw cel at new scale
                DisplayPrevCel();
                _ = SendMessage(pnlPreview.Handle, WM_SETREDRAW, true, 0);
                _ = SendMessage(picPreview.Handle, WM_SETREDRAW, true, 0);
                splitCanvas.Panel2.Refresh();
                pnlPreview.Refresh();
            }
        }

        private void TogglePreviewMotion() {
            // endloop, view, and loops with only one cel can't be cycled
            switch (ViewMode) {
            case ViewEditMode.View:
            case ViewEditMode.EndLoop:
                return;
            case ViewEditMode.Loop:
                if (EditView[SelectedLoop].Cels.Count <= 1) {
                    return;
                }
                break;
            }
            // toggle motion and update button image
            tmrMotion.Enabled = !tmrMotion.Enabled;
            if (tmrMotion.Enabled) {
                tspCycle.Image = StopImage;
            }
            else {
                CyclePreview = false;
                tspCycle.Image = PlayImage;
            }

            // if enabling, reset startcel for endofloop/reverseloop;
            if (tmrMotion.Enabled) {
                // reset cel, if endofloop or reverseloop motion selected
                switch (lngMotion) {
                case 2:
                    // endofloop
                    if (PreviewCel >= EditView[SelectedLoop].Cels.Count - 1) {
                        PreviewCel = 0;
                    }
                    break;
                case 3:
                    // reverseloop
                    if (PreviewCel <= 0) {
                        PreviewCel = EditView[SelectedLoop].Cels.Count - 1;
                    }
                    break;
                }
                CyclePreview = true;
                DisplayPrevLoop();
            }
            else {
                switch (ViewMode) {
                case ViewEditMode.Loop:
                case ViewEditMode.EndCel:
                    DisablePreview();
                    break;
                case ViewEditMode.Cel:
                    if (PreviewCel != SelectedCel) {
                        PreviewCel = SelectedCel;
                        DisplayPrevCel();
                    }
                    break;
                }
            }
            picPreview.Select();
        }

        private void TogglePreviewTransparency() {
            TransparentPreview = !TransparentPreview;
            if (TransparentPreview) {
                tspTransparency.Text = "ON";
            }
            else {
                tspTransparency.Text = "OFF";
                splitCanvas.Panel2.Invalidate();
            }
            if (ViewMode != ViewEditMode.View) {
                DisplayPrevCel();
            }
        }

        public void RefreshCel() {
            // update palette, force view reset then redraw cel
            if (InGame) {
                EditView.Palette = EditGame.Palette.Clone();
            }
            else {
                EditView.Palette = DefaultPalette.Clone();
            }
            EditView.ResetView();
            DisplayCel();
            DisplayPrevCel();
        }

        private byte NearestAGIColor(Color sourcecolor) {
            // this function is used to quickly convert an EGA color
            // into the closest matching AGI color index

            byte closestcolor = 0;
            int colordifference = int.MaxValue, newdifference;

            // loop until exact match is found, or until all
            // colors are compared
            for (byte i = 0; i < 16; i++) {
                byte bytRed = sourcecolor.R;
                byte bytGreen = sourcecolor.G;
                byte bytBlue = sourcecolor.B;
                newdifference = Math.Abs(bytRed - EditPalette[i].R) +
                                Math.Abs(bytGreen - EditPalette[i].G) +
                                Math.Abs(bytBlue - EditPalette[i].B);
                if (newdifference < colordifference) {
                    colordifference = newdifference;
                    closestcolor = i;
                }
                if (colordifference == 0) {
                    break;
                }
            }
            //return best match
            return closestcolor;
        }

        private void AddUndo(ViewUndo NextUndo) {
            //adds the next undo object
            if (!IsChanged) {
                MarkAsChanged();
            }
            UndoCol.Push(NextUndo);
            tsbUndo.Enabled = true;
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
        #endregion

        public class ViewEditViewProperties {
            frmViewEdit parent;
            Engine.View view;
            public ViewEditViewProperties(frmViewEdit parent) {
                this.parent = parent;
                view = parent.EditView;
            }
            public string ID {
                get => view.ID;
            }
            public string Description {
                get => view.Description;
            }
            public string ViewDesc {
                get => view.ViewDescription;
                set {
                    parent.ChangeViewDesc(value);
                }
            }
        }

        public class ViewEditLoopProperties {
            frmViewEdit parent;
            Loop loop;
            public ViewEditLoopProperties(frmViewEdit parent, Loop loop) {
                this.parent = parent;
                this.loop = loop;
            }
            public bool Mirrored {
                get => loop.Mirrored;
            }

            [TypeConverter(typeof(MiorrorLoopOptionsConverter))]
            public MirrorLoopOptions Mirror {
                get {
                    if (loop.MirrorLoop == -1) {

                        return MirrorLoopOptions.None;
                    }
                    else {
                        return (MirrorLoopOptions)(Math.Abs(loop.MirrorLoop));
                    }
                }
                set {
                    // validate the mirror loop before setting
                    parent.ChangeMirror(value);
                    // re-select and expand current loop
                    (parent.tvwView.SelectedNode = parent.tvwView.Nodes[0].Nodes[loop.Index]).EnsureVisible();
                }
            }
            public int CelCount {
                get => loop.Cels.Count;
            }
        }

        public class ViewEditLoop2089Properties {
            frmViewEdit parent;
            Loop loop;
            public ViewEditLoop2089Properties(frmViewEdit parent, Loop loop) {
                this.parent = parent;
                this.loop = loop;
            }
            public bool Mirrored {
                get => false;
            }

            [TypeConverter(typeof(MiorrorLoopOptionsConverter))]
            public MirrorLoopOptions Mirror {
                get {
                    return MirrorLoopOptions.None;
                }
            }
            public int CelCount {
                get => loop.Cels.Count;
            }
        }

        public class ViewEditCelProperties {
            frmViewEdit parent;
            Cel cel;
            public ViewEditCelProperties(frmViewEdit parent, Cel cel) {
                this.parent = parent;
                this.cel = cel;
            }
            public int Width {
                get {
                    if (parent.CurrentOperation == ViewEditOperation.opChangeBoth ||
                       parent.CurrentOperation == ViewEditOperation.opChangeWidth) {
                        return parent.ChangingWidth;
                    }
                    else {
                        return cel.Width;
                    }
                }
                set {
                    // validate the width before setting
                    if (value > 0 && value < 160) {
                        // use form's change method to set the width
                        parent.ChangeWidth(value);
                    }
                    else {
                        // show error message
                        MessageBox.Show("Width must be between 1 and 159 pixels.");
                    }
                }
            }

            public int Height {
                get {
                    if (parent.CurrentOperation == ViewEditOperation.opChangeBoth ||
                        parent.CurrentOperation == ViewEditOperation.opChangeHeight) {
                        return parent.ChangingHeight;
                    }
                    else {
                        return cel.Height;
                    }
                }
                set {
                    // validate the height before setting
                    if (value > 0 && value < 168) {
                        // use form's change method to set the height
                        parent.ChangeHeight(value);
                    }
                    else {
                        // show error message
                        MessageBox.Show("Height must be between 1 and 167 pixels.");
                    }
                }
            }
            [TypeConverter(typeof(AGIColorIndexConverter))]
            public AGIColorIndex TransCol {
                get => cel.TransColor;
                set {
                    parent.ChangeTransColor(value);
                }
            }
        }

        public class AGIColorIndexConverter : EnumConverter {
            public AGIColorIndexConverter() : base(typeof(AGIColorIndex)) { }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // Exclude AGIColorIndex.None from the list of standard values
                var values = Enum.GetValues(typeof(AGIColorIndex))
                                 .Cast<AGIColorIndex>()
                                 .Where(color => color != AGIColorIndex.None)
                                 .ToArray();
                return new StandardValuesCollection(values);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return true; // Indicate that this property supports a list of standard values
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                return true; // Restrict the user to the list of standard values
            }
        }

        public enum MirrorLoopOptions {
            None = -1,
            Loop0 = 0,
            Loop1 = 1,
            Loop2 = 2,
            Loop3 = 3,
            Loop4 = 4,
            Loop5 = 5,
            Loop6 = 6,
            Loop7 = 7
        }

        public class MiorrorLoopOptionsConverter : EnumConverter {
            public MiorrorLoopOptionsConverter() : base(typeof(MirrorLoopOptions)) { }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // Exclude loops that don't exist from the list of standard values
                Engine.View view = (MDIMain.ActiveMdiChild as frmViewEdit).EditView;
                int thisloop = (MDIMain.ActiveMdiChild as frmViewEdit).SelectedLoop;
                var values = new MirrorLoopOptions[8];
                int count = 1;
                values[0] = MirrorLoopOptions.None;
                for (int i = 0; i < view.Loops.Count; i++) {
                    if (i > 7) {
                        break;
                    }
                    // allow this loop's mirror, and any loops that are not yet mirrored
                    if (i != thisloop) {
                        if (view[i].MirrorLoop == thisloop || view[i].MirrorLoop == -1) {
                            values[count] = (MirrorLoopOptions)i;
                            count++;
                        }
                    }
                }
                Array.Resize(ref values, count);
                return new StandardValuesCollection(values);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return true; // Indicate that this property supports a list of standard values
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                return true; // Restrict the user to the list of standard values
            }
        }
    }
}
