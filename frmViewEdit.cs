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

        // selection and editing variables
        private ViewEditMode ViewMode;
        public int SelectedLoop;
        private int SelectedCel;
        private ViewEditToolType SelectedTool;
        private Point ViewPt = new(0, 0); // current mouse position in AGI coordinates
        private Point DragPT;
        private ViewCursor CurCursor, ToolCursor;
        private bool blnDragging;
        private ViewEditOperation CurrentOperation;
        private bool SelectionNotMoved;
        private bool DelOriginalData;
        private AGIColorIndex LeftColor, RightColor;
        private bool TransSel = false;

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
        private bool blnDraggingView = false;
        //Private VTopMargin As Long

        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spTool;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCurX;
        internal ToolStripStatusLabel spCurY;


        /*

        Private CelAnchorX As Long, CelAnchorY As Long  'in cel coordinates
        Private OldX As Long, OldY As Long
        Private CelXOffset As Long, CelYOffset As Long
        Private OldPosX As Long, OldPosY As Long
        Private ClipboardData() As AGIColors
        Private DataUnderSel() As AGIColors, UndoDataUnderSel() As AGIColors
        Private SelHeight As Long, SelWidth As Long
        Private OldNode As Long

        Private sngOffsetX As Single, sngOffsetY As Single
        Private DrawCol As AGIColors
        Private TreeX As Single, TreeY As Single

        Private PixelData() As AGIColors
        Private PixelCount As Long

        Private Const MAX_CEL_W = 160
        Private Const MAX_CEL_H = 168
        */
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
            picPreview.MouseWheel += picPreview_MouseWheel;

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
            PreviewScale = WinAGISettings.ViewScalePreview.Value;
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
            case ViewEditMode.EndCel:
                tsbPaste.Enabled = ClipboardHasCel();
                break;
            case ViewEditMode.Selection:
                tsbPaste.Enabled = Clipboard.ContainsImage();
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
                        picPreview_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
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
        }

        private void frmViewEdit_Resize(object sender, EventArgs e) {
            if (picPalette.Visible) {
                picPalette.Refresh();
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
                mnuCut.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                mnuCut.Text = "Cut Cel";
                mnuCopy.Visible = true;
                mnuCopy.Enabled = true;
                mnuCopy.Text = "Copy Cel";
                mnuPaste.Visible = true;
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
                mnuDelete.Visible = true;
                mnuDelete.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                mnuDelete.Text = "Delete Cel";
                mnuClear.Visible = true;
                mnuClear.Enabled = true;
                mnuClear.Text = "Clear Cel";
                mnuInsert.Visible = true;
                mnuInsert.Enabled = true;
                mnuInsert.Text = "Insert New Cel";
                mnuSelectAll.Visible = true;
                mnuSelectAll.Enabled = true;
                mnuSelectAll.Text = "Select All";
                toolStripSeparator7.Visible = true;
                mnuFlipH.Visible = true;
                mnuFlipH.Enabled = EditView[SelectedLoop][SelectedCel].Width > 1;
                mnuFlipH.Text = "Flip Cel Horizontally";
                mnuFlipV.Visible = true;
                mnuFlipV.Enabled = EditView[SelectedLoop][SelectedCel].Height > 1;
                mnuFlipV.Text = "Flip Cel Vertically";
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
            case ViewEditMode.Selection:
                mnuCut.Visible = true;
                mnuCut.Enabled = true;
                mnuCut.Text = "Cut";
                mnuCopy.Visible = true;
                mnuCopy.Enabled = true;
                mnuCopy.Text = "Copy";
                mnuPaste.Visible = true;
                mnuPaste.Enabled = Clipboard.ContainsImage();
                mnuPaste.Text = "Paste";
                mnuDelete.Visible = true;
                mnuDelete.Enabled = true;
                mnuDelete.Text = "Delete";
                mnuClear.Visible = false;
                mnuInsert.Visible = false;
                mnuSelectAll.Visible = true;
                mnuSelectAll.Enabled = true;
                mnuSelectAll.Text = "Select All";
                toolStripSeparator7.Visible = false;
                mnuFlipH.Visible = true;
                mnuFlipH.Enabled = true;
                mnuFlipH.Text = "Flip Selection Horizontally";
                mnuFlipV.Visible = true;
                mnuFlipV.Enabled = true;
                mnuFlipV.Text = "Flip Selection Vertically";
                break;
            }
            mnuTogglePreview.Enabled = true;
            mnuTogglePreview.Checked = ShowPreview;

            mnuToggleGrid.Enabled = true;
            mnuToggleGrid.Checked = ShowGrid;

            mnuToggleSelectionMode.Enabled = true;
            mnuToggleSelectionMode.Checked = TransSel;
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
                tsbCut.Enabled = EditView[SelectedLoop].Cels.Count > 1;
                tsbCut.ToolTipText = "Cut Cel";
                tsbCopy.Enabled = true;
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
            case ViewEditMode.Selection:
                tsbCut.Enabled = true;
                tsbCut.ToolTipText = "Cut Selection";
                tsbCopy.Enabled = true;
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
                tsbFlipH.Enabled = true;
                tsbFlipH.ToolTipText = "Flip Horizontal";
                tsbFlipV.Enabled = true;
                tsbFlipV.ToolTipText = "Flip Vertical";
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
            byte CelStartX, CelStartY, CelEndX, CelEndY;

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
                // get loop and cel to insert
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
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo);
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
                DisplayCel(true);
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                break;
            case DecWidth:
                SelectCel(NextUndo.UDLoopNo, NextUndo.UDCelNo);
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
                // display cel
                DisplayCel(true);
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
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
                /*
                // set selection shapes
                shpView.Left = NextUndo.UndoData[0] * ScaleFactor * 2 - 1;
                shpView.Top = NextUndo.UndoData[1] * ScaleFactor - 1;
                shpView.Width = NextUndo.UndoData[2] * ScaleFactor * 2 + 2;
                shpView.Height = NextUndo.UndoData[3] * ScaleFactor + 2;
                shpSurface.Width = shpView.Width;
                shpSurface.Height = shpView.Height;
                shpSurface.Move(picCel.Left + shpView.Left, picCel.Top + shpView.Top);
                shpView.Visible = true;
                shpSurface.Visible = true;
                Timer2.Enabled = true;
                // restore the clipboard
                SelWidth = NextUndo.UndoData[2];
                SelHeight = NextUndo.UndoData[3];
                // fill clipoboard with data
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        // copy data under pasted selection to clipboard
                        SetPixelV(picClipboard.hDC, i, j, EGAColor[NextUndo.CelData[i, j]]);
                    }
                }
                */
                break;
            case ViewUndo.ActionType.FlipSelectionV:
                /*
                // set selection shapes
                shpView.Left = NextUndo.UndoData[0] * ScaleFactor * 2 - 1;
                shpView.Top = NextUndo.UndoData[1] * ScaleFactor - 1;
                shpView.Width = NextUndo.UndoData[2] * ScaleFactor * 2 + 2;
                shpView.Height = NextUndo.UndoData[3] * ScaleFactor + 2;
                shpSurface.Width = shpView.Width;
                shpSurface.Height = shpView.Height;
                shpSurface.Move(picCel.Left + shpView.Left, picCel.Top + shpView.Top);
                shpView.Visible = true;
                shpSurface.Visible = true;
                Timer2.Enabled = true;
                // restore the clipboard
                SelWidth = NextUndo.UndoData[2];
                SelHeight = NextUndo.UndoData[3];
                // fill clipoboard with data
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        // copy data under pasted selection to clipboard
                        SetPixelV(picClipboard.hDC, i, j, EGAColor[NextUndo.CelData[i, j]]);
                    }
                }
                */
                break;
            case Line:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // retrieve start and end points
                CelStartX = NextUndo.UndoData[0];
                CelStartY = NextUndo.UndoData[1];
                CelEndX = NextUndo.UndoData[2];
                CelEndY = NextUndo.UndoData[3];
                // draw the line
                DrawLineOnCel(CelStartX, CelStartY, CelEndX, CelEndY, NextUndo, true);
                // display the cel
                DisplayCel();
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                    DisplayPrevCel();
                }
                */
                break;
            case Box:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // get start and end points
                int CelStartX = NextUndo.UndoData[0];
                int CelStartY = NextUndo.UndoData[1];
                int CelEndX = NextUndo.UndoData[2];
                int CelEndY = NextUndo.UndoData[3];
                for (int i = CelStartX; i <= CelEndX; i++) {
                    EditView[SelectedLoop][SelectedCel].CelData[i, CelStartY] = NextUndo.UndoData[i - CelStartX + 4];
                    EditView[SelectedLoop][SelectedCel].CelData[i, CelEndY] = NextUndo.UndoData[i - 2 * CelStartX + 5 + CelEndX];
                }
                for (int j = CelStartY; j <= CelEndY; j++) {
                    EditView[SelectedLoop][SelectedCel].CelData[CelStartX, j] = NextUndo.UndoData[2 * (CelEndX - CelStartX) + 6 + j - CelStartY];
                    EditView[SelectedLoop][SelectedCel].CelData[CelEndX, j] = NextUndo.UndoData[2 * (CelEndX - CelStartX) + (CelEndY - CelStartY) + 7 + j - CelStartY];
                }
                DisplayCel();
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                    DisplayPrevCel();
                }
                */
                break;
            case BoxFill:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // get start and end points
                int CelStartX = NextUndo.UndoData[0];
                int CelStartY = NextUndo.UndoData[1];
                int CelEndX = NextUndo.UndoData[2];
                int CelEndY = NextUndo.UndoData[3];
                for (int i = CelStartX; i <= CelEndX; i++) {
                    for (int j = CelStartY; j <= CelEndY; j++) {
                        EditView[SelectedLoop][SelectedCel].CelData[i, j] = NextUndo.UndoData[4 + (i - CelStartX) + (j - CelStartY) * (CelEndX - CelStartX + 1)];
                    }
                }
                DisplayCel();
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                    DisplayPrevCel();
                }
                */
                break;
            case Draw or Erase:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // undo pixels by stepping backward through array
                j = NextUndo.CelData[0];
                for (int i = j; i > 0; i--) {
                    // get x, Y, and color (use CelEndX for color)
                    int CelStartX = (NextUndo.CelData[i] & 0xFF0000) / 0x10000;
                    int CelStartY = (NextUndo.CelData[i] & 0xFF00) / 0x100;
                    int CelEndX = NextUndo.CelData[i] & 0xFF;
                    // set pixel
                    EditView[SelectedLoop][SelectedCel].CelData[CelStartX, CelStartY] = CelEndX;
                }
                DisplayCel();
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                    DisplayPrevCel();
                }
                */
                break;
            case PaintFill:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                EditView[SelectedLoop][SelectedCel].AllCelData = NextUndo.CelData;
                DisplayCel();
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                    DisplayPrevCel();
                }
                */
                break;
            case CutSelection or DelSelection:
                /*
                // get affected cel
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // display the cel in its current state
                DisplayCel();
                // get starting location and size of deleted data
                CelStartX = NextUndo.UndoData[0];
                CelStartY = NextUndo.UndoData[1];
                SelWidth = NextUndo.UndoData[2];
                SelHeight = NextUndo.UndoData[3];
                // reset selection shape positions
                shpView.Move(CelStartX * 2 * ScaleFactor - 1, CelStartY * ScaleFactor - 1);
                shpSurface.Move(shpView.Left + picCel.Left, shpView.Top + picCel.Top);
                shpView.Height = SelHeight * ScaleFactor + 2;
                shpView.Width = SelWidth * 2 * ScaleFactor + 2;
                shpSurface.Height = SelHeight * ScaleFactor + 2;
                shpSurface.Width = SelWidth * 2 * ScaleFactor + 2;
                // show them
                Timer2.Enabled = true;
                shpView.Visible = true;
                shpSurface.Visible = true;
                // restore deleted data to clipboard
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        SetPixelV(picClipboard.hDC, i, j, NextUndo.CelData[i, j]);
                    }
                }
                picClipboard.Refresh();
                // copy clipboard image to pcel
                BitBlt(picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY);
                picPCel.Refresh();
                // if this was deleted from its original location
                if (NextUndo.UndoData[4] != 0) {
                    // ensure selection is cleared
                    picSelection.BackColor = EGAColor(EditView[SelectedLoop][SelectedCel].TransColor);
                }
                else {
                    // restore original DataUnderSel
                    DataUnderSel = new byte[SelWidth, SelHeight];
                    for (i = 0; i < SelWidth; i++) {
                        for (j = 0; j < SelHeight; j++) {
                            DataUnderSel[i, j] = tmpCelData[i, j];
                            SetPixelV(picSelection.hDC, i, j, EGAColor[tmpCelData(i, SelHeight + j)]);
                        }
                    }
                }
                // update the actual view with correct data
                BuildCelData(0, 0, CelWidth - 1, CelHeight - 1);
                // stretch it to cel draw pic
                StretchBlt(picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView[SelectedLoop][SelectedCel].Width, EditView[SelectedLoop][SelectedCel].Height, SRCCOPY);
                if (ShowGrid && ScaleFactor > 3) {
                    DrawCelGrid();
                }
                // set not moved flag
                SelectionNotMoved = true;
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                */
                break;
            case PasteSelection:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                DisplayCel();
                CelStartX = NextUndo.UndoData[0];
                CelStartY = NextUndo.UndoData[1];
                CelEndX = NextUndo.UndoData[2];
                CelEndY = NextUndo.UndoData[3];
                CelWidth = EditView[SelectedLoop][SelectedCel].Width;
                CelHeight = EditView[SelectedLoop][SelectedCel].Height;
                for (i = CelStartX; i <= CelEndX; i++) {
                    for (j = CelStartY; j < CelEndY; j++) {
                        // if within bounds
                        if (i >= 0 && i < CelWidth && j >= 0 && j < CelHeight) {
                            // save this data
                            EditView[SelectedLoop][SelectedCel).CelData[i, j] = NextUndo.CelData[i - CelStartX, j - CelStartY];
                            SetPixelV(picPCel.hDC, i, j, EGAColor[NextUndo.CelData[i - CelStartX, j - CelStartY]]);
                        }
                    }
                }
                // hide selection shapes
                Timer2.Enabled = false;
                shpView.Visible = false;
                shpSurface.Visible = false;
                // stretch it to cel draw pic
                StretchBlt(picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView[SelectedLoop][SelectedCel].Width, EditView[SelectedLoop][SelectedCel].Height, SRCCOPY);
                picCel.Refresh();
                if (ShowGrid && ScaleFactor > 3) {
                    DrawCelGrid();
                }
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                */
                break;
            case MoveSelection:
                /*
                SelectedLoop = NextUndo.UDLoopNo;
                SelectedCel = NextUndo.UDCelNo;
                // display the cel in its current state
                DisplayCel();
                // get temp copy of height/width
                CelWidth = EditView[SelectedLoop][SelectedCel].Width;
                CelHeight = EditView[SelectedLoop][SelectedCel].Height;
                // get location and size of selection
                CelStartX = NextUndo.UndoData[0];
                CelStartY = NextUndo.UndoData[1];
                SelWidth = NextUndo.UndoData[2];
                SelHeight = NextUndo.UndoData[3];
                // get location of position being 'unmoved'
                CelEndX = NextUndo.UndoData[4];
                CelEndY = NextUndo.UndoData[5];
                // reset selection shape positions
                shpView.Move(CelStartX * 2 * ScaleFactor - 1, CelStartY * ScaleFactor - 1);
                shpSurface.Move(shpView.Left + picCel.Left, shpView.Top + picCel.Top);
                shpView.Height = SelHeight * ScaleFactor + 2;
                shpView.Width = SelWidth * 2 * ScaleFactor + 2;
                shpSurface.Height = SelHeight * ScaleFactor + 2;
                shpSurface.Width = SelWidth * 2 * ScaleFactor + 2;
                // show them
                Timer2.Enabled = true;
                shpView.Visible = true;
                shpSurface.Visible = true;
                // restore area under area 'unmoved'
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        SetPixelV(picSelection.hDC, i, j, NextUndo.CelData[i, SelHeight + j]);
                    }
                }
                BitBlt(picPCel.hDC, CelEndX, CelEndY, SelWidth, SelHeight, picSelection.hDC, 0, 0, SRCCOPY);
                picPCel.Refresh();
                // restore area moved
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        SetPixelV(picClipboard.hDC, i, j, NextUndo.CelData[i, 2 * SelHeight + j]);
                    }
                }
                picClipboard.Refresh();
                // move the area back to old position
                BitBlt(picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY);
                picPCel.Refresh();
                // restore original DataUnderSel
                DataUnderSel = new byte[SelWidth, SelHeight];
                for (i = 0; i < SelWidth; i++) {
                    for (j = 0; j < SelHeight; j++) {
                        DataUnderSel[i, j] = NextUndo.CelData[i, j];
                        SetPixelV(picSelection.hDC, i, j, EGAColor[NextUndo.CelData[i, j]]);
                    }
                }
                // update the actual view with correct data
                BuildCelData(0, 0, CelWidth - 1, CelHeight - 1);
                // stretch it to cel draw pic
                StretchBlt(picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView[SelectedLoop][SelectedCel].Width, EditView[SelectedLoop][SelectedCel].Height, SRCCOPY);
                if (ShowGrid && ScaleFactor > 3) {
                    DrawCelGrid();
                }
                // set not moved flag
                SelectionNotMoved = true;
                // update preview
                if (ShowPreview) {
                    DisplayPrevLoop();
                }
                */
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
                // copy cel
                viewCB = new(ViewClipboardMode.Cel);
                viewCB.Cel.CloneFrom(EditView[SelectedLoop][SelectedCel]);
                cbData = new(VIEW_CB_FMT, viewCB);
                Clipboard.SetDataObject(cbData, true);
                break;
            case ViewEditMode.Selection:
                // copy selection
                MessageBox.Show("Copying selection");
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
                if (ClipboardHasCel()) {
                    ViewClipboardData vcb = Clipboard.GetData(VIEW_CB_FMT) as ViewClipboardData;
                    if (InsertCel(SelectedLoop, SelectedCel, vcb.Cel)) {
                        // select this cel
                        SelectCel(SelectedLoop, SelectedCel);
                    }
                }
                break;
            case ViewEditMode.Selection:
                if (Clipboard.ContainsImage()) {
                    MessageBox.Show("insert bitmap");
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
                // delete cel
                DeleteCel((byte)SelectedLoop, (byte)SelectedCel);
                break;
            case ViewEditMode.Selection:
                // delete selection
                MessageBox.Show("Delete selection");
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
                    // select this cel
                    SelectCel(SelectedLoop, SelectedCel);
                }
                break;
            case ViewEditMode.Selection:
                if (Clipboard.ContainsImage()) {
                    MessageBox.Show("insert bitmap");
                }
                break;
            }
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            MessageBox.Show("Select All");
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
                FlipCelH(SelectedLoop, SelectedCel);
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case ViewEditMode.Selection:
                MessageBox.Show("flip selection V");
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
                FlipCelV(SelectedLoop, SelectedCel);
                SelectCel(SelectedLoop, SelectedCel, true);
                break;
            case ViewEditMode.Selection:
                MessageBox.Show("flip selection H");
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
            MessageBox.Show("Tool set to " + SelectedTool);
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

            // toggle motion and image
            tmrMotion.Enabled = !tmrMotion.Enabled;
            if (tmrMotion.Enabled) {
                tspCycle.Image = StopImage;
            }
            else {
                CyclePreview = false;
                tspCycle.Image = PlayImage;
            }
            if (ViewMode == ViewEditMode.EndCel) {
                DisablePreview();
            }
            else if (tmrMotion.Enabled) {
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
                    if (PreviewCel == 0) {
                        PreviewCel = EditView[SelectedLoop].Cels.Count - 1;
                    }
                    break;
                }
            }
            picPreview.Select();
        }

        private void tspTransparency_Click(object sender, EventArgs e) {
            TransparentPreview = !TransparentPreview;
            if (TransparentPreview) {
                tspTransparency.Text = "ON";
            }
            else {
                tspTransparency.Text = "OFF";
                splitCanvas.Panel2.Invalidate();
            }
            if (ShowPreview) {
                DisplayPrevCel();
            }
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

            if (SelectedTool == ViewEditToolType.Edit || ModifierKeys == Keys.Shift) {
                // when edit tool is active, click to drag; otherwise,
                // shift-click to drag
                if (hsbCel.Visible || vsbCel.Visible) {
                    DragPT = e.Location;
                    blnDragging = true;
                    SetCursor(ViewCursor.DragSurface);
                }
                return;
            }
        }

        private void picCel_MouseMove(object sender, MouseEventArgs e) {
            Point tmpPt = new(0, 0), SelAnchor = new(0, 0);

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

            // calculate agi coordinates
            tmpPt.X = (int)(e.X / (2 * ScaleFactor));
            tmpPt.Y = (int)(e.Y / ScaleFactor);
            if (tmpPt.X < 0) {
                tmpPt.X = 0;
            }
            else if (tmpPt.X > 159) {
                tmpPt.X = 159;
            }
            if (tmpPt.Y < 0) {
                tmpPt.Y = 0;
            }
            else if (tmpPt.Y > 167) {
                tmpPt.Y = 167;
            }
            // if no change, do nothing
            if (tmpPt == ViewPt) {
                return;
            }
            ViewPt = tmpPt;

            // when moving mouse, we always need to update the status bar
            spCurX.Text = "X: " + ViewPt.X;
            spCurY.Text = "Y: " + ViewPt.Y;
        }

        private void picCel_MouseUp(object sender, MouseEventArgs e) {
            if (blnDragging) {
                blnDragging = false;
                SetCursor(ToolCursor);
                return;
            }
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
                    pnlPreview.Top = newT;
                    vsbPreview.Value = -newT;
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

        private void picPreview_MouseWheel(object sender, MouseEventArgs e) {
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
        }

        private void splitCanvas_Panel2_Paint(object sender, PaintEventArgs e) {
            if (TransparentPreview) {
                DrawTransGrid(splitCanvas.Panel2, 0, 0);
            }
        }

        private void pnlPreview_Paint(object sender, PaintEventArgs e) {
            if (TransparentPreview) {
                DrawTransGrid(pnlPreview, 10 - pnlPreview.Left % 10, 10 - pnlPreview.Top % 10);
            }
        }

        private void pnlPreview_Move(object sender, EventArgs e) {
            if (TransparentPreview) {
                splitCanvas.Panel2.Invalidate();
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
        }

        private void vsbPreview_Scroll(object sender, ScrollEventArgs e) {
            pnlPreview.Top = -vsbPreview.Value + toolStrip2.Bottom;
        }

        private void trkSpeed_ValueChanged(object sender, EventArgs e) {
            //  tmrMotion.Interval = 600 / trkSpeed.Value - 45;
            //  tmrMotion.Interval = 750 / trkSpeed.Value - 45;
            tmrMotion.Interval = 900 / trkSpeed.Value - 50;
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            // update the palette to show available colors, as well as the current
            // visual and priority pen colors

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

        #endregion

        #region temp code
        void tmpviewform() {
            /*

        ' use picPCel to hold the cel data without gridlines;
        ' when pixel ops are needed, blit between picCel and picPCel as
        ' needed

        'selection and clipboard pics will all be set to scale 1


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
                  .UndoData(i - CelStartX + 4) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY)
                  .UndoData(i - 2 * CelStartX + 5 + CelEndX) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelEndY)
              Next i
              For j = CelStartY To CelEndY
                  .UndoData(2 * (CelEndX - CelStartX) + 6 + j - CelStartY) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j)
                  .UndoData(2 * (CelEndX - CelStartX) + (CelEndY - CelStartY) + 7 + j - CelStartY) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelEndX, j)
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
                  .UndoData(4 + (i - CelStartX) + (j - CelStartY) * (CelEndX - CelStartX + 1)) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
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
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If
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
            If SelectedLoop <> EditView.Loops.Count Or SelectedCel >= 0 Then
              Set tvwView.SelectedItem = tvwView.Nodes(tvwView.Nodes.Count)
              tvwView_NodeClick tvwView.SelectedItem
            End If
            KeyCode = 0
            Shift = 0

          Case vbKeyLeft
            'move to first cel
            If SelectedLoop >= 0 And SelectedLoop <= EditView.Loops.Count Then
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
            If SelectedLoop >= 0 And SelectedLoop <= EditView.Loops.Count Then
              'if a loop is selected
              If SelectedCel = -1 Then
                Set tvwView.SelectedItem = tvwView.SelectedItem.Child.LastSibling
                tvwView_NodeClick tvwView.SelectedItem
              Else
                'cel selected; move to last sibling if not already there
                If SelectedCel < EditView.Loops(SelectedLoop).Cels.Count Then
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

    Public Sub MenuClickHelp()

        On Error GoTo ErrHandler

        'help
        Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\winagi\View_Editor.htm");
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub MenuClickCopy()
        Select Case ViewMode
        Case vmView
        Case vmLoop
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
            Set ClipViewCel = EditView.Loops(SelectedLoop).Cels(SelectedCel)

            'clear clipboard loop
            Set ClipViewLoop = Nothing
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
        CelWidth = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height
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
      End Sub

      Public Sub MenuClickPaste()
        Select Case ViewCBMode
        Case vmLoop
        Case vmCel
        Case vmBitmap
          PasteSelection 0, 0

          'update preview
          If ShowPreview Then
            DisplayPrevCel
          End If

        End Select
      End Sub

      Public Sub MenuClickSelectAll()

        Select Case ViewMode
        Case vmLoop
        Case vmCel
          'only allow this if tool is 'select'
          If SelectedTool <> Select Then
            SelectedTool = Select
            'press the button
            tlbView.Buttons("select").Value = tbrPressed
          End If

          'select entire Image
          CelAnchorX = 0
          CelAnchorY = 0

          SelWidth = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width
          SelHeight = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height

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
        tmpCelData = EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
        CelWidth = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height
        For i = 0 To SelWidth - 1
          For j = 0 To SelHeight - 1
            'if within bounds,
            If CelX - CelXOffset + i < CelWidth And CelX - CelXOffset + i >= 0 And _
               CelY - CelYOffset + j < CelHeight And CelY - CelYOffset + j >= 0 Then
              tmpCelData(CelX - CelXOffset + i, CelY - CelYOffset + j) = DataUnderSel(i, j)
            End If
          Next j
        Next i
        EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

        'turn off selection flashing
        Timer2.Enabled = False
        shpView.BorderStyle = 3
        shpSurface.BorderStyle = 3
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
          UndoCol(UndoCol.Count).UDAction = CutSelection
          frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(VIEWUNDOTEXT + CutSelection) & vbTab & "Ctrl+Z"
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
        If EditView.Loops.Count > 1 Then
          'if cutting
          If CutLoop Then
            'set clipboard loop to this loop
            Set ClipViewLoop = New AGILoop
            ClipViewLoop.CopyLoop EditView.Loops(SelectedLoop)
          End If

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo <> 0 Then
            Set NextUndo = New ViewUndo
            If CutLoop Then
              NextUndo.UDAction = CutLoop
            Else
              NextUndo.UDAction = DelLoop
            End If
            NextUndo.UDLoopNo = SelectedLoop

            'if mirrored,
            If EditView.Loops(SelectedLoop).Mirrored Then
              'only need to store mirror loop
              NextUndo.ResizeData 1
              NextUndo.UndoData(0) = True
              NextUndo.UndoData(1) = EditView.Loops(SelectedLoop).MirrorLoop
            Else
              'not mirrored, so save the entire loop
              Set NextUndo.UndoLoop = New AGILoop
              NextUndo.UndoLoop.CopyLoop EditView.Loops(SelectedLoop)
            End If
            AddUndo NextUndo
          End If
          'now delete it
          EditView.Loops.Remove SelectedLoop

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
              .UDAction = DelSelection
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
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
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

      Private Sub MoveSelection(MoveX As Long, MoveY As Long)

        'move selection shapes
        shpView.Move MoveX * ScaleFactor * 2 - 1, MoveY * ScaleFactor - 1
        shpSurface.Move picCel.Left + shpView.Left, picCel.Top + shpView.Top

        'move selection back to old location in pixel cel
        BitBlt picPCel.hDC, OldPosX, OldPosY, SelWidth, SelHeight, picSelection.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY

        'copy data at this location to selection
        BitBlt picSelection.hDC, 0, 0, SelWidth, SelHeight, picPCel.hDC, MoveX, MoveY, SRCCOPY
        picSelection.Refresh

        'put clipboard Image at new location on pixel cel
            if TransSel ...
        BitBlt picPCel.hDC, MoveX, MoveY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
        picPCel.Refresh

        'and update the viewedit
      '  BuildCelData OldPosX, OldPosY, OldPosX + SelWidth - 1, OldPosY + SelHeight - 1
        BuildCelData 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1, True

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
        SelectedTool = Select
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
            if TransSel ...
        BitBlt picPCel.hDC, CelX, CelY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(EditView.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
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
          NextUndo.UDAction = PasteSelection
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
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY

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
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY)
              'set linecolor in celdata
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY) = DrawCol
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
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j)
              'set linecolor in celdata
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j) = DrawCol
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
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              'set linecolor in celdata
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = DrawCol
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
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              'set linecolor in celdata
              EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = DrawCol
            End If
            SetPixelV picPCel.hDC, i, j, EGAColor(DrawCol)
            udIndex = udIndex + 1
          Next j
        End If

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
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
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY

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

        If CelEndX > EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          CelEndX = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If CelEndY > EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          CelEndY = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        'copy current cel data
        tmpData = EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

        'rebuild cel data (using tmpData)
        With EditView.Loops(SelectedLoop).Cels(SelectedCel)
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
        EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpData

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
      End Sub

      Private Sub Form_KeyPress(KeyAscii As Integer)
        'if editing a propeety, allow txtbox to handle key input
        If txtProperty.Visible Then
          Exit Sub
        End If

        'respond to keypresses that change scale of the preview window
        If ShowPreview Then
          KeyHandler KeyAscii
        End If
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

      Private Sub picCel_DblClick()
        'if current tool is selection
        If SelectedTool = Select Then
          'if moving a selection
          If CurrentOperation = opMoveSelection Then
            'put it back first
            PasteSelection CelAnchorX, CelAnchorY
          End If
          MenuClickSelectAll
        End If
      End Sub

      Private Sub picCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim rtn As Long, FillCol As Long
        Dim hBrush As Long, hOldBrush As Long

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
        If SelectedTool = Erase Then
          'override color
          DrawCol = EditView.Loops(SelectedLoop).Cels(SelectedCel).TransColor
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
        If CelAnchorX > EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          CelAnchorX = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If CelAnchorY < 0 Then
          CelAnchorY = 0
        End If
        If CelAnchorY > EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          CelAnchorY = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        Select Case SelectedTool
        Case Edit
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

        Case Select
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

        Case Draw, Erase
          'save pixel to array
          ReDim PixelData(1)
          PixelCount = 1
          PixelData(1) = &H10000 * CelAnchorX + &H100 * CelAnchorY + EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(CelAnchorX), CByte(CelAnchorY))
          'draw the new pixel
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'update cel data
          EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(CelAnchorX), CByte(CelAnchorY)) = DrawCol
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

        Case Line
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case Rectangle
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case BoxFill
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case Paint
          FloodFill CelAnchorX, CelAnchorY, EGAColor(CInt(DrawCol))

          'update preview
          If ShowPreview Then
            DisplayPrevLoop
            DisplayPrevCel
          End If
        End Select

        'if any tool other than select is active or not selecting or moving selection
        If (SelectedTool <> Select) Or (CurrentOperation <> opMoveSelection And CurrentOperation <> opSetSelection) Then
          'hide the selection shapes
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
        End If
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
        If ViewX > EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          ViewX = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If ViewY > EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          ViewY = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
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
          Case Edit
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

          Case Select
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
          Case Draw, Erase
            'save pixel data
            PixelCount = PixelCount + 1
            ReDim Preserve PixelData(PixelCount)
            PixelData(PixelCount) = &H10000 * ViewX + &H100 * ViewY + EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(ViewX), CByte(ViewY))
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
            EditView.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(ViewX), CByte(ViewY)) = DrawCol
            'reset anchor
            CelAnchorX = ViewX
            CelAnchorY = ViewY

          Case Line
            DrawLineOnSurface ViewX, ViewY

          Case Rectangle
            DrawBoxOnSurface ViewX, ViewY, False

          Case BoxFill
            DrawBoxOnSurface ViewX, ViewY, True

          Case Erase
            'erase the new pixel
            picCel.Line (ViewX * ScaleFactor * 2, ViewY * ScaleFactor)-((ViewX + 1) * ScaleFactor * 2 - 1, (ViewY + 1) * ScaleFactor - 1), EGAColor(EditView.Loops(SelectedLoop).Cels(SelectedCel).TransColor), BF
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
          NextUndo.UDAction = PaintFill
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          NextUndo.CelData = EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

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
        BuildCelData 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, EditView.Loops(SelectedLoop).Cels(SelectedCel).Width, EditView.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
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
        If ViewX > EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          ViewX = EditView.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If ViewY > EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          ViewY = EditView.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
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
                .UDAction = MoveSelection
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
            If ShowPreview Then
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
          If SelectedTool = Rectangle Or SelectedTool = BoxFill Then
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
          Case Draw, Erase
            'if erasing
            If SelectedTool = Erase Then
              NextUndo.UDAction = Erase
            Else
              NextUndo.UDAction = Draw
            End If

            'set first element to size
            PixelData(0) = PixelCount
            'add pixel data
            NextUndo.CelData = PixelData

          Case Line
            NextUndo.UDAction = Line

            'draw the line
            DrawLineOnCel CelAnchorX, CelAnchorY, ViewX, ViewY, NextUndo

          Case Rectangle

            NextUndo.UDAction = Box

            DrawBoxOnCel ViewX, ViewY, False, NextUndo

          Case BoxFill
            NextUndo.UDAction = BoxFill

            DrawBoxOnCel ViewX, ViewY, True, NextUndo

          End Select

          If Settings.ViewUndo <> 0 Then
            'add undo item
            AddUndo NextUndo
          End If

          'update preview
          If ShowPreview Then
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
            If EditView.Loops(SelectedLoop).Cels(SelectedCel).Width <> picCel.Width \ (ScaleFactor * 2) Then
              'set new width
              ChangeWidth picCel.Width \ (ScaleFactor * 2)
            End If
          End If
          'if height was being adjusted
          If CurrentOperation = opChangeHeight Or CurrentOperation = opChangeBoth Then
            'if height has changed
            If EditView.Loops(SelectedLoop).Cels(SelectedCel).Height <> ViewY Then
              'set new height
              ChangeHeight ViewY
            End If
          End If
          'reset cursor
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

          'update preview
          If ShowPreview Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        End Select
        'always reset operation
        CurrentOperation = opNone
      End Sub

      Private Sub SetSelection()

        Dim rtn As Long
        Dim tmpTransCol As AGIColors
        Dim i As Long, j As Long

        On Error GoTo ErrHandler

        'clear out selection area picture box
        'set selection to blank with current transcolor
        picSelection.BackColor = EGAColor(EditView.Loops(SelectedLoop).Cels(SelectedCel).TransColor)
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
        tmpTransCol = EditView.Loops(SelectedLoop).Cels(SelectedCel).TransColor
        ReDim DataUnderSel(SelWidth - 1, SelHeight - 1)
        For i = 0 To SelWidth - 1
          For j = 0 To SelHeight - 1
            DataUnderSel(i, j) = tmpTransCol
          Next j
        Next i

        'enable selection outline flashing
        Timer2.Enabled = True
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
          If ShowPreview Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case "flipv"
          FlipV
          'update preview
          If ShowPreview Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case "none"
          SelectedTool = Edit

        Case "select"
          SelectedTool = Select
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "draw"
          SelectedTool = Draw
          picCel.MouseIcon = LoadResPicture("EVC_DRAW", vbResCursor)

        Case "line"
          SelectedTool = Line
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "box"
          SelectedTool = Rectangle
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "boxfill"
          SelectedTool = BoxFill
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "fill"
          SelectedTool = Paint
          picCel.MouseIcon = LoadResPicture("EVC_PAINT", vbResCursor)

        Case "erase"
          SelectedTool = Erase
          picCel.MouseIcon = LoadResPicture("EVC_ERASE" & CStr(ScaleFactor), vbResCursor)
        End Select

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
        MainStatusBar.Panels("Tool") = LoadResString(VIEWTOOLTYPETEXT + SelectedTool)

        'if tool is not select
        If SelectedTool <> Select Then
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
            tsbPaste.Enabled = ClipboardHasCel();
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
            if (ShowPreview) {
                DisablePreview();
            }
            if (!ViewNode.IsSelected) {
                tvwView.SelectedNode = ViewNode;
                ViewNode.EnsureVisible();
            }
            ConfigureToolbar();
        }

        private void SelectLoop(int loopnum) {
            SelectedLoop = loopnum;
            SelectedCel = -1;
            if (loopnum < ViewNode.Nodes.Count - 1) {
                ViewMode = ViewEditMode.Loop;
                propertyGrid1.SelectedObject = new ViewEditLoopProperties(this, EditView[SelectedLoop]);
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
            ConfigureToolbar();
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
            // update palette
            picPalette.Invalidate();
            ConfigureToolbar();
        }

        private void DisplayCel(bool ResetPosition = false) {
            Debug.Assert(ViewMode == ViewEditMode.Cel);

            picCel.Visible = true;
            if (ResetPosition) {
                picCel.Location = new(VE_MARGIN, VE_MARGIN);
            }
            Cel currentcel = EditView[SelectedLoop][SelectedCel];
            //set transparent color
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
            gc.DrawImage(currentcel.CelBMP, 0, 0, picCel.Width, picCel.Height);
            if (ScaleFactor > 3 && ShowGrid) {
                // draw grid over the cel
                DrawCelGrid(gc);
            }
            picCel.Refresh();
            // update scrollbars
            SetEScrollbars();
            // reset mode to cel
            ViewMode = ViewEditMode.Cel;
            // force redraw of property grid
            propertyGrid1.Refresh();
        }

        private void DrawCelGrid(Graphics gc) {

            // draw grid over the cel
            int sngH = picCel.Height;
            int sngW = picCel.Width;

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
                            tmpCelData[i, j - NewCelHeight] = EditView[SelectedLoop][SelectedCel][(byte)i, (byte)j];
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
                            tmpCelData[i - NewCelWidth, j] = EditView[SelectedLoop][SelectedCel][(byte)i, (byte)j];
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
            NextUndo.UndoLoop = EditView[loopnum].Clone();
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
                    (EditView[loopnum][celnum][(byte)(CelWidth - 1 - i), j],
                        EditView[loopnum][celnum][i, j]) =
                        (EditView[loopnum][celnum][i, j],
                        EditView[loopnum][celnum][(byte)(CelWidth - 1 - i), j]);
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
                    (EditView[loopnum][celnum][i, (byte)(CelHeight - 1 - j)],
                        EditView[loopnum][celnum][i, j]) =
                        (EditView[loopnum][celnum][i, j],
                        EditView[loopnum][celnum][i, (byte)(CelHeight - 1 - j)]);
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

        private void FlipSelectionH() {
            /*
          'get location and size of selection
          CelStartX = (shpView.Left + 1) / ScaleFactor / 2
          CelStartY = (shpView.Top + 1) / ScaleFactor
          CelEndX = (shpView.Left + shpView.Width - 1) / ScaleFactor / 2 - 1
          CelEndY = (shpView.Top + shpView.Height - 1) / ScaleFactor - 1

          'flip the selection on the drawing surface

          'flip clipboard
          With picClipboard
            StretchBlt.hDC, SelWidth - 1, 0, -SelWidth, SelHeight, .hDC, 0, 0, SelWidth, SelHeight, SRCCOPY
          End With

          'copy flipped clipboard image to pixel cel
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
          picPCel.Refresh
            if TransSel...
          'stretch the pixel cel into the draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'temp copy of cel data
          tmpCelData = EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
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

              If i >= 0 And i<CelWidth And j >= 0 And j < CelHeight Then
                'save this pixel to cel data
                tmpCelData(i, j) = lngColor
              End If
            Next i
          Next j
          EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo<> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = FlipH
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

            */
        }

        private void FlipSelectionV() {
            /*
          'get location and size of selection
          CelStartX = (shpView.Left + 1) / ScaleFactor / 2
          CelStartY = (shpView.Top + 1) / ScaleFactor
          CelEndX = (shpView.Left + shpView.Width - 1) / ScaleFactor / 2 - 1
          CelEndY = (shpView.Top + shpView.Height - 1) / ScaleFactor - 1

          'flip the selection on the drawing surface

          'flip clipboard
          With picClipboard
            StretchBlt.hDC, 0, SelHeight - 1, SelWidth, -SelHeight, .hDC, 0, 0, SelWidth, SelHeight, SRCCOPY
          End With

            if TransSel...
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
          tmpCelData = EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
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

              If i >= 0 And i<CelWidth And j >= 0 And j < CelHeight Then
                'save this pixel to cel data
                tmpCelData(i, j) = lngColor
              End If
            Next i
          Next j
          EditView.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo<> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = FlipV
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
            */
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
            picPreview.Visible = true;
        }

        private void DisablePreview() {
            tmrMotion.Enabled = false;
            tspCycle.Image = PlayImage;
            tspCycle.Enabled = false;
            pnlPreview.Visible = false;
            hsbPreview.Visible = false;
            vsbPreview.Visible = false;
        }

        private void DisplayPrevLoop() {
            switch (ViewMode) {
            case ViewEditMode.Loop:
            case ViewEditMode.Cel:
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
            //copy view Image
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
            // set transparency
            EditView[SelectedLoop][PreviewCel].Transparency = TransparentPreview;
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
                gc.DrawImage(EditView[SelectedLoop][PreviewCel].CelBMP, 0, 0, picPreview.Width, picPreview.Height);
                if (TransparentPreview) {
                    // TODO: there's a reference to drawing a grid somewhere
                    // in the custom control used on the InsertChar form...
                    //
                    // draw single pixel dots spaced 10 pixels apart over transparent pixels only
                    Bitmap b = new(picPreview.Image);
                    //picPreview.Refresh();
                    int ofX = 10 - (picPreview.Left + pnlPreview.Left) % 10;
                    int ofY = 10 - (picPreview.Top + pnlPreview.Top) % 10;
                    //ofX = 0; ofY = 2;
                    // 28 == 8 wrong; should be 2
                    // 35 == 5 is correct
                    //
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
            _ = SendMessage(pnlPreview.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picPreview.Handle, WM_SETREDRAW, false, 0);
            // resize the holding panel
            ResizePreviewPanel();
            // resize image
            picPreview.Width = (int)(EditView[SelectedLoop][SelectedCel].Width * ScaleFactor * 2);
            picPreview.Height = (int)(EditView[SelectedLoop][SelectedCel].Height * ScaleFactor);
            // then set the scrollbars
            SetPScrollbars(oldscale);
            // redraw cel at new scale
            DisplayPrevCel();
            _ = SendMessage(pnlPreview.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(picPreview.Handle, WM_SETREDRAW, true, 0);
            splitCanvas.Panel2.Refresh();
            pnlPreview.Refresh();
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
                this.view = parent.EditView;
            }
            public string ID {
                get => view.ID;
                //set {
                //    string strErrMsg = "";
                //    // validate the ID before setting?
                //    if (view.ID != value) {
                //        //validate new id
                //        DefineNameCheck rtn = ValidateID(value, view.ID);
                //        switch (rtn) {
                //        case DefineNameCheck.OK:
                //            break;
                //        case DefineNameCheck.Empty:
                //            strErrMsg = "Resource ID cannot be blank.";
                //            break;
                //        case DefineNameCheck.Numeric:
                //            strErrMsg = "Resource ID cannot be numeric.";
                //            break;
                //        case DefineNameCheck.ActionCommand:
                //            strErrMsg = "'" + value + "' is an AGI command, and cannot be used as a resource ID.";
                //            break;
                //        case DefineNameCheck.TestCommand:
                //            strErrMsg = "'" + value + "' is an AGI test command, and cannot be used as a resource ID.";
                //            break;
                //        case DefineNameCheck.KeyWord:
                //            strErrMsg = "'" + value + "' is a compiler reserved word, and cannot be used as a resource ID.";
                //            break;
                //        case DefineNameCheck.ArgMarker:
                //            strErrMsg = "Resource IDs cannot be argument markers";
                //            break;
                //        case DefineNameCheck.BadChar:
                //            strErrMsg = "Invalid character in resource ID:" + Environment.NewLine + "   !" + QUOTECHAR + "&//()*+,-/:;<=>?[\\]^`{|}~ and spaces" + Environment.NewLine + "are not allowed.";
                //            break;
                //        case DefineNameCheck.ResourceID:
                //            // only enforce if in a game
                //            if (parent.InGame) {
                //                strErrMsg = "'" + value + "' is already in use as a resource ID.";
                //            }
                //            else {
                //                rtn = DefineNameCheck.OK;
                //            }
                //            break;
                //        }
                //        // if there is an error
                //        if (rtn != DefineNameCheck.OK) {
                //            MessageBox.Show(MDIMain,
                //                strErrMsg,
                //                "Change View ID",
                //                MessageBoxButtons.OK,
                //                MessageBoxIcon.Information,
                //                0, 0,
                //                WinAGIHelp,
                //                "htm\\winagi\\Managing Resources.htm#resourceids");
                //        }
                //        else {
                //            view.ID = value;
                //            if (parent.InGame) {
                //                // update the ID in the game
                //                EditGame.Views[view.Number].ID = value;
                //            }
                //            // update the ID in the tree
                //            parent.tvwView.Nodes[0].Text = ResourceName(view, false, true);
                //            //refresh tree,preview, file
                //            UpdateGameResID(AGIResType.View, view.Number, value, strOldResFile, strOldID, true);

                //        }
                //    }
                //}
            }
            public string Description {
                get => view.Description;
                //set {
                //    view.Description = value;
                //    if (parent.InGame) {
                //        // update the description in the game
                //        EditGame.Views[view.Number].Description = value;
                //    }
                //}
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

        public class ViewEditCelProperties {
            frmViewEdit parent;
            Cel cel;
            public ViewEditCelProperties(frmViewEdit parent, Cel cel) {
                this.parent = parent;
                this.cel = cel;
            }
            public int Width {
                get => cel.Width;
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
                get => cel.Height;
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
