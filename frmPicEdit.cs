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
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.PictureUndo.ActionType;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.API;
using EnvDTE;
using System.Diagnostics;

namespace WinAGI.Editor {
    public partial class frmPicEdit : Form, IMessageFilter {
        public int PictureNumber;
        public Picture EditPicture;
        public bool InGame;
        public bool IsChanged;
        public EPicCursorMode CursorMode;
        private bool closing = false;
        private EPicMode PicMode;
        private bool VisVisible, PriVisible;

        private Stack<PictureUndo> UndoCol = [];

        // variables to support tool selection/manipulation/use
        private TPicDrawOpEnum PicDrawMode; //used to indicate what current drawing op is happening (determines what mouse ops mean)
        private TPicToolTypeEnum SelectedTool;  // used to indicate what tool is currently selected
        private PenStatus SelectedPen;
        private PenStatus CurrentPen;
        private bool CurCmdIsLine = false;
        private Rectangle Selection = new(0, 0, 0, 0);
        private Point PicPt, Anchor, Delta;
        private bool gInsertBefore = false; // to manage insertion of new commands
        private int SelectedCmdIndex; // must be the last cmd if multiples selected so the picdraw feature works correctly
        private DrawFunction SelectedCmdType;
        private int SelectedCmdCount;
        private Point[] CoordPT = new Point[100];
        private Point CurPt = new(255, 255);

        private EGAColors EditPalette = DefaultPalette.CopyPalette();

        // variables to support graphics/display
        private Color VCColor; //color of 'x's in 'x' cursor mode for visual
        private Color PCColor; // color of 'x's in 'x' cursor mode for priority
        public double ScaleFactor;
        public bool ShowBands = false;
        public int OldPri;
        public bool ShowTextMarks = false;
        private const int PE_MARGIN = 5;
        public Bitmap BkgdImage;
        public byte BkgdTrans;
        private EPicCur CurCursor;

        // view testing
        private Engine.View TestView;
        private EPicStatusMode StatusMode = EPicStatusMode.psPixel;
        private int StopReason;
        private Point TestCelPos;
        private bool ShowTestCel;
        private ObjDirection TestDir;

        // text screen testing
        private byte PicOffset, TextMode, MsgMaxW;
        private string MsgText;
        private byte MsgTop, MsgLeft;
        private byte MsgWidth, MsgHeight;
        private byte MsgBG, MsgFG;
        private int MaxCol, CharWidth;

        // toolbar dropdowns
        ToolStripDropDown tdTools;
        ToolStripDropDown tdPlotStyle;
        ToolStripDropDown tdPlotSize;



        /*
        // variables to support tool selection/manipulation/use
        private bool OnPoint;
        private PenStatus CurrentPen;
        private DrawFunction EditCmd;
        private int EditCoordNum;
        private Point[] ArcPt = new Point[120];
        private int Segments;
        private bool Activating;

        // variables to support graphics/display
        public double tgtX, tgtY, tgtW, tgtH;
        public double srcX, srcY, srcW, srcH;

        private bool blnDragging, blnInPri;
        private double sngOffsetX;
        private double sngOffsetY;

        private int OneWindow; // 0=both; 1=vis only; 2=pri only
        private int PrevState;

        // variables to support testing
        private int TestViewNum;
        private string TestViewFile;
        private byte CurTestLoop, CurTestCel, CurTestLoopCount;

        private AGIColors[] TestCelData;
        private byte CelHeight, CelWidth;
        private  AGIColorsCelTrans;
        private TPicTest TestSettings;
        */
        int mCmdAnchor = 0;
        int mCmdEnd = 0;

        #region temp code
        void tmpPicForm() {
            /*

private bool ConfigureBackground() {

  // shows background configuration form (which will automatically show
  // the loadimage dialog if no Image loaded yet)
  
  Set frmConfigureBkgd.PicEditForm = Me
  // initialize the form (will get a bkgd Image if there isn't one yet)
  frmConfigureBkgd.InitForm EditPicture.VisualBMP
  
  // if no bkgd Image (i.e. user canceled), just exit
  if (frmConfigureBkgd.Canceled) {
    Unload frmConfigureBkgd
    return false;
  }
  
  // now set canceled flag (it's the default)
  frmConfigureBkgd.Canceled = true
  
  // show the form
  frmConfigureBkgd.Show vbModal, frmMDIMain
  
  // do something with it...
  if (!frmConfigureBkgd.Canceled) {
    // copy bkgd Image and filename
    Set BkgdImage = frmConfigureBkgd.BkgdImage
    
    // save the bkgd parameters
      // local values used in draw functions
      tgtX = frmConfigureBkgd.tgtX
      tgtY = frmConfigureBkgd.tgtY
      tgtW = frmConfigureBkgd.tgtW
      tgtH = frmConfigureBkgd.tgtH
      srcX = frmConfigureBkgd.srcX
      srcY = frmConfigureBkgd.srcY
      srcW = frmConfigureBkgd.srcW
      srcH = frmConfigureBkgd.srcH
      BkgdTrans = frmConfigureBkgd.BkgdTrans
      //  now update the picture resource properties
      EditPicture.BkgdImgFile = RelativeFileName(GameDir, frmConfigureBkgd.BkgdImgFile)
      EditPicture.BkgdTrans = CLng(BkgdTrans)
      EditPicture.BkgdSize = tgtW & "|" & tgtH & "|" & srcW & "|" & srcH
      EditPicture.BkgdPosition = tgtX & "|" & tgtY & "|" & srcX & "|" & srcY

    // if in game
    if (InGame) {
      // copy properties back to actual picture resource
        Pictures(EditPicture.Number).BkgdImgFile = EditPicture.BkgdImgFile
        Pictures(EditPicture.Number).BkgdPosition = EditPicture.BkgdPosition
        Pictures(EditPicture.Number).BkgdShow = EditPicture.BkgdShow
        Pictures(EditPicture.Number).BkgdSize = EditPicture.BkgdSize
        Pictures(EditPicture.Number).BkgdTrans = EditPicture.BkgdTrans
        //  save it (this will only write the properties
        //  since the real picture is not being edited
        //  in this piceditor)
        Pictures(EditPicture.Number).Save
    }
    
    // return true
    ConfigureBackground = true
  } else {
    // return false
    ConfigureBackground = false
  }
  
  // unload the form
  Unload frmConfigureBkgd
}

private void ExportPicAsGif() {

  // export a loop as a gif
  
  Dim blnCanceled As Boolean, rtn As Long
  Dim PGOptions As GifOptions
  
  // show options form
  Load frmViewGifOptions
    // set up form to export this picture
    frmViewGifOptions.InitForm 1, EditPicture
    frmViewGifOptions.Show vbModal, frmMDIMain
    blnCanceled = frmViewGifOptions.Canceled
    
    // if not canceled, get a filename
    if (!blnCanceled) {
    
      // set up commondialog
        MainSaveDlg.DialogTitle = "Export Picture GIF"
        MainSaveDlg.DefaultExt = "gif"
        MainSaveDlg.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*"
        MainSaveDlg.Flags = cdlOFNHideReadOnly | cdlOFNPathMustExist | cdlOFNExplorer
        MainSaveDlg.FilterIndex = 1
        MainSaveDlg.InitDir = DefaultResDir
        MainSaveDlg.FullName = ""
        MainSaveDlg.hWndOwner = frmMDIMain.hWnd
      
      do {
        MainSaveDlg.ShowSaveAs
        // if canceled,
        if (Err.Number = cdlCancel) {
          // cancel the export
          blnCanceled = true
          break; // exit do
        }
        
        DefaultResDir = JustPath(MainSaveDlg.FileName)
        // if file exists,
        if (FileExists(MainSaveDlg.FullName)) {
          // verify replacement
          rtn = MsgBox(MainSaveDlg.FileName & " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
          
          if (rtn = vbYes) {
            break; // exit do
          } else if (rtn = vbCancel) {
            blnCanceled = true
            break; // exit do
          }
        } else {
          break; // exit do
        }
      } while(true);
    }
    
    // if NOT canceled after getting filename, then export!
    if (!blnCanceled) {
      // show progress form
      Load frmProgress
        frmProgress.Text = "Exporting Picture as GIF"
        frmProgress.lblProgress = "Depending in size of picture, this may take awhile. Please wait..."
        frmProgress.pgbStatus.Max = EditPicture.Resource.Size
        frmProgress.pgbStatus.Value = 0
        frmProgress.pgbStatus.Visible = true
        frmProgress.Show
        frmProgress.Refresh
      
      // show wait cursor
      WaitCursor
      
      PGOptions.Cycle = (frmViewGifOptions.chkLoop.Value = vbChecked)
      PGOptions.Delay = Val(frmViewGifOptions.txtDelay.Text)
      PGOptions.Zoom = Val(frmViewGifOptions.txtZoom.Text)
      
      // set options
      MakePicGif EditPicture, PGOptions, MainSaveDlg.FullName
      
      // all done!
      Unload frmProgress
      MsgBox "Success!", vbInformation + vbOKOnly, "Export Picture as GIF"
      
      Screen.MousePointer = vbDefault
    }
    
    // done with the options form
    Unload frmViewGifOptions
}

public void MenuClickInsert() {

  // insert coord without starting a move action
  InsertCoordinate false
}

public void MenuClickECustom3() {

  // toggle test view mode
  
  switch (PicMode) {
  case pmViewTest or pmPrintTest:
    // always switch back to edit
    SetMode pmEdit
    
  case pmEdit:
    // assume view test mode is desired
    
    // if no test view,
    if (TestView == null) {
      // get one
      GetTestView
    
      // if still no test view
      if (TestView == null) {
        // just exit
        return;
      }
    }
    
    // switch to test mode
    SetMode pmViewTest
  }
}

public void MenuClickECustom4() {

  Dim i As Long
  Dim bytCmd As Byte
  Dim bytData() As Byte
  Dim lngPos As Long
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  Dim blnRelX As Boolean, PatternNum As Byte
  Dim strBrush As String
  Dim stlOut As StringList
  
  Debug.Assert PicMode = pmEdit
  Debug.Assert lstCommands.SelCount >= 1
  
  // get data (for better speed)
  bytData = EditPicture.Resource.AllData
  Set stlOut = New StringList
  
  for (int i = SelectedCmd - (lstCommands.SelCount - 1);  i <= SelectedCmd; i++) {
    stlOut.Add lstCommands.List(i)
    
    // set starting pos for the selected cmd
    lngPos = lstCommands.Items[i].Tag;
    
    // get command Type
    bytCmd = bytData(lngPos)
    
    // add command based on Type
    switch (bytCmd) {
    case 0xF0 or 0xF1 or 0xF2 or 0xF3 or 0x9:
            // pen functions; no coords.
      
    case 0xF4 or 0xF5:
            // Draw an X or Y corner.
      // set initial direction
      blnRelX = (bytCmd = 0xF5)
      // get coordinates
      lngPos = lngPos + 1
      bytX = bytData(lngPos)
      do {
        if (bytX >= 0xF0) {
          break; // exit do
        }
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        if (bytX >= 0xF0) {
          break; // exit do
        }
        
        stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
      
        // get next byte as potential command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
        
        while (bytCmd < 0xF0) {
          if (blnRelX) {
            bytX = bytCmd
          } else {
            bytY = bytCmd
          }
          blnRelX = !blnRelX
          
          stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
          
          // get next coordinate or command
          lngPos = lngPos + 1
          bytCmd = bytData(lngPos)
        }
      } while (false);
      
    case 0xF6:
            // Absolute line (long lines).
      // get coordinates
      lngPos = lngPos + 1
      bytX = bytData(lngPos)
      do {
        if (bytX >= 0xF0) {
          break; // exit for
        }
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        if (bytY >= 0xF0) {
          break; // exit for
        }
        
        stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
        
        // get next byte as potential command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
        
        while (bytCmd < 0xF0) {
          bytX = bytCmd
          lngPos = lngPos + 1
          bytY = bytData(lngPos)
          
          stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
          
          // read in next command
          lngPos = lngPos + 1
          bytCmd = bytData(lngPos)
        }
      while (false) {
      
    case 0xF7:
            // Relative line (short lines).
       // get coordinates
      lngPos = lngPos + 1
      bytX = bytData(lngPos)
      do {
        if (bytX >= 0xF0) {
          break; // exit for
        }
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        if (bytY >= 0xF0) {
          break; // exit for
        }
        
        stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
        
       // get next byte as potential command
       lngPos = lngPos + 1
       bytCmd = bytData(lngPos)
       
       while (bytCmd < 0xF0) {
            // if horizontal negative bit set
         if ((bytCmd & 0x80)) {
           xdisp = -((bytCmd & 0x70) / 0x10)
         } else {
           xdisp = ((bytCmd & 0x70) / 0x10)
         }
         // if vertical negative bit is set
         if ((bytCmd & 0x8)) {
           ydisp = -(bytCmd & 0x7)
         } else {
           ydisp = (bytCmd & 0x7)
         }
         bytX = bytX + xdisp
         bytY = bytY + ydisp
        
        stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
        
        // read in next command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
      }
    } while (false);
    
  case 0xF8:
            // Fill.
     // get next byte as potential command
     lngPos = lngPos + 1
     bytCmd = bytData(lngPos)
     
     while (bytCmd < 0xF0) {
       // get coordinates
       bytX = bytCmd
       lngPos = lngPos + 1
       bytY = bytData(lngPos)
       
        stlOut.Add "   (" & CStr(bytX) & ", " & CStr(bytY) & ")"
       
       // read in next command
       lngPos = lngPos + 1
       bytCmd = bytData(lngPos)
     }
      
    case 0xFA:
            // Plot with pen.
      // get next byte as potential command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
      while (bytCmd < 0xF0) {
        // if brush is splatter
        if (CurrentPen.PlotStyle) {
          PatternNum = CLng(bytCmd \ 2)
          strBrush = CStr(PatternNum) & " -- "
          // get next byte
          lngPos = lngPos + 1
          bytCmd = bytData(lngPos)
          // set offset to 2 (to account for pattern number and x coord)
          xdisp = 2
        } else {
          strBrush = vbNullString
          // set offset to 1 (to account for x coord)
          xdisp = 1
        }
  
        // get coordinates
        bytX = bytCmd
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        
        stlOut.Add "   " & strBrush & "(" & CStr(bytX) & ", " & CStr(bytY) & ")"
        
        // read in next command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
      }
    }
  }
  Clipboard.Clear
  Clipboard.SetText stlOut.Text
}

public void MenuClickHelp() {
  
  switch (PicMode) {
  case pmEdit:
    HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Picture_Editor.htm"
  case pmPrintTest:
    HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\pictestmode.htm#textprint"
  case pmViewTest:
    HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\pictestmode.htm"
  }
}

private void FlipCmds(int FlipCmd, int Count, int Axis, bool DontUndo = false) {

  // flips the selected commands
  // axis=0 means horizontal flip
  // axis=1 means vertical flip
  
  Dim i As Long, lngPos As Long
  Dim NextUndo As PictureUndo, bytData() As Byte
  Dim blnX As Boolean, lngOldPos As Long
  Dim bytX As Byte, bytY As Byte
  Dim tmpStyle As PlotStyle
  
  // select the cmds ???why isn't this already done????
  GetSelectionBounds SelectedCmd, Count, true
  
  // bounding rectangle should always be defined
  // *'Debug.Assert Selection.Width != 0 || Selection.Height != 0
  
  // get current pen status for first command
  tmpStyle = GetPenStyle(lstCommands.Items[SelectedCmd - Count + 1].Tag)
  
  // local copy of data (for speed)
  bytData = EditPicture.Resource.AllData
  
  // if horizontal flip:
  if (Axis = 0) {
    // step through each cmd
    for (i = Count; i > 0; i--) {
      lngPos = lstCommands.Items[FlipCmd - i + 1].Tag
      
      // each cmd handles flip differently
      switch (bytData(lngPos) {
      case dfRelLine:
        // increment position marker
        lngPos = lngPos + 1
        
        // when flipping relative lines horizontally, need to flip the actual order
        // of coordinates; this ensures that we avoid situations where the flipped
        // line creates data bytes that the interpreter might confuse as commands
        // (remember that the delta x offset is the four highest bits of the data
        // byte; bit 7 is set if the delta is negative; bits 6-5-4 determine Value;
        // if the delta amount is -7, this means the data byte will be >=0xF0; this
        // is read by the interpreter as a new cmd; not as a delt of -7; so for
        // rel lines, the delta x Value is limited to -6; when flipping, we can't just
        // flip the first coord, then change the direction of the x-delta values;
        // there may be some +7 delta values that will result in errors when converted
        // to -7 delta x values
        
        // solution is to build the command backwards; start with the LAST point in
        // the command; then build the line backwards to finish the swap
        
        // if at least one valid coordinate
        if (bytData(lngPos) < 0xF0) {
          // determine ending point
          bytX = bytData(lngPos)
          bytY = bytData(lngPos + 1)
          
          // set pointer to next delta Value
          lngPos = lngPos + 2
          
         while (bytData(lngPos) < 0xF0) {
            // add deltax
            if (bytData(lngPos) & 0x80) {
              bytX = bytX - (CLng(bytData(lngPos)) & 0x70) / 0x10
            } else {
              bytX = bytX + (CLng(bytData(lngPos)) & 0x70) / 0x10
            }
            
            // add deltay
            if (bytData(lngPos) & 0x8) {
              bytY = bytY - (CLng(bytData(lngPos)) & 0x7)
            } else {
              bytY = bytY + (CLng(bytData(lngPos)) & 0x7)
            }
            
              
            // get next delta Value
            lngPos = lngPos + 1
          }
          
          // flip the x Value of the end point (which will now be the start point)
          bytX = 2 * CLng(Selection.X) + Selection.Width - bytX - 1
          
          // save ending point position (remember to back up one unit)
          lngOldPos = lngPos - 1
          // restore original pointer (remember to skip cmd valus so pos points
          // to the first coordinate pair)
          lngPos = lstCommands.Items[FlipCmd - i + 1].Tag + 1
          // store this point
          bytData(lngPos) = bytX
          bytData(lngPos + 1) = bytY
          // now move pointer to first delta Value
          lngPos = lngPos + 2
          
          // now rebuild cmds, starting with last command, and going backwards
          // because the cmds are being built in reverse, the delta x and delta y
          // values should be inverted (+ to - and - to +), BUT because cmd is being
          // flipped,  we only invert the y direction, resulting in the x
          // direction being flipped properly
          
          // (also, because cmds have to be re-built backwards, we access the delta
          // values from the picedit object when reconstructing the delta values)
          
          do {
            // copy the delta Value from picdata to bytdata
            bytData(lngPos) = EditPicture.Resource.Data(lngOldPos)
            // if the delta y Value is currently negative
            if ((bytData(lngPos) & 0x8)) {
              // clear the bit to make the direction positive
              bytData(lngPos) = bytData(lngPos) & 0xF7
            
            // if the delta y Value is currently positive (or mayber zero?)
            } else {
              // if the delta Value is not zero
              if (bytData(lngPos) & 0x7) {
                // set the bit
                bytData(lngPos) = bytData(lngPos) | 0x8
              }
            }
            
            // get next delta Value
            lngPos = lngPos + 1
            lngOldPos = lngOldPos - 1
          while (bytData(lngPos) < 0xF0);
        }
        
      case dfAbsLine or dfFill:
        // each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        while (bytData(lngPos) < 0xF0) {
          if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) {
            bytData(lngPos) = 2 * CLng(Selection.X) + Selection.Width - bytData(lngPos) - 1
          } else {
            // end found
            break; // exit do
          }
          // get next cmd pair
          lngPos = lngPos + 2
        }
        
      case dfPlotPen:
        // each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        
        while (bytData(lngPos) < 0xF0) {
          // if pen is splatter
          if (tmpStyle = psSplatter) {
            // skip first byte; its the splatter Value
            lngPos = lngPos + 1
          }
        
          if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) {
            bytData(lngPos) = 2 * CLng(Selection.X) + Selection.Width - bytData(lngPos) - 1
          } else {
            // end found
            break; // exit do
          }
          
          // get next cmd pair
          lngPos = lngPos + 2
        }
        
      case dfXCorner or dfYCorner:
        // if this is a 'x' corner, then next coord is a 'x' Value
        // (make sure to check this BEFORE incrementing lngPos)
        blnX = (bytData(lngPos) = dfXCorner)
          
        // move pointer to first coordinate pair
        lngPos = lngPos + 1
        
        // if a valid coordinatee
        if (bytData(lngPos) < 0xF0) {
          // flip first coordinate
          bytData(lngPos) = 2 * CLng(Selection.X) + Selection.Width - bytData(lngPos) - 1
          
          // move pointer to next coordinate point
          lngPos = lngPos + 2
          
          while (bytData(lngPos) < 0xF0) {
            // if this is a 'x' point
            if (blnX) {
              // flip it
              bytData(lngPos) = 2 * CLng(Selection.X) + Selection.Width - bytData(lngPos) - 1
            }
            
            // toggle next coord Type
            blnX = !blnX
            // increment pointer
            lngPos = lngPos + 1
          }
        }
      case dfChangePen:
        tmpStyle = (bytData(lngPos + 1) & 0x20) / 0x20
      }
    }
  
  } else {
    // step through each cmd
    for (i = 1; i <= Count; i++) {
      lngPos = lstCommands.Items[FlipCmd - i + 1].Tag
      
      // each cmd handles flip differently
      switch (bytData(lngPos)
      case dfRelLine
        // when flipping the y axis, we don't need to worry about
        // the swap causing errors in the delta values; all we need
        // to do is just swap the first coordinate, and then change the
        // y direction of all delta values
        
        // increment position marker
        lngPos = lngPos + 1
        
        if (bytData(lngPos) < 0xF0) {
          // flip the y Value of starting point
          bytData(lngPos + 1) = 2 * CLng(Selection.Y) + Selection.Height - bytData(lngPos + 1) - 1
          // increment lngpos  (by two so first relative pt data byte is selected)
          lngPos = lngPos + 2
        }
        
        while (bytData(lngPos) < 0xF0) {
          // toggle direction bit for y displacement
          
          // if the delta y Value is currently negative
          if ((bytData(lngPos) & 0x8)) {
            // clear the bit to make the direction positive
            bytData(lngPos) = bytData(lngPos) & 0xF7
          
          // if the delta y Value is currently positive (or mayber zero?)
          } else {
            // if the delta Value is not zero
            if (bytData(lngPos) & 0x7) {
              // set the bit
              bytData(lngPos) = bytData(lngPos) | 0x8
            }
          }
          
          // neyt byte
          lngPos = lngPos + 1
        }
        
      case dfAbsLine or dfFill:
        // each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        while (bytData(lngPos) < 0xF0) {
          if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) {
            bytData(lngPos + 1) = 2 * CLng(Selection.Y) + Selection.Height - bytData(lngPos + 1) - 1
          } else {
            // end found
            break; // exit do
          }
          // get neyt cmd pair
          lngPos = lngPos + 2
        }
        
      case dfPlotPen:
        // each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        
        while (bytData(lngPos) < 0xF0) {
          // if pen is splatter
          if (tmpStyle = psSplatter) {
            // skip first byte; its the splatter Value
            lngPos = lngPos + 1
          }
        
          if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) {
            bytData(lngPos + 1) = 2 * CLng(Selection.Y) + Selection.Height - bytData(lngPos + 1) - 1
          } else {
            // end found
            break; // exit do
          }
          
          // get next cmd pair
          lngPos = lngPos + 2
        }
        
      case dfYCorner or dfXCorner:
        // if this is a 'y' corner, then next coord is a 'y' Value
        // (make sure to check this BEFORE incrementing lngPos)
        blnX = bytData(lngPos) = dfXCorner
          
        // move pointer to first coordinate pair
        lngPos = lngPos + 1
        
        // if a valid coordinatee
        if (bytData(lngPos) < 0xF0) {
          // flip first coordinate
          bytData(lngPos + 1) = 2 * CLng(Selection.Y) + Selection.Height - bytData(lngPos + 1) - 1
          
          // move pointer to next coordinate point
          lngPos = lngPos + 2
          
          while (bytData(lngPos) < 0xF0) {
            // if this is a 'y' point
            if (!blnX) {
              // flip it
              bytData(lngPos) = 2 * CLng(Selection.Y) + Selection.Height - bytData(lngPos) - 1
            }
            
            // toggle next coord Type
            blnX = !blnX
            // increment pointer
            lngPos = lngPos + 1
          }
        }
        
      case dfChangePen:
        tmpStyle = (bytData(lngPos + 1) & 0x20) / 0x20
      }
    }
  }
  
  // copy data back to resource
  EditPicture.Resource.SetData bytData
    
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    Set NextUndo = New PictureUndo
      if (Axis = 0) {
        NextUndo.UDAction = FlipH
      } else {
        NextUndo.UDAction = FlipV
      }
      NextUndo.UDCmdIndex = FlipCmd
      NextUndo.UDCoordIndex = Count
    AddUndo NextUndo
  }
}

private void GetTestView() {
  
  // get a test view to use in test mode
  
  // if game is loaded
  if (GameLoaded) {
    // use the get resource form
      frmGetResourceNum.WindowFunction = grTestView
      frmGetResourceNum.ResType = rtView
      frmGetResourceNum.OldResNum = TestViewNum
      // setup before loading so ghosts don't show up
      frmGetResourceNum.FormSetup
      // show the form
      frmGetResourceNum.Show vbModal, frmMDIMain
    
      // if canceled, unload and exit
      if (frmGetResourceNum.Canceled) {
        Unload frmGetResourceNum
        return;
      }
    
      // set testview id
      TestViewNum = frmGetResourceNum.NewResNum
    Unload frmGetResourceNum
    
  } else {
    // get test view from file
      MainDialog.DialogTitle = "Choose Test View"
      MainDialog.Filter = "AGI View Resource (*.agv)|*.agv|All files (*.*)|*.*"
      MainDialog.FilterIndex = ReadSettingLong(SettingsList, sVIEWS, sOPENFILTER, 1)
      MainDialog.DefaultExt = vbNullString
      MainDialog.FileName = vbNullString
      MainDialog.InitDir = DefaultResDir
      
      MainDialog.ShowOpen
      if (Err.Number = cdlCancel) {
        // exit
        return;
      }
      TestViewFile = MainDialog.FileName
      
      WriteAppSetting SettingsList, sVIEWS, sOPENFILTER, MainDialog.FilterIndex
      DefaultResDir = JustPath(MainDialog.FileName)
  }
  
  // reload testview
  LoadTestView
  
  // if in motion
  if (TestDir != odStopped) {
    // stop motion
    TestDir = odStopped
    tmrTest.Enabled = TestSettings.CycleAtRest
  }
}

public void MenuClickCustom2() {

  // toggles background Image
  ToggleBkgd !EditPicture.BkgdShow
}

public void MenuClickSelectAll() {

  // selects all commands if in edit mode,
  // or select entire picture if in edit-select mode
  
  Dim i As Long
  
  // in test mode, this should be disabled, but...
  if (PicMode = pmViewTest) {
    return;
  }
  
  // if editselect tool is chosen, change selection to cover entire area
  switch (SelectedTool) {
  case ttSelectArea:
    Selection.X = 0
    Selection.Y = 0
    Selection.Width = 160
    Selection.Height = 168
    // now show the selection
    ShowCmdSelection
    
  // case ttSetPen:
    // not used
  case ttLine or ttRelLine or ttCorner:
    // not sure what I should do if this is case?
  case ttPlot or ttFill:
    // not sure what I should do if this is case?
  case ttRectangle or ttTrapezoid or ttEllipse:
    // not sure what I should do if this is case?
  case ttEdit:
    // if nothing to select
    if (lstCommands.ListCount = 1) {
      return;
    }
    
    // disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    // select all cmds in the cmd list (except 'END' place holder)
    SelectedCmd = lstCommands.ListCount - 2
    
    for (int i = lstCommands.ListCount - 2; i >= 0; i--) {
      NoSelect = true
      lstCommands.Selected(i) = true
    }
    NoSelect = false
    
    // reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    // force update
    DrawPicture
  
    // if more than one cmd (account for END placeholder)
    if (lstCommands.ListCount > 2) {
      // get bounds, and select the cmds
      GetSelectionBounds lstCommands.ListCount - 2, lstCommands.ListCount - 1, true
    }
  }
}
private void MoveCmds(int MoveCmd, int Count, int DeltaX, int DeltaY, bool DontUndo = false) {

  Dim NextUndo As PictureUndo
  Dim i As Long, bytData() As Byte
  Dim lngPos As Long, bytCmd As Byte
  Dim blnX As Boolean
  Dim CurPen As PlotStyle, FirstCmd As Long
  
  // if more than one command selected, MoveCmd is the LAST command in the group of selected commands!
  FirstCmd = MoveCmd - Count + 1
  
  // if no delta
  if (DeltaX = 0 && DeltaY = 0) {
    return;
  }
  
  // local copy of data (for speed)
  bytData = EditPicture.Resource.AllData
  
  // we need to know the pen style in case a plot command is being moved
  // and make sure we get FIRST command, not the last one
  CurPen = GetPenStyle(lstCommands.Items[MoveCmd - Count + 1].Tag)
  
  // step through each cmd
  for (i = 1; i <= Count; i++) {
    lngPos = lstCommands.Items[FirstCmd + i - 1].Tag
    
    // each cmd handles move differently
    switch (bytData(lngPos)) {
    case dfRelLine:
      // only first pt needs to be changed
      if (bytData(lngPos + 1) < 0xF0 && bytData(lngPos + 2) < 0xF0) {
        bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaX
        bytData(lngPos + 2) = bytData(lngPos + 2) + DeltaY
      }
      
    case dfAbsLine or dfFill
      // each pair of coordinates are adjusted for offset
      lngPos = lngPos + 1
      while (bytData(lngPos) < 0xF0) {
        if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) {
          bytData(lngPos) = bytData(lngPos) + DeltaX
          bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        } else {
          // end found
          break; // exit do
        }
        // get next cmd pair
        lngPos = lngPos + 2
      }
      
    case dfChangePen:
      // need to make sure we keep up with any plot style changes
        // get pen size and style
        lngPos = lngPos + 1
        
        if ((bytData(lngPos) && 0x20) / 0x20 = 0) {
          // solid
          CurPen = psSolid
        } else {
          CurPen = psSplatter
        }
  
        // get next command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
        
    case dfPlotPen:
      // each group of coordinates are adjusted for offset
      lngPos = lngPos + 1
      while (bytData(lngPos) < 0xF0) {
        // if splattering, skip the splatter code
        if (CurPen = psSplatter) {
          lngPos = lngPos + 1
        }
        
        if (bytData(lngPos) < 0xF0 && bytData(lngPos + 1) < 0xF0) { // ? didn't we already check that in the 'do' statement?
          bytData(lngPos) = bytData(lngPos) + DeltaX
          bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        } else {
          // end found
          break; // exit do
        }
        // get next cmd pair
        lngPos = lngPos + 2
      }
      
      
      
    case dfXCorner or dfYCorner:
      // if this is a 'x' corner, then next coord is a 'x' Value
      // (make sure to check this BEFORE incrementing lngPos)
      blnX = bytData(lngPos) = dfXCorner
        
      // move pointer to first coordinate pair
      lngPos = lngPos + 1
      
      // if a valid coordinatee
      if (bytData(lngPos) < 0xF0) {
        // move first coordinate
        bytData(lngPos) = bytData(lngPos) + DeltaX
        bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        
        // move pointer to next coordinate point
        lngPos = lngPos + 2
        
        while (bytData(lngPos) < 0xF0) {
          // if this is a 'x' point
          if (blnX) {
            // add delta x
            bytData(lngPos) = bytData(lngPos) + DeltaX
          } else {
            // add delta y
            bytData(lngPos) = bytData(lngPos) + DeltaY
          }
          // toggle next coord Type
          blnX = !blnX
          // increment pointer
          lngPos = lngPos + 1
        }
      }
    }
  }
  
  // copy data back to resource
  EditPicture.Resource.SetData bytData
  
  // add undo (if necessary)
  if (!DontUndo && Settings.PicUndo != 0) {
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = MoveCmds
      NextUndo.UDCmdIndex = MoveCmd
      NextUndo.UDCoordIndex = Count
      NextUndo.UDText = CStr(-1 * DeltaX) & "|" & CStr(-1 * DeltaY)
    AddUndo NextUndo
  }
}

private Point RelLineCoord(ByVal CoordPos As Long) {

  // returns the relative line coordinate for the relative line
  // at CoordPos
  
  Dim bytData() As Byte, bytCmd As Byte
  Dim lngPos As Long, tmpPT As Point
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  
  // get data (for better speed)
  bytData = EditPicture.Resource.AllData
  
  // find start by stepping backwards until relline cmd is found
  lngPos = CoordPos
  while (bytData(lngPos - 1) != dfRelLine) {
    lngPos = lngPos - 1
  }
  
  // get coordinates
  bytX = bytData(lngPos)
  lngPos = lngPos + 1
  bytY = bytData(lngPos)
  
  // get next byte as potential command
  lngPos = lngPos + 1
  bytCmd = bytData(lngPos)
  
  while (lngPos <= CoordPos) {
    // if horizontal negative bit set
    if ((bytCmd & 0x80)) {
      xdisp = -((bytCmd & 0x70) / 0x10)
    } else {
      xdisp = ((bytCmd & 0x70) / 0x10)
    }
    // if vertical negative bit is set
    if ((bytCmd & 0x8)) {
      ydisp = -(bytCmd & 0x7)
    } else {
      ydisp = (bytCmd & 0x7)
    }
    bytX = bytX + xdisp
    bytY = bytY + ydisp
    
    // read in next command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
  }
  
  // return this point
  tmpPT.X = bytX
  tmpPT.Y = bytY
  RelLineCoord = tmpPT
}


private bool GetVerticalStatus(ByVal CmdPos As Long) {
  // will detrmine if a step line is currently drawing vertical
  
  Dim bytData() As Byte
  Dim lngPos As Long, bytCmd As Byte
  
  // get data (for better speed)
  bytData = EditPicture.Resource.AllData
  
  // set starting pos for the selected cmd
  lngPos = CmdPos
  
  // get command Type
  bytCmd = bytData(lngPos)
  
  if (bytCmd = 0xF4) {
    GetVerticalStatus = true
  }
  
  // flop vert status for first point
  GetVerticalStatus = !GetVerticalStatus

  // skip first coordinates
  lngPos = lngPos + 3
  
  // get next byte as potential command
  bytCmd = bytData(lngPos)
    
  while (bytCmd < 0xF0) {
    
    // flop vert status for each point
    GetVerticalStatus = !GetVerticalStatus
    
    // get next coordinate or command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
  }
}

public void MenuClickFind() {

  // toggles the priority bands on and off
    
  // toggle showband flag
  ShowBands = !ShowBands
  
  // redraw
  DrawPicture
  
  // reset caption
    if (ShowBands) {
      frmMDIMain.mnuEFind.Text = "Hide Priority Bands" & vbTab & "Alt+P"
    } else {
      frmMDIMain.mnuEFind.Text = "Show Priority Bands" & vbTab & "Alt+P"
    }
}
public void MenuClickFindAgain() (

  // allow user to set the priority base,if v2.936 or above
  // OR if not in a game!
  
  Dim lngNewBase As String, lngOldBase As Long
  Dim NextUndo As PictureUndo
  
  lngOldBase = EditPicture.PriBase
  lngNewBase = lngOldBase
  do {
    lngNewBase = InputBox("Enter new priority base value: ", "Set Priority Base", lngNewBase, frmMDIMain.Left + (frmMDIMain.Width - 4500) / 2, frmMDIMain.Top + (frmMDIMain.Height - 2100) / 2)
    
    // if canceled, it will be empty string
    if (lngNewBase = vbNullString) {
      return;
    }
    
    // validate
    if (!IsNumeric(lngNewBase)) {
      // invalid
      MsgBoxEx "You must enter a value between 0 and 158", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Invalid Base Value", WinAGIHelp, "htm\winagi\Picture_Editor.htm#pribands"
    } else if ( lngNewBase < 0 || lngNewBase > 158) {
      // invalid
      MsgBoxEx "You must enter a value between 0 and 158", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Invalid Base Value", WinAGIHelp, "htm\winagi\Picture_Editor.htm#pribands"
    } else {
      // OK!
      break; // exit do
    }
  } while(true);
  
  // set new pri base
  EditPicture.PriBase = lngNewBase
  DrawPicture
  
  // add undo!
  if (Settings.PicUndo != 0) {
    // create undo object
    Set NextUndo = New PictureUndo
    NextUndo.UDAction = SetPriBase
    // use cmdIndex for old base
    NextUndo.UDCmdIndex = lngOldBase
    // add the undo object without setting edit menu
    UndoCol.Add NextUndo
  }
  
  MarkAsChanged();
}

public void MenuClickRedo() {

  // when showing full visual or full priority,
  // this menu item swaps between the two
  
  Dim sngSplit As Single
  
  //  if currently showing visual
  if (OneWindow = 1) {
    // switch to priority
    OneWindow = 2
    sngSplit = 0
  } else if (OneWindow = 2) {
    // otherwise switch to visual
    OneWindow = 1
  }
  
  // redraw
  DrawPicture
}

private void AddPatternData(int tmpIndex, byte[] bytPatDat, bool DontUndo = false) {

  // add pattern bytes to tmpIndex command coordinates
  // if skipping undo, pattern values will be
  // passed in bytPatDat; if not skipping undo
  // generate random pattern data

  Dim i As Long, lngNewPos As Long
  Dim bytPattern As Byte
  Dim NextUndo As PictureUndo
  
  // set insertpos so first iteration will add
  // pattern data in front of first coord x Value
  lngNewPos = lstCommands.Items[tmpIndex].Tag - 2
  
  do {
    // if skipping undo
    if (DontUndo) {
      // if first byte of array is 255,
      if (bytPatDat(0) = 0xFF) {
        // need to provide the random bytes for this set of coordinates
        bytPattern = 2 * CByte(Int(Rnd * 119))
      } else {
        // get pattern from array
        bytPattern = bytPatDat(i)
      }
    } else {
      // get random pattern
      bytPattern = 2 * CByte(Int(Rnd * 119))
    }
    
    // adjust pos (include offset
    lngNewPos = lngNewPos + 3
    
    // add it to resource
    EditPicture.Resource.InsertData bytPattern, lngNewPos
    
    // increment byte insertion counter
    i = i + 1
  while (EditPicture.Resource.Data(lngNewPos + 3) < 0xF0);
  
  // adjust positions (i equals number of bytes added)
  UpdatePosValues tmpIndex + 1, i
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    // save undo info
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = AddPlotPattern
      NextUndo.UDPicPos = lstCommands.Items[tmpIndex].Tag
      NextUndo.UDCmdIndex = tmpIndex
    // add the undo object without setting edit menu
    UndoCol.Add NextUndo
  }
}

private void BeginDraw(TPicToolTypeEnum CurrentTool, Point PicPt) {
  // initiates draw operation based on selected tool
  
  Dim bytData() As Byte
  
  // begin drawing using selected tool
  switch (CurrentTool) {
  case ttLine:
    // set anchor
    Anchor = PicPt
    // set data to draw command and first point
    ReDim bytData(2)
    bytData(0) = dfAbsLine
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    // insert command
    InsertCommand bytData(), SelectedCmd, "Abs Line", gInsertBefore
    // select this cmd
    SelectCmd lstCommands.NewIndex, false
    // now set mode (do it AFTER selecting command otherwise
    // draw mode will get canceled)
    PicDrawMode = doLine
    // and select first coordinate
    NoSelect = true
    lstCoords.ListIndex = 0
    
  case ttRelLine:
    // insert rel line cmd
    
    // set anchor
    Anchor = PicPt
    // set data to draw command and first point
    ReDim bytData(2)
    bytData(0) = dfRelLine
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    // insert command
    InsertCommand bytData(), SelectedCmd, "Rel Line", gInsertBefore
    // select this cmd
    SelectCmd lstCommands.NewIndex, false
    // now set mode (do it AFTER selecting command otherwise
    // draw mode will get canceled)
    PicDrawMode = doLine
    // and select first coordinate
    NoSelect = true
    lstCoords.ListIndex = 0
    
  case ttCorner:
    // set anchor
    Anchor = PicPt
    // set data to draw command and first point
    ReDim bytData(2)
    // assume xcorner
    bytData(0) = dfXCorner
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    // insert command
    InsertCommand bytData(), SelectedCmd, "X Corner", gInsertBefore
    SelectCmd lstCommands.NewIndex, false
    // now set mode (do it AFTER selecting command otherwise
    // draw mode will get canceled)
    PicDrawMode = doLine
    // and select first coordinate
    NoSelect = true
    lstCoords.ListIndex = 0
  
  }
}

private void ChangeColor(int CmdIndex, AGIColors NewColor, bool DontUndo = false) {
  
  // changes the color for this command
  Dim NextUndo As PictureUndo
  Dim OldColor As AGIColors
  Dim bytData() As Byte
  Dim lngPos As Long
  
  ReDim bytData(0)
  
  // get position of command
  lngPos = lstCommands.Items[CmdIndex].Tag
  
  // get color of current command
  if (Right$(lstCommands.List(CmdIndex), 3) = "Off") {
    OldColor = agNone
  } else {
    OldColor = EditPicture.Resource.Data(lngPos + 1)
  }
  
  // it is possible that a change request is made
  // even though colors are the same
  if (OldColor = NewColor) {
    // just exit
    return;
  }
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = ChangeColor
      NextUndo.UDPicPos = lngPos
      NextUndo.UDCmdIndex = CmdIndex
      bytData(0) = OldColor
      NextUndo.UDData = bytData()
    AddUndo NextUndo
  }
  
  // if old color is none
  if (OldColor = agNone) {
    // change command to enable by subtracting one
    EditPicture.Resource.Data(lngPos) = EditPicture.Resource.Data(lngPos) - 1
    // insert color
    EditPicture.Resource.InsertData CByte(NewColor), CLng(lngPos) + 1
    // update all following commands
    UpdatePosValues CmdIndex + 1, 1
    
    // build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & LoadResString(COLORNAME + NewColor)
    
  } else if ( NewColor = agNone) {
    // change command to disable by adding one
    EditPicture.Resource.Data(lngPos) = EditPicture.Resource.Data(lngPos) + 1
    // delete color byte
    EditPicture.Resource.RemoveData lngPos + 1
    // update all following commands
    UpdatePosValues CmdIndex + 1, -1
    // build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & "Off"
  } else {
    // change color byte
    EditPicture.Resource.Data(lngPos + 1) = NewColor
    // build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & LoadResString(COLORNAME + NewColor)
  }
}

public void ChangeDir(int ByVal KeyCode) {
  
  // should ONLY be called when in test mode
  // *'Debug.Assert PicMode = pmViewTest
  
  // if view is not on picture
  if (!ShowTestCel) {
    return;
  }
  
  // takes a keycode as the input, and changes direction if appropriate
  switch (KeyCode) {
  case vbKeyUp or vbKeyNumpad8:
    // if direction is currently up
    if (TestDir = odUp) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to up
      TestDir = odUp
      // set loop to 3, if there are four AND loop is not 3 AND in auto
      if (TestView.Loops.Count >= 4 && CurTestLoop != 3 && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 3
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyPageUp or vbKeyNumpad9:
    // if direction is currently UpRight
    if (TestDir = odUpRight) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to upright
      TestDir = odUpRight
      // set loop to 0, if not already 0 AND in auto
      if (CurTestLoop != 0 && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 0
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyRight or vbKeyNumpad6:
    // if direction is currently Right
    if (TestDir = odRight) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to right
      TestDir = odRight
      // set loop to 0, if not already 0 AND in auto
      if (CurTestLoop != 0 && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 0
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyPageDown or vbKeyNumpad3:
    // if direction is currently DownRight
    if (TestDir = odDownRight) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to downright
      TestDir = odDownRight
      // set loop to 0, if not already 0 AND in auto
      if (CurTestLoop != 0 && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 0
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyDown or vbKeyNumpad2:
    // if direction is currently down
    if (TestDir = odDown) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to down
      TestDir = odDown
      // set loop to 2, if there are four AND loop is not 2 AND in auto
      if (TestView.Loops.Count >= 4 && CurTestLoop != 2 && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 2
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyEnd or vbKeyNumpad1:
    // if direction is currently DownLeft
    if (TestDir = odDownLeft) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to downLeft
      TestDir = odDownLeft
      // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
      if ((TestView.Loops.Count >= 2) && (CurTestLoop != 1) && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 1
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyLeft or vbKeyNumpad4:
    // if direction is currently Left
    if (TestDir = odLeft) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to Left
      TestDir = odLeft
      // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
      if ((TestView.Loops.Count >= 2) && (CurTestLoop != 1) && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 1
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyHome or vbKeyNumpad7:
    // if direction is currently UpLeft
    if (TestDir = odUpLeft) {
      // stop movement
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
    } else {
      // set direction to UpLeft
      TestDir = odUpLeft
      // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
      if ((TestView.Loops.Count >= 2) && (CurTestLoop != 1) && (TestSettings.TestLoop = -1)) {
        CurTestLoop = 1
        CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
        CurTestCel = 0
        TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
      }
      // enable timer
      tmrTest.Enabled = true
    }
    
  case vbKeyNumpad5:
    // always stop
    TestDir = 0
    tmrTest.Enabled = TestSettings.CycleAtRest
  }
}


private void DeleteCommand(int DelIndex, bool DontUndo = false) {

  // delete an entire command
    
  Dim DelCount As Long
  Dim DelPos As Long
  Dim i As Long
  Dim NextUndo As PictureUndo
  Dim bytUndoData() As Byte
  
  // get starting position
  DelPos = lstCommands.Items[DelIndex].Tag
  
  // calculate bytes to delete
  DelCount = lstCommands.Items[DelIndex + 1].Tag - DelPos
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    // create new undo object
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = DelCmd
      NextUndo.UDPicPos = DelPos
      NextUndo.UDCmdIndex = DelIndex
      NextUndo.UDCmd = lstCommands.List(DelIndex)
    
      ReDim bytUndoData(DelCount - 1)
      for (int i = 0; i < DelCount - 1; i++) {
        bytUndoData(i) = EditPicture.Resource.Data(DelPos + i)
      }
      NextUndo.UDData = bytUndoData
      NextUndo.UDCoordIndex = 1
    // add to undo
    AddUndo NextUndo
  }
  
  // remove from resource
  EditPicture.Resource.RemoveData DelPos, DelCount
  
  // adjust position values to account for deleted data
  UpdatePosValues DelIndex, -DelCount
  
  // remove from cmd list
  lstCommands.RemoveItem DelIndex
  
  // select cmd at delindex position
  SelectCmd DelIndex, DontUndo
}
private void AddUndo(PictureUndo NextUndo) {

  if (!IsChanged) {
    MarkAsChanged();
  }
  
  // remove old undo items until there is room for this one
  // to be added
  if (Settings.PicUndo > 0) {
    while (UndoCol.Count >= Settings.PicUndo) {
      UndoCol.Remove 1
    }
  }
  
  // adds the next undo object
  UndoCol.Add NextUndo
  
  // set undo menu
  frmMDIMain.mnuEUndo.Enabled = true
  frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(PICUNDOTEXT + NextUndo.UDAction) & NextUndo.UDCmd & vbTab & "Ctrl+Z"
}



public void DeleteCoordinate(int DelCoord, bool DontUndo = false) {

  Dim DelCount As Long
  Dim DelPos As Long
  Dim i As Long, j As Long
  Dim NextUndo As PictureUndo
  Dim bytUndoData() As Byte
  
  // if this is last item
  if (lstCoords.ListCount = 1) {
    // send focus to cmd list
    lstCommands.SetFocus
    // use command delete
    DeleteCommand SelectedCmd
    // stop coordinate flashing
    tmrSelect.Enabled = false
  return;
  } else {
    // remove the coordinates at this position
    DelPos = lstCoords.Items[DelCoord].Tag
    
    // if deleting a plot point in splatter mode
    if (InStr(1, lstCoords.List(DelCoord), "-") != 0) {
      DelCount = 3
    // if deleting a relative line, or a step line
    } else if ( lstCommands.Text = "Rel Line" || lstCommands.Text = "X Corner" || lstCommands.Text = "Y Corner") {
      DelCount = 1
    } else {
      DelCount = 2
    }
    
    // if not skipping undo
    if (!DontUndo && Settings.PicUndo != 0) {
      // create new undo object
      Set NextUndo = New PictureUndo
        NextUndo.UDAction = DelCoord
        NextUndo.UDPicPos = DelPos
        NextUndo.UDCmdIndex = SelectedCmd
        if (DelCoord = lstCoords.ListCount - 1) {
          // if deleting last coordinate, use -1 as coordpos
          // so it can get added back to end of list
          NextUndo.UDCoordIndex = -1
        } else {
          NextUndo.UDCoordIndex = DelCoord
        }
        NextUndo.UDText = lstCoords.List(DelCoord)
        ReDim bytUndoData(DelCount - 1)
        for (int i = 0; i <= DelCount - 1; i++) {
          bytUndoData(i) = EditPicture.Resource.Data(DelPos + i)
        }
        NextUndo.UDData = bytUndoData
      // add to undo
      AddUndo NextUndo
    }
    
    // remove from resource
    EditPicture.Resource.RemoveData DelPos, DelCount
    
    // adjust position values to account for deleted data
    UpdatePosValues SelectedCmd + 1, -DelCount
    
    // update position values for coords
    for (int i = DelCoord + 1; i <= lstCoords.ListCount - 1; i++) {
      lstCoords.Items[i].Tag = lstCoords.Items[i].Tag - DelCount
    }
    
    // remove from coord list
    for (int j = DelCoord; j <= lstCoords.ListCount - 2; j++) {
      CoordPT(j) = CoordPT(j + 1)
    ]
    
    // remove from listbox
    lstCoords.RemoveItem DelCoord
    
  }
  
  // redraw by selecting next coord
  if (DelCoord = lstCoords.ListCount) {
    DelCoord = DelCoord - 1
  }
  NoSelect = true
  lstCoords.ListIndex = DelCoord
}

private void DelPatternData(int tmpIndex, bool DontUndo = false) {
  // remove pattern bytes

  Dim i As Long, lngNewPos As Long
  Dim bytPatDat() As Byte, bytPattern As Byte
  Dim NextUndo As PictureUndo
  
  // if not skipping undo
  if (!DontUndo) {
    // reset random generator by using timer
    Randomize Timer
    
    // create array to hold patterns for undo in 25 byte segments
    ReDim bytPatDat(24)
  }
  
  // set start pos so first iteration will select pattern byte for first coord
  lngNewPos = lstCommands.Items[tmpIndex].Tag + 1
    
  do {
    // if not skipping undo
    if (!DontUndo) {
      bytPattern = EditPicture.Resource.Data(lngNewPos)
      
      if (i > UBound(bytPatDat)) {
        ReDim Preserve bytPatDat(UBound(bytPatDat) + 25)
      }
      
      // save to array
      bytPatDat(i) = bytPattern
    }
    
    // remove from picture resource
    EditPicture.Resource.RemoveData lngNewPos
    
    // adjust pos
    lngNewPos = lngNewPos + 2
    
    // increment offset
    i = i + 1
  while (EditPicture.Resource.Data(lngNewPos) < 0xF0);
  
  // remove any extra bytes in pattern array
  if (!DontUndo) {
    ReDim Preserve bytPatDat(i - 1)
  }
  
  // adjust positions of follow on commands (i now equals number of bytes removed)
  UpdatePosValues tmpIndex + 1, -i
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    // save undo info
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = DelPlotPattern
      NextUndo.UDPicPos = lstCommands.Items[tmpIndex].Tag
      NextUndo.UDCmdIndex = tmpIndex
      NextUndo.UDData = bytPatDat
    // add the undo object without setting edit menu
    UndoCol.Add NextUndo
  }
}

public void RefreshPic() {

  // force picture reset by accessing bmp
  Dim rtn As Long
  rtn = EditPicture.VisualBMP(true)
  
  // now redraw the pictures
  DrawPicture
}

private void EndEditCoord(DrawFunction CmdType, int CoordNum, Point PicPt, int lngPos, string strCoord, ByVboolal DontUndo = false) {
  
  Dim lngPosOffset As Long
  Dim strPattern As String
  Dim NextUndo As PictureUndo
  Dim tmpPT As Point, tmpPrevPT As Point, tmpNextPT As Point
  Dim bytData() As Byte
  
  // save old pt if undoing
  if (!DontUndo && Settings.PicUndo) {
    // use data section to hold old coord values
    ReDim bytData(1)
    tmpPT = ExtractCoordinates(strCoord)
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    // if no change,
    if (tmpPT.X = PicPt.X && tmpPT.Y = PicPt.Y) {
      // reset drawing mode, but no update is necessary
      PicDrawMode = doNone
      return;
    }
  }
  
  // validate for Type of node being edited
  switch (CmdType) {
  case dfAbsLine or dfFill or dfPlotPen:
    // if this node includes a pattern command,
    if (InStr(1, strCoord, "-") != 0) {
      // adjust resource pos by 1
      lngPosOffset = 1
      strPattern = Left$(strCoord, InStr(1, strCoord, "(") - 1)
    }
    // update resource data
    EditPicture.Resource.Data(lngPos + lngPosOffset) = PicPt.X
    EditPicture.Resource.Data(lngPos + lngPosOffset + 1) = PicPt.Y
    
  case dfRelLine:
    // get point being edited
    tmpPT = RelLineCoord(lngPos)
    
    // validate x and Y:
    
    // if not first point
    if (CoordNum > 0) {
      // validate against previous point
      tmpPrevPT = RelLineCoord(lngPos - 1)
      // validate x and Y against previous pt
      // (note that delta x is limited to -6 to avoid
      // values above 0xF0, which would mistakenly be interpreted
      // as a new command)
      if (PicPt.X > tmpPrevPT.X + 7) {
        PicPt.X = tmpPrevPT.X + 7
      } else if ( PicPt.X < tmpPrevPT.X - 6) {
        PicPt.X = tmpPrevPT.X - 6
      }
      if (PicPt.Y > tmpPrevPT.Y + 7) {
        PicPt.Y = tmpPrevPT.Y + 7
      } else if (PicPt.Y < tmpPrevPT.Y - 7) {
        PicPt.Y = tmpPrevPT.Y - 7
      }
    }
    
    // if not last point (next pt is not a new cmd)
    if (EditPicture.Resource.Data(lngPos + IIf(CoordNum = 0, 2, 1)) < 0xF0) {
      // validate against next point
      // note that delta x is limited to +6 (swapped because we are
      // comparing against NEXT vs. PREVIOUS coordinate)
      // for same reason as given above
      tmpNextPT = RelLineCoord(lngPos + IIf(CoordNum = 0, 2, 1))
      if (PicPt.X > tmpNextPT.X + 6) {
        PicPt.X = tmpNextPT.X + 6
      } else if (PicPt.X < tmpNextPT.X - 7) {
        PicPt.X = tmpNextPT.X - 7
      }
      if (PicPt.Y > tmpNextPT.Y + 7) {
        PicPt.Y = tmpNextPT.Y + 7
      } else if (PicPt.Y < tmpNextPT.Y - 7) {
        PicPt.Y = tmpNextPT.Y - 7
      }
    }
    
    // if first coordinate
    if (CoordNum = 0) {
      // recalculate delta to second point
      if (EditPicture.Resource.Data(lngPos + 2) < 0xF0) {
        EditPicture.Resource.Data(lngPos + 2) = Abs(CLng(tmpNextPT.X) - PicPt.X) * 16 + IIf(Sgn(CLng(tmpNextPT.X) - PicPt.X) = -1, 128, 0) + Abs(CLng(tmpNextPT.Y) - PicPt.Y) + IIf(Sgn(CLng(tmpNextPT.Y) - PicPt.Y) = -1, 8, 0)
      }
      // update data
      EditPicture.Resource.Data(lngPos) = PicPt.X
      EditPicture.Resource.Data(lngPos + 1) = PicPt.Y
    } else {
      // if not last point
      if (EditPicture.Resource.Data(lngPos + 1) < 0xF0) {
        // calculate new relative change in x and Y between next pt and this point
        EditPicture.Resource.Data(lngPos + 1) = Abs(CLng(tmpNextPT.X) - PicPt.X) * 16 + IIf(Sgn(CLng(tmpNextPT.X) - PicPt.X) = -1, 128, 0) + Abs(CLng(tmpNextPT.Y) - PicPt.Y) + IIf(Sgn(CLng(tmpNextPT.Y) - PicPt.Y) = -1, 8, 0)
      }
      
      // calculate new relative change in x and Y between previous pt and this point
      EditPicture.Resource.Data(lngPos) = Abs(CLng(PicPt.X) - tmpPrevPT.X) * 16 + IIf(Sgn(CLng(PicPt.X) - tmpPrevPT.X) = -1, 128, 0) + Abs(CLng(PicPt.Y) - tmpPrevPT.Y) + IIf(Sgn(CLng(PicPt.Y) - tmpPrevPT.Y) = -1, 8, 0)
    }
    
  case dfXCorner:
    // if editing first point,
    if (CoordNum = 0) {
      // update resource data
      EditPicture.Resource.Data(lngPos) = PicPt.X
      EditPicture.Resource.Data(lngPos + 1) = PicPt.Y
    } else {
      // if odd
      if ((Int(CoordNum / 2) != CoordNum / 2)) {
        // x Value is at lngPos; Y Value is at lngPos-1
        EditPicture.Resource.Data(lngPos) = PicPt.X
        EditPicture.Resource.Data(lngPos - 1) = PicPt.Y
      } else {
        // x Value is at lngPos-1, Y Value is at lngPos
        EditPicture.Resource.Data(lngPos - 1) = PicPt.X
        EditPicture.Resource.Data(lngPos) = PicPt.Y
      }
    }
    
  case dfYCorner:
    // if editing first point,
    if (CoordNum = 0) {
      // update resource data
      EditPicture.Resource.Data(lngPos) = PicPt.X
      EditPicture.Resource.Data(lngPos + 1) = PicPt.Y
    } else {
      // if even
      if (((Int(CoordNum / 2) = CoordNum / 2))) {
        // x Value is lngpos, Y Value is at lngpos-1
        EditPicture.Resource.Data(lngPos) = PicPt.X
        EditPicture.Resource.Data(lngPos - 1) = PicPt.Y
      } else {
        // special check for Y lines; for the second coord, the x Value is actaully
        // two bytes in front of the edited coord (since cmd gives first coord
        // as two bytes, then shifts to single byte per coord; Y Value is at lngpos
        if (CoordNum = 1) {
          // x Value is at lngPos-2
          EditPicture.Resource.Data(lngPos - 2) = PicPt.X
        } else {
          // x Value is at lngPos-1
          EditPicture.Resource.Data(lngPos - 1) = PicPt.X
        }
        EditPicture.Resource.Data(lngPos) = PicPt.Y
      }
    }
  }
  
  if (!DontUndo && Settings.PicUndo != 0) {
    // create undo object
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = EditCoord
      NextUndo.UDText = CStr(CmdType)
      NextUndo.UDCoordIndex = CoordNum
      NextUndo.UDCmdIndex = SelectedCmd
      NextUndo.UDPicPos = lngPos
      NextUndo.UDData = bytData()
    AddUndo NextUndo
  }
  
  // reset edit mode
  PicDrawMode = doNone
  
  // begin highlighting selected coord again
  tmrSelect.Enabled = true
  if (CursorMode = pcmWinAGI) {
    // save area under cursor
    BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
  }
}

private void InsertCoordinate(bool BeginMove) {
  
  // inserts a new coord, then begins editing it
  
  Dim tmpPT As Point, tmpPos As Long
  Dim bytData() As Byte, bytPattern As Byte
  
  // get edit cmd
  EditCmd = EditPicture.Resource.Data(CLng(lstCommands.Items[SelectedCmd].Tag))
  
  // get current point
  tmpPT = ExtractCoordinates(lstCoords.Text)
  
  // add a new coordinate after this coordinate
  switch (EditCmd) {
  case dfAbsLine or dfFill:
    // store new abs coord point
    ReDim bytData(1)
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    
    // insert copy of selected coordinate at current position
    AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y
    // don't need to redraw, since picture doesn't change when coord is added
  
  case dfPlotPen:
    // depends on pen- if splatter, we need an extra data element
    if (CurrentPen.PlotStyle = psSolid) {
      // store new abs coord point
      ReDim bytData(1)
      bytData(0) = tmpPT.X
      bytData(1) = tmpPT.Y
      
      // insert copy of selected coordinate at current position
      AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y
      // don't need to redraw, since picture doesn't change when coord is added
    } else {
      // need three
      // store new abs coord point
      ReDim bytData(2)
      Randomize Now()
      bytPattern = 1 + CByte(Int(Rnd * 119))
      bytData(0) = 2 * bytPattern
      bytData(1) = tmpPT.X
      bytData(2) = tmpPT.Y
      // insert copy of selected coordinate at current position
      AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y, CStr(bytPattern) & " -- "
      // don't need to redraw, since picture doesn't change when coord is added
    }
    
  case dfRelLine:
    // insert single command with Value of zero
    ReDim bytData(0)
    bytData(0) = 0
    // insert zero offest coordinate AFTER selected coordinate
    tmpPos = lstCoords.ListIndex + 1
    if (tmpPos = lstCoords.ListCount) {
      tmpPos = -1
    }
    AddCoordToPic bytData, tmpPos, tmpPT.X, tmpPT.Y
    // select it
    lstCoords.ListIndex = lstCoords.ListIndex + 1
    // don't need to redraw, since picture doesn't change when coord is added
   
  case dfXCorner:
    // can only add to end
    if (lstCoords.ListIndex = lstCoords.ListCount - 1) {
      ReDim bytData(0)
      // if this current end pt an odd numbered coord
      if (EditCoordNum / 2 != Int(EditCoordNum / 2)) {
        // adding a new Y
        bytData(0) = tmpPT.Y
      } else {
        // adding a new x
        bytData(0) = tmpPT.X
      }
      // insert coordinate to end
      AddCoordToPic bytData, -1, tmpPT.X, tmpPT.Y
      // select it
      lstCoords.ListIndex = lstCoords.ListCount - 1
      // don't need to redraw, since picture doesn't change when coord is added
    }
    
  case dfYCorner:
    // can only add to end
    if (lstCoords.ListIndex = lstCoords.ListCount - 1) {
      ReDim bytData(0)
      // if current end pt is an even numbered coord
      if (EditCoordNum / 2 = Int(EditCoordNum / 2)) {
        // adding a new Y
        bytData(0) = tmpPT.Y
      } else {
        // adding a new x
        bytData(0) = tmpPT.X
      }
      // insert coordinate at end
      AddCoordToPic bytData, -1, tmpPT.X, tmpPT.Y
      // select it
      lstCoords.ListIndex = lstCoords.ListCount - 1
      // don't need to redraw, since picture doesn't change when coord is added
    }
  }
  
  if (BeginMove) {
    // now begin moving
    PicDrawMode = doMovePt
    
    // turn off cursor flasher
    tmrSelect.Enabled = false
  }
}

private void EndDraw(Point PicPt) {
  // finish drawing command
  // for lines and shapes, add the correct commands to complete the draw;
  // for other draw commands, add another point
  
  Dim bytData() As Byte
  Dim i As Long, lngInsertPos As Long
  
  // *'Debug.Assert CurrentPen.PlotShape = SelectedPen.PlotShape
  // *'Debug.Assert CurrentPen.PlotSize = SelectedPen.PlotSize
  // *'Debug.Assert CurrentPen.PlotStyle = SelectedPen.PlotStyle
  // *'Debug.Assert CurrentPen.PriColor = SelectedPen.PriColor
  // *'Debug.Assert CurrentPen.VisColor = SelectedPen.VisColor
  
  // if cursor hasn't moved, just exit
  if (Anchor.X = PicPt.X && Anchor.Y = PicPt.Y) {
    return;
  }
  
  switch (PicDrawMode) {
  case doLine:
    // depending on line Type, need to complete the current line segment
    // (note that we don't end the draw mode; this let's us continue adding more
    // line segments to this command)
    switch (SelectedTool) {
    case ttLine:
      // set data to add this point
      ReDim bytData(1)
      bytData(0) = PicPt.X
      bytData(1) = PicPt.Y
      
    case ttRelLine:
      // validate x and y
      // (note that delta x is limited to -6 to avoid
      // values above 0xF0, which would mistakenly be interpreted
      // as a new command)
      if (PicPt.X < Anchor.X - 6) {
        PicPt.X = Anchor.X - 6
      } else if (PicPt.X > Anchor.X + 7) {
        PicPt.X = Anchor.X + 7
      }
      if (PicPt.Y < Anchor.Y - 7) {
        PicPt.Y = Anchor.Y - 7
      } else if (PicPt.Y > Anchor.Y + 7) {
        PicPt.Y = Anchor.Y + 7
      }
      
      // calculate delta to this point
      ReDim bytData(0)
      bytData(0) = Abs(CLng(PicPt.X) - Anchor.X) * 16 + IIf(Sgn(CLng(PicPt.X) - Anchor.X) = -1, 128, 0) + Abs(CLng(PicPt.Y) - Anchor.Y) + IIf(Sgn(CLng(PicPt.Y) - Anchor.Y) = -1, 8, 0)
      
      // '*'Debug.Assert EditCoordNum = lstCoords.ListCount - 1
      
    case ttCorner:
      // draw next line coordinate
      ReDim bytData(0)
     // get insert pos
     lngInsertPos = lstCommands.Items[SelectedCmd].Tag
     
      // if drawing second point
      if (lstCoords.ListCount = 1) {
        // if mostly vertical
        if (Abs(CLng(PicPt.X) - Anchor.X) < Abs(CLng(PicPt.Y) - Anchor.Y)) {
          // command should be Y corner
          if (Asc(lstCommands.Text) != 89) {
            lstCommands.List(SelectedCmd) = "Y Corner"
            EditPicture.Resource.Data(lngInsertPos) = dfYCorner
          }
          // limit change to vertical direction only
          PicPt.X = Anchor.X
          bytData(0) = PicPt.Y
        } else {
          // command should be X corner
          if (Asc(lstCommands.Text) = 89) {
            lstCommands.List(SelectedCmd) = "X Corner"
            EditPicture.Resource.Data(lngInsertPos) = dfXCorner
          }
          // limit change to horizontal direction only
          PicPt.Y = Anchor.Y
          bytData(0) = PicPt.X
        }
      } else {
        // determine which direction to allow movement
        if ((Asc(lstCommands.Text) = 88 && (Int(lstCoords.ListCount / 2) = lstCoords.ListCount / 2)) ||
           (Asc(lstCommands.Text) = 89 && (Int(lstCoords.ListCount / 2) != lstCoords.ListCount / 2))) {
          // limit change to vertical direction
          PicPt.X = Anchor.X
          bytData(0) = PicPt.Y
        } else {
          // limit change to horizontal direction
          PicPt.Y = Anchor.Y
          bytData(0) = PicPt.X
        }
      }
     
    }
    // if cursor hasn't moved, just exit
    if (Anchor.X = PicPt.X && Anchor.Y = PicPt.Y) {
      return;
    }
    
    // set anchor to new point
    Anchor = PicPt
    // insert coordinate
    AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y
  
  case doShape:
    // depending on shape Type, add appropriate commands to add the
    // selected element
    // (note that when shapes are completed, we go back to 'none' as the draw mode
    // each shape is drawn as a separate action)
    
    switch (SelectedTool) {
    case ttRectangle:
      // finish drawing box
      ReDim bytData(6)
      bytData(0) = dfXCorner
      bytData(1) = Anchor.X
      bytData(2) = Anchor.Y
      bytData(3) = PicPt.X
      bytData(4) = PicPt.Y
      bytData(5) = Anchor.X
      bytData(6) = Anchor.Y
      
      // add command
      InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
      
      // select this command
      SelectCmd lstCommands.NewIndex
      
      // adjust last undo text
      UndoCol(UndoCol.Count).UDAction = Rectangle
      UndoCol(UndoCol.Count).UDCmd = vbNullString
      
    case ttTrapezoid:
      // finish drawing trapezoid
      ReDim bytData(10)
      bytData(0) = dfAbsLine
      bytData(1) = Anchor.X
      bytData(2) = Anchor.Y
      bytData(3) = 159 - Anchor.X
      bytData(4) = Anchor.Y
      // ensure sloping side is on same side of picture
      if ((Anchor.X < 80 && PicPt.X < 80) || (Anchor.X >= 80 && PicPt.X >= 80)) {
        bytData(5) = 159 - PicPt.X
        bytData(6) = PicPt.Y
        bytData(7) = PicPt.X
        bytData(8) = PicPt.Y
      } else {
        bytData(5) = PicPt.X
        bytData(6) = PicPt.Y
        bytData(7) = 159 - PicPt.X
        bytData(8) = PicPt.Y
      }
      bytData(9) = Anchor.X
      bytData(10) = Anchor.Y
      
      // add command
      InsertCommand bytData, SelectedCmd, "Abs Line", gInsertBefore
      // ensure it is selected
      SelectCmd lstCommands.NewIndex
      
      // adjust last undo text
      UndoCol(UndoCol.Count).UDAction = Trapezoid
      UndoCol(UndoCol.Count).UDCmd = vbNullString
      
    case ttEllipse:
      // finish drawing ellipse
      
      // if both height and width are one pixel
      if (((CLng(Anchor.X) - PicPt.X) = 0) && ((CLng(Anchor.Y) - PicPt.Y) = 0)) {
        // draw just a single pixel
        ReDim bytData(2)
        bytData(0) = dfXCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        // insert the command
        InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
        // then select it
        SelectCmd lstCommands.NewIndex
        
      // if height is one pixel,
      } else if ((CLng(Anchor.Y) - PicPt.Y = 0)) {
        // just draw a horizontal line
        ReDim bytData(3)
        bytData(0) = dfXCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        bytData(3) = PicPt.X
        // add command
        InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
        SelectCmd lstCommands.NewIndex
        
      // if width is one pixel,
      } else if ((CLng(Anchor.X) - PicPt.X = 0)) {
        // just draw a vertical line
        ReDim bytData(3)
        bytData(0) = dfYCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        bytData(3) = PicPt.Y
        // add command
        InsertCommand bytData, SelectedCmd, "Y Corner", gInsertBefore
        // and select it
        SelectCmd lstCommands.NewIndex
        
      } else {
        // ensure we are in a upperleft-lower right configuration
        if (Anchor.X > PicPt.X) {
          i = Anchor.X
          Anchor.X = PicPt.X
          PicPt.X = i
        }
        if (Anchor.Y > PicPt.Y) {
          i = Anchor.Y
          Anchor.Y = PicPt.Y
          PicPt.Y = i
        }
          
        // call drawellipse to update arc segment data
        DrawCircle Anchor.X, Anchor.Y, PicPt.X, PicPt.Y
        ReDim bytData((Segments + 1) * 2)
        bytData(0) = dfAbsLine
        
        // now draw the arc segments:
        
        // add first arc
        for (int i = 0; i <= Segments; i++) {
          bytData(i * 2 + 1) = Anchor.X + ArcPt(0).X - ArcPt(i).X
          bytData(i * 2 + 2) = Anchor.Y + ArcPt(Segments).Y - ArcPt(i).Y
        }
        InsertCommand bytData, SelectedCmd, "Abs Line", gInsertBefore
        
        // add second arc (skip undo)
        for (int i = 0; i <= Segments; i++) {
          bytData(2 * i + 1) = PicPt.X - ArcPt(0).X + ArcPt(i).X
          bytData(2 * i + 2) = Anchor.Y + ArcPt(Segments).Y - ArcPt(i).Y
        }
        InsertCommand bytData, SelectedCmd, "Abs Line", false, true
        
        // add third arc (skip undo)
        for (int i = 0; i <= Segments; i++) {
          bytData(2 * i + 1) = PicPt.X - ArcPt(0).X + ArcPt(i).X
          bytData(2 * i + 2) = PicPt.Y - ArcPt(Segments).Y + ArcPt(i).Y
        }
        InsertCommand bytData, SelectedCmd + 1, "Abs Line", false, true
        
        // add fourth arc (skip undo)
        for (int i = 0; i <= Segments; i++) {
          bytData(2 * i + 1) = Anchor.X + ArcPt(0).X - ArcPt(i).X
          bytData(2 * i + 2) = PicPt.Y - ArcPt(Segments).Y + ArcPt(i).Y
        }
        InsertCommand bytData, SelectedCmd + 2, "Abs Line", false, true
        
        // select the last command added
        SelectCmd SelectedCmd + 4
        
        // adjust last undo text
        UndoCol(UndoCol.Count).UDAction = Ellipse
        UndoCol(UndoCol.Count).UDCmd = vbNullString
      }
    }
    
    // end draw mode
    StopDrawing
  }
}


public void AddCoordToPic(byte[] NewData, int CoordPos, byte bytX, byte bytY, string Prefix = "", bool DontUndo = false) {

  // inserts coordinate data into EditPicture
  
  Dim NextUndo As PictureUndo
  Dim lngInsertPos As Long
  Dim bytData() As Byte, lngCount As Long
  Dim i As Long
  
  // copy data to local array
  bytData = NewData
  lngCount = UBound(bytData) + 1
  
  // if no coord yet
  if (lstCoords.ListCount = 0) {
    // get insert pos from the cmd
    lngInsertPos = lstCommands.Items[SelectedCmd].Tag
    // *'Debug.Assert CoordPos = -1
  } else {
    if (CoordPos = -1) {
      lngInsertPos = lstCommands.Items[SelectedCmd + 1].Tag
      CoordPos = lstCoords.ListCount
    } else {
      // *'Debug.Assert CoordPos <= lstCoords.ListCount - 1
      // get insert pos from coord list
      lngInsertPos = lstCoords.Items[CoordPos].Tag
    }
  }
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    // create new undo object
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = AddCoord
      NextUndo.UDPicPos = lngInsertPos
      NextUndo.UDCmdIndex = SelectedCmd
      NextUndo.UDCoordIndex = CoordPos
      NextUndo.UDText = Prefix & CoordText(bytX, bytY)
    // add to undo
    AddUndo NextUndo
  }
  
  // insert data
  EditPicture.Resource.InsertData bytData, lngInsertPos
  
  // insert coord text
  AddCoordToList bytX, bytY, lngInsertPos, Prefix, CoordPos
  lstCoords.Refresh

  EditCoordNum = lstCoords.ListIndex
  
  // update position values for rest of coord list
  for (int i = CoordPos + 1; i <= lstCoords.ListCount - 1; i++) {
    lstCoords.Items[i].Tag = lstCoords.Items[i].Tag + UBound(bytData()) + 1
  }
  
  // update position values in rest of cmd list
  UpdatePosValues SelectedCmd + 1, UBound(bytData()) + 1
}

private void JoinCommands(int SecondCmdIndex, bool DontUndo = false) {
  // joins two commands that are adjacent, where
  // first coord of SecondCmdIndex is same point as end of previous command
  // or if both cmds are plots or fills
  
  Dim NextUndo As PictureUndo
  Dim CmdType As DrawFunction, FirstCmdIndex As Long
  Dim lngPos As Long
  Dim lngCount As Long
  Dim IsVertical As Boolean  // false = horizontal, true = vertical
  
  // get position of command
  lngPos = lstCommands.Items[SecondCmdIndex].Tag
  // get first cmd index
  FirstCmdIndex = SecondCmdIndex - 1
  
  // get command Type
  CmdType = EditPicture.Resource.Data(lngPos)
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = JoinCmds
      // if cmd requires one byte per coord pair
      // need to move picpos marker back one
      switch (CmdType) {
      case dfXCorner or dfYCorner or dfRelLine:
        // position of the split coord is back one
        NextUndo.UDPicPos = lstCommands.Items[SecondCmdIndex].Tag - 1
      default:
        NextUndo.UDPicPos = lstCommands.Items[SecondCmdIndex].Tag
      }
      
      // set cmdindex to first cmd
      NextUndo.UDCmdIndex = FirstCmdIndex
      // set coord index to Count of firstcmd
    AddUndo NextUndo
  }
  
  switch (CmdType) {
  case dfFill or dfPlotPen:
    // just delete the command
    lngCount = 1
  case dfXCorner or dfYCorner:
    // get orientation of last line of first cmd
    IsVertical = GetVerticalStatus(lstCommands.Items[FirstCmdIndex].Tag)
    
    // if orientation of last line of first cmd
    // is same as first line of this cmd
    if (((CmdType = dfXCorner) && !IsVertical) ||
       ((CmdType = dfYCorner) && IsVertical)) {
      // delete last coordinate from previous cmd AND command and first coordinate
      lngPos = lngPos - 1
      lngCount = 4
      
    } else {
      // delete command and first coordinate
      lngCount = 3
    }
    
  default:
    // delete command and first coordinate
    lngCount = 3
  }
  
  // delete the data
  EditPicture.Resource.RemoveData lngPos, lngCount
  // remove the second cmd
  lstCommands.RemoveItem SecondCmdIndex
  
  // update follow on cmds
  UpdatePosValues SecondCmdIndex, -lngCount
  
  // update
  SelectCmd FirstCmdIndex, false
}

public void MenuClickClear() {
  // clears the picture
  
  Dim i As Long
  
  // verify
  if (MsgBox("This will reset the picture, deleting all commands. This action cannot be undone. Do you want to continue?", vbQuestion + vbYesNo, "Clear Picture") = vbNo) {
    return;
  }
  
  // clear drawing surfaces
  picVisual.Cls
  picPriority.Cls
  
  // clear picture
  EditPicture.Clear
  
  // redraw tree
  LoadCmdList
  
  // select the end
  SelectCmd 0, false
  
  // reset pen
  SelectedPen.PriColor = agNone
  SelectedPen.VisColor = agNone
  SelectedPen.PlotShape = psCircle
  SelectedPen.PlotSize = 0
  SelectedPen.PlotStyle = psSolid
  CurrentPen = SelectedPen
  
  // refresh palette
  picPalette.Refresh
  
  // clear the undo buffer
  if (UndoCol.Count > 0) {
    for (int i = UndoCol.Count; i >= 1; i--) {
      UndoCol.Remove i
    }
    SetEditMenu
  }
}

private void ReadjustPlotCoordinates(int StartIndex, PlotStyle NewPlotStyle, bool DontUndo = false, int StopIndex = -1) {
  // starting at command in list at StartIndex, step through all
  // commands until another setplotpen command or end is reached;
  // any plot commands identified during search are checked to
  // see if they match format of desired plot pen style (solid or
  // splatter); if they don't match, they are adjusted (by adding
  // or removing the pattern byte)
  
  // if stopindex is passed, only cmds from StartIndex to StopIndex
  // are checked; if stopindex is not passed, all cmds to end of
  // cmd list are checked
   
  Dim i As Long
  Dim bytTemp() As Byte
  
  if (StopIndex = -1) {
    StopIndex = lstCommands.ListCount - 1
  }
  
  i = StartIndex
  do {
    // check for plot command or change plot pen command
    switch (Left$(lstCommands.List(i), 4)) {
    case "Plot":
      // if style is splatter
      if (NewPlotStyle = psSplatter) {
        // if skipping the undo feature
        if (DontUndo) {
          // need to set tmp byte array so addpatterndata method
          // will know to create the random bytes for this set of coordinates
          ReDim bytTemp(0)
          bytTemp(0) = 0xFF
        }
        
        // add pattern bytes (use a temp array as place holder for byte array argument)
        AddPatternData i, bytTemp, DontUndo
        
      // if style is solid,
      } else {
        // delete pattern bytes
        DelPatternData i, DontUndo
      }
      
    case "Set ":
            // set pen
      // can exit here because this pen command
      // ensures future plot commands are correct
      break; // exit do
    }
    // get next cmd
    i = i + 1
  while (i <= StopIndex);
}


private void SplitCommand(int CoordIndex, bool DontUndo = false) {
  // splits a command into two separate commands of the same Type
  
  Dim NextUndo As PictureUndo
  Dim CmdType As DrawFunction
  Dim lngPos As Long, lngCmdIndex As Long
  Dim lngCount As Long
  Dim IsVertical As Boolean  // false = horizontal, true = vertical
  Dim tmpPT As Point
  Dim bytData(1) As Byte
  
  // get cmd index
  lngCmdIndex = SelectedCmd
  
  // get coordinate values
  tmpPT = ExtractCoordinates(lstCoords.List(CoordIndex))
  
  // get command Type
  CmdType = EditPicture.Resource.Data(lstCommands.Items[lngCmdIndex].Tag)
  
  // get insert pos
  
  // if a fill or plot,
  if (CmdType = dfFill || CmdType = dfPlotPen) {
    lngPos = lstCoords.Items[CoordIndex].Tag
  } else {
    // insertion point is NEXT coord
    lngPos = lstCoords.Items[CoordIndex + 1].Tag
  }
  
  // insert a new command in resource and listbox
  EditPicture.Resource.InsertData CByte(CmdType), lngPos
  lstCommands.AddItem LoadResString(DRAWFUNCTIONTEXT + CmdType - 0xF0), lngCmdIndex + 1
  lstCommands.Items[lngCmdIndex + 1].Tag = lngPos
  
  // take command specific actions
  switch (CmdType) {
  case dfXCorner or dfYCorner:
    // get orientation of line being split
    IsVertical = (Int(CoordIndex / 2) != (CoordIndex / 2))
    // if cmd is a yCorner
    if (CmdType = dfYCorner) {
      // flip it
      IsVertical = !IsVertical
    }
    
    // if splitting a vertical line,
    if (IsVertical) {
      // if inserted byte is not a Ycorner
      if (CmdType != dfYCorner) {
        // change inserted cmd to YCorner
        EditPicture.Resource.Data(lngPos) = dfYCorner
        lstCommands.List(lngCmdIndex + 1) = LoadResString(DRAWFUNCTIONTEXT + dfYCorner - 0xF0)
      }
    } else {
      // if inserted byte is not a Xcorner
      if (CmdType != dfXCorner) {
        // change inserted cmd to XCorner
        EditPicture.Resource.Data(lngPos) = dfXCorner
        lstCommands.List(lngCmdIndex + 1) = LoadResString(DRAWFUNCTIONTEXT + dfXCorner - 0xF0)
      }
    }
    
    // insert starting point in resource
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    EditPicture.Resource.InsertData bytData(), lngPos + 1
    
    // three bytes inserted
    lngCount = 3
  
  case dfAbsLine or dfRelLine:
    // insert starting point into new cmd
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    EditPicture.Resource.InsertData bytData(), lngPos + 1
    
    // three bytes inserted
    lngCount = 3
    
  case dfFill or dfPlotPen:
    // only one byte inserted
    lngCount = 1
    
  }
  
  // update positions for cmds AFTER the new cmd
  UpdatePosValues lngCmdIndex + 2, lngCount
  
  // select the newly added command
  SelectCmd lngCmdIndex + 1, false
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = SplitCmd
      NextUndo.UDPicPos = lngPos
      NextUndo.UDCmdIndex = lngCmdIndex + 1
    AddUndo NextUndo
  }
}

private void StopDrawing() {

  // cancels a drawing action without adding a command or coordinate
  
  // reset draw mode
  PicDrawMode = doNone
  // if on a coordinate
  if (lstCoords.ListIndex != -1) {
    // select entire cmd
    lstCoords.ListIndex = -1
    // set curpt to impossible value so it will have to be reset when coords are selected
    CurPt.X = 255
    CurPt.Y = 255
  }
  
  // force redraw
  CodeClick = true
  lstCommands_Click
}
private void ToggleBkgd(bool NewVal As Boolean, bool ShowConfig = false) {

  // sets background Image display to match newval
  // loads a background if one is needed
  
  Dim OldVal As Boolean
  
  // note curent value
  OldVal = EditPicture.BkgdShow
  
  EditPicture.BkgdShow = NewVal

  // if showing background AND there is not a picture (OR if forcing re-configure)
  if ((EditPicture.BkgdShow && (BkgdImage == null)) || ShowConfig) {
    // use configure screen, which will load a background
    if (!ConfigureBackground()) {
      // if user cancels, and still no background, force flag to false
      if ((BkgdImage == null)) {
        EditPicture.BkgdShow = false
// ''      }
// ''      else {
// ''        // there is a bkgd, but no change made
      }
    }
  }
  
  // set button status, and set value for stored image in picresource
  if (EditPicture.BkgdShow) {
    Toolbar1.Buttons("bkgd").Value = tbrPressed
  } else {
    Toolbar1.Buttons("bkgd").Value = tbrUnpressed
  }
  
  // update menu caption
    // toggle bkgd visible only if a bkgd Image is loaded
    frmMDIMain.mnuRCustom2.Visible = !(BkgdImage == null)
    if (frmMDIMain.mnuRCustom2.Visible) {
      frmMDIMain.mnuRCustom2.Enabled = true
      if (EditPicture.BkgdShow && frmMDIMain.mnuRCustom2.Visible) {
        frmMDIMain.mnuRCustom2.Text = "Hide Background" & vbTab & "Alt+B"
      } else {
        frmMDIMain.mnuRCustom2.Text = "Show Background" & vbTab & "Alt+B"
      }
    }
    //  allow removal if an image is loaded
    frmMDIMain.mnuRCustom3.Visible = frmMDIMain.mnuRCustom2.Visible
    if (frmMDIMain.mnuRCustom3.Visible) {
      frmMDIMain.mnuRCustom3.Enabled = true
      frmMDIMain.mnuRCustom3.Text = "Remove Background Image" & vbTab & "Shift+Alt+B"
    }
  
  // if current command has coordinates, do more than just redraw picture
  if (lstCoords.ListCount > 0) {
    if (lstCoords.ListIndex != -1) {
      // use coordinate click method if a coordinate is currently selected
      lstCoords_Click
    } else {
      // use command click method if no coordinates selected
      CodeClick = true
      lstCommands_Click
    }
  } else {
    // if selected command doesn't have any coordinates
    // redrawing is sufficient to set correct state of editor
    DrawPicture
  }
}

private void UpdatePosValues(int CmdPos, int PosOffset) {
  // updates the command list so itemtag values have correct position Value
  // cmdpos is the index of first command that needs to be adjusted
  
  Dim i As Long
  
  
  
  // need to increment position info for all commands after the insert point
  
  for (int i = CmdPos; i <= lstCommands.ListCount - 1; i++) {
    lstCommands.Items[i].Tag = lstCommands.Items[i].Tag + PosOffset
  }
}


private void InsertCommand(byte[] NewData, int CmdPos, string CmdText, bool InsertBefore, bool DontUndo = false) {
  // inserts NewData into EditPicture and CmdText into cmd list at CmdPos
  
  Dim NextUndo As PictureUndo
  Dim bytData() As Byte
  Dim lngInsertPos As Long
  
  // whenever a command is inserted, set the flag so any additional
  // inserts will occur AFTER the currently selected command
  gInsertBefore = false
  
  // copy data to local array
  bytData = NewData
  
  // if at end, force insertbefore to true
  if (CmdPos = lstCommands.ListCount - 1) {
    InsertBefore = true
  }
   
  if (!InsertBefore) {
    // insert at next cmd location
    CmdPos = CmdPos + 1
  }
  lngInsertPos = lstCommands.Items[CmdPos].Tag
  
  // if not skipping undo
  if (!DontUndo && Settings.PicUndo != 0) {
    // create new undo object
    Set NextUndo = New PictureUndo
      NextUndo.UDAction = AddCmd
      NextUndo.UDPicPos = lngInsertPos
      NextUndo.UDCmdIndex = CmdPos
      NextUndo.UDCmd = CmdText
    // add to undo
    AddUndo NextUndo
  }
  
  // insert data
  EditPicture.Resource.InsertData bytData(), lngInsertPos
  
  // insert into cmd list
  lstCommands.AddItem CmdText, CmdPos
  
  // set position Value
  lstCommands.Items[CmdPos].Tag = lngInsertPos
  
  // update position values in rest of tree
  UpdatePosValues CmdPos + 1, UBound(bytData()) + 1
}

private void LoadTestView() {
    
  // if a test view is currently loaded,
  if (!(TestView == null)) {
    // unload it and release it
    TestView.Unload
    Set TestView = Nothing
    //  disable test cel drawing
    ShowTestCel = false
  }
  
  Set TestView = New AGIView
  // if in a game
  if (GameLoaded) {
    // copy from game
    if (!Views(TestViewNum).Loaded) {
      Views(TestViewNum).Load
      TestView.SetView Views(TestViewNum)
      Views(TestViewNum).Unload
    } else {
      TestView.SetView Views(TestViewNum)
    }
  } else {
    // load from file
    TestView.Import TestViewFile
  }
  
  // if error
  if (Err.Number != 0) {
    ErrMsgBox "Unable to load view resource due to error:", "Test view not set.", "Test View Error"
    Set TestView = Nothing
    return;
  }
  // reset loop and cel and direction, and motion
  CurTestLoop = 0
  TestSettings.TestLoop = -1 // 0
  CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
  CurTestCel = 0
  TestSettings.TestCel = -1 //  0
  TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
  TestDir = 0
  // set cel height/width/transcolor
  CelWidth = TestView.Loops(CurTestLoop).Cels(CurTestCel).Width
  CelHeight = TestView.Loops(CurTestLoop).Cels(CurTestCel).Height
  CelTrans = TestView.Loops(CurTestLoop).Cels(CurTestCel).TransColor
  
  // disable drawing until user actually places the cel
  ShowTestCel = false
  
  // if already in test mode (and we're changing
  // the view being used)
  if (PicMode = pmViewTest) {
    // redraw picture to clear old testview
    DrawPicture
  }
}

public void MenuClickDelete() {
  // delete a coordinate or command
  
  Dim NewStyle As PlotStyle, lngStartPos As Long
  Dim i As Long, NextUndo As PictureUndo
  Dim bytData() As Byte, blnSetPen As Boolean
  Dim lngCount As Long
  
  // if on the root,
  if (lstCommands.ListIndex = -1) {
    // exit sub
    return;
  }
  
  // if on a command, (i.e. no coord is selected)
  if (lstCoords.ListIndex = -1) {
    // (can't delete END)
    // *'Debug.Assert lstCommands.ListIndex != lstCommands.ListCount - 1
    
    // if more than one cmd selected
    if (lstCommands.SelCount > 1) {
      // check for 'set pen' cmd
      for (int i = 0; i < lstCommands.SelCount; i++) {
        if (Left$(lstCommands.List(SelectedCmd - i), 3) = "Set") {
          blnSetPen = true
          break; // exit for
        }
      }
      
      if (blnSetPen) {
        // determine new pen style to apply from this point
        // start at first cmd above the top selected cmd
        i = SelectedCmd - lstCommands.SelCount
        while (i >= 0) {
          // if this command is a set command
          if (Left$(lstCommands.List(i), 3) = "Set") {
            // get pen status
            NewStyle = IIf(InStr(lstCommands.List(i), "Solid"), psSolid, psSplatter)
            break; // exit do
          }
          // get previous cmd
          i = i - 1
        }
        if (NewStyle != CurrentPen.PlotStyle) {
          // adjust plot pattern starting with next command after the one being deleted
          // (this must be done BEFORE the resource is modified, otherwise the undo
          // feature won't work correctly)
          ReadjustPlotCoordinates SelectedCmd + 1, NewStyle
        }
      }
      
      // save position of first command that is selected
      lngStartPos = lstCommands.Items[SelectedCmd - lstCommands.SelCount + 1].Tag
      
      if (Settings.PicUndo != 0) {
        // create undo object
        Set NextUndo = New PictureUndo
        NextUndo.UDAction = DelCmd
        // save position of first command that is selected
        NextUndo.UDPicPos = lngStartPos
        // save cmd location and Count of commands
        NextUndo.UDCmdIndex = SelectedCmd
        NextUndo.UDCoordIndex = lstCommands.SelCount
        NextUndo.UDCmd = "Command"
        // copy data from picedit to undo array
        lngCount = lstCommands.Items[SelectedCmd + 1].Tag - NextUndo.UDPicPos
        ReDim bytData(lngCount - 1)
        for (int i = 0; i < lngCount; i++) {
          bytData(i) = EditPicture.Resource.Data(i + NextUndo.UDPicPos)
        }
        NextUndo.UDData = bytData
        // add to undo
        AddUndo NextUndo
      }
      
      // delete data from array
// ''      EditPicture.Resource.RemoveData NextUndo.UDPicPos, lstCommands.Items[SelectedCmd + 1].Tag - NextUndo.UDPicPos
      EditPicture.Resource.RemoveData lngStartPos, lstCommands.Items[SelectedCmd + 1].Tag - lngStartPos
      
      // delete the command box entries
      for (int i = SelectedCmd; i >= SelectedCmd - lstCommands.SelCount + 1; i--) {
        lstCommands.RemoveItem i
      }
      
      // update remaining items (i+1 points to the cmd where updating needs to start)
      UpdatePosValues i + 1, -lngCount
      
      // select the cmd that is just after the deleted items
      SelectCmd i + 1, false
      
    } else {
      // if command being deleted is a 'set pen' command
      if (Left$(lstCommands.Text, 3) = "Set") {
        // determine new pen style to apply from this point
        i = SelectedCmd - 1
        while (i >= 0) {
          // if this command is a set command
          if (Left$(lstCommands.List(i), 3) = "Set") {
            // get pen status
            NewStyle = IIf(InStr(lstCommands.List(i), "Solid"), psSolid, psSplatter)
            break; // exit do
          }
          // get previous cmd
          i = i - 1
        }
        if (NewStyle != CurrentPen.PlotStyle) {
          // adjust plot pattern starting with next command after the one being deleted
          ReadjustPlotCoordinates SelectedCmd + 1, NewStyle
        }
      }
      
      // delete command
      DeleteCommand SelectedCmd
    }
  } else {
    // fill, plot and absolute lines allow deleting of individual coordinates
    // only last coordinate of other commands can be deleted
    switch (lstCommands.Text) {
    case "Abs Line" or "Fill" or "Plot":
      DeleteCoordinate lstCoords.ListIndex
      if (lstCoords.ListCount != 0) {
        lstCoords_Click
      }
      
    default:
      if (lstCoords.ListIndex = lstCoords.ListCount - 1) {
        DeleteCoordinate lstCoords.ListIndex
        if (lstCoords.ListCount != 0) {
          //  force the click event to select current coord
          lstCoords_Click
        }
      }
    }
  }
}

public void MenuClickSave() {
  // save this picture
  
  Dim blnLoaded As Boolean
  
  // if in a game,
  if (InGame) {
    // show wait cursor
    WaitCursor
    
    // get current load status
    blnLoaded = Pictures(PicNumber).Loaded
    
    // copy view back to game resource
    Pictures(PicNumber).SetPicture EditPicture
    
    // save the picture using save method
    Pictures(PicNumber).Save
    
    // copy back into edit object
    EditPicture.SetPicture Pictures(PicNumber)
    
    // setpicture copies load status to ingame pic resource; may need to unload it
    if (!blnLoaded) {
      Pictures(PicNumber).Unload
    }
    
    // update preview
    UpdateSelection rtPicture, PicNumber, umPreview
  
    // if autoexporting,
    if (Settings.AutoExport) {
      // export using default name
      EditPicture.Export ResDir & EditPicture.ID & ".agp"
      // reset ID (cuz
      EditPicture.ID = Pictures(EditPicture.Number).ID
    }
    
    // if no more errors, clear entry from warning grid
    if (EditPicture.BMPErrLevel = 0) {
      frmMDIMain.ClearWarningList EditPicture.Number, rtPicture
    }
    
    // restore cursor
    Screen.MousePointer = vbDefault
  } else {
    // if no name yet,
    if (LenB(EditPicture.Resource.ResFile) = 0) {
      // use export to get a name
      MenuClickExport
      return;
    } else {
      // show wait cursor
      WaitCursor
      
      // save the picture
      EditPicture.Export EditPicture.Resource.ResFile
    
      // restore cursor
      Screen.MousePointer = vbDefault
    }
  }
  
  // reset dirty flag
  IsChanged = false
  // reset caption
  Caption = sPICED & ResourceName(EditPicture, InGame, true)
  // disable save menu/button
  frmMDIMain.mnuRSave.Enabled = false
  frmMDIMain.Toolbar1.Buttons("save").Enabled = false
}

public void MenuClickImport() {
  
  Dim tmpPic As AGIPicture
  Dim i As Long
  
  // this method is only called by the Main form's Import function
  // the MainDialog object will contain the name of the file
  // being imported.
  
  // steps to import are to import the picture to tmp object
  // clear the existing Image, copy tmpobject to this item
  // and reset it
  
  Set tmpPic = New AGIPicture
  tmpPic.Import MainDialog.FileName
  if (Err.Number != 0) {
    ErrMsgBox "An error occurred while importing this picture:", "", "Import Picture Error"
    Set tmpPic = Nothing
    return;
  }
      // now check to see if it's a valid picture resource (by trying to reload it)
  tmpPic.Load
  if (Err.Number != 0) {
    ErrMsgBox "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource"
    Set tmpPic = Nothing
    return;
  }
  
  // clear drawing surfaces
  picVisual.Cls
  picPriority.Cls
  
  // clear picture
  EditPicture.Clear
  // copy tmppicture data to picedit
  EditPicture.Resource.InsertData tmpPic.Resource.AllData, 0
  // remove the last byte (it is left over from the insert process)
  EditPicture.Resource.RemoveData EditPicture.Resource.Size - 1
  
  // discard the temp pic
  tmpPic.Unload
  Set tmpPic = Nothing
  
  // redraw tree
  LoadCmdList
  
  // select the end
  SelectCmd lstCommands.ListCount - 1, false
  
  // refresh palette
  picPalette.Refresh
  
  // clear the undo buffer
  if (UndoCol.Count > 0) {
    for (int i = UndoCol.Count; i >= 1; i--) {
      UndoCol.Remove i
    }
    SetEditMenu
  }
  
  // mark as dirty
  MarkAsChanged
}

public void MenuClickECustom1() {
  
  // in edit mode, split commands
  // in view test mode, choose test view
  // in print test view, swap screen size if powerpack is active

  switch (PicMode) {
  case pmEdit:
    // split cmd
    SplitCommand lstCoords.ListIndex
  
  case pmViewTest:
    // get a test view
    GetTestView
    
    // redraw
    DrawPicture
    
  case pmPrintTest:
    // show print options dialog
    Load frmPicPrintPrev
      frmPicPrintPrev.SetForm TextMode, MaxCol
      frmPicPrintPrev.txtCol.Text = MsgLeft
      frmPicPrintPrev.txtRow.Text = MsgTop
      frmPicPrintPrev.txtMW.Text = MsgMaxW
      frmPicPrintPrev.cmbBG.ListIndex = MsgBG
      frmPicPrintPrev.cmbFG.ListIndex = MsgFG
      frmPicPrintPrev.txtMessage.Text = MsgText
      frmPicPrintPrev.Show vbModal
    
    if (!frmPicPrintPrev.Canceled) {
      // display the preview text
      ShowTextPreview
    }
    Unload frmPicPrintPrev
  
  }
}

public void MenuClickECustom2() {
  
  // in edit mode, join commands
  // in test mode, set test options
    
  switch (PicMode) {
  case pmEdit:
    // join commands
    JoinCommands SelectedCmd
    
  case pmViewTest:
    // if in test mode, and in motion
    if (TestDir != odStopped) {
      // stop motion and stop cycling
      TestDir = odStopped
    }
    
    // if cycling, stop; doesn't matter if
    // at rest or in motion
    tmrTest.Enabled = false
    
    // if testview not loaded,
    if (TestView == null) {
      // load one first
      MenuClickECustom1
      // if still no testview
      if (TestView == null) {
        // exit
        return;
      }
    }
  
    Load frmPicTestOptions
      // set global testpic settings
      Settings.PicTest = TestSettings
      // set form properties
      frmPicTestOptions.SetOptions TestView
      
      // show options form
      frmPicTestOptions.Show vbModal, frmMDIMain
      
      // if not canceled
      if (!frmPicTestOptions.Canceled) {
        // retreive option values
        TestSettings = Settings.PicTest
        // if test loop and/or cel are NOT auto, force current loop/cel
        if (TestSettings.TestLoop != -1) {
          CurTestLoop = TestSettings.TestLoop
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          // just in case, check current cel; if it exceeds
          // loop count, reset it to zero
          if (CurTestCel > CurTestLoopCount - 1) {
            CurTestCel = CurTestLoopCount - 1
          }
          if (TestSettings.TestCel != -1) {
            CurTestCel = TestSettings.TestCel
          }
          // if either loop or cel is forced, we
          // have to update cel data
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        }
        
        // update cel height/width/transcolor
        CelWidth = TestView.Loops(CurTestLoop).Cels(CurTestCel).Width
        CelHeight = TestView.Loops(CurTestLoop).Cels(CurTestCel).Height
        CelTrans = TestView.Loops(CurTestLoop).Cels(CurTestCel).TransColor
        
        // set timer based on speed
        switch (TestSettings.ObjSpeed) {
        case 0:
            // slow
          tmrTest.Interval = 200
        case 1:
            // normal
          tmrTest.Interval = 50
        case 2:
            // fast
          tmrTest.Interval = 13
        case 3:
            // fastest
          tmrTest.Interval = 1
        }
      }
      
      // redraw cel at current position
      DrawPicture
      
      // now enable cycling
      tmrTest.Enabled = TestSettings.CycleAtRest
    
    // unload options form
    Unload frmPicTestOptions
    
  case pmPrintTest:
    // toggle width (if powerpack is enabled)
    // always confirm
    if (MsgBox("Change screen size to " & IIf(MaxCol = 39, "80", "40") & " columns?", vbQuestion + vbYesNo, "Toggle Screen Size") = vbYes) {
      if (MaxCol = 39) {
        MaxCol = 79
        CharWidth = 4
      } else {
        MaxCol = 39
        CharWidth = 8
      }
      if (ShowTextMarks) {
        // redraw
        DrawPicture
      }
    }
  }
}

public void MenuClickCustom1() {

  // show bkgd configuration options
  ToggleBkgd true, true
}

public void MenuClickCustom3(bool Toggle = false) {
  
  // remove the background
  
  // if currently showing background,
  if (EditPicture.BkgdShow) {
    ToggleBkgd false
  }
  
  // set image to nothing
  Set BkgdImage = Nothing
  EditPicture.BkgdImgFile = ""
  
  // update menus
  frmMDIMain.mnuRCustom2.Visible = false
  frmMDIMain.mnuRCustom3.Visible = false
  
  // if in game
  if (InGame) {
    // update the ingame pic
      Pictures(EditPicture.Number).BkgdImgFile = ""
      Pictures(EditPicture.Number).BkgdShow = false
      Pictures(EditPicture.Number).BkgdPosition = ""
      Pictures(EditPicture.Number).BkgdSize = ""
      Pictures(EditPicture.Number).BkgdTrans = 0
      //  save it (this will only write the properties
      //  since the real picture is not being edited
      //  in this piceditor)
      Pictures(EditPicture.Number).Save
  }
}

private void DrawLine(int X1, int Y1, int X2, int Y2) {
  
  Dim xPos As Long, yPos As Long
  Dim DY As Long, DX As Long
  Dim vDir As Long, hDir As Long
  Dim XC As Long, YC As Long, MaxDelta As Long
  Dim i As Long
  
  // determine height/width
  DY = Y2 - Y1
  vDir = Sgn(DY)
  DX = X2 - X1
  hDir = Sgn(DX)
  
  // if a point, vertical line, or horizontal line
  // (it's easier to combine all these options, since
  // graphics methods are easier to work with than
  // buffer memory)
  if (DY = 0 || DX = 0) {
    // convert line to top-bottom/left-right format so graphics methods
    // work correctly
    if (Y2 < Y1) {
      // swap
      yPos = Y1
      Y1 = Y2
      Y2 = yPos
    }
    if (X2 < X1) {
      xPos = X1
      X1 = X2
      X2 = xPos
    }

    if (SelectedPen.VisColor < agNone) {
      picVisual.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-((X2 + 1) * ScaleFactor * 2 - 1, (Y2 + 1) * ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
    }
    if (SelectedPen.PriColor < agNone) {
      picPriority.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-((X2 + 1) * ScaleFactor * 2 - 1, (Y2 + 1) * ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
    }
  
  } else {
    // this line drawing function EXACTLY matches the Sierra
    // drawing function
    
    // set the starting point
    if (SelectedPen.VisColor < agNone) {
      picVisual.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
    }
    if (SelectedPen.PriColor < agNone) {
      picPriority.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
    }
    xPos = X1
    yPos = Y1
  
    // invert DX and DY if they are negative
    if (DY < 0) {
      DY = DY * -1
    }
    if ((DX < 0)) {
      DX = DX * -1
    }
  
    // set up the loop, depending on which direction is largest
    if (DX >= DY) {
      MaxDelta = DX
      YC = DX \ 2
      XC = 0
    } else {
      MaxDelta = DY
      XC = DY \ 2
      YC = 0
    }
  
    for (int i = 1; i <= MaxDelta; i++) {
      YC = YC + DY
      if (YC >= MaxDelta) {
        YC = YC - MaxDelta
        yPos = yPos + vDir
      }
    
      XC = XC + DX
      if (XC >= MaxDelta) {
        XC = XC - MaxDelta
        xPos = xPos + hDir
      }
  
      if (SelectedPen.VisColor < agNone) {
        picVisual.Line (xPos * ScaleFactor * 2, yPos * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
      }
      if (SelectedPen.PriColor < agNone) {
        picPriority.Line (xPos * ScaleFactor * 2, yPos * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
      }
    }
  }
}

private void DrawCircle(int StartX, int StartY, int EndX, int EndY) {
  
  // draws a circle/ellipse that is bounded by start/end points
  
  Dim DX As Long, DY As Long
  Dim a As Double, b As Double
  Dim a2b2 As Double
  Dim pX As Long, pY As Long
  Dim i As Long, j As Long, k As Long
  Dim DelPt As Boolean
  Dim cy As Double, cx As Double
  Dim s1 As Double, s2 As Double
  
  // ensure we are in a upperleft-lower right configuration
  if (StartX > EndX) {
    i = StartX
    StartX = EndX
    EndX = i
  }
  if (StartY > EndY) {
    i = StartY
    StartY = EndY
    EndY = i
  }
  
  DX = EndX - StartX
  DY = EndY - StartY
  
  if (DX = 0 || DY = 0) {
    // just draw a line
    DrawLine StartX, StartY, EndX, EndY
    
  } else if (DX = 1 || DY = 1) {
    // draw a simple box
    DrawLine StartX, StartY, EndX, StartY
    DrawLine EndX, StartY, EndX, EndY
    DrawLine EndX, EndY, StartX, EndY
    DrawLine StartX, EndY, StartX, StartY
    
    // set segment data
    Segments = 1
    ArcPt(0).X = Int(DX / 2)
    ArcPt(0).Y = 0
    ArcPt(1).X = 0
    ArcPt(1).Y = Int(DY / 2)
  } else {
    // get ellipse parameters
    a = Int(DX / 2)
    b = Int(DY / 2)
    a2b2 = a ^ 2 / b ^ 2
    
    // start with Y values;
    // increment until slope is >=1
    i = 0
    do {
      ArcPt(i).Y = i
      // calculate x Value for this Y
      cx = a * Sqr(1 - ArcPt(i).Y ^ 2 / b ^ 2)
      // round it (0.3 is an empirical Value that seems to
      // result in more accurate circles)
      ArcPt(i).X = vCint(cx - 0.3)
      
      // if past limit
      if (i / cx * a2b2 >= 1) {
        break; // exit do
      }
      // increment Y
      i = i + 1
      // continue until last point reached
      // (necessary in case tall skinny oval
      // is drawn; slope won't reach 1 before last point)
    } while (i < b);
    
    // start with last x
    j = ArcPt(i - 1).X
    // now, decrement x until we get to zero
    do {
      ArcPt(i).X = j
      // calculate Y Value for this x
      cy = b * Sqr(1 - (ArcPt(i).X) ^ 2 / a ^ 2)
      // round it
      // (vCint doesn't work quite right; Int seems to work better
      // ArcPt(i).Y = Int(cY)
      
      // using vCint with a modifier seems to work ok?
      ArcPt(i).Y = vCint(cy - 0.3)

      // decrement x, increment counter
      j = j - 1
      i = i + 1
    } while (j >= 0);
    
    // segments is equal to i-1
    // **NOTE that Segments is equal to UPPER BOUND of array; not total number
    // of segment points
    // e.g., 3 segments means upper bound of array is 3 (0,1,2,3) and total
    // number of points is 4
    Segments = i - 1
    
    // zero out next point to avoid conflict
    ArcPt(i + 1).X = 0
    ArcPt(i + 1).Y = 0
    
    // strip out any zero delta points
    // and any points that are on exact 45 line
    i = 1
    do {
      // if same
      if (ArcPt(i).X = ArcPt(i - 1).X && ArcPt(i).Y = ArcPt(i - 1).Y) {
        DelPt = true
      // if horizontal line
      } else if (ArcPt(i).X = ArcPt(i - 1).X && ArcPt(i).X = ArcPt(i + 1).X) {
        DelPt = true
      // if vertical line
      } else if (ArcPt(i).Y = ArcPt(i - 1).Y && ArcPt(i).Y = ArcPt(i + 1).Y) {
        DelPt = true
      // if line has a slope of 1
      } else if ((CLng(ArcPt(i).X) - ArcPt(i - 1).X = CLng(ArcPt(i - 1).Y) - ArcPt(i).Y) && (CLng(ArcPt(i + 1).X) - ArcPt(i).X = CLng(ArcPt(i).Y) - ArcPt(i + 1).Y)) {
        DelPt = true
      } else {
        DelPt = false
      }
      if (DelPt) {
        // move all segments down one space
        for (int k = i + 1; k <= Segments; k++) {
          ArcPt(k - 1) = ArcPt(k)
        }
        ArcPt(Segments).X = 0
        ArcPt(Segments).Y = 0
        Segments = Segments - 1
        i = i - 1
      }
      i = i + 1
    } while (i < Segments);
    
    // if more than one segment
    if (Segments > 1) {
      // strip out any points that create uneven slopes
      i = 1
      do {
        if (ArcPt(i - 1).X = ArcPt(i).X) {
          s1 = -160
        } else {
          s1 = (CLng(ArcPt(i - 1).Y) - ArcPt(i).Y) / (CLng(ArcPt(i - 1).X) - ArcPt(i).X)
        }
        if (ArcPt(i).X = ArcPt(i + 1).X) {
          s2 = -160
        } else {
          s2 = (CLng(ArcPt(i).Y) - ArcPt(i + 1).Y) / (CLng(ArcPt(i).X) - ArcPt(i + 1).X)
        }
        if (s1 >= s2 || ArcPt(i).X < ArcPt(i + 1).X) {
          // remove point (move all segments down one space)
          for (int k = i + 1; k <= Segments; k++) {
            ArcPt(k - 1) = ArcPt(k)
          }
          ArcPt(Segments).X = 0
          ArcPt(Segments).Y = 0
          Segments = Segments - 1
          // back up to recheck slope of altered segment
          i = i - 1
        }
        i = i + 1
      } while (i < Segments);
    }
    
    // now draw the arc segments
    pX = StartX
    pY = StartY + ArcPt(Segments).Y
    for (int i = 1; i <= Segments; i++) {
      DrawLine pX, pY, StartX + ArcPt(0).X - ArcPt(i).X, StartY + ArcPt(Segments).Y - ArcPt(i).Y
      pX = StartX + ArcPt(0).X - ArcPt(i).X
      pY = StartY + ArcPt(Segments).Y - ArcPt(i).Y
    }
    for (int i = Segments; i >=0; i--) {
      DrawLine EndX - ArcPt(0).X + ArcPt(i).X, StartY + ArcPt(Segments).Y - ArcPt(i).Y, pX, pY
      pX = EndX - ArcPt(0).X + ArcPt(i).X
      pY = StartY + ArcPt(Segments).Y - ArcPt(i).Y
    }
    for (int i = 0; i <= Segments; i++) {
      DrawLine pX, pY, EndX - ArcPt(0).X + ArcPt(i).X, EndY - ArcPt(Segments).Y + ArcPt(i).Y
      pX = EndX - ArcPt(0).X + ArcPt(i).X
      pY = EndY - ArcPt(Segments).Y + ArcPt(i).Y
    }
    for (int i = Segments; i >= 0; i--) {
      DrawLine StartX + ArcPt(0).X - ArcPt(i).X, EndY - ArcPt(Segments).Y + ArcPt(i).Y, pX, pY
      pX = StartX + ArcPt(0).X - ArcPt(i).X
      pY = EndY - ArcPt(Segments).Y + ArcPt(i).Y
    }
  }
}

private void Form_Activate() {

  // if minimized, exit
  // (to deal with occasional glitch causing focus to lock up)
  if (Me.WindowState = vbMinimized) {
    return;
  }
  
  ActivateActions
  
  // if visible,
  if (Visible) {
    // force resize
    Form_Resize
  }
}

private void Form_KeyDown() {

  Dim tmpStatusMode As Long
  
  // if testing text, only allow enter or escape
  //  or ALT/CTRL+ALT combos
  if (PicMode = pmPrintTest) {
    if (KeyCode = vbKeyEscape || KeyCode = vbKeyReturn) {
      // dismiss message by redrawing picture
      DrawPicture
      KeyCode = 0
      Shift = 0
      return;
    }
    if (Shift != vbAltMask && Shift != vbAltMask + vbCtrlMask) {
      KeyCode = 0
      Shift = 0
      return;
    }
  }
  
  // always check for help first
  if (Shift = 0 && KeyCode = vbKeyF1) {
    MenuClickHelp
    KeyCode = 0
    return;
  }
  
 // check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  if (KeyCode = 0) {
    return;
  }
  
  switch (Shift) {
  case vbCtrlMask:
    switch (KeyCode) {
    case vbKeyZ:
      // undo
      if (frmMDIMain.mnuEUndo.Enabled) {
        MenuClickUndo
      }
    
    case vbKeyX:
      if (frmMDIMain.mnuECut.Enabled) {
        MenuClickCut
        KeyCode = 0
      }
    
    case vbKeyC:
      if (frmMDIMain.mnuECopy.Enabled) {
        MenuClickCopy
        KeyCode = 0
      }
      
    case vbKeyV:
        MenuClickPaste
        KeyCode = 0
    
    case vbKeyA:
            // select all
      if (frmMDIMain.mnuESelectAll.Enabled) {
        MenuClickSelectAll
        KeyCode = 0
      }
    }
    
  case 0:
    // no shift, ctrl, alt
    switch (KeyCode) {
    case vbKeyEscape:
      // if a coord is selected, unselect it
      if (lstCoords.ListIndex != -1) {
        CodeClick = true
        lstCommands_Click
      }
    
    case vbKeyDelete:
      if (frmMDIMain.mnuEDelete.Enabled) {
        // same as menuclickdelete
        MenuClickDelete
      }
      
    case vbKeyUp or vbKeyLeft:
      if (PicMode = pmEdit) {
        // if a coord is selected
        if (lstCoords.ListIndex != -1) {
          // if not on first coord,
          if (lstCoords.ListIndex > 0) {
            // move up one coord pt
            lstCoords.ListIndex = lstCoords.ListIndex - 1
          }
        } else {
          // if not on first cmd
          if (SelectedCmd > 0) {
            // move up one cmd
            SelectCmd SelectedCmd - 1, false
          }
        }
        // reset keycode to prevent double movement of cursor
        KeyCode = 0
      }
      
    case vbKeyDown or vbKeyRight:
      if (PicMode = pmEdit) {
        // if a coord is selected
        if (lstCoords.ListIndex != -1) {
          // if not on last coord,
          if (lstCoords.ListIndex < lstCoords.ListCount - 1) {
            // move down one coord pt
            lstCoords.ListIndex = lstCoords.ListIndex + 1
          }
        } else {
          // if not on last cmd
          if (SelectedCmd != lstCommands.ListCount - 1) {
            // move down one cmd
            SelectCmd SelectedCmd + 1, false
          }
        }
        // reset keycode to prevent double-movement of cursor
        KeyCode = 0
      }
    
    case vbKeySpace:
      // toggle status bar coordinate display -
      //  normal -- text row/col in draw mode
      //  normal -- test coords in test mode
      
      // toggle status bar display if in test mode
      if (PicMode = pmViewTest) {
        // toggle status bar source
        if (StatusMode = psPixel) {
          tmpStatusMode = psCoord // to test coords
        } else {
          tmpStatusMode = psPixel // to normal
        }
      } else {
        // only if text markers are visible
        if (ShowTextMarks) {
          if (StatusMode = psPixel) {
            tmpStatusMode = psText // to text row/col
          } else {
            tmpStatusMode = psPixel // to normal
          }
        } else {
          tmpStatusMode = StatusMode
        }
      }
      
      if (tmpStatusMode != StatusMode) {
        StatusMode = tmpStatusMode
        // force update of statusbar
          switch (StatusMode) {
          case psPixel:
            //  normal
            // use cusor position
            spCurX").Text = "X: " & CStr(PicPt.X)
            spCurY").Text = "Y: " & CStr(PicPt.Y)
            spPriBand").Text = "Band: "
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 0, 0
            spPriBand").Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, EditPicture.PriBase) - 3).Picture
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 1, 0
          case psCoord:
            //  test coords
            // use test object position
            spCurX").Text = "vX: " & CStr(TestCelPos.X)
            spCurY").Text = "vY: " & CStr(TestCelPos.Y)
            spPriBand").Text = "vBand: " & GetPriBand(TestCelPos.Y, EditPicture.PriBase)
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 0, 0
            spPriBand").Picture = imlPriBand.ListImages(GetPriBand(TestCelPos.Y, EditPicture.PriBase) - 3).Picture
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 1, 0
          case psText:
            //  text row/col (note row/col swap 'x/y')
            spCurX").Text = "R: " & CStr(Int(PicPt.Y / 8))
            spCurY").Text = "C: " & CStr(Int(PicPt.X / (CharWidth / 2)))
            spPriBand").Text = "Band: "
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 0, 0
            spPriBand").Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, EditPicture.PriBase) - 3).Picture
            SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 1, 0
          }
      }
    }
    
    // if in test mode
    if (PicMode = pmViewTest) {
      ChangeDir KeyCode
    }
  
  case vbShiftMask:
    switch (KeyCode) {
    case vbKeyDelete:
      if (frmMDIMain.mnuEClear.Enabled) {
        MenuClickClear
        KeyCode = 0
      }
    
    case vbKeyInsert:
      if (frmMDIMain.mnuEInsert.Enabled) {
        MenuClickInsert
        KeyCode = 0
      }
    }
    
  case vbAltMask:
    switch (KeyCode) {
    case vbKeyB:
      // toggle background
      if (frmMDIMain.mnuRCustom3.Enabled) {
        ToggleBkgd !EditPicture.BkgdShow
        KeyCode = 0
      }
    
    case vbKeyD:
      // display text test options
      if (PicMode = pmPrintTest) {
        GetTextOptions
        KeyCode = 0
      }
      
    case vbKeyM:
      // toggle mode
      MenuClickReplace
      
    case vbKeyS:
      // toggle visual/priority surfaces if
      // in single pane mode
      if (frmMDIMain.mnuERedo.Visible) {
        MenuClickRedo
      }
      
    case vbKeyP:
      // toggle priority bands
      if (frmMDIMain.mnuEFind.Enabled) {
        MenuClickFind
        KeyCode = 0
      }
    
    case vbKeyT:
      // toggle text marks
      if (frmMDIMain.mnuEReplace.Enabled) {
        MenuClickReplace
        KeyCode = 0
      }
    
    case vbKeyV:
      // in test mode only, choose a test view
      if (frmMDIMain.mnuECustom1.Enabled && PicMode = pmViewTest) {
        MenuClickECustom1
        KeyCode = 0
      }
    
    case vbKeyO:
      // in test mode only display test view options
      if (frmMDIMain.mnuECustom2.Enabled && PicMode = pmViewTest) {
        MenuClickECustom2
        KeyCode = 0
      }
      
    case vbKeyW:
      // toggle screen width
      if (PicMode = pmPrintTest) {
        MenuClickECustom2
        KeyCode = 0
      }
    }
    
  case vbShiftMask + vbCtrlMask:
    switch (KeyCode) {
    case vbKeyT:
            // split = custom1
      if (frmMDIMain.mnuECustom1.Enabled && PicMode = pmEdit) {
        MenuClickECustom1
        KeyCode = 0
      }
    
    case vbKeyJ:
            // join = custom2
      if (frmMDIMain.mnuECustom2.Enabled && PicMode = pmEdit) {
        MenuClickECustom2
        KeyCode = 0
      }
    
    case vbKeyS:
      // save Image as
      MenuClickCustom1
    
    case vbKeyG:
      // export as gif
      ExportPicAsGif
    }
  
  case vbAltMask + vbShiftMask:
    switch (KeyCode) {
    case vbKeyB:
      if (frmMDIMain.mnuRCustom3.Enabled) {
        // remove bkgd image
        MenuClickCustom3
        KeyCode = 0
      }
    }
  
  case vbCtrlMask + vbAltMask:
    switch (KeyCode) {
    case vbKeyB:
      // show background options dialog
      ToggleBkgd true, true
  
    case vbKeyC:
      // copy commands to clipboard as text
      if (PicMode = pmEdit && lstCommands.SelCount >= 1) {
        MenuClickECustom4
      }

    case vbKeyP:
      // adjust priority base
      if (frmMDIMain.mnuEFindAgain.Enabled) {
        MenuClickFindAgain
        KeyCode = 0
      }
    }
  }
  KeyCode = 0
  Shift = 0
  
}

private void lstCommands_Click() {

}

private void lstCommands_DblClick() {

  // make sure only one item is selected
  if (lstCommands.SelCount = 1) {
    GetSelectionBounds lstCommands.ListIndex, 1, true
  }
}

private void lstCommands_KeyDown() {

  // block all others
  if (Shift != vbShiftMask && KeyCode != vbKeyDown && KeyCode != vbKeyUp) {
    // ignore keycode
    KeyCode = 0
    Shift = 0
  }
}


private void lstCommands_KeyPress() {

  // need to ignore key presses to stop the auto-selection feature of the list box
  KeyAscii = 0
}

private void lstCommands_KeyUp() {

  // ignore keycode
  KeyCode = 0
  Shift = 0
}


private void lstCommands_MouseDown() {

  Dim lngClickRow As Long
  
  // if nothing selected
  if (lstCommands.SelCount = 0) {
    return;
  }
  
  switch (Button) {
  case vbRightButton:
  // if right button
    lngClickRow = (Y / ScreenTWIPSY) \ SendMessage(lstCommands.hWnd, LB_GETITEMHEIGHT, 0, 0) + lstCommands.TopIndex
    if (lngClickRow > lstCommands.ListCount - 1) {
      lngClickRow = lstCommands.ListCount - 1
    }
    
    // if on a cmd that is NOT selected
    if (!lstCommands.Selected(lngClickRow)) {
      // select it
      SelectCmd lngClickRow, false
    }
    // reset edit menu first
    SetEditMenu
    // make sure this form is the active form
    if (!(frmMDIMain.ActiveForm Is Me)) {
      // set focus before showing the menu
      Me.SetFocus
    }
    // need doevents so form activation occurs BEFORE popup
    // otherwise, errors will be generated because of menu
    // adjustments that are made in the form_activate event
    SafeDoEvents
    // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , lstCommands.Left + X / ScreenTWIPSX, lstCommands.Top + Y / ScreenTWIPSY
  case vbLeftButton:
    // ignore ctrl; should not allow selecting commands that are not adjacent
    if (Shift = vbCtrlMask) {
      Shift = 0
    }
  }
}

private void lstCoords_Click() {

  Dim i As Long
  
  if (NoSelect) {
    NoSelect = false
    return;
  }
  
  if (lstCoords.ListIndex = -1) {
    return;
  }
  
  // always cancel any drawing operation
  PicDrawMode = doNone
  
  // set selection to nothing
  // (this hides any selected graphics)
  Selection.X = 0
  Selection.Y = 0
  Selection.Width = 0
  Selection.Height = 0
  ShowCmdSelection
  
  // set CurCmdIsLine to false until proven otherwise
  CurCmdIsLine = false
  
  // if NOT in select mode (i.e. selected tool = none)
  if (SelectedTool != ttEdit) {
    // change edit mode by clicking toolbar
    // (this will reset ListIndex to -1, so need to hold on to it, and restore after this call)
    i = lstCoords.ListIndex
    NoSelect = true
    Toolbar1.Buttons("select").Value = tbrPressed
    Toolbar1_ButtonClick Toolbar1.Buttons("select")
    NoSelect = true
    lstCoords.ListIndex = i
  }
  // *'Debug.Assert lstCoords.ListIndex != -1
      
  // current command is a line if coordinate is NOT a plot or paint
  CurCmdIsLine = (lstCommands.Text != "Plot" && lstCommands.Text != "Fill")
  // extract position
  
  CurPt = ExtractCoordinates(lstCoords.Text)
  
  // enable cursor highlighting if edit tool selected
  tmrSelect.Enabled = (SelectedTool = ttEdit)
  
  // if original wingagi cursor mode AND timer is enabled,
  if (CursorMode = pcmWinAGI && tmrSelect.Enabled) {
    // save area under cursor
    BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
  }
  
  // get coordinate number
  EditCoordNum = lstCoords.ListIndex
  
  // draw picture
  DrawPicture
  
  // update tool status AFTER drawing
  CurrentPen = EditPicture.CurrentToolStatus
  
  // set selected tools to match current
  SelectedPen = CurrentPen
  
  // if on a line command coordinate
  if (CurCmdIsLine) {
    // draw temp line based on current node
    DrawTempLine false, 0, 0
  }
  
  // highlight the coords
  // (always, since coordinates are only enabled in edit mode)
  HighlightCoords
  
  // update toolbar
  UpdateToolBar
  
  // reset edit menu
  SetEditMenu
}

private void lstCoords_DblClick() {

  // if dblclicking a non-relative coordinate, display
  // coordinate edit form so user can set values
  
  Dim bytNewPattern As Byte
  Dim bytOldCoord() As Byte
  Dim lngPos As Long, lngCoord As Long, rtn As Long
  Dim NextUndo As PictureUndo
  Dim ptEdit As Point, ptNew As Point
  Dim PlotPt As LCoord, PlotSz As LCoord, StartPt As LCoord
  Dim CmdType As DrawFunction, Splat As Boolean
  
  if (lstCoords.ListIndex = -1) {
    // just in case a coordinate is not actually selected,
    return;
  }
  
  // editable coords include Plot, Abs Line, X Corner, Y Corner, Fill
  switch (lstCommands.List(lstCommands.ListIndex)) {
  case "Plot":
    CmdType = dfPlotPen
    Splat = (CurrentPen.PlotStyle = psSplatter)
  case "Abs Line":
    CmdType = dfAbsLine
  case "X Corner":
    CmdType = dfXCorner
  case "Y Corner":
    CmdType = dfYCorner
  case "Fill":
    CmdType = dfFill
  case "Rel Line":
    // can't edit rel lines, because it would be too hard to enforce
    // distance limits
    MsgBoxEx "Relative Line coordinates cannot be manually edited because of" & vbNewLine & "the need to enforce distance limits.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Edit Coordinates", WinAGIHelp, "htm\agi\pictures.htm#f7"
    return;
    
  default:
    // just exit
    return;
  }
  
  // show the plot edit form
  Load frmPlotEdit
  // default position is center of main form; we could keep track of its position
  //  as the user moves it around, but it's probably OK to always go back to center

  ptEdit = ExtractCoordinates(lstCoords.Text)
  // save the coordinate index for later undo
  lngCoord = lstCoords.ListIndex
  // configure coordinate edit form based on type, and pass coordinates
  frmPlotEdit.FormSetup CmdType, ptEdit.X, ptEdit.Y, Splat
  
  // default is 2 byte array for undo
  ReDim bytOldCoord(1)
  bytOldCoord(0) = CByte(ptEdit.X)
  bytOldCoord(1) = CByte(ptEdit.Y)
  
  // if editing a plot point, pass pen info
  if (lstCommands.List(lstCommands.ListIndex) = "Plot") {
    // pass current pen shape and size
    frmPlotEdit.PenShape = CurrentPen.PlotShape
    frmPlotEdit.PenSize = CurrentPen.PlotSize
  }
  
  // if editing a splatter, also pass pattern, color and background
  //  only splatter shows the coordinates in the coordinate editor
  if (Splat) {
    // need extra byte for the pattern
    ReDim bytOldCoord(2) // declare as three element array to facilitate undo action
    bytOldCoord(1) = CByte(ptEdit.X)
    bytOldCoord(2) = CByte(ptEdit.Y)
  
    // save old pattern and send it to the form
    bytOldCoord(0) = CByte(Val(lstCoords.Text))
    frmPlotEdit.OldPattern = bytOldCoord(0)
  
    // set up plot point and nominal size of background to copy
    PlotPt.X = ptEdit.X
    PlotPt.Y = ptEdit.Y
    PlotSz.X = 14
    PlotSz.Y = 27
    
    // need to adjust plot point to account for edge cases
    if (PlotPt.X < (CurrentPen.PlotSize + 1) \ 2) {
      PlotPt.X = (CurrentPen.PlotSize + 1) \ 2
    }
    // there is a bug in AGI that uses 160 as edge limit for
    // plotting, so we use same so pictures draw the same
// ''    if (PlotPt.X > 159 - CurrentPen.PlotSize \ 2) {
// ''      PlotPt.X = 159 - CurrentPen.PlotSize \ 2
// ''    }
    if (PlotPt.X > 160 - CurrentPen.PlotSize \ 2) {
      PlotPt.X = 160 - CurrentPen.PlotSize \ 2
      // need to let form know value was adjusted
      frmPlotEdit.AdjX = PlotPt.X
    }
    if (PlotPt.Y < CurrentPen.PlotSize) {
      PlotPt.Y = CurrentPen.PlotSize
    }
    if (PlotPt.Y > 167 - CurrentPen.PlotSize) {
      PlotPt.Y = 167 - CurrentPen.PlotSize
    }
      
    // now convert plot point into desired upper-right corner
    // coordinate to pass to the editing window
    PlotPt.X = PlotPt.X - 7
    PlotPt.Y = PlotPt.Y - 13
    StartPt.X = 0
    StartPt.Y = 0
    if (PlotPt.X < 0) {
      PlotSz.X = PlotSz.X + PlotPt.X
      StartPt.X = -PlotPt.X
      PlotPt.X = 0
    }
    if (PlotPt.Y < 0) {
      PlotSz.Y = PlotSz.Y + PlotPt.Y
      StartPt.Y = -PlotPt.Y
      PlotPt.Y = 0
    }

    // adjust picture so it shows drawing up to this plot point
    EditPicture.DrawPos = EditPicture.DrawPos - 3
    // if only priority window is active, then use priority picture
    if (CurrentPen.VisColor = 16 && CurrentPen.PriColor < 16) {
      // copy from priority picture
      rtn = BitBlt(frmPlotEdit.picBackground.hDC, StartPt.X, StartPt.Y, PlotSz.X, PlotSz.Y, EditPicture.PriorityBMP, PlotPt.X, PlotPt.Y, SRCCOPY)
      frmPlotEdit.PenColor = EGAColor(CurrentPen.PriColor)
      frmPlotEdit.NoPen = false
    } else {
      // otherwise, use visual picture
      rtn = BitBlt(frmPlotEdit.picBackground.hDC, StartPt.X, StartPt.Y, PlotSz.X, PlotSz.Y, EditPicture.VisualBMP, PlotPt.X, PlotPt.Y, SRCCOPY)
      // make sure vispen is active
      if (CurrentPen.VisColor < 16) {
        frmPlotEdit.NoPen = false
        frmPlotEdit.PenColor = EGAColor(CurrentPen.VisColor)
      } else {
        // no pen active; nothing to display on coordinate editor
        frmPlotEdit.NoPen = true
      }
    }
  }
  
  // show the form as modal so user can't do anything else until done editing
  frmPlotEdit.Show vbModal, frmMDIMain
  
  // if splatter was being edited, need to reset picture back to original drawpos
  if (Splat) { EditPicture.DrawPos = EditPicture.DrawPos + 3
  
  // if user canceled the edit, then unload the form and exit
  if (frmPlotEdit.Canceled) {
    Unload frmPlotEdit
    return;
  }
  
  // get the new cursor point
  ptNew.X = CLng(frmPlotEdit.txtX.Text)
  ptNew.Y = CLng(frmPlotEdit.txtY.Text)
  
  // retrieve the new plot pattern Value
  bytNewPattern = frmPlotEdit.NewPattern
  // and unload the form
  Unload frmPlotEdit
  
  // if nothing changed, exit
  if (Splat) {
    if (ptNew.X = ptEdit.X && ptNew.Y = ptEdit.Y && bytNewPattern = bytOldCoord(0)) {
      return;
    }
  } else {
    if (ptNew.X = ptEdit.X && ptNew.Y = ptEdit.Y) {
      return;
    }
  }
  
  // now update the coord (and pattern, if a splatter)
  if (CmdType = dfPlotPen) {
    if (Splat) {
      // find position in the pic resource data for this coordinate
      lngPos = lstCommands.Items[lstCommands.ListIndex].Tag + 1 + 3 * lstCoords.ListIndex
      
      // change the plot pattern data
      EditPicture.Resource.Data(lngPos) = bytNewPattern * 2
      // update the coords
      EditPicture.Resource.Data(lngPos + 1) = ptNew.X
      EditPicture.Resource.Data(lngPos + 2) = ptNew.Y
      
    } else {
      // find position in the pic resource data for this coordinate
      lngPos = lstCommands.Items[lstCommands.ListIndex].Tag + 1 + 2 * lstCoords.ListIndex
      // *'Debug.Assert lngPos = lstCoords.Items[lngCoord].Tag
      // update the coords
      EditPicture.Resource.Data(lngPos) = ptNew.X
      EditPicture.Resource.Data(lngPos + 1) = ptNew.Y
    }
      
    // if not skipping undo
    if (Settings.PicUndo != 0) {
      // create new undo object
      Set NextUndo = New PictureUndo
        NextUndo.UDAction = EditPlotCoord
        NextUndo.UDPicPos = lngPos
        NextUndo.UDCmdIndex = lstCommands.ListIndex
        NextUndo.UDCoordIndex = lngCoord
        NextUndo.UDData = bytOldCoord // size tells us if there is a splat code or not
      // add to undo
      AddUndo NextUndo
    }
    
    // reset the text for the coordinate list
    if (Splat) {
      lstCoords.List(lstCoords.ListIndex) = CStr(bytNewPattern) & " -- " & CoordText(ptNew.X, ptNew.Y)
    } else {
      lstCoords.List(lstCoords.ListIndex) = CoordText(ptNew.X, ptNew.Y)
    }
    
  } else {
    lngPos = lstCoords.Items[lngCoord].Tag
    // use the regular endeditcoord function!
    EndEditCoord CmdType, lngCoord, ptNew, lngPos, lstCoords.Text
    // force update
    BuildCoordList lstCommands.ListIndex
    lstCoords.ListIndex = lngCoord
  
  }
  
  // update the coord list so hilites work
  CoordPT(lngCoord) = ptNew
  
  // use coord list click to refresh
  lstCoords_Click
}

private void lstCoords_KeyDown() {

  // ignore all keycodes
  KeyCode = 0
}

private void lstCoords_KeyPress() {
 
  // need to ignore key presses to stop the auto-selection feature of the list box
  KeyAscii = 0
}

private void lstCoords_KeyUp() {

  // ignore keycode
  KeyCode = 0
  Shift = 0
}

private void lstCoords_MouseDown() {
  
  Dim lngClickRow As Long
  
  // if nothing (a cmd with no coords is currently selected)
  if (lstCoords.ListCount = 0) {
    return;
  }
  
  // if right button
  if (Button = vbRightButton) {
  
    lngClickRow = (Y / ScreenTWIPSY) \ SendMessage(lstCoords.hWnd, LB_GETITEMHEIGHT, 0, 0) + lstCoords.TopIndex
    if (lngClickRow > lstCoords.ListCount - 1) {
      lngClickRow = lstCoords.ListCount - 1
    }
    
    // if on a different coord
    if (lngClickRow != lstCoords.ListIndex) {
      // select it
      lstCoords.ListIndex = lngClickRow
    }
    // reset edit menu first
    SetEditMenu
    // make sure this form is the active form
    if (!(frmMDIMain.ActiveForm Is Me)) {
      // set focus before showing the menu
      Me.SetFocus
    }
    // need doevents so form activation occurs BEFORE popup
    // otherwise, errors will be generated because of menu
    // adjustments that are made in the form_activate event
    SafeDoEvents
    // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , lstCoords.Left + X / ScreenTWIPSX, lstCoords.Top + Y / ScreenTWIPSY
  }
}

private void picPriSurface_MouseMove() {

  Dim tmpX As Single, tmpY As Single
  
  // if not active form
  if (!frmMDIMain.ActiveForm Is Me) {
    return;
  }
  
  // if dragging picture
  if (blnDragging) {
    // get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y
    
    // if vertical scrollbar is visible
    if (vsbPri.Visible) {
      // limit positions to valid values
      if (tmpY < vsbPri.Min) {
        tmpY = vsbPri.Min
      } else if (tmpY > vsbPri.Max) {
        tmpY = vsbPri.Max
      }
      // set vertical scrollbar
      vsbPri.Value = tmpY
    }
    
    // if horizontal scrollbar is visible
    if (hsbPri.Visible) {
      // limit positions to valid values
      if (tmpX < hsbPri.Min) {
        tmpX = hsbPri.Min
      } else if (tmpX > hsbPri.Max) {
        tmpX = hsbPri.Max
      }
      // set horizontal scrollbar
      hsbPri.Value = tmpX
    }
  }
}

private void picPriSurface_MouseUp() {

  Dim rtn As Long
  
  // if dragging
  if (blnDragging) {
    // cancel dragmode
    blnDragging = false
    // release mouse capture
    rtn = ReleaseCapture()
    SetCursors pcEdit
  }
}


private void Form_MouseDown() {
  
  // if right button
  if (Button = vbRightButton) {
    // reset edit menu first
    SetEditMenu
    // make sure this form is the active form
    if (!(frmMDIMain.ActiveForm Is Me)) {
      // set focus before showing the menu
      Me.SetFocus
    }
    // need doevents so form activation occurs BEFORE popup
    // otherwise, errors will be generated because of menu
    // adjustments that are made in the form_activate event
    SafeDoEvents
    // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , X, Y
  }
}

public void MenuClickCopy() {

  Dim rtn As Long, bytData() As Byte
  Dim i As Long, blnTrackPen As Boolean
  
  // if editing, tool is editselect, and a selection is visible
  // which means selwidth/height != 0)
  if ((PicMode = pmEdit) && (SelectedTool = ttSelectArea) && (Selection.Width != 0) && (Selection.Height != 0)) {
    // set copy picture height/width
    picCopy.Height = Selection.Height
    picCopy.Width = Selection.Width
    picCopy.Cls
    // if user specified priority AND priority is visible
    if (shpPri.Visible && blnInPri) {
      // copy from priority picture
      rtn = BitBlt(picCopy.hDC, 0, 0, Selection.Width, Selection.Height, EditPicture.PriorityBMP, Selection.X, Selection.Y, SRCCOPY)
    } else {
      // copy from visual picture
      rtn = BitBlt(picCopy.hDC, 0, 0, Selection.Width, Selection.Height, EditPicture.VisualBMP, Selection.X, Selection.Y, SRCCOPY)
    }
    // refresh the copied picture
    picCopy.Refresh
    // send to clipboard
    Clipboard.Clear
    Clipboard.SetData picCopy.Image, vbCFBitmap
    ViewCBMode = vmBitmap
    
    // clear PicClipBoardObj
    Set PicClipBoardObj = Nothing
    return;
  }
  
  // if one or more commands enabled
  if (lstCommands.SelCount >= 1) {
    // *'Debug.Assert lstCoords.ListIndex = -1
    // *'Debug.Assert lstCommands.ListIndex != -1
    // *'Debug.Assert lstCommands.ListIndex != lstCommands.ListCount - 1
    
    // if 'End' marker is selected, need to skip it when copying
    if (lstCommands.Selected(lstCommands.ListCount - 1)) {
      lstCommands.Selected(lstCommands.ListCount - 1) = false
    }
  
    Set PicClipBoardObj = New PictureUndo
    // get starting pt of resource data
    rtn = lstCommands.Items[SelectedCmd - lstCommands.SelCount + 1].Tag
    // allocate enough space for all cmd info
    ReDim bytData(lstCommands.Items[SelectedCmd + 1].Tag - rtn - 1)
    for (int i = 0; i < bytData.Length; i++) {
      bytData(i) = EditPicture.Resource.Data(rtn + i)
    }
    
    // if any plot cmds, need to determine pen status at beginning of selection:
    
    // note current pen style at selected position
    rtn = CurrentPen.PlotStyle
    
    // step through selected cmds (starting at bottom, going up)
    for (int i = 0; i < lstCommands.SelCount; i++) {
      if (lstCommands.List(SelectedCmd - i) = "Plot") {
        // need to track pen style
        blnTrackPen = true
        break; // exit for
      }
    }
    
    // if tracking is necessary
    if (blnTrackPen) {
      // assume no pen cmd (pen is solid)
      rtn = psSolid
      
      // back up from first selected cmd until a set pen is found (or beginning of cmd list)
      for (int i = SelectedCmd - lstCommands.SelCount; i >= 0; i--) {
        // if this is a set pen cmd
        if (Left$(lstCommands.List(SelectedCmd - 1), 7) = "Set Pen") {
          // readjust pen status
          if (InStr(1, lstCommands.List(SelectedCmd - 1), "Splatter") != 0) {
            rtn = psSplatter
          } else {
            rtn = psSolid
          }
          // exit; we now know what the pen status is for cmds in the copied cmd list
          break; // exit for
        }
      }
    }
    
    // for clipboard, only need to store the data, the number of cmds, and status of pen
    PicClipBoardObj.UDCoordIndex = lstCommands.SelCount
    PicClipBoardObj.UDData = bytData()
    if (blnTrackPen) {
      // increment penstyle, and save it
      // (incrementing allows 0 to mean no pen status
      // 1 to mean solid pen and 2 to mean splatter pen)
      rtn = rtn + 1
      PicClipBoardObj.UDCmdIndex = rtn
    }
    // if only one cmd, also save cmd text
    if (lstCommands.SelCount = 1) {
      PicClipBoardObj.UDCmd = lstCommands.List(SelectedCmd)
    }
    
    // update edit menu
    SetEditMenu
  }
}
public void MenuClickCut() {

  // first copy
  MenuClickCopy
  
  // then delete
  MenuClickDelete
  
  if (UndoCol.Count != 0) {
    // change last undo object so it reads as 'undo cut'
    UndoCol(UndoCol.Count).UDAction = CutCmds
    frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(PICUNDOTEXT + CutCmds)
    if (UndoCol(UndoCol.Count).UDCoordIndex > 1) {
      frmMDIMain.mnuEUndo.Text = frmMDIMain.mnuEUndo.Text & "s" & vbTab & "Ctrl+Z"
    } else {
      frmMDIMain.mnuEUndo.Text = frmMDIMain.mnuEUndo.Text & vbTab & "Ctrl+Z"
    }
  }
}

public void MenuClickPaste() {

  Dim NextUndo As PictureUndo
  Dim psOldStyle As PlotStyle, psNewStyle As PlotStyle
  Dim i As Long, InsertIndex As Long, InsertPos As Long
  
  // UDCoordIndex = # of cmds
  // UDData is dataset to add
  // UDCmd is text of cmd (only used if clipboard has a single cmd)
  // UDCmdIndex: 0 means no plot cmds
  //             1 means plots start with solid pen
  //             2 means plots start with splatter pen
  
  
  // always set cmd to select
  if (SelectedTool != ttEdit) {
    Toolbar1.Buttons("select").Value = tbrPressed
    Toolbar1_ButtonClick Toolbar1.Buttons("select")
  }
  
  // if more than one command selected
  if (lstCommands.SelCount > 1) {
    // paste after the last selected item
    InsertIndex = lstCommands.ListIndex + 1
  } else {
    // get current index position
    InsertIndex = SelectedCmd
  }
  
  // if only one cmd
  if (PicClipBoardObj.UDCoordIndex = 1) {
    // insert command, ALWAYS in front
    InsertCommand PicClipBoardObj.UDData, SelectedCmd, PicClipBoardObj.UDCmd, true
  
    // change last undo
    if (UndoCol.Count != 0) {
      UndoCol(UndoCol.Count).UDAction = PasteCmds
      UndoCol(UndoCol.Count).UDCoordIndex = PicClipBoardObj.UDCoordIndex
    }
  } else {
    // multiple commands
    
    // get current plot style
    psOldStyle = CurrentPen.PlotStyle
    
    // get current insert position
    InsertPos = lstCommands.Items[InsertIndex].Tag
    
    // insert the data
    EditPicture.Resource.InsertData PicClipBoardObj.UDData, InsertPos
    
    // rebuild cmd list (but no update)
    LoadCmdList true
    
    // now, check for any plot cmds in the pasted section and in the section following the
    // pasted cmds that need to be adjusted due to changes in pen style; normally this is
    // done BEFORE the resource is modified otherwise the undo feature won't work correctly;
    // however, since the paste method has to actually insert the commands and rebuild the
    // cmd list before the check for plot adjustments is complete, the adjustments are done
    // AFTER the cmds are pasted; the undo method will need to compensate by adjusting
    // the insert index Value so plot adjustments get restored properly; see undo method
    // for additional comments
    
    // if the pasted stuff includes pen information
    if (PicClipBoardObj.UDCmdIndex != 0) {
      // if plot styles DONT match (i.e. the pasted cmds expect a solid brush, but current brush is splatter
      // (or vice versa)) need to add/delete pattern info as appropriate
      // (remember that style info is actually cmdindex Value -1!)
      if (psOldStyle != PicClipBoardObj.UDCmdIndex - 1) {
        // update patterns for the cmds being pasted to match old style
        // (dont add undo info, though; if the paste operation is
        // undone, the adjustments made in the pasted commands becomes moot
        // since they are being deleted)
        ReadjustPlotCoordinates InsertIndex, psOldStyle, true, InsertIndex + PicClipBoardObj.UDCoordIndex - 1
      }
    }
    
    // now need to verify that inserted cmds did not add a new set pen cmd that
    // could affect plot cmds that occur after the inserted cmds
    for (int i = PicClipBoardObj.UDCoordIndex - 1; i >= 0; i--) {
      if (Left$(lstCommands.List(i + InsertIndex), 3) = "Set") {
        // this is last 'set pen' cmd;
        // if it is different from the pen style BEFORE insertion of cmd
        // (check for the word 'splatter' in the cmd text to determine style)
        if (InStr(7, lstCommands.List(i + InsertIndex), "Splat") = 0) {
          psNewStyle = psSolid
        } else {
          psNewStyle = psSplatter
        }
        // if styles don't match
        if (psNewStyle != psOldStyle) {
          // need to adjust plot cmds that occur AFTER the inserted cmds to match style set in pasted cmds
          ReadjustPlotCoordinates InsertIndex + PicClipBoardObj.UDCoordIndex, psNewStyle
        }
        
        // no need to check further, since we already found the set cmd that
        // could possibly affect plot cmds
        break; // exit for
      }
    }
    
    if (Settings.PicUndo != 0) {
      // add the undo object
      Set NextUndo = New PictureUndo
        NextUndo.UDAction = PasteCmds
        NextUndo.UDPicPos = InsertPos
        NextUndo.UDCmdIndex = InsertIndex
        NextUndo.UDCoordIndex = PicClipBoardObj.UDCoordIndex
      AddUndo NextUndo
    }
  }
  
  // need to make sure that the selection process
  // doesn't recurse and cause trouble
  NoSelect = true
  
  // unselect current cmds
  for (int i = 0; i < lstCommands.ListCount; i++) {
    lstCommands.Selected(i) = false
  }
  // select the cmds that were pasted, and force update
  for (int i = InsertIndex; i <= InsertIndex + PicClipBoardObj.UDCoordIndex - 1; i++) {
    lstCommands.Selected(i) = true
  }
  
  NoSelect = false
  CodeClick = true
  lstCommands_Click
}

public void MenuClickUndo() {

  Dim NextUndo As PictureUndo, tmpUndo As PictureUndo
  Dim bytData() As Byte, lngCmdIndex As Long
  Dim tmpPT As Point
  Dim blnSetPen As Boolean, strPrefix As String
  Dim i As Long, j As Long, lngNewPos As Long
  
  // if there are no undo actions
  if (UndoCol.Count = 0) {
    // just exit
    return;
  }
  
  // get next undo object
  Set NextUndo = UndoCol(UndoCol.Count)
  // remove undo object
  UndoCol.Remove UndoCol.Count
  // reset undo menu
  frmMDIMain.mnuEUndo.Enabled = (UndoCol.Count > 0)
  if (frmMDIMain.mnuEUndo.Enabled) {
    frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(PICUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & UndoCol(UndoCol.Count).UDCmd & vbTab & "Ctrl+Z"
  } else {
    frmMDIMain.mnuEUndo.Text = "&Undo " & vbTab & "Ctrl+Z"
  }
  
  // copy data to local array
  bytData = NextUndo.UDData
  // get index of command
  lngCmdIndex = NextUndo.UDCmdIndex
  
  // undo the action
  switch (NextUndo.UDAction) {
  case ChangeColor:
    // select the affected node
    SelectCmd lngCmdIndex
    // change color
    ChangeColor SelectedCmd, bytData(0), true
    
    // use click event to force update
    CodeClick = true
    lstCommands_Click
    
  case ChangePlotPen:
    // select the affected cmd
    SelectCmd lngCmdIndex
    
    // restore command
    EditPicture.Resource.Data(NextUndo.UDPicPos + 1) = bytData(0)
    lstCommands.List(SelectedCmd) = NextUndo.UDText
    
    // check for any pattern changes
    if (UndoCol.Count > 0) {
      // get next undo item
      Set tmpUndo = UndoCol(UndoCol.Count)
      while (tmpUndo.UDAction == AddPlotPattern || tmpUndo.UDAction == DelPlotPattern) {
        if (tmpUndo.UDAction = AddPlotPattern) {
          // remove this pattern data
          DelPatternData tmpUndo.UDCmdIndex, true
        } else {
          // add this pattern data
          AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, true
        }
        // remove undo object from stack
        UndoCol.Remove UndoCol.Count
        // if any more
        if (UndoCol.Count > 0) {
          // get next object
          Set tmpUndo = UndoCol(UndoCol.Count)
        } else {
          // exit loop
          break; // exit do
        }
      }
    }
    
    // force update
    SelectCmd lngCmdIndex, false
    
  case DelCmd or CutCmds:
    // if only one cmd
    if (NextUndo.UDCoordIndex = 1) {
      // select the affected cmd
      SelectCmd lngCmdIndex
      
      // reinsert command, ALWAYS in front
      InsertCommand NextUndo.UDData, lngCmdIndex, NextUndo.UDCmd, true, true
    
      // if command was set plot
      if (Left$(NextUndo.UDCmd, 3) = "Set") {
        // check for any pattern changes
        if (UndoCol.Count > 0) {
          // get next undo item
          Set tmpUndo = UndoCol(UndoCol.Count)
          while (tmpUndo.UDAction == AddPlotPattern || tmpUndo.UDAction == DelPlotPattern) {
            if (tmpUndo.UDAction = AddPlotPattern) {
              // remove this pattern data
              DelPatternData tmpUndo.UDCmdIndex, true
            } else {
              // add this pattern data
              AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, true
            }
            // remove undo object from stack
            UndoCol.Remove UndoCol.Count
            // if any more
            if (UndoCol.Count > 0) {
              // get next object
              Set tmpUndo = UndoCol(UndoCol.Count)
            } else {
              // exit loop
              break; // exit do
            }
          }
        }
      }
      
      // now select the added cmd
      SelectCmd lngCmdIndex
      
    } else {
      // multiple commands
      
      // insert the data
      EditPicture.Resource.InsertData NextUndo.UDData, NextUndo.UDPicPos
      
      // rebuild it (but no update)
      LoadCmdList true
      
      // check for a 'set pen' cmd
      for (int i = 0; i < NextUndo.UDCoordIndex; i++) {
        if (Left$(lstCommands.List(NextUndo.UDCmdIndex - i), 3) = "Set") {
          blnSetPen = true
          break; // exit for
        }
      }

      if (blnSetPen) {
        // check for any pattern changes
        if (UndoCol.Count > 0) {
          // get next undo item
          Set tmpUndo = UndoCol(UndoCol.Count)
          while (tmpUndo.UDAction == AddPlotPattern || tmpUndo.UDAction == DelPlotPattern) {
            if (tmpUndo.UDAction = AddPlotPattern) {
              // remove this pattern data
              DelPatternData tmpUndo.UDCmdIndex, true
            } else {
              // add this pattern data
              AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, true
            }
            // remove undo object from stack
            UndoCol.Remove UndoCol.Count
            // if any more
            if (UndoCol.Count > 0) {
              // get next object
              Set tmpUndo = UndoCol(UndoCol.Count)
            } else {
              // exit loop
              break; // exit do
            }
          }
        }
      }
    
      // disable painting
      SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
      
      // unselect 'end' place holder
      lstCommands.Selected(lstCommands.ListCount - 1) = false
      
      // select cmds in the cmd list
      for (int i = NextUndo.UDCoordIndex - 1; i >= 0; i--) {
        NoSelect = true
        lstCommands.Selected(lngCmdIndex - i) = true
      }
      NoSelect = false
      
      // reenable painting
      SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
      // select the bottom cmd and redraw
      SelectedCmd = NextUndo.UDCmdIndex
      DrawPicture
    
      // if more than one cmd was selected
      if (NextUndo.UDCoordIndex > 1) {
        // get bounds, and select the cmds
        // (sets selstart and selsize)
        GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, true
      }
    }
    
  case PasteCmds:
    // calculate amount of data to remove
    lngNewPos = lstCommands.Items[lngCmdIndex + NextUndo.UDCoordIndex].Tag - lstCommands.Items[lngCmdIndex].Tag
    
    // remove the data from the resource
    EditPicture.Resource.RemoveData NextUndo.UDPicPos, lngNewPos
    
    // remove the cmds from the cmd list
    for (int i = NextUndo.UDCoordIndex - 1; i >= 0; i--) {
      lstCommands.RemoveItem lngCmdIndex + i
    }
    
    // update position values for follow on cmds
    UpdatePosValues lngCmdIndex, -lngNewPos
    
    // check for any plot adjustments that need to be restored
    // (the index passed to addpattern/delpattern need to be
    // adjusted by the number of commands that were removed so
    // the correct plot cmd is restored
    if (UndoCol.Count > 0) {
      // get next undo item
      Set tmpUndo = UndoCol(UndoCol.Count)
      // continue adjusting patterns, until an undo item that is not a pattern adjust item is found
      while (tmpUndo.UDAction == AddPlotPattern || tmpUndo.UDAction == DelPlotPattern) {
        if (tmpUndo.UDAction = AddPlotPattern) {
          // remove this pattern data
          DelPatternData tmpUndo.UDCmdIndex - NextUndo.UDCoordIndex, true
        } else {
          // add this pattern data
          AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, true
        }
        // remove undo object from stack
        UndoCol.Remove UndoCol.Count
        // if any more
        if (UndoCol.Count > 0) {
          // get next object
          Set tmpUndo = UndoCol(UndoCol.Count)
        } else {
          // exit loop
          break; // exit do
        }
      }
    }
    
    // select the cmd to refresh everything
    SelectCmd lngCmdIndex, false
    
    
  case DelCoord:
    // select the cmd/coord
    SelectCmd lngCmdIndex, false
    
    // reinsert the coordinate
    tmpPT = ExtractCoordinates(NextUndo.UDText)
    if (InStr(NextUndo.UDText, "-") > 0) {
     strPrefix = Left$(NextUndo.UDText, InStr(NextUndo.UDText, "-") + 2)
    } else {
      strPrefix = ""
    }
    AddCoordToPic bytData, NextUndo.UDCoordIndex, tmpPT.X, tmpPT.Y, strPrefix, true
    
    //  avoid the automatic click event, we will manually call it
    //  after setting the index value
    NoSelect = true
    //  set the correct
    if (NextUndo.UDCoordIndex = -1) {
      lstCoords.ListIndex = lstCoords.ListCount - 1
    } else {
      lstCoords.ListIndex = NextUndo.UDCoordIndex
    }
    NoSelect = false
    
    // force the click event
    lstCoords_Click
    
  case AddCmd or Rectangle or Trapezoid:
    // delete the command
    DeleteCommand lngCmdIndex, true
    
    // if command was set plot
    if (Left$(NextUndo.UDCmd, 3) = "Set") {
      // check for any pattern changes
      if (UndoCol.Count > 0) {
        // get next undo item
        Set tmpUndo = UndoCol(UndoCol.Count)
        while (tmpUndo.UDAction == AddPlotPattern || tmpUndo.UDAction == DelPlotPattern) {
          if (tmpUndo.UDAction = AddPlotPattern) {
            // remove this pattern data
            DelPatternData tmpUndo.UDCmdIndex, true
          } else {
            // add this pattern data
            AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, true
          }
          // remove undo object from stack
          UndoCol.Remove UndoCol.Count
          // if any more
          if (UndoCol.Count > 0) {
            // get next object
            Set tmpUndo = UndoCol(UndoCol.Count)
          } else {
            // exit loop
            break; // exit do
          }
        }
      }
    }
        
    // use click event to force update
    CodeClick = true
    lstCommands_Click
    
  case Ellipse:
    // delete this command, and next three commands
    for (int i = 1; i <= 4; i++) {
      DeleteCommand lngCmdIndex, true
    }
    
    // force update
    SelectCmd lngCmdIndex, false
    
  case AddCoord:
    // select cmd first
    SelectCmd lngCmdIndex
    // build coord list
    BuildCoordList lngCmdIndex
    
    // delete the coordinate
    DeleteCoordinate NextUndo.UDCoordIndex, true
    
    // force update
    SelectCmd lngCmdIndex, false
    
  case EditCoord:
    // select command first
    SelectCmd lngCmdIndex
    
    // now edit coord
    tmpPT.X = bytData(0)
    tmpPT.Y = bytData(1)
    EndEditCoord CLng(NextUndo.UDText), NextUndo.UDCoordIndex, tmpPT, NextUndo.UDPicPos, vbNullString, true
    
    // force update
    BuildCoordList lngCmdIndex
    lstCoords.ListIndex = NextUndo.UDCoordIndex
    
  case SplitCmd:
    // select the cmd
    SelectCmd lngCmdIndex
    
    // now rejoin the commands
    JoinCommands SelectedCmd, true
    
  case JoinCmds:
    // select the cmd
    SelectCmd lngCmdIndex, false
    
    for (int i = 0; i < lstCoords.ListCount; i++) {
      if (lstCoords.Items[i].Tag = NextUndo.UDPicPos) {
        break; // exit for
      }
    }
    
    // now split the commands
    SplitCommand i, true
  
  case MoveCmds:
    // select the cmd
    SelectCmd lngCmdIndex
    
    // extract delta values
    i = Val(NextUndo.UDText)
    j = Val(Right$(NextUndo.UDText, Len(NextUndo.UDText) - InStr(1, NextUndo.UDText, "|")))
    
    // move cmds back
    MoveCmds lngCmdIndex, NextUndo.UDCoordIndex, i, j, true
    
    // disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    // select cmds in the cmd list
    for (int i = NextUndo.UDCoordIndex - 1; i >= 0; i--) {
      NoSelect = true
      lstCommands.Selected(lngCmdIndex - i) = true
    }
    NoSelect = false
    
    // if only one command selected, reload the coord list
    if (NextUndo.UDCoordIndex = 1) {
      // build coord list
      BuildCoordList SelectedCmd
      // and make sure selection bounds are hidden
      tmrSelect.Enabled = false
      shpVis.Visible = false
      shpPri.Visible = false
    }
    
    // reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    // if more than one cmd
    if (NextUndo.UDCoordIndex > 1) {
      // get bounds, and select the cmds
      GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, true
    }
    
//      do we need all these activities? they are what is done when
//      something is selected, which is basically the same thing here
//      This will need some studying and testing to make sure it's
//      what I need done here

    // always cancel any drawing operation
    PicDrawMode = doNone
    
    // set CurCmdIsLine to false until proven otherwise
    CurCmdIsLine = false
    
    // always set cursor highlighting to match selection status
    tmrSelect.Enabled = (Selection.Width > 0 && Selection.Height > 0)
    
    // get current tool status
    CurrentPen = EditPicture.CurrentToolStatus
    
    // set selected tools to match current
    SelectedPen = CurrentPen
    
    // update toolbar
    UpdateToolBar
    
    // force redraw
    DrawPicture
    
    // reset edit menu
    SetEditMenu
  
  case FlipH or FlipV:
    // if current cmd is not this cmd
    if (lstCommands.ListIndex != lngCmdIndex) {
      // select the cmd
      SelectCmd lngCmdIndex
    }
    
    // re-flip
    FlipCmds lngCmdIndex, NextUndo.UDCoordIndex, IIf(NextUndo.UDAction = FlipH, 0, 1), true
    
    // disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    // select cmds in the cmd list
    for (int i = NextUndo.UDCoordIndex - 1; i >= 0; i--) {
      NoSelect = true
      lstCommands.Selected(lngCmdIndex - i) = true
    }
    NoSelect = false
    
    // reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    // if more than one cmd
    if (NextUndo.UDCoordIndex > 1) {
      // get bounds, and select the cmds
      GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, true
      
    // if only one cmd,
    } else {
      // rebuld coord list
      BuildCoordList SelectedCmd
      
      // select 'nothing'
      Selection.X = 0
      Selection.Y = 0
      Selection.Width = 0
      Selection.Height = 0
      ShowCmdSelection
    }
    
    // force redraw
    DrawPicture
    
  case EditPlotCoord:
    lngNewPos = NextUndo.UDPicPos
    bytData = NextUndo.UDData
    lngCmdIndex = NextUndo.UDCmdIndex
    
    // was a splat edited? different actions if so
    if (UBound(bytData) = 2) {
      // change pattern back
      EditPicture.Resource.Data(lngNewPos) = bytData(0) * 2
      // change the coord values
      EditPicture.Resource.Data(lngNewPos + 1) = bytData(1)
      EditPicture.Resource.Data(lngNewPos + 2) = bytData(2)
    } else {
      // change the coord values
      EditPicture.Resource.Data(lngNewPos) = bytData(0)
      EditPicture.Resource.Data(lngNewPos + 1) = bytData(1)
    }
    
    // disable updating of redraw until done
    SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0

    // if current cmd is not this cmd
    if (lstCommands.ListIndex != lngCmdIndex) {
      // select the cmd
      SelectCmd lngCmdIndex
    }
    CodeClick = true
    lstCommands_Click
    lstCoords.ListIndex = NextUndo.UDCoordIndex
  
    // refresh pictures
    SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
    picVisual.Refresh
    picPriority.Refresh
    
  case SetPriBase:
    // change pribase back
    EditPicture.PriBase = lngCmdIndex
    // if showing priority lines
    if (ShowBands) {
      DrawPicture
    }
  }
  
  Set NextUndo = Nothing
  
  // update menu
  SetEditMenu
  
  // undo should always set dirty flag
  MarkAsChanged
}

public void SetEditMenu() {
  // sets the menu captions on the Edit menu
  // based on current selection
  Dim tmpCmd As DrawFunction
  
    // always show undo, cut, copy, paste, select all, bar1, find, findagain, replace, both customs
    frmMDIMain.mnuEUndo.Visible = true
    frmMDIMain.mnuEBar0.Visible = true
    frmMDIMain.mnuECut.Visible = true
    frmMDIMain.mnuECopy.Visible = true
    frmMDIMain.mnuEPaste.Visible = true
    frmMDIMain.mnuESelectAll.Visible = true
    frmMDIMain.mnuEBar1.Visible = true
    frmMDIMain.mnuEBar2.Visible = true
    frmMDIMain.mnuEFind.Visible = true
    frmMDIMain.mnuEFindAgain.Visible = !InGame || Val(InterpreterVersion) >= 2.936
    frmMDIMain.mnuEReplace.Visible = true
    frmMDIMain.mnuECustom1.Visible = true
    frmMDIMain.mnuECustom2.Visible = true
    frmMDIMain.mnuECustom3.Visible = true
    frmMDIMain.mnuECustom4.Visible = (PicMode = pmEdit)
    
    // redo used for swapping drawing surfaces
    if (VisVisible Xor PriVisible) {
      frmMDIMain.mnuERedo.Visible = true
      frmMDIMain.mnuERedo.Enabled = true
      if (VisVisible) {
        frmMDIMain.mnuERedo.Text = "Show Priority Screen" & vbTab & "Alt+S"
      } else {
        frmMDIMain.mnuERedo.Text = "Show Visual Screen" & vbTab & "Alt+S"
      }
    } else {
      frmMDIMain.mnuERedo.Visible = false
    }
    
    // find used for showing/hiding priority bands
    frmMDIMain.mnuEFind.Enabled = true
    if (ShowBands) {
      frmMDIMain.mnuEFind.Text = "Hide Priority Bands" & vbTab & "Alt+P"
    } else {
      frmMDIMain.mnuEFind.Text = "Show Priority Bands" & vbTab & "Alt+P"
    }
    
    // find again used for setting priority base
    if (frmMDIMain.mnuEFindAgain.Visible) {
      frmMDIMain.mnuEFindAgain.Enabled = true
      frmMDIMain.mnuEFindAgain.Text = "Adjust Priority Base" & vbTab & "Ctrl+Alt+P"
    }
    
    // replace used for showing/hiding text markers
    frmMDIMain.mnuEReplace.Enabled = true
    if (ShowTextMarks) {
      frmMDIMain.mnuEReplace.Text = "Hide Text Marks" & vbTab & "Alt+T"
    } else {
      frmMDIMain.mnuEReplace.Text = "Show Text Marks" & vbTab & "Alt+T"
    }
    
    // toggle bkgd visible only if a bkgd Image is loaded
    frmMDIMain.mnuRCustom2.Visible = !(BkgdImage == null)
    if (frmMDIMain.mnuRCustom2.Visible) {
      frmMDIMain.mnuRCustom2.Enabled = true
      if (EditPicture.BkgdShow && frmMDIMain.mnuRCustom2.Visible) {
        frmMDIMain.mnuRCustom2.Text = "Hide Background" & vbTab & "Alt+B"
      } else {
        frmMDIMain.mnuRCustom2.Text = "Show Background" & vbTab & "Alt+B"
      }
    }
    //  allow removal if an image is loaded
    frmMDIMain.mnuRCustom3.Visible = frmMDIMain.mnuRCustom2.Visible
    if (frmMDIMain.mnuRCustom3.Visible) {
      frmMDIMain.mnuRCustom3.Enabled = true
      frmMDIMain.mnuRCustom3.Text = "Remove Background Image" & vbTab & "Shift+Alt+B"
    }

    // edit custom3 used to toggle test mode
    frmMDIMain.mnuECustom3.Visible = true
    frmMDIMain.mnuECustom3.Enabled = true
    // Caption depends on current mode

    
    switch (PicMode) {
    case pmPrintTest or pmViewTest:
      // show but disable undo, cut, copy, paste, select all
      frmMDIMain.mnuEUndo.Enabled = false
      frmMDIMain.mnuEUndo.Text = "&Undo" & vbTab & "Ctrl+Z"
      frmMDIMain.mnuECut.Enabled = false
      frmMDIMain.mnuECut.Text = "Cu&t" & vbTab & "Ctrl+X"
      frmMDIMain.mnuECopy.Enabled = false
      frmMDIMain.mnuECopy.Text = "&Copy" & vbTab & "Ctrl+C"
      frmMDIMain.mnuEPaste.Enabled = false
      frmMDIMain.mnuEPaste.Text = "&Paste" & vbTab & "Ctrl+V"
      frmMDIMain.mnuESelectAll.Enabled = false
      frmMDIMain.mnuESelectAll.Text = "Select &All" & vbTab & "Ctrl+A"
      
      // delete, insert, clear, bkgd items not used in test mode
      frmMDIMain.mnuEDelete.Visible = false
      frmMDIMain.mnuEInsert.Visible = false
      frmMDIMain.mnuEClear.Visible = false
      frmMDIMain.mnuRCustom2.Enabled = false
      frmMDIMain.mnuRCustom3.Enabled = false
      
      // show custom menus, depending on test mode
      if (PicMode = pmViewTest) {
        frmMDIMain.mnuECustom1.Enabled = true
        frmMDIMain.mnuECustom1.Text = "Test View..." & vbTab & "Alt+V"
        frmMDIMain.mnuECustom2.Enabled = true
        frmMDIMain.mnuECustom2.Text = "Test Options..." & vbTab & "Alt+O"
      } else {
        frmMDIMain.mnuECustom1.Enabled = true
        frmMDIMain.mnuECustom1.Text = "Display/Print Options..." & vbTab & "Alt+D"
        frmMDIMain.mnuECustom2.Visible = PowerPack
        if (PowerPack) {
          frmMDIMain.mnuECustom2.Enabled = true
          frmMDIMain.mnuECustom2.Text = "Screen Size: " & CStr(MaxCol + 1) & vbTab & "Alt+W"
        }
        frmMDIMain.mnuECustom3.Text = "Disable Test &Mode" & vbTab & "Alt+M"
      }
      
    case pmEdit:
      // delete, insert, clear visible
      frmMDIMain.mnuEDelete.Visible = true
      frmMDIMain.mnuEInsert.Visible = true
      frmMDIMain.mnuEClear.Visible = true
      // custom3 enabled
      frmMDIMain.mnuRCustom3.Enabled = true
      // enable test mode switch
      frmMDIMain.mnuECustom3.Text = "Enable Test &Mode" & vbTab & "Alt+M"
      
      // if there is something to undo
      if (UndoCol.Count != 0) {
        frmMDIMain.mnuEUndo.Enabled = true
        frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(PICUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & UndoCol(UndoCol.Count).UDCmd
        // some commands need 's' added to end if more than one command to undo
        switch (UndoCol(UndoCol.Count).UDAction) {
        case DelCmd or AddCmd or CutCmds or PasteCmds or MoveCmds or FlipH or FlipV:
          if (UndoCol(UndoCol.Count).UDCoordIndex > 1) {
            frmMDIMain.mnuEUndo.Text = frmMDIMain.mnuEUndo.Text & "s" & vbTab & "Ctrl+Z"
          } else {
            frmMDIMain.mnuEUndo.Text = frmMDIMain.mnuEUndo.Text & vbTab & "Ctrl+Z"
          }
        default:
          frmMDIMain.mnuEUndo.Text = frmMDIMain.mnuEUndo.Text & vbTab & "Ctrl+Z"
        }
      } else {
        frmMDIMain.mnuEUndo.Enabled = false
        frmMDIMain.mnuEUndo.Text = "&Undo" & vbTab & "Ctrl+Z"
      }
      
      // if tool is editselect
      if ((SelectedTool = ttSelectArea)) {
        // always disable cut, delete, insert, paste
        frmMDIMain.mnuEDelete.Enabled = false
        frmMDIMain.mnuEDelete.Text = "Delete" & vbTab & "Del"
        frmMDIMain.mnuEInsert.Enabled = false
        frmMDIMain.mnuEInsert.Text = "&Insert Coordinate" & vbTab & "Shift+Ins"
        frmMDIMain.mnuECut.Enabled = false
        frmMDIMain.mnuECut.Text = "Cut" & vbTab & "Ctrl+X"
        frmMDIMain.mnuEPaste.Enabled = false
        frmMDIMain.mnuEPaste.Text = "Paste" & vbTab & "Ctrl+V"
        // copy is enabled if something selected
        frmMDIMain.mnuECopy.Enabled = ((Selection.Width > 0) && (Selection.Height > 0))
        frmMDIMain.mnuECopy.Text = "Copy Selection" & vbTab & "Ctrl+C"
          
      // if NO coordinate is selected
      } else if (lstCoords.ListIndex = -1) {
        // cut, copy, delete enabled if at least one cmd selected
        frmMDIMain.mnuECopy.Enabled = (lstCommands.ListIndex != -1) && (SelectedCmd != lstCommands.ListCount - 1)
        frmMDIMain.mnuECopy.Text = "Copy Command"
        frmMDIMain.mnuECut.Enabled = frmMDIMain.mnuECopy.Enabled
        frmMDIMain.mnuECut.Text = "Cut Command"
        frmMDIMain.mnuEDelete.Enabled = frmMDIMain.mnuECopy.Enabled
        frmMDIMain.mnuEDelete.Text = "Delete Command"
        if (lstCommands.SelCount > 1) {
          frmMDIMain.mnuECopy.Text = frmMDIMain.mnuECopy.Text & "s"
          frmMDIMain.mnuECut.Text = frmMDIMain.mnuECut.Text & "s"
          frmMDIMain.mnuEDelete.Text = frmMDIMain.mnuEDelete.Text & "s"
        }
        frmMDIMain.mnuECopy.Text = frmMDIMain.mnuECopy.Text & vbTab & "Ctrl+C"
        frmMDIMain.mnuECut.Text = frmMDIMain.mnuECut.Text & vbTab & "Ctrl+X"
        frmMDIMain.mnuEDelete.Text = frmMDIMain.mnuEDelete.Text & vbTab & "Del"
        
        // paste enabled only if clipboard has pic cmds on it
        frmMDIMain.mnuEPaste.Enabled = (!PicClipBoardObj == null)
        frmMDIMain.mnuEPaste.Text = "Paste"
        if (frmMDIMain.mnuEPaste.Enabled) {
          if (PicClipBoardObj.UDCoordIndex > 1) {
            frmMDIMain.mnuEPaste.Text = frmMDIMain.mnuEPaste.Text & " Commands" & vbTab & "Ctrl+V"
          } else {
            frmMDIMain.mnuEPaste.Text = frmMDIMain.mnuEPaste.Text & " Command" & vbTab & "Ctrl+V"
          }
        } else {
          frmMDIMain.mnuEPaste.Text = frmMDIMain.mnuEPaste.Text & vbTab & "Ctrl+V"
          frmMDIMain.mnuECustom4.Text = "Copy Commands As Text"
        }
        // insert not enabled if no coord selected
        frmMDIMain.mnuEInsert.Enabled = false
        frmMDIMain.mnuEInsert.Text = "&Insert Coordinate" & vbTab & "Shift+Ins"
        
      } else {
        // no cut or copy for coords
        frmMDIMain.mnuECopy.Enabled = false
        frmMDIMain.mnuECopy.Text = "Copy" & vbTab & "Ctrl+C"
        frmMDIMain.mnuECut.Enabled = false
        frmMDIMain.mnuECut.Text = "Cut" & vbTab & "Ctrl+X"
        
        // insert always available for absline, relline, fill, plot
        // delete always available for absline, fill, plot
        // insert/delete only available for other commands if on last coord
        frmMDIMain.mnuEDelete.Text = "Delete "
        frmMDIMain.mnuEInsert.Text = "&Insert Coordinate" & vbTab & "Shift+Ins"
        
        switch (lstCommands.List(SelectedCmd)) {
        case "Abs Line" or "Fill" or "Plot" or "Rel Line":
          // enable delete
          frmMDIMain.mnuEDelete.Enabled = true
          frmMDIMain.mnuEDelete.Text = frmMDIMain.mnuEDelete.Text & " Coordinate" & vbTab & "Del"
          // enable insert
          frmMDIMain.mnuEInsert.Enabled = true
          
        default:
          // delete
          // if on last coord of any other command,
          if (lstCoords.ListIndex = lstCoords.ListCount - 1) {
            // enable delete
            frmMDIMain.mnuEDelete.Enabled = true
            frmMDIMain.mnuEDelete.Text = frmMDIMain.mnuEDelete.Text & " Coordinate" & vbTab & "Del"
          } else {
            // disable delete
            frmMDIMain.mnuEDelete.Enabled = false
            frmMDIMain.mnuEDelete.Text = frmMDIMain.mnuEDelete.Text & " Coordinate" & vbTab & "Del"
          }
          
          // insert
          if (lstCommands.List(SelectedCmd) = "RelLine") {
            // enable
            frmMDIMain.mnuEInsert.Enabled = true
          } else {
            // if on last coord of any other command,
            frmMDIMain.mnuEInsert.Enabled = (lstCoords.ListIndex = lstCoords.ListCount - 1)
          }
        }
      }
          
      // enable clear
      frmMDIMain.mnuEClear.Enabled = true
      frmMDIMain.mnuEClear.Text = "Clear Picture" & vbTab & "Shift+Del"
      
      // enable select all if at least one cmd
      frmMDIMain.mnuESelectAll.Enabled = lstCommands.ListCount > 1
      frmMDIMain.mnuESelectAll.Text = "Select &All" & vbTab & "Ctrl+A"
      
      // copy cmd text matches regular copy
      frmMDIMain.mnuECustom4.Enabled = frmMDIMain.mnuECopy.Enabled
      frmMDIMain.mnuECustom4.Text = "Copy Commands As Text" & vbTab & "Ctrl+Alt+C"
      
      // custom1 is split
      // disable if no cmd selected, OR no no coord selected,
      //  OR first coord is selected, OR (last is selected and NOT plot or fill)
      //  OR more than one selected
      if (lstCommands.ListIndex = -1 || lstCoords.ListIndex = -1 || lstCoords.ListIndex = 0 || (lstCoords.ListIndex = lstCoords.ListCount - 1 && EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag) < dfFill) || lstCommands.SelCount > 1) {
        frmMDIMain.mnuECustom1.Enabled = false
      } else {
        // if on a line, fill, or plot cmd
        switch (EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag)) {
        case dfAbsLine or dfRelLine or dfXCorner or dfYCorner or dfFill or dfPlotPen:
          // enable splitting
          frmMDIMain.mnuECustom1.Enabled = true
        default:
          frmMDIMain.mnuECustom1.Enabled = false
        }
      }
      frmMDIMain.mnuECustom1.Text = "&Split Command" & vbTab & "Ctrl+Shift+T"
      
      // custom2 = join
      // only enable joining if on a fill or plot cmd AND previous cmd matches
      // OR on a line cmd AND previous cmd matches AND last coordinate of prev cmd
      // matches first coord of this cmd
      
      // a valid cmd other than first or last, is selected AND only one cmd selected
      if (SelectedCmd > 0 && SelectedCmd < lstCommands.ListCount - 1 && lstCommands.SelCount <= 2) {
        tmpCmd = EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag)
        // if on a command node
        switch (tmpCmd) {
        case dfAbsLine or dfRelLine or dfFill or dfPlotPen:
          // if same as cmd above
          if (lstCommands.List(SelectedCmd) = lstCommands.List(SelectedCmd - 1)) {
            // if cmd is paint or plot
            if (tmpCmd = dfPlotPen || tmpCmd = dfFill) {
              frmMDIMain.mnuECustom2.Enabled = true
            } else {
              // if points match
              if (MatchPoints(SelectedCmd)) {
                frmMDIMain.mnuECustom2.Enabled = true
              } else {
                frmMDIMain.mnuECustom2.Enabled = false
              }
            }
          } else {
            frmMDIMain.mnuECustom2.Enabled = false
          }
        case dfXCorner or dfYCorner:
          // if cmd above is a corner
          if ((Asc(lstCommands.List(SelectedCmd - 1)) = 89) || (Asc(lstCommands.List(SelectedCmd - 1)) = 88)) {
            // if points match
            if (MatchPoints(SelectedCmd)) {
              frmMDIMain.mnuECustom2.Enabled = true
            } else {
              frmMDIMain.mnuECustom2.Enabled = false
            }
          } else {
            frmMDIMain.mnuECustom2.Enabled = false
          }
  
        default:
          frmMDIMain.mnuECustom2.Enabled = false
        }
      } else {
        frmMDIMain.mnuECustom2.Enabled = false
      }
      frmMDIMain.mnuECustom2.Text = "&Join Commands" & vbTab & "Ctrl+Shift+J"
    }
  
  // set toolbar buttons to match menu items
    Toolbar1.Buttons("undo").Enabled = frmMDIMain.mnuEUndo.Enabled
    Toolbar1.Buttons("cut").Enabled = frmMDIMain.mnuECut.Enabled
    Toolbar1.Buttons("copy").Enabled = frmMDIMain.mnuECopy.Enabled
    Toolbar1.Buttons("paste").Enabled = frmMDIMain.mnuEPaste.Enabled
    Toolbar1.Buttons("delete").Enabled = frmMDIMain.mnuEDelete.Enabled
    // if only one cmd selected
    if (lstCommands.SelCount = 1 && SelectedTool != ttSelectArea) {
      // cant flip end cmd
      if (SelectedCmd != lstCommands.ListCount - 1) {
        switch (Left$(lstCommands.List(SelectedCmd), 3)) {
        case "Set" or "Vis" or "Pri":
          // these cmds don't have coordinates, so can't be flipped
          Toolbar1.Buttons(13).Enabled = false
          Toolbar1.Buttons(14).Enabled = false
        default:
          Toolbar1.Buttons(13).Enabled = true
          Toolbar1.Buttons(14).Enabled = true
        }
      } else {
        Toolbar1.Buttons(13).Enabled = false
        Toolbar1.Buttons(14).Enabled = false
      }
      
    // if more than one cmd selected
    } else if (lstCommands.SelCount > 1 && SelectedTool != ttSelectArea) {
      // if the selection shapes are visible, then cmds with coords
      // are in the selection
      Toolbar1.Buttons(13).Enabled = ((Selection.Width > 0) && (Selection.Height > 0))
      Toolbar1.Buttons(14).Enabled = ((Selection.Width > 0) && (Selection.Height > 0))
      
    // if no cmds selected
    } else {
      Toolbar1.Buttons(13).Enabled = false
      Toolbar1.Buttons(14).Enabled = false
    }
}

private void DrawBox(int StartX, int StartY, int EndX, int EndY) {
  
    DrawLine StartX, StartY, EndX, StartY
    DrawLine EndX, StartY, EndX, EndY
    DrawLine EndX, EndY, StartX, EndY
    DrawLine StartX, EndY, StartX, StartY
    picVisual.Refresh
    picPriority.Refresh
}

private void picPriority_GotFocus() {
  
  // ensure flyout toolbars hidden
  tbStyle.Visible = false
  tbSize.Visible = false
}

private void picPriority_MouseDown() {
  
  // set inpri flag
  blnInPri = true
  // pass to visual
  picVisual_MouseDown Button, Shift, X, Y
}

private void picPriority_MouseMove() {

  picVisual_MouseMove Button, Shift, X, Y
}

private void picPriority_MouseUp() {
  
  picVisual_MouseUp Button, Shift, X, Y
}

private void picPriSurface_MouseDown() {

  // if right button
  if (Button = vbRightButton) {
    // reset edit menu first
    SetEditMenu
    // make sure this form is the active form
    if (!(frmMDIMain.ActiveForm Is Me)) {
      // set focus before showing the menu
      Me.SetFocus
    }
    // need doevents so form activation occurs BEFORE popup
    // otherwise, errors will be generated because of menu
    // adjustments that are made in the form_activate event
    SafeDoEvents
    // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , picPriSurface.Left + X, picPriSurface.Top + Y
  }
}


private void picVisSurface_MouseDown() {
  
  // if right button
  if (Button = vbRightButton) {
    // reset edit menu first
    SetEditMenu
    // make sure this form is the active form
    if (!(frmMDIMain.ActiveForm Is Me)) {
      // set focus before showing the menu
      Me.SetFocus
    }
    // need doevents so form activation occurs BEFORE popup
    // otherwise, errors will be generated because of menu
    // adjustments that are made in the form_activate event
    SafeDoEvents
    // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , picVisSurface.Left + X, picVisSurface.Top + Y
  }
}


private void picVisSurface_MouseMove() {

  Dim tmpX As Single, tmpY As Single
  
  // if not active form
  if (!frmMDIMain.ActiveForm Is Me) {
    return;
  }
  
  // if dragging picture
  if (blnDragging) {
    // get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y
    
    // if vertical scrollbar is visible
    if (vsbVis.Visible) {
      // limit positions to valid values
      if (tmpY < vsbVis.Min) {
        tmpY = vsbVis.Min
      } else if (tmpY > vsbVis.Max) {
        tmpY = vsbVis.Max
      }
      // set vertical scrollbar
      vsbVis.Value = tmpY
    }
    
    // if horizontal scrollbar is visible
    if (hsbVis.Visible) {
      // limit positions to valid values
      if (tmpX < hsbVis.Min) {
        tmpX = hsbVis.Min
      } else if (tmpX > hsbVis.Max) {
        tmpX = hsbVis.Max
      }
      // set horizontal scrollbar
      hsbVis.Value = tmpX
    }
  }
}


private void picVisSurface_MouseUp() {

  Dim rtn As Long
  
  // if dragging
  if (blnDragging) {
    // cancel dragmode
    blnDragging = false
    // release mouse capture
    rtn = ReleaseCapture()
    SetCursors pcEdit
  }
}

private void picVisual_Click() {

  // if print preview, redraw
  if (PicMode = pmPrintTest) {
    DrawPicture
  }
}

private void picVisual_MouseDown() {

  Dim PicPt As Point, lInPri As Boolean
  Dim bytData() As Byte, i As Long
  Dim bytPattern As Byte
  
  // if activating, ignore
  if (Activating) {
    Activating = false
    return;
  }
  
  // save inpri status
  lInPri = blnInPri
  
  // calculate position
  PicPt.X = X \ (2 * ScaleFactor)
  PicPt.Y = Y \ ScaleFactor
  
  // reset flag that tracks which drawing surface was clicked
  blnInPri = false
  
  // first check for right-mouse click
  if ((Button = vbRightButton)) {
    // if currently drawing something, right-click ends it
    if (PicDrawMode != doNone) {
      StopDrawing
    } else {
      // reset edit menu first
      SetEditMenu
      // make sure this form is the active form
      if (!(frmMDIMain.ActiveForm Is Me)) {
        // set focus before showing the menu
        Me.SetFocus
      }
      // need doevents so form activation occurs BEFORE popup
      // otherwise, errors will be generated because of menu
      // adjustments that are made in the form_activate event
      SafeDoEvents
      // show edit menu
      if (lInPri) {
        PopupMenu frmMDIMain.mnuEdit, , picPriority.Left + picPriSurface.Left + X, picPriority.Top + picPriSurface.Top + Y
      } else {
        PopupMenu frmMDIMain.mnuEdit, , picVisual.Left + picVisSurface.Left + X, picVisual.Top + picVisSurface.Top + Y
      }
    }
    return;
  }
  
  // if print preview, ignore any other mouse-clicks
  if (PicMode = pmPrintTest) {
    return;
  }
  
  switch (PicMode) {
  case pmEdit:
    // if we are drawing something
    if ((PicDrawMode != doNone)) {
      // finish drawing function
      EndDraw PicPt
      return;
    }
  
    // what to do depends mostly on what the selected tool is:
    switch (SelectedTool) {
    case ttEdit:
      // no tool selected; check for a coordinate being moved or group of commands being moved
      // if none of those apply, drag the drawing surface
      
      // first, see if we need to select the current coordinate:
      if (CursorMode = pcmXMode) {
        if (CurCursor = pcCross && (CurPt.X != PicPt.X || CurPt.Y != PicPt.Y)) {
          // we are on a coordinate that is NOT the currently selected coordinate!
          // select it, and then continue
          for (int i = 0; i < lstCoords.ListCount; i++) {
            if (CoordPT(i).X = PicPt.X && CoordPT(i).Y = PicPt.Y) {
              lstCoords.ListIndex = i
              break; // exit for
            }
          }
        }
      }
      
      if ((CurPt.X = PicPt.X) && (CurPt.Y = PicPt.Y)) {
        // *'Debug.Assert lstCoords.ListIndex != -1
        // *'Debug.Assert tmrSelect.Enabled || CursorMode = pcmXMode
        // three cases; if on any coord and SHIFT key is pressed, then move entire command
        //              if on any coord and CTRL key is pressed, add a new coord, then begin moving it
        //              if on any coord and no key is pressed, begin moving just the coord
        //              (if combo of keys pressed, just ignore)
        switch (Shift) {
        case 0:
            // no key pressed
          // begin editing the coordinate
          PicDrawMode = doMovePt
          // get edit cmd
          EditCmd = EditPicture.Resource.Data(CLng(lstCommands.Items[SelectedCmd].Tag))
          // turn off cursor flasher
          tmrSelect.Enabled = false
          return;
          
        case vbCtrlMask:
          // insert a new coord, then begin moving it
          InsertCoordinate true
          return;
        
        case vbShiftMask:
          // set draw mode to move cmd
          PicDrawMode = doMoveCmds
          // set anchor
          Anchor = PicPt
          
          // get start and end coords of selection and show selection
          GetSelectionBounds SelectedCmd, lstCommands.SelCount, true
          
          // get delta from current point to selstart
          Delta.X = PicPt.X - Selection.X
          Delta.Y = PicPt.Y - Selection.Y
          
          // change cursor
          SetCursors pcMove
          // set curpt to invalid Value so mousemove will
          // update the selection even if moved back to starting point
          CurPt.X = 255
          return;
        
        default:
          // ignore
          return;
        }
        
      // if multiple cmds selected (i.e. the selection size is >0), begin moving them
      // (need to make sure cmds are selected, and NOT showing a screen grab selection
      
      } else if ((Selection.Width > 0) && (Selection.Height > 0)) {
        // *'Debug.Assert shpPri.Visible
        // is cursor within the shape?
        if (PicPt.X >= Selection.X && PicPt.X <= Selection.X + Selection.Width &&
           PicPt.Y >= Selection.Y && PicPt.Y <= Selection.Y + Selection.Height) {
            
          // set draw mode to move cmd
          PicDrawMode = doMoveCmds
          
          // get start and end coords of selection, then draw box around them
          GetSelectionBounds SelectedCmd, lstCommands.SelCount, true
          
          // set anchor
          Anchor = PicPt
          // get delta from current point to selstart
          Delta.X = PicPt.X - Selection.X
          Delta.Y = PicPt.Y - Selection.Y
          
          // set curpt to invalid Value so mousemove will
          // update the selection even if moved back to starting point
          CurPt.X = 255
          return;
        } else {
          // not moving commands; drag the picture
          StartDrag lInPri, X, Y
          return;
        }
      } else {
        // not moving commands; drag the picture
        StartDrag lInPri, X, Y
        return;
      }
      
    case ttLine or ttRelLine or ttCorner:
      // begin draw operation based on selected tool
      BeginDraw SelectedTool, PicPt
    
    case ttFill:
      // if on a Fill cmd
      if (lstCommands.Text = "Fill") {
        // if cursor hasn't moved, just exit
        if (Anchor.X = PicPt.X && Anchor.Y = PicPt.Y) {
          return;
        }
        // add this coordinate to end of list
        ReDim bytData(1)
        bytData(0) = PicPt.X
        bytData(1) = PicPt.Y
        AddCoordToPic bytData, -1, PicPt.X, PicPt.Y
        // need to select the coord just added so it will show
        NoSelect = true
        lstCoords.ListIndex = lstCoords.NewIndex
        // save point as anchor
        Anchor = PicPt
      } else {
        // add fill command
        ReDim bytData(2)
        bytData(0) = dfFill
        bytData(1) = PicPt.X
        bytData(2) = PicPt.Y
        InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfFill - 0xF0), gInsertBefore
        // select this cmd
        SelectCmd lstCommands.NewIndex, false
        // and select first coord
        NoSelect = true
        lstCoords.ListIndex = 0
        // save point as anchor
        Anchor = PicPt
      }
      
      // redraw
      DrawPicture
      
    case ttPlot:
      // need to bound the x value (AGI has a bug which actually allows
      // X values to be +1 more than they should; WinAGI enforces the
      // the actual boundary
      if (PicPt.X > 159 - CurrentPen.PlotSize \ 2) {
        PicPt.X = 159 - CurrentPen.PlotSize \ 2
      }
      
      // if on a coordinate that is part of a plot cmd
      if (lstCommands.Text = "Plot") {
        // only need to add the plot coordinate
      
        // if current pen is solid,
        if (CurrentPen.PlotStyle = psSolid) {
          // add this coordinate to end of list
          ReDim bytData(1)
          bytData(0) = PicPt.X
          bytData(1) = PicPt.Y
          AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y
          NoSelect = true
          lstCoords.ListIndex = lstCoords.NewIndex
        } else {
          // add pattern too
          Randomize Now()
          bytPattern = 1 + CByte(Int(Rnd * 119))
          // add this coordinate to end of list
          ReDim bytData(2)
          // pattern is multiplied by two before storing
          bytData(0) = 2 * bytPattern
          bytData(1) = PicPt.X
          bytData(2) = PicPt.Y
          AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y, CStr(bytPattern) & " -- "
          NoSelect = true
          lstCoords.ListIndex = lstCoords.NewIndex
        }
      } else {
        // if not already in a plot command, the plot command
        // needs to be included with the plot coordinates
        
        // if current pen is solid,
        if (CurrentPen.PlotStyle = psSolid) {
          ReDim bytData(2)
          bytData(0) = dfPlotPen
          bytData(1) = PicPt.X
          bytData(2) = PicPt.Y
          // add command
          InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfPlotPen - 0xF0), gInsertBefore
          // select this cmd
          SelectCmd lstCommands.NewIndex, false
          // and select first coordinate
          NoSelect = true
          lstCoords.ListIndex = 0
        } else {
          ReDim bytData(3)
          // add pattern too
          bytPattern = 1 + CByte(Int(Rnd * 119))
          bytData(0) = dfPlotPen
          // pattern is multiplied by two before storing
          bytData(1) = 2 * bytPattern
          bytData(2) = PicPt.X
          bytData(3) = PicPt.Y
          // add command
          InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfPlotPen - 0xF0), gInsertBefore
          // select this cmd
          SelectCmd lstCommands.NewIndex, false
          // and select first coordinate
          NoSelect = true
          lstCoords.ListIndex = 0
        }
      }
      
      // redraw
      DrawPicture
      
    case ttRectangle or ttTrapezoid or ttEllipse:
      // set anchor
      Anchor = PicPt
      // set mode
      PicDrawMode = doShape
      
    case ttSelectArea:
      // if shift key, drag the picture
      switch (Shift) {
      case 0:
            // no key pressed
        // begin selecting an area
        PicDrawMode = doSelectArea
        Anchor = PicPt
        // reset selection
        Selection.X = 0
        Selection.Y = 0
        Selection.Width = 0
        Selection.Height = 0
        ShowCmdSelection
        //  verify status bar is correctly set
          if (MainStatusBar.Tag != CStr(rtPicture)) {
            AdjustMenus rtPicture, Me.InGame, true, Me.IsChanged
          }
          spAnchor").Visible = false
          spBlock").Visible = false
      case vbShiftMask:

        // not moving commands; drag the picture
        StartDrag lInPri, X, Y
      }
    }
    
  case pmViewTest:
    // stop testview object motion
    TestDir = 0
    tmrTest.Enabled = TestSettings.CycleAtRest
    // if above top edge OR
    //    (NOT ignoring horizon AND above horizon ) OR
    //    (on water AND restricting to land) OR
    //    (NOT on water AND restricting to water)
    if ((PicPt.Y - (CelHeight - 1) < 0) || (PicPt.Y < TestSettings.Horizon && !TestSettings.IgnoreHorizon) ||
       (EditPicture.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) && TestSettings.ObjRestriction = 2) ||
      (!EditPicture.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) && TestSettings.ObjRestriction = 1)) {
        return;
    }
    // draw testview in new location
    TestCelPos = PicPt
    ShowTestCel = true
    DrawPicture
  }
}

private void picVisual_MouseMove() {
  
  Dim i As Long
  Dim SelAnchor As Point
  Dim tmpX As Long, tmpY As Long
  Dim NewPri As Long

  // if this form is not active
  if (!frmMDIMain.ActiveForm Is Me) {
    return;
  }
  
  // calculate position
  tmpX = X \ (2 * ScaleFactor)
  tmpY = Y \ ScaleFactor

  // bound position
  if (tmpX < 0) {
    PicPt.X = 0
  } else if (tmpX > 159) {
    PicPt.X = 159
  } else {
    PicPt.X = tmpX
  }
  if (tmpY < 0) {
    PicPt.Y = 0
  } else if (tmpY > 167) {
    PicPt.Y = 167
  } else {
    PicPt.Y = tmpY
  }

  switch (PicMode) {
  // in edit mode, action taken depends primarily on
  // current draw operation mode
  case pmEdit:
    switch (PicDrawMode) {
    case doNone:
      // not drawing anything- need to determine correct mousepointer based on current
      // mouse position and selected tool

      switch (SelectedTool) {
      case ttEdit:
        // not drawing anything, but there could be a highlighted coordinate
        // or a selected group of commands- cursor will depend on which of
        // those states exist

        // if selection is visible and tool=none, then must be moving cmds
        if ((Selection.Width > 0) && (Selection.Height > 0)) {
          // is cursor over cmds?
          if (PicPt.X >= Selection.X && PicPt.X <= Selection.X + Selection.Width && PicPt.Y >= Selection.Y && PicPt.Y <= Selection.Y + Selection.Height) {
            // use move cursor
            SetCursors pcMove
          } else {
            // use normal 'edit' cursor
            SetCursors pcEdit
          }
        } else {
        // check for editing coordinate (stepdraw should not matter)
//         if ((EditPicture.StepDraw) && lstCoords.ListIndex != -1) {
          if ((CurPt.X = PicPt.X) && (CurPt.Y = PicPt.Y)) {
            SetCursors pcCross
            OnPoint = true
            if (CursorMode = pcmWinAGI) {
              // reset area under cursor
              BitBlt picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 0, SRCCOPY
              BitBlt picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 12, SRCCOPY
              picVisual.Refresh
              picPriority.Refresh
            }
          } else {
            if (CursorMode = pcmXMode) {
              // check to see if cursor is over one of the other coordinates
              // if cursor is over one of the coord points
              for (int i = 0; i < lstCoords.ListCount; i++) {
                if ((PicPt.X = CoordPT(i).X) && (PicPt.Y = CoordPT(i).Y)) {
                  // this is one of the vertices; can't be the currently selected one - that would
                  // have been detected already... not that it really matters
                  SetCursors pcCross
                  i = -1  // so we can tell if loop exited due to finding a point
                  break; // exit for
                }
              }
              if (i != -1) {
                // nothing going on- use normal cursor
                SetCursors pcEdit
                OnPoint = false
              }
            } else {
              // nothing going on- use normal cursor
              SetCursors pcEdit
              OnPoint = false
            }
          }
        }

      // hmmm, don't think we need to do ANYTHING when one of these tools
      // is active while no draw ops are in progress;
      // cursor should already be set, with no need to change cursor
      // while mouse is moving
      case ttSetPen:
      case ttLine:
      case ttRelLine:
      case ttCorner:
      case ttFill:
      case ttPlot:
      case ttRectangle:
      case ttTrapezoid:
      case ttEllipse:
      case ttSelectArea:


      }

    case doSelectArea:
      // adjust selection bounds to match current mouse location

      // set shape anchor, based on relation of this point to actual anchor
      SelAnchor.X = IIf(PicPt.X < Anchor.X, PicPt.X, Anchor.X)
      SelAnchor.Y = IIf(PicPt.Y < Anchor.Y, PicPt.Y, Anchor.Y)
      // set selection parameters to match current selected area
      Selection = SelAnchor
      Selection.Width = Abs(CLng(PicPt.X) - Anchor.X) + 1
      Selection.Height = Abs(CLng(PicPt.Y) - Anchor.Y) + 1
      // draw the cmd selection box
      ShowCmdSelection

    case doLine or doShape or doMoveCmds or doMovePt:
      // only need to do something if cursor position has changed
      // since last time drawing surface was updated

      // if coordinates have changed,
      if (PicPt.X != CurPt.X || PicPt.Y != CurPt.Y) {

        // disable updating of redraw until done
        SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
        SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0

        // redraw picture
        DrawPicture

        // take action as appropriate
        switch (PicDrawMode) {
        case doLine:
          // action to take depends on what Type of line is being drawn
          switch (SelectedTool) {
          case ttLine:
            // draw current line up to anchor point
            DrawTempLine false, 0, 0
            // now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y

          case ttRelLine:
            // draw current line up to anchor point
            DrawTempLine false, 0, 0
            // validate x and Y
            // (note that delta x is limited to -6 to avoid
            // values above 0xF0, which would mistakenly be interpreted
            // as a new command)
            if (PicPt.X > Anchor.X + 7) {
              PicPt.X = Anchor.X + 7
            } else if (PicPt.X < Anchor.X - 6) {
              PicPt.X = Anchor.X - 6
            }
            if (PicPt.Y > Anchor.Y + 7) {
              PicPt.Y = Anchor.Y + 7
            } else if (PicPt.Y < Anchor.Y - 7) {
              PicPt.Y = Anchor.Y - 7
            }

            // now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y

          case ttCorner:
            // draw up to this coordinate
            DrawTempLine false, 0, 0
            // if drawing second point
            if (lstCoords.ListCount = 1) {
              // if mostly vertical
              if (Abs(CLng(PicPt.X) - Anchor.X) < Abs(CLng(PicPt.Y) - Anchor.Y)) {
                // command should be Y corner
                if (Asc(lstCommands.Text) != 89) {
                  lstCommands.List(SelectedCmd) = "Y Corner"
                  EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag) = dfYCorner
                  // correct last undo
                  UndoCol(UndoCol.Count).UDCmd = "Y Corner"
                }
                // limit change to vertical direction only
                PicPt.X = Anchor.X
              } else {
                // command should be X corner
                if (Asc(lstCommands.Text) = 89) {
                  lstCommands.List(SelectedCmd) = "X Corner"
                  EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag) = dfXCorner
                  // correct last undo
                  UndoCol(UndoCol.Count).UDCmd = "X Corner"
                }
                // limit change to horizontal direction only
                PicPt.Y = Anchor.Y
              }
            } else {
              // determine which direction to allow movement
              if ((Asc(lstCommands.Text) = 88 && (Int(lstCoords.ListCount / 2) = lstCoords.ListCount / 2)) ||
                 (Asc(lstCommands.Text) = 89 && (Int(lstCoords.ListCount / 2) != lstCoords.ListCount / 2))) {
                // limit change to vertical direction
                PicPt.X = Anchor.X
              } else {
                // limit change to horizontal direction
                PicPt.Y = Anchor.Y
              }
            }

            // now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y
          }

        case doShape:
          // action to take depends on what Type of line is being drawn
          switch (SelectedTool) {
          case ttRectangle:
            // simulate rectangle
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawBox Anchor.X, Anchor.Y, CurPt.X, CurPt.Y

          case ttTrapezoid:
            // simulate a trapezoid
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawLine Anchor.X, Anchor.Y, 159 - Anchor.X, Anchor.Y
            // ensure sloping side is on same side of picture
            if ((Anchor.X < 80 && PicPt.X < 80) || (Anchor.X >= 80 && PicPt.X >= 80)) {
             DrawLine 159 - Anchor.X, Anchor.Y, 159 - CurPt.X, CurPt.Y
              DrawLine 159 - CurPt.X, CurPt.Y, CurPt.X, CurPt.Y
              DrawLine CurPt.X, CurPt.Y, Anchor.X, Anchor.Y
            } else {
              DrawLine 159 - Anchor.X, Anchor.Y, CurPt.X, CurPt.Y
              DrawLine CurPt.X, CurPt.Y, 159 - CurPt.X, CurPt.Y
              DrawLine 159 - CurPt.X, CurPt.Y, Anchor.X, Anchor.Y
            }

          case ttEllipse:
            // simulate circle
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawCircle Anchor.X, Anchor.Y, CurPt.X, CurPt.Y
          }

        case doMoveCmds:
          // limit selection box movement to stay within picture bounds
          if (CLng(PicPt.X) - Delta.X < 0) {
            PicPt.X = Delta.X
          } else if (CLng(PicPt.X) - Delta.X + Selection.Width > 160) {
            PicPt.X = 160 - Selection.Width + Delta.X
          }
          if (CLng(PicPt.Y) - Delta.Y < 0) {
            PicPt.Y = Delta.Y
          } else if (CLng(PicPt.Y) - Delta.Y + Selection.Height > 168) {
            PicPt.Y = 168 - Selection.Height + Delta.Y
          }
          
          // now adjust selection start pos to match new location, then move selection box
          Selection.X = PicPt.X - Delta.X
          Selection.Y = PicPt.Y - Delta.Y
          
          // flicker can be reduced *a little* by forcing redraw here
          //  but the shape still disappears during movement, and only
          // re-appears when the mouse stops - don't know why
          SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
          ShowCmdSelection

        case doMovePt:
          // simulate new coordinate
          CurPt.X = PicPt.X
          CurPt.Y = PicPt.Y

          // if currently editing a line
          if (lstCommands.Text != "Fill" && lstCommands.Text != "Plot") {
            // draw temp line to include edited position
            DrawTempLine true, CurPt.X, CurPt.Y
          }

        }

        // refresh pictures
        SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
        SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
        picVisual.Refresh
        picPriority.Refresh
      }

    case doFill:
      // should never get here

    }

  case pmViewTest:
    // in test mode, all we need to do is provide feeback to user
    // on whether or not it's OK to drop the test object at this location

    // if above top edge OR
    //    (NOT ignoring horizon AND above horizon ) OR
    //    (on water AND restricting to land) OR
    //    (NOT on water AND restricting to water)
    if ((PicPt.Y - (CelHeight - 1) < 0) || (PicPt.Y < TestSettings.Horizon && !TestSettings.IgnoreHorizon) ||
       (EditPicture.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) && TestSettings.ObjRestriction = 2) ||
      (!EditPicture.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) && TestSettings.ObjRestriction = 1)) {
      // set cursor to NO
      SetCursors pcNO
    } else {
      // set cursor to normal
      SetCursors pcDefault
    }
  }

  // in some cases, the main form's status bar and menus
  // can get out of synch- so we test for that, and resynch
  // if necessary
  if (MainStatusBar.Tag != CStr(rtPicture)) {
 // *'Debug.Print "AdjustMenus 8"
    AdjustMenus rtPicture, InGame, true, IsChanged
    // then update it
    UpdateStatusBar
    return;
  }

  // *'Debug.Assert frmMDIMain.ActiveForm Is Me

  // when moving mouse, we always need to reset the status bar
    // if NOT in test mode OR statussrc is NOT testcoord mode (1)
    if (PicMode = pmEdit || StatusMode != psCoord) {
      switch (StatusMode) {
      case psPixel:
            //  normal pixel mode
        spCurX").Text = "X: " & CStr(PicPt.X)
        spCurY").Text = "Y: " & CStr(PicPt.Y)
        NewPri = GetPriBand(PicPt.Y, EditPicture.PriBase)
        spPriBand").Text = "Band: " & NewPri
        if (SelectedTool = ttSelectArea) {
          if (shpVis.Visible) {
            if (Button = vbLeftButton) {
            spAnchor").Visible = true
            spBlock").Visible = true
            spAnchor").Text = "Anchor: " & Selection.X & ", " & Selection.Y
            spBlock").Text = "Block: " & Selection.X & ", " & Selection.Y & ", " & Selection.X + Selection.Width - 1 & ", " & Selection.Y + Selection.Height - 1
            }
          } else {
  //           spAnchor").Text = "Anchor: " & PicPt.X & ", " & PicPt.Y
  //           spBlock").Text = "Block: " & PicPt.X & ", " & PicPt.Y
            spAnchor").Visible = false
            spBlock").Visible = false
          }
        }
      case psCoord:
            //  testcoord mode (never updated by mousemove)
            break;
      case psText:
            //  text row/col mode
        spCurX").Text = "R: " & CStr(Int(PicPt.Y / 8))
        spCurY").Text = "C: " & CStr(Int(PicPt.X / (CharWidth / 2)))
        NewPri = GetPriBand(PicPt.Y, EditPicture.PriBase)
        spPriBand").Text = "Band: " & NewPri
      }
      
      // if priority has changed, update the color box
      if (NewPri != OldPri) {
        spPriBand").Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, EditPicture.PriBase) - 3).Picture
      }
      OldPri = NewPri
    }
}

private void picVisual_MouseUp() {

  Dim PicPt As Point
  
  // if print preview, ignore
  if (PicMode = pmPrintTest) {
    return;
  }
  
  if (X < 0) {
    PicPt.X = 0
  } else if (X \ (2 * ScaleFactor) > 159) {
    PicPt.X = 159
  } else {
    PicPt.X = X \ (2 * ScaleFactor)
  }
  
  if (Y < 0) {
    PicPt.Y = 0
  } else if (Y \ ScaleFactor > 167) {
    PicPt.Y = 167
  } else {
    PicPt.Y = Y \ ScaleFactor
  }
  
  // how to handle mouseup event depends primarily on what was being drawn (or not)
  
  switch (PicDrawMode) {
  case doNone:
    // will occasionally get this case
    // such as when right-clicking or...
    
  // case doLine or doFill or doShape:
  // lines and shapes are not completed on mouse_up actions; they
  // are done by clicking to start, then clicking again to end
  // so it's the mouse-down action that both starts and ends the operation
  // that's why we don't need to check for them here in the MouseUp event
  
  case doSelectArea:
    // reset the draw mode
    PicDrawMode = doNone
    
  case doMovePt:
            // editing a coordinate
    // reset drawmode
    PicDrawMode = doNone
    
    // edit the coordinate
    EndEditCoord EditCmd, EditCoordNum, PicPt, lstCoords.Items[lstCoords.ListIndex].Tag, lstCoords.Text
    
    // update by re-building coordlist, and selecting
    BuildCoordList SelectedCmd
    lstCoords.ListIndex = EditCoordNum
    
  case doMoveCmds:
    // reset drawmode
    PicDrawMode = doNone
    
    // limit selection box movement to stay within picture bounds
    if (CLng(PicPt.X) - Delta.X < 0) {
      PicPt.X = Delta.X
    } else if (CLng(PicPt.X) - Delta.X + Selection.Width > 160) {
      PicPt.X = 160 - Selection.Width + Delta.X
    }
    if (CLng(PicPt.Y) - Delta.Y < 0) {
      PicPt.Y = Delta.Y
    } else if (CLng(PicPt.Y) - Delta.Y + Selection.Height > 168) {
      PicPt.Y = 168 - Selection.Height + Delta.Y
    }
    
    // move the command(s)
    MoveCmds SelectedCmd, lstCommands.SelCount, CLng(PicPt.X) - Anchor.X, CLng(PicPt.Y) - Anchor.Y
    
    // if a single cmd was being moved,
    if (lstCommands.SelCount = 1) {
      // update by re-building coordlist, and selecting
      BuildCoordList SelectedCmd
      CodeClick = true
      lstCommands_Click
      // keep highlighting single commands until something else selected
      GetSelectionBounds lstCommands.ListIndex, 1, true
    } else {
      // update by redrawing
      DrawPicture
      // reselect commands, then show selection box
      GetSelectionBounds SelectedCmd, lstCommands.SelCount, true
    }
    
    // restore cursor
    SetCursors pcEdit
      
  }
}

private void ShowTextPreview() {

  Dim i As Long, j As Long
  Dim charval As Long, Pos As Long
  Dim FmtText As String
  Dim tmpRow As Long, tmpCol As Long
  
  // get info from msg preview form
    MsgText = frmPicPrintPrev.txtMessage.Text
    MsgTop = CByte(frmPicPrintPrev.txtRow.Text)
    MsgLeft = CByte(frmPicPrintPrev.txtCol.Text)
    MsgMaxW = CByte(frmPicPrintPrev.txtMW.Text)
    if (PowerPack) {
      MsgBG = CByte(frmPicPrintPrev.cmbBG.ListIndex)
    } else {
      if (frmPicPrintPrev.cmbBG.ListIndex = 0) {
        MsgBG = 0
      } else {
        MsgBG = 15
      }
    }
    MsgFG = CByte(frmPicPrintPrev.cmbFG.ListIndex)
    if (frmPicPrintPrev.optPrint.Value = true) {
      TextMode = 0
      MsgMaxW = 30
    } else if (frmPicPrintPrev.optPrintAt.Value = true) {
      TextMode = 1
      MsgMaxW = CByte(frmPicPrintPrev.txtMW.Text)
    } else if (frmPicPrintPrev.optDisplay.Value = true) {
     TextMode = 2
    }
  
  // ''if priority only, won't work!!!
  
  // display
  if (TextMode = 2) {
    // compile the message
    FmtText = CompileMsg(MsgText)
    
    // validate starting pos
    if (MsgTop < PicOffset) {
      MsgBox "Text would appear above the picture image.", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    if (MsgTop > 20 + PicOffset) {
      MsgBox "Text would appear below the picture image.", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    
    // use picCopy to help manage fg/bg
    picCopy.Width = ScaleFactor * CharWidth
    picCopy.Height = ScaleFactor * 8
    picChar.Width = ScaleFactor * CharWidth
    picChar.Height = ScaleFactor * 8
    
    i = (MsgTop - PicOffset) * ScaleFactor * 8
    j = MsgLeft * ScaleFactor * CharWidth
    for (int Pos = 0; i < FmtText.Length; i++) {
      charval = Asc(Mid$(FmtText, Pos))
      if (charval = 10) {
        j = 0
        i = i + ScaleFactor * 8
      } else {
        // draw the character on vis
        StretchBlt picVisual.hDC, j, i, ScaleFactor * CharWidth, ScaleFactor * 8, picFont.hDC, (charval Mod 16) * 16, (charval \ 16) * 16, 16, 16, SRCCOPY
        // copy/invert it to picChar
        picChar.Cls
        BitBlt picChar.hDC, 0, 0, ScaleFactor * CharWidth, ScaleFactor * 8, picVisual.hDC, j, i, SRCINVERT
        // add bg to vis
        picCopy.BackColor = lngEGACol(MsgBG)
        BitBlt picVisual.hDC, j, i, ScaleFactor * CharWidth, ScaleFactor * 8, picCopy.hDC, 0, 0, SRCAND
        // add fg to surf
        picCopy.BackColor = lngEGACol(MsgFG)
        BitBlt picChar.hDC, 0, 0, ScaleFactor * CharWidth, ScaleFactor * 8, picCopy.hDC, 0, 0, SRCAND
        // combine them
        BitBlt picVisual.hDC, j, i, ScaleFactor * CharWidth, ScaleFactor * 8, picChar.hDC, 0, 0, SRCPAINT
        // advance cursor
        j = j + ScaleFactor * CharWidth
        if (j >= 320 * ScaleFactor) {
          j = 0
          i = i + ScaleFactor * 8
        }
      }
    }
    picVisual.Refresh
  
  // print/print.at
  } else {
    // format the message
    FmtText = FormatMsg(MsgText)
    
    // validate position
    if (MsgLeft < 2) {
      MsgBox "Message box would be off left edge of picture", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    if (MsgLeft + MsgWidth > (MaxCol - 1)) {
      MsgBox "Message box would be off right edge of picture", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    if (MsgTop < 1) {
      MsgBox "Message box would be off top edge of picture", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    if (MsgTop + MsgHeight > 20) {
      MsgBox "Message box would be off bottom edge of picture", vbInformation + vbOKOnly, "Invalid Position"
      return;
    }
    
    // draw the white bounding box (convert row/col to pixels,add border)
    picVisual.Line ((MsgLeft * CharWidth - 10) * ScaleFactor, ((MsgTop) * 8 - 5) * ScaleFactor)-Step((MsgWidth * CharWidth + 20) * ScaleFactor, (MsgHeight * 8 + 10) * ScaleFactor), lngEGACol(15), BF
    // draw red border
    picVisual.Line ((MsgLeft * CharWidth - 8) * ScaleFactor, ((MsgTop) * 8 - 4) * ScaleFactor)-Step((MsgWidth * CharWidth + 15) * ScaleFactor, ScaleFactor), lngEGACol(4), BF
    picVisual.Line ((MsgLeft * CharWidth - 8) * ScaleFactor, ((MsgTop + MsgHeight) * 8 + 3) * ScaleFactor)-Step((MsgWidth * CharWidth + 15) * ScaleFactor, ScaleFactor), lngEGACol(4), BF
    picVisual.Line ((MsgLeft * CharWidth - 8) * ScaleFactor, ((MsgTop) * 8 - 4) * ScaleFactor)-Step(ScaleFactor * 2, (MsgHeight * 8 + 8) * ScaleFactor), lngEGACol(4), BF
    picVisual.Line (((MsgLeft + MsgWidth) * CharWidth + 6) * ScaleFactor, ((MsgTop) * 8 - 4) * ScaleFactor)-Step(ScaleFactor * 2, (MsgHeight * 8 + 8) * ScaleFactor), lngEGACol(4), BF
    
    // draw text
    tmpRow = MsgTop
    tmpCol = MsgLeft
    
    for (int i = 0; i < FmtText.Length; i++) {
      
      charval = Asc(Mid$(FmtText, i))
      if (charval = 10) {
        // move to next line
        tmpRow = tmpRow + 1
        tmpCol = MsgLeft
      } else {
        // draw the character on vis
        StretchBlt picVisual.hDC, tmpCol * CharWidth * ScaleFactor, tmpRow * 8 * ScaleFactor, ScaleFactor * CharWidth, ScaleFactor * 8, picFont.hDC, (charval Mod 16) * 16, (charval \ 16) * 16, 16, 16, SRCCOPY
        tmpCol = tmpCol + 1
      }
    }
  }
}

private string CompileMsg(ByVal msg As String) {

  Dim i As Long
  Dim output As String, intChar As Integer
  Dim intMode As Long
  
  for (int i = 0; i < msg.Length; i++) {
    intChar = Asc(Mid$(msg, i))
    switch (intMode) {
    case 0:
            // normal
      if (intChar = 92) {
        intMode = 1
      } else {
        // add the char
        output = output & Chr$(intChar)
      }
      
    case 1:
            // back slash
      intMode = 0
      switch (intChar) {
      case 78 or 110:
            //  \n
        // add cr
        output = output & Chr$(10)
      default:
            // includes \\, \"
        // add the char
        output = output & Chr$(intChar)
      }
    }
  }
  
  CompileMsg = output
}

private string FormatMsg(ByVal msg As String) {
  
  Dim i As Long, lineWidth As Long
  Dim output As String, intChar As Integer
  Dim brk As Long, NewMax As Long, rowcount As Long
  Dim intMode As Long, blnESC As Boolean
  //  0=normal
  //  1=format
  
  // first, replace compiler format codes
  msg = CompileMsg(msg)
  
  // then, format to fit width
  intMode = 0
  output = ""
  rowcount = 1
  
  for (int i = 0; i < msg.Length; i++) {
    do {
      intChar = Asc(Mid$(msg, i))
      switch (intMode) {
      case 0:
            // normal
        if (blnESC) {
          blnESC = false
          // always add the char
          output = output & Chr$(intChar)
          // increment width
          lineWidth = lineWidth + 1
          if (intChar = 32) {
            // update the break
            brk = lineWidth
          }
        } else {
          switch (intChar) {
          case 10:
            // newline
            // add the char
            output = output & Chr$(intChar)
            // increment row
            rowcount = rowcount + 1
            // update actual max
            if (lineWidth > NewMax) {
              NewMax = lineWidth
            }
            // reset width
            lineWidth = 0
            // reset break
            brk = 0
            
          case 32:
            // space
            // add the char
            output = output & Chr$(intChar)
            // increment width
            lineWidth = lineWidth + 1
            // update the break
            brk = lineWidth
            
          case 37:
            // %
            // check for percent format code
            intMode = 2
            break; // exit do
          case 92:
            // \
            // esc code
            blnESC = true
            break; // exit do
          default:
            // add the char
            output = output & Chr$(intChar)
            // increment width
            lineWidth = lineWidth + 1
          }
        }
        
      case 1:
            // % format code
        intMode = 0
      // agiTODO
      
      }
      
      if (lineWidth = MsgMaxW) {
        if (brk = 0) {
          // add break here
          output = output & Chr$(10)
          rowcount = rowcount + 1
          // update actual max
          if (lineWidth > NewMax) {
            NewMax = lineWidth
          }
          // reset width
          lineWidth = 0
        } else {
          // insert cr at break, clearing ending space
          output = Left$(output, Len(output) - lineWidth + brk - 1) & Chr$(10) & Right$(output, lineWidth - brk)
          rowcount = rowcount + 1
          // update actual max
          if (brk - 1 > NewMax) {
            NewMax = brk - 1
          }
          // reset width
          lineWidth = lineWidth - brk
          // reset break
          brk = 0
        }
      }
    } while (false);
  }
  // update actual max on last row
  if (lineWidth > NewMax) {
    NewMax = lineWidth
  }
  
  // save height/width
  MsgHeight = rowcount
  MsgWidth = NewMax
  
  if (TextMode = 0) {
    // calculate msgbox top/left
    
    // texttop=(maxH-1-height)/2+1
    MsgTop = Int((20 - 1 - rowcount) / 2) + 1
    // textleft=(screenwidth-textwidth)/2
    MsgLeft = Int(((MaxCol + 1) - NewMax) / 2)
    // textbottom=texttop+rowcount-1
    // textright=textleft+textwidth
  }
  
  // return formatted string
  FormatMsg = output
}

private void AddCelToPic() {
  // mask copies the test Image onto the visual screen
  // 
  // also adds the test Image to the priority screen, if visible
  Dim i As Long, j As Long, X As Byte, Y As Byte
  Dim CelPri As Long
  Dim PixelPri As Long // PixelPri is pixel priority, determined by finding non-control line priority closest to this pixel
  Dim PriPixel As Long // PriPixel is actual pixel Value of priority screen
  Dim CelPixel As Long
  Dim TestCel As AGICel
  
  // set testcel
  Set TestCel = TestView.Loops(CurTestLoop).Cels(CurTestCel)
  
  // we are using picVisSurface and picPriSurface to temporarily hold bmps
  // while we add the cels; (remember they're offset by 5 pixels in both directions
  // so the temp bitmaps don't show through)
  
  // set priority (if in auto, get priority from current band)
  if (TestSettings.ObjPriority < 16) {
    CelPri = TestSettings.ObjPriority
  } else {
    // calculate band incode, for speed
    //     (y - base) / (168 - base) * 10 + 5
    if (TestCelPos.Y < EditPicture.PriBase) {
      CelPri = 4
    } else {
      CelPri = Int((CLng(TestCelPos.Y) - EditPicture.PriBase) / (168 - EditPicture.PriBase) * 10) + 5
    }
// *'Debug.Assert CelPri >= 4
  }

  // reposition just in case any portion of view is off screen
  if (TestCelPos.Y - (CelHeight - 1) < 0) {
    TestCelPos.Y = CelHeight - 1
  }
  if (TestCelPos.X + CelWidth > 160) {
    TestCelPos.X = 160 - CelWidth
  }
  
  for (int i = 0; i < CelWidth; i++) {
    for (int j = 0; j < CelHeight; j++) {
      X = TestCelPos.X + i
      Y = TestCelPos.Y - (CelHeight - 1) + j
      // get cel pixel color
      CelPixel = TestCelData(i, j)
      // if not a transparent cel
      if (CelPixel != CelTrans) {
        // get pixelpri
        PixelPri = EditPicture.PixelPriority(X, Y)
        PriPixel = EditPicture.PriPixel(X, Y)
        if (VisVisible) {
          // if priority of cel is equal to or higher than priority of pixel
          if (CelPri >= PixelPri) {
            // set this pixel on visual screen
            SetPixelV picVisDraw.hDC, CLng(X), CLng(Y), EGAColor(CelPixel)
          }
        }
        if (PriVisible) {
          if (CelPri >= PixelPri && PriPixel >= 3) {
            // set this pixel on priority screen
            SetPixelV picPriDraw.hDC, CLng(X), CLng(Y), EGAColor(CelPri)
          }
        }
      }
    }
  }
  
  
  // if status bar is showing object info
  if (StatusMode = psCoord && PicMode = pmViewTest) {
    //  update status bar ONLY if this editor is active
    if (frmMDIMain.ActiveForm Is Me) {
        if (MainStatusBar.Tag != CStr(rtPicture)) {
          AdjustMenus rtPicture, InGame, true, IsChanged
        }
        // use test object position
        spCurX").Text = "vX: " & CStr(TestCelPos.X)
        spCurY").Text = "vY: " & CStr(TestCelPos.Y)
        spPriBand").Text = "vBand: " & GetPriBand(TestCelPos.Y, EditPicture.PriBase)
        SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 0, 0
        spPriBand").Picture = imlPriBand.ListImages(GetPriBand(TestCelPos.Y, EditPicture.PriBase) - 3).Picture
        SendMessage MainStatusBar.hWnd, WM_SETREDRAW, 1, 0
    }
  }
}

private void tbSize_ButtonClick() {

  // set plot size
  SelectedPen.PlotSize = Button.Index - 1
  
  // set button face on main toolbar
  Toolbar1.Buttons("size").Image = Button.Image
  
  tbSize.Visible = false
  
  // refresh pens
  RefreshPens
}


private void tbStyle_ButtonClick() {

  switch (Button.Key) {
  case "SolidSquare":
    SelectedPen.PlotStyle = psSolid
    SelectedPen.PlotShape = psRectangle
    
  case "SplatSquare":
    SelectedPen.PlotStyle = psSplatter
    SelectedPen.PlotShape = psRectangle
    
  case "SolidCircle":
    SelectedPen.PlotStyle = psSolid
    SelectedPen.PlotShape = psCircle
    
  case "SplatCircle":
    SelectedPen.PlotStyle = psSplatter
    SelectedPen.PlotShape = psCircle
    
  }
  
  // change style button face on main toolbar
  Toolbar1.Buttons("style").Image = Button.Image
  
  // hide the style flyout toolbar
  tbStyle.Visible = false
  
  // refresh pens
  RefreshPens
}

private void tmrTest_Timer() {
  // timer1 controls test view movement
  
  Dim rtn As Long
    
  Dim ControlLine As AGIColors, OnWater As Boolean
  Dim NewX As Byte, NewY As Byte
  Dim DX As Long, DY As Long
  
  Dim TestCel As AGICel
  
  rtn = GetTickCount()
  // *'Debug.Assert CurTestLoopCount != 0
  
  // if cel not set
  if (TestSettings.TestCel = -1) {
    // increment cel
    CurTestCel = CurTestCel + 1
    // if at loopcount, reset back to zero
    if (CurTestCel = CurTestLoopCount) {
      CurTestCel = 0
    }
    TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
  }
  
  Set TestCel = TestView.Loops(CurTestLoop).Cels(CurTestCel)
  
  // set cel height/width/transcolor
  CelWidth = TestCel.Width
  CelHeight = TestCel.Height
  CelTrans = TestCel.TransColor
  
  // use do loop to control flow
  // (need to remember to exit do after setting NewX, NewY, TestDir and StopReason)
  while (true) {
    // assume no movement
    NewX = TestCelPos.X
    NewY = TestCelPos.Y
    
    // check for special case of no motion
    if (TestDir = odStopped) {
      // cycle in place
      break; // exit do
    }
    
    // calculate dX and dY based on direction
    // (these are empirical formulas based on relationship between direction and change in x/Y)
    DX = Sgn(5 - TestDir) * Sgn(TestDir - 1)
    DY = Sgn(3 - TestDir) * Sgn(TestDir - 7)
  
    // test for edges
    switch (TestDir) {
    case odUp:
      // if on horizon and not ignoring,
      if ((NewY = TestSettings.Horizon) && !TestSettings.IgnoreHorizon) {
        // dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // if at top
      if (NewY - (CelHeight - 1) <= 0) {
        // dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX, NewY - 1, CelWidth)
    
    case odUpRight:
      // if on horizon and not ignoring,
      if ((NewY = TestSettings.Horizon) && !TestSettings.IgnoreHorizon) {
        // dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // if at top
      if (NewY - (CelHeight - 1) <= 0) {
        // dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // if at right edge,
      if (NewX + CelWidth - 1 >= 159) {
        // dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX + 1, NewY - 1, CelWidth)
      
    case odRight:
      // if at right edge
      if (NewX + CelWidth - 1 >= 159) {
        // dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX + CelWidth, NewY)
      
    case odDownRight:
      // if at bottom edge
      if (NewY = 167) {
        // dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
        
      // if at right edge
      if (NewX + CelWidth - 1 = 159) {
        // dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX + 1, NewY + 1, CelWidth)
        
    case odDown:
      // if at bottom
      if (NewY = 167) {
        // dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX, NewY + 1, CelWidth)
        
    case odDownLeft:
      // if at bottom
      if (NewY = 167) {
        // dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      
      // if at left edge
      if (NewX = 0) {
        // stop motion
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX - 1, NewY + 1, CelWidth)
        
     
    case odLeft:
      // if at left edge
      if (NewX = 0) {
        // dont go
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX - 1, NewY)
        
      
    case odUpLeft:
      // if on horizon or at left edge,
      if (((NewY = TestSettings.Horizon) && !TestSettings.IgnoreHorizon) || NewX = 0) {
        // dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      
      // if at top
      if (NewY - (CelHeight - 1) <= 0) {
        // dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      
      // if at left edge
      if (NewX = 0) {
        // dont go
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        break; // exit do
      }
      
      // get controlline status
      ControlLine = EditPicture.PixelControl(NewX - 1, NewY - 1, CelWidth)
    }
    
    // get control line and onwater status
    OnWater = EditPicture.ObjOnWater(NewX + DX, NewY + DY, CelWidth)

    // if at an obstacle line OR (at a conditional obstacle line AND NOT blocking)
    if ((ControlLine <= 1) && !TestSettings.IgnoreBlocks) {
      // don't go
      StopReason = ControlLine + 1
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      break; // exit do
    }
    
    // if restricting access to land AND at water edge*****no, on water!
    if ((TestSettings.ObjRestriction = 2) && OnWater) { // (ControlLine = 3)) {
      // need to go back!
      // don't go
      StopReason = 5
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      break; // exit do
    }
    
    // if restricting access to water AND at land edge
    if ((TestSettings.ObjRestriction = 1) && !OnWater) {
      // don't go
      StopReason = 6
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      break; // exit do
    }
      
    // ok to move
    NewX = NewX + DX
    NewY = NewY + DY
    
    // if on water, set status
    StopReason = IIf(OnWater, 4, 0)
    
    // if at an alarm line
    if (ControlLine = 2) {
      // stop motion
      StopReason = 3
      TestDir = 0
      tmrTest.Enabled = TestSettings.CycleAtRest
    }
    // exit to draw the cel
    break; // exit do
  }
  
  // update current position
  TestCelPos.X = NewX
  TestCelPos.Y = NewY
  
  // draw cel
  DrawPicture
  
  // manage status bar - if stopped, show reason why
  // if moving, clear status bar
  
  if (StopReason != 0) {
    // update status bar
    UpdateStatusBar
  } else {
    // if testdir is anything but stopped, clear the panel
    if (TestDir != odStopped) {
      if (frmMDIMain.ActiveForm Is Me) {
        spTool").Text = vbNullString
      }
    }
  }
}


private void tmrSelect_Timer() {
  // cursor and selection timer
  
  // cycle current pixel through all colors
  Static VCColor As AGIColors
  Static CurSize As Single
  
  Dim cOfX As Single, cOfY As Single, cSzX As Single, cSzY As Single
  cOfX = 1.5 / ScaleFactor ^ 0.5
  cOfY = cOfX * 2 // 3 / ScaleFactor ^ 0.5
  cSzX = cOfX * 2 // 3 / ScaleFactor ^ 0.5
  cSzY = cOfY * 2 // 6 / ScaleFactor ^ 0.5
  
  // if selection shape is visible
  if (shpVis.Visible) {
    // toggle shape style
    shpVis.BorderStyle = shpVis.BorderStyle + 1
    if (shpVis.BorderStyle = 6) {
      shpVis.BorderStyle = 2
    }
    shpPri.BorderStyle = shpPri.BorderStyle + 1
    if (shpPri.BorderStyle = 6) {
      shpPri.BorderStyle = 2
    }
  
  } else {
    // toggle cursor
    VCColor = IIf(VCColor < 15, VCColor + 1, 0)
    
    // if using original WinAGI flashing cursor
    if (CursorMode = pcmWinAGI) {
      // if cursor is on the selected point
      if (OnPoint) {
        // size is always 0
        CurSize = 0
      } else {
        CurSize = CurSize + 0.5
        if (CurSize > 1) {
          CurSize = 0
        }
      }
      
      // if visual is enabled
      if (CurrentPen.VisColor < agNone) {
        picVisual.Line ((CurPt.X - CurSize) * ScaleFactor * 2, (CurPt.Y - CurSize) * ScaleFactor)-Step((2 * CurSize + 1) * ScaleFactor * 2 - 1, ((2 * CurSize + 1) * ScaleFactor - 1)), EGAColor(VCColor), BF
      }
      
      // if priority is enabled,
      if (CurrentPen.PriColor < agNone) {
        picPriority.Line ((CurPt.X - CurSize) * ScaleFactor * 2, (CurPt.Y - CurSize) * ScaleFactor)-Step((2 * CurSize + 1) * ScaleFactor * 2 - 1, ((2 * CurSize + 1) * ScaleFactor - 1)), EGAColor(VCColor), BF
      }
    } else {
      // if using 'x' marks:
      // draw a box
      if (CurrentPen.VisColor < agNone) {
        picVisual.Line ((CurPt.X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CurPt.Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), EGAColor(VCColor), B
      }
      if (CurrentPen.PriColor < agNone) {
        picPriority.Line ((CurPt.X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CurPt.Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), EGAColor(VCColor), B
      }
    }
  }
}

private void Toolbar1_ButtonClick() {

  Dim blnCursor As Boolean, blnClearCmdList As Boolean
  Dim PrevTool As TPicToolTypeEnum
  
//      button parameters:
//   Index  Tip               Key
//     1    Undo              undo
//     2    Print Test        printtest
//     3    separator
//     4    Edit              edit
//     5    View Test         test
//     6    separator
//     7    Set Bkgd          bkgd
//     8    Enable Full Draw  full
//     9    Zoom In           zoomin
//    10    Zoom Out          zoomout
//    11    separator
//    12    Cut               cut
//    13    Copy              copy
//    14    Paste             paste
//    15    Delete            delete
//    16    Flip Horizontal   fliph
//    17    Flip Vertical     flipv
//    18    separator
//    19    Edit Select       editsel
//    20    Select            select
//    21    Abs Line          absline
//    22    Rel Line          relline
//    23    Corner            corner
//    24    Rectangle         rectangle
//    25    Trapezoid         floor
//    26    Ellipse           ellipse
//    27    Fill              fill
//    28    Plot              plot
//    29    separator
//    30    Style             style
//    31    Size              size
  
  // what's the tool before we change?
  PrevTool = SelectedTool
  
  if (!blnWheel) {
    // if drawing
    if (PicDrawMode != doNone) {
      StopDrawing
    }
  
    // if current tool is Edit-Area then ALWAYS reset selection
    // whenever a toolbar button other than zoomin or zoomout is pressed
    if (SelectedTool = ttSelectArea && (Button.Index != 8 && Button.Index != 9)) {
      // reset selection
      Selection.X = 0
      Selection.Y = 0
      Selection.Width = 0
      Selection.Height = 0
      ShowCmdSelection
    }
  }
  
  switch (Button.Key) {
  case "undo":
    MenuClickUndo
    return;
    
  case "select":
            // Select Command tool
    SelectedTool = ttEdit
    // normal cursor
    SetCursors pcEdit
    // reset selection
    Selection.X = 0
    Selection.Y = 0
    Selection.Width = 0
    Selection.Height = 0
    ShowCmdSelection
    // reset menus
    SetEditMenu
    blnClearCmdList = true
  
  case "editsel":
            // Select Area tool
    SelectedTool = ttSelectArea
    // area select cursor
    SetCursors pcEditSel
    
    // if current point is highlighted,
    if (tmrSelect.Enabled) {
      // in original mode, need to reset area under cursor
      if (CursorMode = pcmWinAGI) {
        // reset area under cursor
        BitBlt picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 0, SRCCOPY
        BitBlt picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 12, SRCCOPY
        picVisual.Refresh
        picPriority.Refresh
      }
      // then disable cursor highlighting
      tmrSelect.Enabled = false
    }
    
    // reset selection
    Selection.X = 0
    Selection.Y = 0
    Selection.Width = 0
    Selection.Height = 0
    ShowCmdSelection
    // reset menus
    SetEditMenu
    blnClearCmdList = true
    
  case "absline":
            // Draw Line tool
    SelectedTool = ttLine
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "relline":
            // Draw Relative Line tool
    SelectedTool = ttRelLine
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "corner":
            // Draw Corner Line tool
    SelectedTool = ttCorner
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "rectangle":
            // Draw Rectangle tool
    SelectedTool = ttRectangle
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "floor":
            // Draw Trapezoid tool
    SelectedTool = ttTrapezoid
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "ellipse" // Draw Ellipse tool
    SelectedTool = ttEllipse
    SetCursors pcSelect
    blnClearCmdList = true
    
  case "fill":
            // Fill tool
    SelectedTool = ttFill
    SetCursors pcPaint
    blnClearCmdList = true
    
  case "plot":
            // Plot Pen tool
    SelectedTool = ttPlot
    // should select a cursor that matches current brush style, shape, size
    SetCursors pcBrush
    blnClearCmdList = true
    
  case "style":
            // Change Pen Style
    // position toolbar next to its main toolbar button
    tbStyle.Left = Button.Left + Toolbar1.Left + Button.Width
    // if not enough room below the main button,
    if (Button.Top - tbStyle.Buttons(1).Top + tbStyle.Height > picPalette.Top) {
      // put it as far down as possible
      tbStyle.Top = CalcHeight - picPalette.Height - tbStyle.Height
    } else {
      tbStyle.Top = Button.Top - tbStyle.Buttons(1).Top
    }
    // show it
    tbStyle.Visible = true
    return;
    
  case "size":
            // Change Pen Size
    // position toolbar next to its main toolbar button
    tbSize.Left = Button.Left + Button.Width + Toolbar1.Left
    // if not enough room below the main button,
    if (Button.Top - tbSize.Buttons(1).Top + tbSize.Height > picPalette.Top) {
      // put it as far down as possible
      tbSize.Top = CalcHeight - picPalette.Height - tbSize.Height
    } else {
      tbSize.Top = Button.Top - tbSize.Buttons(1).Top
    }
    // show it
    tbSize.Visible = true
    return;
    
  
  case "bkgd":
            // Toggle Background Image
    // turn background on or off
    ToggleBkgd !EditPicture.BkgdShow
    
    // if current command has coordinates, do more than just redraw picture
    if (lstCoords.ListCount > 0) {
      if (lstCoords.ListIndex != -1) {
        // use coordinate click method if a coordinate is currently selected
        lstCoords_Click
      } else {
        // use command click method if no coordinates selected
        CodeClick = true
        lstCommands_Click
      }
    } else {
      // if selected command doesn't have any coordinates
      // redrawing is sufficient to set correct state of editor
      DrawPicture
    }
    
  case "full":
            // Toggle StepDraw
    // set flag to show full picture or individual steps depending on button status
    EditPicture.StepDraw = (Button.Value = tbrUnpressed)
    
    // if current command has coordinates, do more than just redraw picture
    if (lstCoords.ListCount > 0) {
      if (lstCoords.ListIndex != -1) {
        // use coordinate click method if a coordinate is currently selected
        lstCoords_Click
      } else {
        // use command click method if no coordinates selected
        CodeClick = true
        lstCommands_Click
      }
    } else {
      // if selected command doesn't have any coordinates
      // redrawing is sufficient to set correct state of editor
      DrawPicture
    }
    
  case "cut":
            // Edit-Cut
    MenuClickCut
    return;
    
  case "copy":
            // Edit-Copy
    MenuClickCopy
    return;
    
  case "paste":
            // Edit-Paste
    MenuClickPaste
    return;
    
  case "delete":
            // Edit-Delete
    MenuClickDelete
    return;
    
  case "fliph":
            // Edit-Flip Horizontal
    FlipCmds SelectedCmd, lstCommands.SelCount, 0
    // redraw
    DrawPicture
    // if only one cmd,
    if (lstCoords.ListCount = 1) {
      // rebuld coord list
      BuildCoordList SelectedCmd
    }
    
  case "flipv":
            // Edit-Flip Vertical
    FlipCmds SelectedCmd, lstCommands.SelCount, 1
    DrawPicture
    // if only one cmd,
    if (lstCoords.ListCount = 1) {
      // rebuld coord list
      BuildCoordList SelectedCmd
    }
    
  }
  
  // *'Debug.Assert MainStatusBar.Tag = rtPicture
  if (MainStatusBar.Tag != rtPicture) {
    // show picture menus, and enable editing
 // *'Debug.Print "AdjustMenus 10"
    AdjustMenus rtPicture, InGame, true, IsChanged
  }
  
  // show/hide anchor and block status panels
  spAnchor").Visible = (SelectedTool = ttSelectArea)
  if ((SelectedTool = ttSelectArea)) {
    spAnchor").Text = "Anchor:"
  }
  spBlock").Visible = (SelectedTool = ttSelectArea)
  if ((SelectedTool = ttSelectArea)) {
    spBlock").Text = "Block:"
  }
  
  // if a tool was selected OR if mode changed from edit to draw
  // need to clear command list
  if (blnClearCmdList) {
    // if multiple selections
    if (lstCommands.SelCount > 1) {
      // use selectcmd method to force single selection
      SelectCmd lstCommands.ListIndex
      // reset selection
      Selection.X = 0
      Selection.Y = 0
      Selection.Width = 0
      Selection.Height = 0
      ShowCmdSelection
      
    // if a coord is selected
    } else if (lstCoords.ListIndex != -1) {
      // unselect it
      lstCoords.ListIndex = -1
      CurPt.X = 255
      CurPt.Y = 255
      lstCommands.SetFocus
      // if previous tool was 'none', then
      // cursor x's need to be hidden
      if (PrevTool = ttEdit) {
        // draw picture to eliminate cursor
        DrawPicture
      }
        
    // if using 'x' marker cursor mode, still
    // need to enable/disable the'x's based on selected tool
    } else if (CursorMode = pcmXMode) {
      if (SelectedTool = ttEdit) {
        // only if in edit mode
        if (PicMode = pmEdit) {
          HighlightCoords
        }
      } else {
        // if previous tool was 'none', then
        // cursor x's need to be hidden
        if (PrevTool = ttEdit) {
          // draw picture to eliminate cursor
          DrawPicture
        }
      }
    }
  }
  
  // if a cmd is selected and no selection window is visible
  if (lstCommands.ListIndex != -1 && (Selection.Width = 0 || Selection.Height = 0)) {
    // is cursor flashing?
    blnCursor = tmrSelect.Enabled

    // enable cursor highlighting if edit tool selected
    tmrSelect.Enabled = (SelectedTool = ttEdit) && lstCoords.ListIndex != -1
    if (tmrSelect.Enabled && CursorMode = pcmWinAGI) {
      // save area under cursor
      BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
      BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    }

    // if cursor was enabled, but isn't now,
    if (blnCursor && !tmrSelect.Enabled) {
      // draw picture to eliminate cursor
      DrawPicture
    }
  } else {
    // if no selection, turn off timer
    if (Selection.Width = 0 || Selection.Height = 0) {
      tmrSelect.Enabled = false
    }
  }
  
  UpdateToolBar
}

            */
        }
        #endregion

        public frmPicEdit() {
            InitializeComponent();
            Application.AddMessageFilter(this);
            Disposed += (sender, e) => Application.RemoveMessageFilter(this);
            InitFonts();

            MdiParent = MDIMain;
            // re-build toolstrips to show only buttons
            ToolStripDropDown tddMode = new();
            tddMode.ImageScalingSize = new Size(24, 24);
            tddMode.Items.Add(tsbEditMode);
            tddMode.Items.Add(tsbViewTest);
            tddMode.Items.Add(tsbPrintTest);
            tddMode.ItemClicked += tsbModeItemClicked;
            tsbMode.DropDown = tddMode;
            tdTools = new();
            tdTools.ImageScalingSize = new Size(24, 24);
            tdTools.Items.Add(tsbSelect);
            tdTools.Items.Add(tsbEditSelect);
            tdTools.Items.Add(tsbLine);
            tdTools.Items.Add(tsbRelLine);
            tdTools.Items.Add(tsbStepLine);
            tdTools.Items.Add(tsbBox);
            tdTools.Items.Add(tsbTrapezoid);
            tdTools.Items.Add(tsbEllipse);
            tdTools.Items.Add(tsbFill);
            tdTools.Items.Add(tsbPlot);
            tsbTool.DropDown = tdTools;
            tdPlotStyle = new();
            tdPlotStyle.ImageScalingSize = new Size(24, 24);
            tdPlotStyle.Items.Add(tsbCircleFull);
            tdPlotStyle.Items.Add(tsbSquareFull);
            tdPlotStyle.Items.Add(tsbCircleSplat);
            tdPlotStyle.Items.Add(tsbSquareSplat);
            tsbPlotStyle.DropDown = tdPlotStyle;
            tdPlotSize = new();
            tdPlotSize.ImageScalingSize = new Size(24, 24);
            tdPlotSize.Items.Add(tsbSize0);
            tdPlotSize.Items.Add(tsbSize1);
            tdPlotSize.Items.Add(tsbSize2);
            tdPlotSize.Items.Add(tsbSize3);
            tdPlotSize.Items.Add(tsbSize4);
            tdPlotSize.Items.Add(tsbSize5);
            tdPlotSize.Items.Add(tsbSize6);
            tdPlotSize.Items.Add(tsbSize7);
            tsbPlotSize.DropDown = tdPlotSize;

            // other initializations:
            SelectedPen.VisColor = AGIColorIndex.None;
            SelectedPen.PriColor = AGIColorIndex.None;
            picVisual.MouseWheel += picVisual_MouseWheel;
            picPriority.MouseWheel += picPriority_MouseWheel;

            hsbVisual.Visible = false;
            vsbVisual.Visible = false;
            hsbVisual.BringToFront();
            vsbVisual.BringToFront();

            // defaults
            ScaleFactor = WinAGISettings.PicScaleEdit.Value;
            picVisual.Width = picPriority.Width = (int)(ScaleFactor * 320);
            picVisual.Height = picPriority.Height = (int)(ScaleFactor * 168);
            ShowBands = WinAGISettings.ShowBands.Value;
            CursorMode = (EPicCursorMode)WinAGISettings.CursorMode.Value;

            PicMode = EPicMode.pmEdit;
            SelectedTool = TPicToolTypeEnum.ttEdit;

            // set undo collection
            PictureUndo[] UndoCol = [];

            /*
 
  Dim rtn As Long
  
  // get default pic test settings
    TestSettings.ObjSpeed = ReadSettingLong(SettingsList, sPICTEST, "Speed", DEFAULT_PICTEST_OBJSPEED)
      if (TestSettings.ObjSpeed < 0) { TestSettings.ObjSpeed = 0
      if (TestSettings.ObjSpeed > 3) { TestSettings.ObjSpeed = 3
    TestSettings.ObjPriority = ReadSettingLong(SettingsList, sPICTEST, "Priority", DEFAULT_PICTEST_OBJPRIORITY)
      if (TestSettings.ObjPriority < 4) { TestSettings.ObjPriority = 4
      if (TestSettings.ObjPriority > 16) { TestSettings.ObjPriority = 16
    TestSettings.ObjRestriction = ReadSettingLong(SettingsList, sPICTEST, "Restriction", DEFAULT_PICTEST_OBJRESTRICTION)
      if (TestSettings.ObjRestriction < 0) { TestSettings.ObjRestriction = 0
      if (TestSettings.ObjRestriction > 2) { TestSettings.ObjRestriction = 2
    TestSettings.Horizon = ReadSettingLong(SettingsList, sPICTEST, "Horizon", DEFAULT_PICTEST_HORIZON)
      if (TestSettings.Horizon < 0) { TestSettings.Horizon = 0
      if (TestSettings.Horizon > 167) { TestSettings.Horizon = 167
    TestSettings.IgnoreHorizon = ReadSettingBool(SettingsList, sPICTEST, "IgnoreHorizon", DEFAULT_PICTEST_IGNOREHORIZON)
    TestSettings.IgnoreBlocks = ReadSettingBool(SettingsList, sPICTEST, "IgnoreBlocks", DEFAULT_PICTEST_IGNOREBLOCKS)
    TestSettings.CycleAtRest = ReadSettingBool(SettingsList, sPICTEST, "CycleAtRest", DEFAULT_PICTEST_CYCLEATREST)
    TestSettings.TestCel = -1
    TestSettings.TestLoop = -1
    // set timer based on speed
    switch (TestSettings.ObjSpeed) {
    case 0:
            // slow
      tmrTest.Interval = 200
    case 1:
            // normal
      tmrTest.Interval = 50
    case 2:
            // fast
      tmrTest.Interval = 13
    case 3:
            // fastest
      tmrTest.Interval = 1
    }
  
  // default message test settings
  TextMode = 0
  MsgText = ""
  MsgTop = 0
  MsgLeft = 0
  MsgMaxW = 30
  MsgBG = 0
  MsgFG = 15
  PicOffset = 1
  */
            if (InGame && EditGame.PowerPack) {
                // default to 80 in powerpack mode
                MaxCol = 79;
                CharWidth = 4;
            }
            else {
                MaxCol = 39;
                CharWidth = 8;
            }
  /*
  if (ValidCodePages.Contains(SessionCodePage)) {
    Set picFont.Picture = LoadResPicture("CP" & CStr(SessionCodePage), vbResBitmap)
  } else {
    // should never happen, but just in case
    Set picFont.Picture = LoadResPicture("CP437", vbResBitmap)
  }
            */

        }

        internal void InitFonts() {
            lstCommands.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            lstCoords.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
        }

        public bool PreFilterMessage(ref Message m) {
            // in splitcontainers, mousewheel auto-scrolls the vertical scrollbar
            // only way to stop it is to catch the mousewheel message
            // and handle it manually
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_MOUSEWHEEL) {
                //if (Control.FromHandle(m.HWnd) is Control control && InSplitContainer(control)) {
                if (Control.FromHandle(m.HWnd) is Control control) {
                    if (control == picVisual) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000) >> 16;
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000) >> 16;
                        picVisual_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
                    }
                    else if (control == picPriority) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000);
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000);
                        picPriority_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
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

        private void tsbModeItemClicked(object sender, ToolStripItemClickedEventArgs e) {

        }

        #region Event Handlers
        #region Form Event Handlers
        private void frmPicEdit_Load(object sender, EventArgs e) {

        }

        private void frmPicEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmPicEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // dereference picture
            EditPicture?.Unload();
            EditPicture = null;
            // remove from PicEditor collection
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm == this) {
                    PictureEditors.Remove(frm);
                    break;
                }
            }

            //// if a test view is currently loaded,
            //if (TestView != null) {
            //    // unload it and release it
            //    TestView.Unload();
            //    TestView = null;
            //}

            //// destroy background picture
            //BkgdImage = null;
        }

        private void frmPicEdit_Resize(object sender, EventArgs e) {
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
                mnuRInGame.Text = "Add Picture to Game";
                mnuRRenumber.Enabled = false;
                // mnuRProperties no change
                // mnuRSavePicImage no change
                // mnuRExportGIF no change
                // mnuRBackground no change
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export Picture" : "Save As ...";
                mnuRInGame.Enabled = true;
                mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                mnuRRenumber.Enabled = InGame;
                // mnuRProperties no change
                // mnuRSavePicImage no change
                // mnuRExportGIF no change
                // mnuRBackground no change
            }
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            SavePicture();
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportPicture();
        }

        public void mnuRInGame_Click(object sender, EventArgs e) {
            ToggleInGame();
        }

        private void mnuRRenumber_Click(object sender, EventArgs e) {
            RenumberPicture();
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditPictureProperties(1);
        }

        private void mnuRSavePicImage_Click(object sender, EventArgs e) {
            ExportOnePicImg(EditPicture);
        }

        private void mnuRBackground_Click(object sender, EventArgs e) {
            MessageBox.Show("TODO: background");
        }
        private void mnuRExportGIF_Click(object sender, EventArgs e) {
            ExportPicAsGif(EditPicture);
        }

        private void cmEdit_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange([mnuUndo, mnuESep0, mnuCut, mnuCopy,
                mnuPaste, mnuDelete, mnuClearPicture, mnuSelectAll, mnuESep1,
                mnuInsertCoord, mnuSplitCommand, mnuJoinCommands, mnuFlipH, mnuFlipV, mnuESep2,
                mnuEditMode, mnuViewTestMode, mnuTextTestMode, mnuESep3,
                mnuSetTestView, mnuTestViewOptions, mnuTestTextOptions,
                mnuTestPrintCommand, mnuTextScreenSize, mnuESep4,
                mnuToggleScreen, mnuToggleBands, mnuEditPriBase,
                mnuToggleTextMarks, mnuESep5,
                mnuToggleBackground, mnuEditBackground, mnuRemoveBackground]);
            SetEditMenu();
        }

        private void SetEditMenu() {
            // mode
            mnuEditMode.Checked = PicMode == EPicMode.pmEdit;
            mnuESep3.Visible = !(PicMode == EPicMode.pmEdit);
            mnuViewTestMode.Checked = PicMode == EPicMode.pmViewTest;
            mnuTextTestMode.Checked = PicMode == EPicMode.pmPrintTest;
            mnuSetTestView.Visible =
                mnuTestViewOptions.Visible =
                PicMode == EPicMode.pmViewTest;
            mnuTestTextOptions.Visible =
                mnuTestPrintCommand.Visible =
                mnuTextScreenSize.Visible =
                PicMode == EPicMode.pmPrintTest;
            mnuTextScreenSize.Visible = InGame && PicMode == EPicMode.pmPrintTest && EditGame.PowerPack;
            if (mnuTextScreenSize.Visible) {
                mnuTextScreenSize.Text = "Text Screen Size: " + (MaxCol + 1).ToString();
            }
            // background and overlays
            if (BkgdImage == null) {
                mnuEditBackground.Text = "Set Background Image...";
                mnuToggleBackground.Visible = false;
                mnuRemoveBackground.Visible = false;
            }
            else {
                mnuEditBackground.Text = "Background Settings...";
                mnuToggleBackground.Visible = true;
                if (EditPicture.BkgdShow) {
                    mnuToggleBackground.Text = "Hide Background";
                }
                else {
                    mnuToggleBackground.Text = "Show Background";
                }
                mnuRemoveBackground.Visible = true;
            }
            if (ShowBands) {
                mnuToggleBands.Text = "Hide Priority Bands";
            }
            else {
                mnuToggleBands.Text = "Show Priority Bands";
            }
            mnuEditPriBase.Enabled = (!InGame || EditGame.InterpreterVersion[0] == '3' || int.Parse(EditGame.InterpreterVersion) >= 2.936);
            if (mnuEditPriBase.Visible) {
                mnuEditPriBase.Text = "Adjust Priority Base";
            }
            if (ShowTextMarks) {
                mnuToggleTextMarks.Text = "Hide Text Marks";
            }
            else {
                mnuToggleTextMarks.Text = "Show Text Marks";
            }
            // screen toggle
            if (VisVisible ^ PriVisible) {
                mnuToggleScreen.Visible = true;
                if (VisVisible) {
                    mnuToggleScreen.Text = "Show Priority Screen";
                }
                else {
                    mnuToggleScreen.Text = "Show Visual Screen";
                }
            }
            else {
                mnuToggleScreen.Visible = false;
            }
            // mode dependent items
            switch (PicMode) {
            case EPicMode.pmPrintTest or EPicMode.pmViewTest:
                // disable undo, cut, copy, paste, select all
                mnuUndo.Enabled = false;
                mnuUndo.Text = "Undo";
                mnuCut.Enabled = false;
                mnuCut.Text = "Cut";
                mnuCopy.Enabled = false;
                mnuCopy.Text = "&Copy";
                mnuPaste.Enabled = false;
                mnuPaste.Text = "Paste";
                mnuSelectAll.Enabled = false;
                mnuSelectAll.Text = "Select All";
                mnuDelete.Visible = false;
                mnuInsertCoord.Visible = false;
                mnuClearPicture.Visible = false;
                mnuSplitCommand.Enabled = false;
                mnuJoinCommands.Enabled = false;
                mnuFlipH.Enabled = false;
                mnuFlipV.Enabled = false;
                break;
            case EPicMode.pmEdit:
                if (UndoCol.Count != 0) {
                    mnuUndo.Enabled = true;
                    mnuUndo.Text = "Undo " + Editor.Base.LoadResString(PICUNDOTEXT + (int)UndoCol.Peek().UDAction) + UndoCol.Peek().UDCmd;
                    // some commands need 's' added to end if more than one command to undo
                    switch (UndoCol.Peek().UDAction) {
                    case DelCmd or AddCmd or CutCmds or PasteCmds or MoveCmds or FlipH or FlipV:
                        if (UndoCol.Peek().UDCoordIndex > 1) {
                            mnuUndo.Text += "s";
                        }
                        break;
                    }
                }
                else {
                    mnuUndo.Enabled = false;
                    mnuUndo.Text = "Undo";
                }
                mnuDelete.Visible = true;
                mnuInsertCoord.Visible = true;
                mnuClearPicture.Visible = true;
                if (SelectedTool == TPicToolTypeEnum.ttSelectArea) {
                    // area selection - no editing commands are enabled,
                    // only copy is available
                    mnuCut.Enabled = false;
                    mnuCut.Text = "Cut";
                    mnuPaste.Enabled = false;
                    mnuPaste.Text = "Paste";
                    mnuDelete.Enabled = false;
                    mnuDelete.Text = "Delete";
                    mnuInsertCoord.Enabled = false;
                    mnuInsertCoord.Text = "Insert Coordinate";
                    mnuSplitCommand.Enabled = false;
                    mnuJoinCommands.Enabled = false;
                    mnuFlipH.Enabled = false;
                    mnuFlipV.Enabled = false;
                    // copy is enabled if something selected
                    mnuCopy.Enabled = (Selection.Width > 0) && (Selection.Height > 0);
                    mnuCopy.Text = "Copy Selection";
                }
                else if (lstCommands.SelectedItems.Count == 0 ||
                    lstCoords.SelectedItems.Count == 0 ||
                    lstCommands.SelectedItems[0].Text[..3] == "Set") {
                    // no coordinate is selected - set editing commands to 
                    // handle the selected drawing commands
                    mnuCut.Enabled = SelectedCmdIndex != lstCommands.Items[^1].Index;
                    mnuCut.Text = "Cut Command";
                    mnuCopy.Enabled = mnuCut.Enabled;
                    mnuCopy.Text = "Copy Command";
                    mnuDelete.Enabled = mnuCut.Enabled;
                    mnuDelete.Text = "Delete Command";
                    if (lstCommands.SelectedItems.Count > 1) {
                        mnuCut.Text += "s";
                        mnuCopy.Text += "s";
                        mnuDelete.Text += "s";
                    }
                    mnuPaste.Enabled = PicClipBoardObj != null;
                    mnuPaste.Text = "Paste";
                    if (mnuPaste.Enabled) {
                        if (PicClipBoardObj.UDCoordIndex > 1) {
                            mnuPaste.Text += " Commands";
                        }
                        else {
                            mnuPaste.Text += " Command";
                        }
                    }
                    mnuInsertCoord.Enabled = false;
                    mnuInsertCoord.Text = "Insert Coordinate";
                    mnuSplitCommand.Enabled = false;
                    // assume no join
                    mnuJoinCommands.Enabled = false;
                    if (lstCommands.SelectedItems.Count == 2) {
                        if ((int)SelectedCmdType >= 0xF4 && SelectedCmdType != DrawFunction.ChangePen) {
                            // if the two commands are same type OR
                            // both types are X/Y lines
                            if (lstCommands.SelectedItems[0].Text == lstCommands.SelectedItems[1].Text ||
                                ((SelectedCmdType == DrawFunction.XCorner || SelectedCmdType == DrawFunction.YCorner) && (lstCommands.SelectedItems[0].Text == "X Corner" || lstCommands.SelectedItems[0].Text == "Y Corner"))) {
                                switch (SelectedCmdType) {
                                case DrawFunction.PlotPen or DrawFunction.Fill:
                                    // ok to join
                                    mnuJoinCommands.Enabled = true;
                                    break;
                                default:
                                    // only if end coordinates match
                                    if (MatchPoints(SelectedCmdIndex)) {
                                        mnuJoinCommands.Enabled = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    Rectangle rect = GetSelectionBounds(lstCommands.SelectedItems[0].Index, lstCommands.SelectedItems.Count);
                    mnuFlipH.Enabled = (rect.Width > 0);
                    mnuFlipV.Enabled = (rect.Height > 0);
                }
                else {
                    // a coordinate is selected, set editing commands to
                    // handle coordinate editing
                    mnuCut.Enabled = false;
                    mnuCut.Text = "Cut";
                    mnuCopy.Enabled = false;
                    mnuCopy.Text = "Copy";
                    // insert always available for absline, relline, fill, plot
                    // delete always available for absline, fill, plot
                    // insert/delete only available for other commands if on last coord
                    mnuDelete.Text = "Delete Coordinate";
                    switch (lstCommands.SelectedItems[0].Text[..4]) {
                    case "Abs " or "Fill" or "Plot":
                        mnuDelete.Enabled = true;
                        mnuInsertCoord.Enabled = true;
                        break;
                    case "Vis " or "Pri " or "Set ":
                        break;
                    default:
                        // Corner lines or relative lines
                        mnuDelete.Enabled = lstCoords.Items[0].Selected;
                        mnuInsertCoord.Enabled = lstCoords.Items[0].Selected;
                        break;
                    }
                    // disable split if not just one cmd selected OR
                    // cmd is set color pen or set plot pen OR
                    // only one coordinate OR
                    // no coord selected
                    if (lstCommands.SelectedItems.Count != 1 ||
                        ((int)SelectedCmdType < 0xF4 || SelectedCmdType == DrawFunction.ChangePen) ||
                        lstCoords.Items.Count < 2 ||
                        lstCoords.SelectedItems.Count == 0) {
                        mnuSplitCommand.Enabled = false;
                    }
                    else {
                        // if on a line, fill, or plot cmd
                        switch (SelectedCmdType) {
                        case DrawFunction.AbsLine or DrawFunction.RelLine or DrawFunction.XCorner or DrawFunction.YCorner:
                            // only if three or more, and not on an end
                            if (lstCoords.Items.Count < 3 || lstCoords.Items[0].Selected || lstCoords.Items[^1].Selected) {
                                mnuSplitCommand.Enabled = false;
                            }
                            else {
                                mnuSplitCommand.Enabled = true;
                            }
                            break;
                        case DrawFunction.Fill or DrawFunction.PlotPen:
                            // only if not on first coordinate
                            mnuSplitCommand.Enabled = !lstCoords.Items[0].Selected;
                            break;
                        }
                    }
                    mnuJoinCommands.Enabled = false;
                    mnuFlipH.Enabled = false;
                    mnuFlipV.Enabled = false;
                }
                mnuClearPicture.Enabled = true;
                mnuSelectAll.Enabled = lstCommands.Items.Count > 2;
                break;
            }
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            cmEdit.Items.AddRange([mnuUndo, mnuESep0, mnuCut, mnuCopy,
                mnuPaste, mnuDelete, mnuClearPicture, mnuSelectAll, mnuESep1,
                mnuInsertCoord, mnuSplitCommand, mnuJoinCommands, mnuFlipH, mnuFlipV, mnuESep2,
                mnuEditMode, mnuViewTestMode, mnuTextTestMode, mnuESep3,
                mnuSetTestView, mnuTestViewOptions, mnuTestTextOptions,
                mnuTestPrintCommand, mnuTextScreenSize, mnuESep4,
                mnuToggleScreen, mnuToggleBands, mnuEditPriBase,
                mnuToggleTextMarks, mnuESep5,
                mnuToggleBackground, mnuEditBackground, mnuRemoveBackground]);
            ResetEditMenu();
        }

        private void ResetEditMenu() {
            foreach (ToolStripItem itm in cmEdit.Items) {
                itm.Enabled = true;
            }
        }

        private void mnuEditMode_Click(object sender, EventArgs e) {
            if (PicMode != EPicMode.pmEdit) {
                SetMode(EPicMode.pmEdit);
            }
        }

        private void mnuViewTestMode_Click(object sender, EventArgs e) {
            if (PicMode != EPicMode.pmViewTest) {
                SetMode(EPicMode.pmViewTest);
            }
        }

        private void mnuTextTestMode_Click(object sender, EventArgs e) {
            if (PicMode != EPicMode.pmPrintTest) {
                SetMode(EPicMode.pmPrintTest);
            }
        }

        private void mnuUndo_Click(object sender, EventArgs e) {

        }

        private void mnuCut_Click(object sender, EventArgs e) {

        }

        private void mnuCopy_Click(object sender, EventArgs e) {

        }

        private void mnuPaste_Click(object sender, EventArgs e) {

        }

        private void mnuDelete_Click(object sender, EventArgs e) {

        }

        private void mnuClearPicture_Click(object sender, EventArgs e) {

        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {

        }

        private void mnuInsertCoord_Click(object sender, EventArgs e) {

        }

        private void mnuSplitCommand_Click(object sender, EventArgs e) {

        }

        private void mnuJoinCommands_Click(object sender, EventArgs e) {

        }

        private void mnuSetTestView_Click(object sender, EventArgs e) {

        }

        private void mnuTestViewOptions_Click(object sender, EventArgs e) {

        }

        private void mnuTestTextOptions_Click(object sender, EventArgs e) {

        }

        private void mnuTestPrintCommand_Click(object sender, EventArgs e) {

        }

        private void mnuTextScreenSize_Click(object sender, EventArgs e) {

        }

        private void mnuToggleScreen_Click(object sender, EventArgs e) {

        }

        private void mnuToggleBands_Click(object sender, EventArgs e) {
            ShowBands = !ShowBands;
            picVisual.Invalidate();
            picPriority.Invalidate();
        }

        private void mnuEditPriBase_Click(object sender, EventArgs e) {

        }

        private void mnuToggleTextMarks_Click(object sender, EventArgs e) {
            ShowTextMarks = !ShowTextMarks;
            picVisual.Invalidate();
            picPriority.Invalidate();
        }

        private void mnuToggleBackground_Click(object sender, EventArgs e) {

        }

        private void mnuEditBackground_Click(object sender, EventArgs e) {

        }

        private void mnuRemoveBackground_Click(object sender, EventArgs e) {

        }
        #endregion

        #region Toolbar Event Handlers
        //tsbEditMode_Click
        //tsbViewTest_Click
        //tsbPrintTest_Click

        private void tsbZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void tsbZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }
        #endregion

        #region Control Event Handlers

        private void vsbVisual_Scroll(object sender, ScrollEventArgs e) {
            picVisual.Top = -vsbVisual.Value;
        }

        private void hsbVisual_Scroll(object sender, ScrollEventArgs e) {
            picVisual.Left = -hsbVisual.Value;
        }

        private void vsbPriority_Scroll(object sender, ScrollEventArgs e) {
            picPriority.Top = -vsbPriority.Value;
        }

        private void hsbPriority_Scroll(object sender, ScrollEventArgs e) {
            picPriority.Left = -hsbPriority.Value;
        }

        private void splitImages_Resize(object sender, EventArgs e) {
            SetScrollbars();
        }

        private void picVisual_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            if (ShowBands) {
                // draw bands in matching priority color one pixel high
                for (int rtn = 5; rtn <= 14; rtn++) {
                    int yp = (int)((int)(Math.Ceiling((rtn - 5) / 10.0 * (168 - EditPicture.PriBase)) + EditPicture.PriBase) * ScaleFactor - 1);
                    g.DrawLine(new(EditPalette[rtn]), 0, yp, picVisual.Width, yp);
                }
            }

            // text marks indicate where text characters are drawn
            if (ShowTextMarks) {
                for (int j = 1; j <= 21; j++) {
                    for (int i = 0; i <= MaxCol; i++) {
                        int x = (int)(i * CharWidth * ScaleFactor);
                        int y = (int)(j * 8 * ScaleFactor - 1);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, (int)(x + ScaleFactor), y);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, x, (int)(y - ScaleFactor));
                    }
                }
            }
        }

        private void picVisual_MouseDown(object sender, MouseEventArgs e) {

        }

        private void lstCommands_Resize(object sender, EventArgs e) {
            //if (lstCommands.Height - lstCommands.ClientSize.Height > 2) {
            //    // bottom scrollbar is showing; this hack seems to
            //    // fix the problem
            //    lstCommands.BeginUpdate();
            //    columnHeader1.Width = lstCommands.ClientSize.Width;
            //    lstCommands.EndUpdate();
            //}
            //else {
            //    columnHeader1.Width = lstCommands.ClientSize.Width;
            //}
        }

        private void lstCommands_MouseClick(object sender, MouseEventArgs e) {
            Debug.Print("mouse click");
            int clickeditem = (int)(lstCommands.HitTest(new(e.X, e.Y)).Item?.Index);

            // multiple selections must always be sequential;
            // if any multiple selection is made to select them
            // out of order, de-select all but the last one selected

            if (lstCommands.SelectedItems.Count > 1) {
                int index = lstCommands.SelectedIndices[0];
                for (int i = 1; i < lstCommands.SelectedItems.Count; i++) {
                    if (lstCommands.SelectedIndices[i] != index + i) {
                        // non-sequential, deselect all but the one clicked
                        lstCommands.SelectedItems.Clear();
                        lstCommands.Items[clickeditem].Selected = true;
                    }
                    break;
                }
            }

            // end command not allowed in multiple selections
            if (lstCommands.SelectedItems.Count > 1 && lstCommands.Items[^1].Selected) {
                lstCommands.Items[^1].Selected = false;
            }
            UpdateCmdSelection();
        }

        private void lstCommands_MouseDoubleClick(object sender, MouseEventArgs e) {

        }

        private void lstCommands_MouseDown(object sender, MouseEventArgs e) {
            return;

            // force selection update now - otherwise mousemove uses wrong start...
            if (e.Button == MouseButtons.Left) {
                if (lstCommands.ClientRectangle.Contains(e.X, e.Y)) {
                    ListViewHitTestInfo info = lstCommands.HitTest(new(e.X, e.Y));
                    if (info.Item != null) {
                        mCmdEnd = mCmdAnchor = info.Item.Index;
                    }
                }
            }
        }

        private void lstCommands_MouseMove(object sender, MouseEventArgs e) {
            return;

            // TODO: decide if it's worth keeping this...
            if (e.Button == MouseButtons.Left) {
                // expand/contract selection as mouse moves
                if (lstCommands.ClientRectangle.Contains(e.X, e.Y)) {
                    ListViewHitTestInfo info = lstCommands.HitTest(new(e.X, e.Y));
                    if (info.Item != null) {
                        int newend = info.Item.Index;
                        if (newend == mCmdEnd) {
                            return;
                        }
                        if (newend > mCmdEnd) {
                            // moving down the list
                            for (int i = mCmdEnd; i < newend; i++) {
                                if (i < mCmdAnchor) {
                                    lstCommands.Items[i].Selected = false;
                                }
                                else if (i > mCmdAnchor) {
                                    lstCommands.Items[i].Selected = true;
                                }
                            }
                            lstCommands.Items[newend].Selected = true;
                            mCmdEnd = newend;
                        }
                        else {
                            // moving up the list
                            for (int i = mCmdEnd; i > newend; i--) {
                                if (i > mCmdAnchor) {
                                    lstCommands.Items[i].Selected = false;
                                }
                                else if (i < mCmdAnchor) {
                                    lstCommands.Items[i].Selected = true;
                                }
                            }
                            lstCommands.Items[newend].Selected = true;
                            mCmdEnd = newend;
                        }
                    }
                }
            }
        }

        private void lstCommands_MouseUp(object sender, MouseEventArgs e) {

        }

        private void lstCommands_MouseCaptureChanged(object sender, EventArgs e) {

        }

        private void lstCommands_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void lstCommands_KeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
            case Keys.Down:
            case Keys.Up:
            case Keys.PageDown:
            case Keys.PageUp:
            case Keys.Home:
            case Keys.End:
                UpdateCmdSelection();
                break;

            }
        }

        private void lstCoords_Resize(object sender, EventArgs e) {
            // ideally, need to set column width to widest item that's added
            //if (lstCoords.Height - lstCoords.ClientSize.Height > 2) {
            //    // bottom scrollbar is showing; this hack seems to
            //    // fix the problem
            //    lstCoords.BeginUpdate();
            //    columnHeader2.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            //    columnHeader2.Width = lstCoords.ClientSize.Width;
            //    lstCoords.EndUpdate();
            //}
            //else {
            //    columnHeader2.Width = lstCoords.ClientSize.Width;
            //}
        }

        private void lstCoords_MouseClick(object sender, MouseEventArgs e) {

        }

        private void lstCoords_MouseDoubleClick(object sender, MouseEventArgs e) {

        }

        private void lstCoords_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void lstCoords_KeyUp(object sender, KeyEventArgs e) {

        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            float dblWidth = picPalette.Width / 9;
            float dblHeight = picPalette.Height / 2;
            Graphics g = e.Graphics;

            // disabled brush area
            g.FillRectangle(Brushes.White, 0, 0, dblWidth, dblHeight * 2);
            Pen pen = new(Color.Black) {
                Width = 2
            };
            g.DrawLine(pen, 0, 0, dblWidth, dblHeight * 2);
            g.DrawLine(pen, dblWidth, 0, 0, dblHeight * 2);

            // color area
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 8; j++) {
                    Color color = EditPalette[i * 8 + j];
                    g.FillRectangle(new SolidBrush(color), (j + 1) * dblWidth, i * dblHeight, dblWidth, dblHeight);
                }
            }
            Color textcolor;
            Font font = new Font("Arial", 10, FontStyle.Bold);

            // add 'V' for current selected visual color
            if (SelectedPen.VisColor < AGIColorIndex.None) {
                if ((int)SelectedPen.VisColor > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                // set x and Y to position 'v' over correct color
                g.DrawString("V", font, new SolidBrush(textcolor), dblWidth * (((int)SelectedPen.VisColor % 8) + 1) + 3, 17 * ((int)SelectedPen.VisColor / 8) - 1);
            }
            else {
                // put 'v' in disabled square
                g.DrawString("V", font, Brushes.Black, 3, 7);
            }
            // add 'P' for current selected priority color
            if (SelectedPen.PriColor < AGIColorIndex.None) {
                if ((int)SelectedPen.PriColor > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                // set x and Y to position 'P' over correct color
                g.DrawString("P", font, new SolidBrush(textcolor), dblWidth * (((int)SelectedPen.PriColor % 8) + 2) - 13, 17 * ((int)SelectedPen.PriColor / 8) - 1);
            }
            else {
                // put 'P' in disabled square
                g.DrawString("P", font, Brushes.Black, dblWidth - 12, 7);
            }
        }

        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            int dblWidth = picPalette.Width / 9;
            bool blnOff = false;
            int lngForce = 0;

            // determine color from x,Y position
            int bytNewCol = (9 * (e.Y / 16)) + (e.X / dblWidth);
            // adjust to account for the disabled block
            if (bytNewCol == 0 || bytNewCol == 9) {
                //color disable was chosen
                blnOff = true;
            }
            else {
                if (bytNewCol < 9) {
                    bytNewCol--;
                }
                else {
                    bytNewCol -= 2;
                }
            }
            switch (Control.ModifierKeys) {
            case Keys.None:
            case Keys.Shift:
                if (PicMode != EPicMode.pmEdit) {
                    return;
                }
                if (lstCommands.SelectedItems.Count == 0) {
                    //    return;
                }
                if (PicDrawMode != TPicDrawOpEnum.doNone) {
                    return;
                }
                switch (e.Button) {
                case MouseButtons.Left:
                    if (blnOff) {
                        SelectedPen.VisColor = AGIColorIndex.None;
                    }
                    else {
                        SelectedPen.VisColor = (AGIColorIndex)bytNewCol;
                        lngForce = 1;
                    }
                    break;
                case MouseButtons.Right:
                    if (blnOff) {
                        SelectedPen.PriColor = AGIColorIndex.None;
                    }
                    else {
                        SelectedPen.PriColor = (AGIColorIndex)bytNewCol;
                    }
                    lngForce = 2;
                    break;
                }
                if (Control.ModifierKeys != Keys.Shift) {
                    lngForce = 0;
                }
                // refresh now to match colors
                RefreshPens(lngForce);
                picPalette.Invalidate();
                break;
            case Keys.Control:
                // change cursor color, (but can't select 'no color')
                if (!blnOff) {
                    if (e.Button == MouseButtons.Left) {
                        VCColor = EditPalette[bytNewCol];
                    }
                    else if (e.Button == MouseButtons.Right) {
                        PCColor = EditPalette[bytNewCol];
                    }
                    else {
                        // ignore any other button scenarios
                        return;
                    }
                    // if showing the cursors (in 'xmode', selected tool is 'none'
                    // and only one cmd is selected
                    if (CursorMode == EPicCursorMode.pcmXMode && SelectedTool == TPicToolTypeEnum.ttEdit &&
                                      lstCommands.SelectedItems.Count == 1) {
                        //only in edit mode
                        if (PicMode == EPicMode.pmEdit) {
                            HighlightCoords();
                        }
                    }
                }
                break;
            }
        }

        private void picPalette_MouseMove(object sender, MouseEventArgs e) {
            // set cursor depending on mode
            if (PicMode == EPicMode.pmEdit) {
                picPalette.Cursor = Cursors.Default;
            }
            else {
                picPalette.Cursor = Cursors.No;
            }
        }

        private void picVisual_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }

            // !!!!!! mouse coordinates appear to be in screen coordinate, NOT
            // relatve to the control!
            // need position relative to the panel, so take scroll & margin into account
            Point anchor = new(e.X, e.Y);
            anchor.X -= splitImages.Panel1.HorizontalScroll.Value;
            anchor.Y -= splitImages.Panel1.VerticalScroll.Value;
            ChangeScale(e.Delta, true);
        }

        private void picPriority_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            // if not over the picture surface (or it's not visible)
            Point panelPt = splitImages.Panel2.PointToClient(Cursor.Position);
            if (!splitImages.Panel2.ClientRectangle.Contains(panelPt)) {
                return;
            }
            ChangeScale(e.Delta, true);
        }

        private void splitForm_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (Cursor.Current != Cursors.VSplit) {
                Cursor.Current = Cursors.VSplit;
            }
        }

        private void splitForm_SplitterMoved(object sender, SplitterEventArgs e) {
            Cursor.Current = Cursors.Default;
            lstCommands.Focus();
        }

        private void splitImages_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (Cursor.Current != Cursors.HSplit) {
                Cursor.Current = Cursors.HSplit;
            }
        }

        private void splitImages_SplitterMoved(object sender, SplitterEventArgs e) {
            Cursor.Current = Cursors.Default;
            lstCommands.Focus();
            SetScrollbars();
        }

        private void splitLists_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (Cursor.Current != Cursors.HSplit) {
                Cursor.Current = Cursors.HSplit;
            }
        }

        private void splitLists_SplitterMoved(object sender, SplitterEventArgs e) {
            Cursor.Current = Cursors.Default;
            lstCommands.Focus();
        }
        #endregion
        #endregion


        public bool LoadPicture(Picture loadpic) {
            InGame = loadpic.InGame;
            if (InGame) {
                PictureNumber = loadpic.Number;
            }
            else {
                // use a number that can never match
                // when searches for open pictures are made
                PictureNumber = 256;
            }
            try {
                loadpic.Load();
            }
            catch {
                return false;
            }
            if (loadpic.ErrLevel < 0) {
                return false;
            }
            EditPicture = loadpic.Clone();
            EditPalette = loadpic.Palette.CopyPalette();
            VCColor = EditPalette[4]; // red
            PCColor = EditPalette[3]; // cyan
            if (!InGame && EditPicture.ID == "NewPicture") {
                PicCount++;
                EditPicture.ID = "NewPicture" + PicCount;
                IsChanged = true;
            }
            else {
                // unlike views/sounds, if picture has errors in it don't force a save
                // because user has to manually edit the bad data to repair it
                IsChanged = EditPicture.IsChanged;
            }
            Text = sPICED + ResourceName(EditPicture, InGame, true);
            if (IsChanged) {
                Text = sDM + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;

            // populate cmd list with commands (this also draws the picture  on edit surface)
            if (!LoadCmdList()) {
                // error- stop the form loading process
                MessageBox.Show(MDIMain,
                    "This picture has corrupt or invalid data. Unable to open it for editing.",
                    "Picture Data Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                EditPicture.Unload();
                EditPicture = null;
                return false;
            }
            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

            // enable stepdrawing
            EditPicture.StepDraw = true;
            // enable editing
            PicMode = EPicMode.pmEdit;
            /*
            // check for a saved background image
            if (EditPicture.BkgdImgFile.Length != 0) {
                try {
                    BkgdImage = LoadImage(Path.GetFullPath(EditGame.GameDir + EditPicture.BkgdImgFile));
                    // get rest of parameters
                    BkgdTrans = EditPicture.BkgdTrans;
                    string[] strTemp = EditPicture.BkgdSize.ToString().Split("|");
                    tgtW = strTemp[0];
                    tgtH = strTemp[1];
                    srcW = strTemp[2];
                    srcH = strTemp[3];
                    strTemp = EditPicture.BkgdPosition.ToString().Split("|");
                    tgtX = strTemp[0];
                    tgtY = strTemp[1];
                    srcX = strTemp[2];
                    srcY = strTemp[3];
                    // validate a few things...
                    if (srcW <= 0 || srcH <= 0) {
                        // reset
                        srcW = MetsToPix(BkgdImage.Width);
                        srcH = MetsToPix(BkgdImage.Height);
                    }
                    if (tgtW <= 0 || tgtH <= 0) {
                        // reset
                        tgtW = 320;
                        tgtH = 168;
                    }
                    if (EditPicture.BkgdShow) {
                        Toolbar1.Buttons["bkgd"].Value = tbrPressed;
                        // turn background on
                        ToggleBkgd(true);
                    }
                }
                catch (Exception ex) {
                    string strMsg;
                    // if error is file not found, let user know
                    if (ex.HResult == 999) {
                        strMsg = "Background file not found. ";
                    }
                    else {
                        strMsg = "Error loading background image.";
                    }
                    // inform user
                    MessageBox.Show(MDIMain,
                        strMsg + "\n\nThe 'BkgdImg' property for this picture will be cleared.",
                        "Picture Background Image Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    // clear picedit background properties
                    EditPicture.BkgdImgFile = "";
                    EditPicture.BkgdPosition = new Picture.PicBkgdPos();
                    EditPicture.BkgdShow = false;
                    EditPicture.BkgdSize = new Picture.PicBkgdSize();
                    EditPicture.BkgdTrans = 0;
                    // clear ingame resource background properties
                    EditGame.Pictures[PicNumber].BkgdImgFile = "";
                    EditGame.Pictures[PicNumber].BkgdPosition = new Picture.PicBkgdPos(); ;
                    EditGame.Pictures[PicNumber].BkgdShow = false;
                    EditGame.Pictures[PicNumber].BkgdSize = new Picture.PicBkgdSize();
                    EditGame.Pictures[PicNumber].BkgdTrans = 0;
                    // update the game wag file
                    WinAGISettingsList.WriteSetting("Picture" + PicNumber, "BkgdImg", "", "Pictures");
                    WinAGISettingsList.WriteSetting("Picture" + PicNumber, "BkgdPosn", "");
                    WinAGISettingsList.WriteSetting("Picture" + PicNumber, "BkgdShow", "");
                    WinAGISettingsList.WriteSetting("Picture" + PicNumber, "BkgdSize", "");
                    WinAGISettingsList.WriteSetting("Picture" + PicNumber, "BkgdTrans", "");
                    WinAGISettingsList.Save();
                    // make sure image is nothing
                    BkgdImage = null;
                    // force background off
                    EditPicture.BkgdShow = false;
                }
            }
            */
            DrawPicture();
            UpdateToolBar();

            return true;
        }

        public void ImportPicture(string importfile) {
            MDIMain.UseWaitCursor = true;
            Picture tmpPicture = new();
            try {
                tmpPicture.Import(importfile);
            }
            catch (Exception e) {
                //something wrong
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(e, "Error while importing picture:", "Unable to load this picture resource.", "Import Picture Error");
                return;
            }
            // now check to see if it's a valid picture resource (by trying to reload it)
            tmpPicture.Load();
            if (tmpPicture.ErrLevel < 0) {
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(tmpPicture.ErrLevel, "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource");
                //restore main form mousepointer and exit
                return;
            }
            // copy only the resource data
            EditPicture.ReplaceData(tmpPicture.Data);
            EditPicture.ResetPicture();
            MarkAsChanged();
            // TODO: redraw

            ShowAGIBitmap(picVisual, EditPicture.VisualBMP);
            ShowAGIBitmap(picPriority, EditPicture.PriorityBMP);
            MDIMain.UseWaitCursor = false;
        }

        public void SavePicture() {
            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool blnLoaded = EditGame.Pictures[PictureNumber].Loaded;
                if (!blnLoaded) {
                    EditGame.Pictures[PictureNumber].Load();
                }
                EditGame.Pictures[PictureNumber].CloneFrom(EditPicture);
                EditGame.Pictures[PictureNumber].Save();
                if (!blnLoaded) {
                    EditGame.Pictures[PictureNumber].Unload();
                }
                RefreshTree(AGIResType.Picture, PictureNumber);
                if (WinAGISettings.AutoExport.Value) {
                    EditPicture.Export(EditGame.ResDir + EditPicture.ID + ".agp");
                    // reset ID (non-game id gets saved by export...)
                    EditPicture.ID = EditGame.Pictures[PictureNumber].ID;
                }
                MarkAsSaved();
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditPicture.ResFile.Length == 0) {
                    ExportPicture();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    EditPicture.Save();
                    MarkAsSaved();
                    MDIMain.UseWaitCursor = false;
                }
            }
        }

        private void ExportPicture() {
            int retval = Base.ExportPicture(EditPicture, InGame);
            if (InGame) {
                // because EditPicture is not the actual ingame picture its
                // ID needs to be reset back to the ingame value
                EditPicture.ID = EditGame.Pictures[PictureNumber].ID;
            }
            else {
                if (retval == 1) {
                    MarkAsSaved();
                }
            }
        }

        public void ToggleInGame() {
            //toggles the game state of an object

            DialogResult rtn;
            string strExportName;
            bool blnDontAsk = false;

            if (InGame) {
                if (WinAGISettings.AskExport.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to export '" + EditPicture.ID + "' before removing it from your game?",
                        "Don't ask this question again",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        "Export Picture Before Removal", ref blnDontAsk);
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
                    strExportName = NewResourceName(EditPicture, InGame);
                    if (strExportName.Length > 0) {
                        EditPicture.Export(strExportName);
                        UpdateStatusBar();
                    }
                    break;
                case DialogResult.No:
                    // nothing to do
                    break;
                }
                // confirm removal
                if (WinAGISettings.AskRemove.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Removing '" + EditPicture.ID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                        "Remove Picture From Game",
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
                // remove the picture (force-closes this editor)
                RemovePicture((byte)PictureNumber);
            }
            else {
                // add to game 
                if (EditGame is null) {
                    return;
                }
                using frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.Picture, 0);
                if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    PictureNumber = frmGetNum.NewResNum;
                    // change id before adding to game
                    EditPicture.ID = frmGetNum.txtID.Text;
                    AddNewPicture((byte)PictureNumber, EditPicture);
                    EditGame.Pictures[PictureNumber].Load();
                    // copy the picture back (to ensure internal variables are copied)
                    EditPicture.CloneFrom(EditGame.Pictures[PictureNumber]);
                    EditPalette = EditPicture.Palette.CopyPalette();
                    // now we can unload the newly added picture;
                    EditGame.Pictures[PictureNumber].Unload();
                    MarkAsSaved();
                    InGame = true;
                    MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[20];
                    MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Remove Picture";
                }
            }
        }

        public void RenumberPicture() {
            if (!InGame) {
                return;
            }
            string oldid = EditPicture.ID;
            int oldnum = PictureNumber;
            byte NewResNum = GetNewNumber(AGIResType.Picture, (byte)PictureNumber);
            if (NewResNum != PictureNumber) {
                // update ID (it may have changed if using default ID)
                EditPicture.ID = EditGame.Pictures[NewResNum].ID;
                PictureNumber = NewResNum;
                Text = sPICED + ResourceName(EditPicture, InGame, true);
                if (IsChanged) {
                    Text = sDM + Text;
                }
                if (EditPicture.ID != oldid) {
                    if (File.Exists(EditGame.ResDir + oldid + ".agp")) {
                        SafeFileMove(EditGame.ResDir + oldid + ".agp", EditGame.ResDir + EditGame.Pictures[NewResNum].ID + ".agp", true);
                    }
                }
            }
        }

        public void EditPictureProperties(int FirstProp) {
            string id = EditPicture.ID;
            string description = EditPicture.Description;
            if (GetNewResID(AGIResType.Picture, PictureNumber, ref id, ref description, InGame, FirstProp)) {
                if (EditPicture.Description != description) {
                    EditPicture.Description = description;
                }
                if (EditPicture.ID != id) {
                    EditPicture.ID = id;
                    Text = sPICED + ResourceName(EditPicture, InGame, true);
                    if (IsChanged) {
                        Text = sDM + Text;
                    }
                }
            }
        }

        private bool LoadCmdList(bool NoUpdate = false) {
            // loads the picture info into the command list
            // assumes there are no errors, since
            // a picture must successfully load before this routine is called

            byte bytCmd = 0;
            int lngPos = 0;
            byte bytX;
            string strBrush;
            bool blnSplat, blnErrors = false;
            bool exitdo = false;

            // clear the tree
            lstCommands.Items.Clear();
            // get first command

            //while (lngPos < EditPicture.Data.Length && !exitdo) ;
            while (lngPos < EditPicture.Data.Length && !exitdo) {
                bytCmd = EditPicture.Data[lngPos];
                // add correct node to the list
                switch (bytCmd) {
                case 0xFF:
                    //  end of file
                    lstCommands.Items.Add(Editor.Base.LoadResString(DRAWFUNCTIONTEXT + 11));
                    lstCommands.Items[^1].Tag = lngPos;
                    exitdo = true;
                    break;
                case >= 0xF0 and <= 0xFA:
                    // add command node
                    lstCommands.Items.Add(Editor.Base.LoadResString(DRAWFUNCTIONTEXT + bytCmd - 0xF0));
                    // store position for this command
                    lstCommands.Items[^1].Tag = lngPos;

                    // add command parameters
                    switch (bytCmd) {
                    case 0xF0:
                        //lngPos++;
                        //// get color
                        //bytCmd = EditPicture.Data[lngPos];
                        //// RARE, but check for color out of bounds
                        //if (bytCmd > 15) {
                        //    lstCommands.Items[^1].Text = "Vis: " + "ERR(0x" + bytCmd.ToString("x2") + ")";
                        //    blnErrors = true;
                        //}
                        //else {
                        //    lstCommands.Items[^1].Text = "Vis: " + Editor.Base.LoadResString(COLORNAME + bytCmd);
                        //}
                        //// move pointer
                        //lngPos++;
                        lstCommands.Items[^1].Text = "Vis: ON";
                        lngPos += 2;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF2:
                        // Change color and enable priority draw.
                        //lngPos++;
                        //if (lngPos >= EditPicture.Data.Length) {
                        //    // error - no color value and end of picture
                        //    // data found; probably bad picture resource data
                        //    lstCommands.Items[^1].Text = "Pri: ERR --no data--";
                        //    blnErrors = true;
                        //    break;
                        //}
                        //// get color
                        //bytCmd = EditPicture.Data[lngPos];
                        //// RARE, but check for color out of bounds
                        //if (bytCmd > 15) {
                        //    lstCommands.Items[^1].Text = "Pri: " + "ERR(0x" + bytCmd.ToString("x2") + ")";
                        //    blnErrors = true;
                        //}
                        //else {
                        //    lstCommands.Items[^1].Text = "Pri: " + Editor.Base.LoadResString(COLORNAME + bytCmd);
                        //}
                        //lngPos++;
                        lngPos += 2;
                        lstCommands.Items[^1].Text = "Pri: ON";
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF1 or 0xF3:
                        // Disable draw.
                        lngPos++;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF4 or 0xF5 or 0xF6 or 0xF7 or 0xF8 or 0xFA:
                        do {
                            // read in data until another command is found or until
                            // end is reached
                            lngPos++;
                            if (lngPos >= EditPicture.Data.Length) {
                                break;
                            }
                            bytCmd = EditPicture.Data[lngPos];
                        } while (bytCmd < 0xF0);
                        break;
                    case 0xF9:
                        // Change pen size and style.
                        //lngPos++;
                        //if (lngPos >= EditPicture.Data.Length) {
                        //    // end of picture data found
                        //    lstCommands.Items[^1].Text = "Set Pen:  ERR --no data--";
                        //    break;
                        //}
                        //bytX = EditPicture.Data[lngPos];
                        //if ((bytX & 0x20) / 0x20 == 0) {
                        //    strBrush = "Solid ";
                        //    blnSplat = false;
                        //}
                        //else {
                        //    strBrush = "Splatter ";
                        //    blnSplat = true;
                        //}
                        //if ((bytX & 0x10) / 0x10 == 0) {
                        //    strBrush += "Circle ";
                        //}
                        //else {
                        //    strBrush += "Rectangle ";
                        //}
                        //strBrush += (bytX & 0x7).ToString();
                        //lstCommands.Items[^1].Text = "Set Pen: " + strBrush;
                        //lngPos++;
                        lngPos += 2;
                        lstCommands.Items[^1].Text = "Set Plot Pen";
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    }
                    break;
                default:
                    //  < 0xF0 or > 0xFA
                    // invalid command  - note it
                    lstCommands.Items.Add("ERR: (0x" + bytCmd.ToString("x2") + ")");
                    lstCommands.Items[^1].Tag = lngPos;
                    blnErrors = true;
                    break;
                }
            }


            // if end cmd not found, need to add it (and let user know)
            if (bytCmd != 0xFF) {
                // add missing end
                EditPicture.WriteByte(0xFF);
                // add 'end' node to list
                lstCommands.Items.Add(Editor.Base.LoadResString(DRAWFUNCTIONTEXT + 11));
                lstCommands.Items[^1].Tag = lngPos;
                MarkAsChanged();
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "Picture is missing end-of-resource marker; marker has been added and picture  loaded, but picture data may be corrupt.",
                    "Missing End Command in Picture",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\pictures.htm#ff");
            }
            else if (lngPos < EditPicture.Data.Length - 1) {
                // remove the extra data
                EditPicture.RemoveData(lngPos + 1, EditPicture.Data.Length - 1 - lngPos);
                MarkAsChanged();
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "Picture has unused data after the end-of-resource marker. The unused data have been removed but the picture data may be corrupt.",
                    "Missing End Command in Picture",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\pictures.htm#ff");
            }
            // if any bad commands or colors encountered
            if (blnErrors) {
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "One or more invalid commands and/or colors encountered; they are marked with 'ERR'. This picture data may be corrupt.",
                    "Anomaly Found in Picture Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\winagi\\Picture_Editor.htm#picerrors");
            }

            // select end cmd, and update listbox
            SelectCmd(lstCommands.Items.Count - 1, NoUpdate);
            return true;
        }

        public void ForceRefresh() {
            lstCommands.SelectedItems[0].EnsureVisible();
        }

        public void SetMode(EPicMode newMode) {
            // always cancel any drawing operation
            PicDrawMode = TPicDrawOpEnum.doNone;
            switch (newMode) {
            case EPicMode.pmEdit:
                PicMode = newMode;
                tsbMode.Image = tsbEditMode.Image;

                // disable view movement
                TestDir = 0;
                //tmrTest.Enabled = false;
                if (lstCommands.SelectedIndices.Count == 0) {
                    SelectCmd(lstCommands.Items.Count - 1);
                }
                else {
                    SelectCmd(SelectedCmdIndex);
                }
                // reset cursor to match selected tool
                switch (SelectedTool) {
                case TPicToolTypeEnum.ttEdit:
                    // arrow cursor
                    SetCursors(EPicCur.pcEdit);
                    break;
                case TPicToolTypeEnum.ttFill:
                    SetCursors (EPicCur.pcPaint);
                    break;
                case TPicToolTypeEnum.ttPlot:
                    SetCursors (EPicCur.pcBrush);
                    break;
                default:
                    SetCursors (EPicCur.pcSelect);
                    break;
                }
                // set status bar depending on grid mode
                if (ShowTextMarks) {
                    // force text row/col
                    StatusMode = EPicStatusMode.psText;
                }
                else {
                    // force normal pixel coordinates
                    StatusMode = EPicStatusMode.psPixel;
                }
                // allow selection of coordinates
                lstCoords.Enabled = true;
                break;

            case EPicMode.pmViewTest:
                if (TestView == null) {
                    //GetTestView();
                }
                // if still no view
                if (TestView != null) {
                    return;
                }

                PicMode = newMode;
                tsbMode.Image = tsbViewTest.Image;
                // if doing anything, cancel it
                if (lstCoords.SelectedItems.Count != 0) {
                    lstCoords.SelectedItems.Clear();
                }
                // coordinates can't be selected in test mode
                lstCoords.Enabled = false;
                if (lstCommands.SelectedIndices.Count == 0) {
                    // select last cmd
                    SelectCmd(lstCommands.Items.Count - 1);
                }
                else {
                    SelectCmd(SelectedCmdIndex);
                }
                if (StatusMode == EPicStatusMode.psCoord) {
                    StatusMode = EPicStatusMode.psPixel;
                }
                break;

            case EPicMode.pmPrintTest:
                PicMode = newMode;
                tsbMode.Image = tsbPrintTest.Image;
                // if doing anything, cancel it
                if (lstCoords.SelectedItems.Count != 0) {
                    lstCoords.SelectedItems.Clear();
                }
                // coordinates can't be selected in test mode
                lstCoords.Enabled = false;
                // disable view movement
                TestDir = 0;
                //tmrTest.Enabled = false;
                if (lstCommands.SelectedIndices.Count == 0) {
                    // select last cmd
                    SelectCmd(lstCommands.Items.Count - 1);
                }
                else {
                    SelectCmd(SelectedCmdIndex);
                }
                // if showing normal pixel, go to row/col mode
                if (StatusMode == EPicStatusMode.psPixel) {
                  StatusMode = EPicStatusMode.psText;
                }
                GetTextOptions();
                break;
            }
            UpdateToolBar();
            UpdateStatusBar();
            /*
            // update status bar on all mode changes

            // draw buttons and select tool disabled if not in edit mode
            // enable/disable other editing buttons
            */
        }

        private void UpdateToolBar() {
            // enable/disable other editing buttons
            // based on mode, and if drawing is selected

            // drawing tools
            if (PicMode == EPicMode.pmEdit) {
                tsbTool.Enabled = true;
                tsbPlotStyle.Enabled = true;
                tsbPlotSize.Enabled = true;
                // if neither draw color is selected and using a drawing tool
                if ((SelectedPen.VisColor == AGIColorIndex.None) && (SelectedPen.PriColor == AGIColorIndex.None) && SelectedTool != TPicToolTypeEnum.ttEdit && SelectedTool != TPicToolTypeEnum.ttSelectArea) {
                    // switch to select tool
                    SelectedTool = TPicToolTypeEnum.ttEdit;
                    tsbTool.Image = tsbSelect.Image;
                    SetCursors(EPicCur.pcEdit);
                }
                if (tsbPlotSize.Image != tdPlotSize.Items[SelectedPen.PlotSize].Image) {
                    tsbPlotSize.Image = tdPlotSize.Items[SelectedPen.PlotSize].Image;
                }
                if (tsbPlotStyle.Image != tdPlotStyle.Items[(int)SelectedPen.PlotShape + 2 * (int)SelectedPen.PlotStyle].Image) {
                    tsbPlotStyle.Image = tdPlotStyle.Items[(int)SelectedPen.PlotShape + 2 * (int)SelectedPen.PlotStyle].Image;
                }
                tsbUndo.Enabled = UndoCol.Count > 0;
                tsbPaste.Enabled = (PicMode == EPicMode.pmEdit) && SelectedTool == TPicToolTypeEnum.ttSelectArea && PicClipBoardObj != null;
                tsbCut.Enabled = true;


                if (SelectedTool == TPicToolTypeEnum.ttSelectArea) {
                    // area selection - no editing commands are enabled,
                    // only copy is available
                    tsbCut.Enabled = false;
                    tsbPaste.Enabled = false;
                    tsbDelete.Enabled = false;
                    tsbFlipH.Enabled = false;
                    tsbFlipV.Enabled = false;
                    // copy is enabled if something selected
                    tsbCopy.Enabled = (Selection.Width > 0) && (Selection.Height > 0);
                }
                else if (lstCommands.SelectedItems.Count == 0 ||
                    lstCoords.SelectedItems.Count == 0 ||
                    lstCommands.SelectedItems[0].Text[..3] == "Set") {
                    // no coordinate is selected - set editing commands to 
                    // handle the selected drawing commands
                    tsbCut.Enabled = SelectedCmdIndex != lstCommands.Items[^1].Index;
                    tsbCopy.Enabled = tsbCut.Enabled;
                    tsbDelete.Enabled = tsbCut.Enabled;
                    tsbPaste.Enabled = PicClipBoardObj != null;
                    Rectangle rect = GetSelectionBounds(lstCommands.SelectedItems[0].Index, lstCommands.SelectedItems.Count);
                    tsbFlipH.Enabled = (rect.Width > 0);
                    tsbFlipV.Enabled = (rect.Height > 0);
                }
                else {
                    // a coordinate is selected, set editing commands to
                    // handle coordinate editing
                    tsbCut.Enabled = false;
                    tsbCopy.Enabled = false;
                    switch (lstCommands.SelectedItems[0].Text[..4]) {
                    case "Abs " or "Fill" or "Plot":
                        tsbDelete.Enabled = true;
                        break;
                    case "Vis " or "Pri " or "Set ":
                        tsbDelete.Enabled = false;
                        break;
                    default:
                        tsbDelete.Enabled = lstCoords.Items[0].Selected;
                        break;
                    }
                    tsbFlipH.Enabled = false;
                    tsbFlipV.Enabled = false;
                }
            }
            else {
                tsbTool.Enabled = false;
                tsbPlotStyle.Enabled = false;
                tsbPlotSize.Enabled = false;
                tsbUndo.Enabled = false;
                tsbCut.Enabled = false;
                tsbCopy.Enabled = false;
                tsbPaste.Enabled = false;
                tsbDelete.Enabled = false;
                tsbFlipH.Enabled = false;
                tsbFlipV.Enabled = false;
            }

            // repaint color palette
            //picPalette.Invalidate();

            // update statusbar
            //UpdateStatusBar();
        }


        private void UpdateStatusBar() {
            // set status bar indicators based on current state of game

            // scale
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";

            //  coordinates (test object coords only)
            switch (StatusMode) {
            case EPicStatusMode.psPixel:
                //  normal pixel mode
                // use cusor position
                spCurX.Text = "X: " + PicPt.X;
                spCurY.Text = "Y: " + PicPt.Y;
                spPriBand.Text = "Band: " + GetPriBand((byte)PicPt.Y, EditPicture.PriBase);
                //spPriBand.Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, EditPicture.PriBase) - 3).Picture;
                break;
            case EPicStatusMode.psCoord:
                //  test object coordinates
                // use test object position
                spCurX.Text = "vX: " + TestCelPos.X;
                spCurY.Text = "vY: " + TestCelPos.Y;
                spPriBand.Text = "vBand: " + GetPriBand((byte)TestCelPos.Y, EditPicture.PriBase);
                //spPriBand.Picture = imlPriBand.ListImages(GetPriBand(TestCelPos.Y, EditPicture.PriBase) - 3).Picture;
                break;
            case EPicStatusMode.psText:
                //  text row/col
                spCurX.Text = "R: " + (PicPt.Y / 8);
                spCurY.Text = "C: " + (PicPt.X / (CharWidth / 2));
                spPriBand.Text = "Band: " + GetPriBand((byte)PicPt.Y, EditPicture.PriBase);
                //spPriBand.Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, EditPicture.PriBase) - 3).Picture;
                break;
            }
            switch (PicMode) {
            case EPicMode.pmEdit:
                spMode.Text = "Edit";
                spTool.Text = Editor.Base.LoadResString(PICTOOLTYPETEXT + (int)SelectedTool);
                spAnchor.Visible = (SelectedTool == TPicToolTypeEnum.ttSelectArea);
                spBlock.Visible = (SelectedTool == TPicToolTypeEnum.ttSelectArea);
                if (SelectedTool == TPicToolTypeEnum.ttSelectArea && StatusMode == EPicStatusMode.psPixel) {
                    if (Selection.Width > 0 || Selection.Height > 0) {
                        spAnchor.Text = "Anchor: " + Selection.X + ", " + Selection.Y;
                        spBlock.Text = "Block: " + Selection.X + ", " + Selection.Y + ", " + (Selection.X + Selection.Width - 1) + ", " + (Selection.Y + Selection.Height - 1);
                    }
                    else {
                        spAnchor.Text = "Anchor: " + PicPt.X + ", " + PicPt.Y;
                        spBlock.Text = "Block: " + PicPt.X + ", " + PicPt.Y;
                    }
                }
                break;
            case EPicMode.pmViewTest:
                spMode.Text = "Test";
                if (StopReason > 0) {
                    spTool.Text = Editor.Base.LoadResString(STOPREASONTEXT + StopReason);
                    StopReason = 0;
                }
                else {
                    // clear tool panel
                    spTool.Text = "";
                }
                break;
            }
        }

        private byte GetPriBand(byte y, byte priBase = 48) {
            // convert Y Value into appropriate priority band

            if (y < priBase) {
                return 4;
            }
            else {
                return (byte)(((int)y - priBase) / (168 - priBase) * 10 + 5);
            }
        }

        private void SetCursors(EPicCur NewCursor) {
            MemoryStream msCursor;

            if (CurCursor == NewCursor) {
                return;
            }
            CurCursor = NewCursor;
            switch (NewCursor) {
            case EPicCur.pcEdit:
                msCursor = new(EditorResources.EPC_EDIT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case EPicCur.pcCross:
                picVisual.Cursor = Cursors.Cross;
                picPriority.Cursor = Cursors.Cross;
                break;
            case EPicCur.pcMove:
                picVisual.Cursor = Cursors.Hand;
                picPriority.Cursor = Cursors.Hand;
                break;
            case EPicCur.pcDefault:
                picVisual.Cursor = Cursors.Default;
                picPriority.Cursor = Cursors.Default;
                break;
            case EPicCur.pcNO:
                picVisual.Cursor = Cursors.No;
                picPriority.Cursor = Cursors.No;
                break;
            case EPicCur.pcPaint:
                msCursor = new(EditorResources.EPC_PAINT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case EPicCur.pcBrush:
                msCursor = new(EditorResources.EPC_BRUSH);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case EPicCur.pcSelect:
                msCursor = new(EditorResources.EPC_SELECT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case EPicCur.pcEditSel:
                msCursor = new(EditorResources.EPC_EDITSEL);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            }
        }

        private void DrawPicture() {

            if (EditPicture.StepDraw) {
                // the complicated part is determining how much of the drawing to include,
                // which depends on which cmd is selected, whether moving or editing the command
                // and what Type of command it is

                // if selected item in list is a command (no coord selected)
                if (lstCoords.SelectedItems.Count == 0 || (int)SelectedCmdType < 0xF4 || (int)SelectedCmdType == 0xF9) {
                    // if not the last cmd
                    if (SelectedCmdType != DrawFunction.End) {
                        // draw pos is pos of next command
                        EditPicture.DrawPos = (int)lstCommands.Items[SelectedCmdIndex + 1].Tag;
                    }
                    else {
                        // position is end of resource
                        EditPicture.DrawPos = -1;
                    }
                }
                else {
                    // a specific coordinate is selected
                    // if moving a cmd
                    if (PicDrawMode == TPicDrawOpEnum.doMoveCmds) {
                        // draw pos is pos of next command (include the command(s) being moved)
                        EditPicture.DrawPos = (int)lstCommands.Items[SelectedCmdIndex + 1].Tag;
                    }
                    else {
                        // where to set draw pos depends on which command is selected,
                        // and which coordinate is chosen and cmd type
                        switch (SelectedCmdType) {
                        case DrawFunction.Fill or DrawFunction.PlotPen:
                            // add one to position, so the coordinate is included
                            EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag + 1;
                            break;
                        case DrawFunction.RelLine:
                            if (lstCoords.SelectedIndices[0] == 0) {
                                EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag - 1;
                            }
                            else {
                                EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag;
                            }
                            break;
                        case DrawFunction.XCorner or DrawFunction.YCorner:
                            if (lstCoords.SelectedIndices[0] == 0) {
                                EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag - 1;
                            }
                            else {
                                // if moving a coordinate,
                                if (PicDrawMode == TPicDrawOpEnum.doMovePt) {
                                    // if editing second coord,
                                    if (lstCoords.SelectedIndices[0] == 1) {
                                        // back up three so first line is not drawn
                                        EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag - 3;
                                    }
                                    else {
                                        // back up one so line in front of edit line is not drawn
                                        EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag - 1;
                                    }
                                }
                                else {
                                    EditPicture.DrawPos = (int)lstCoords.SelectedItems[0].Tag;
                                }
                            }
                            break;
                        default:
                            // draw up to current command
                            EditPicture.DrawPos = (int)lstCoords.Items[SelectedCmdIndex].Tag - 1;
                            break;
                        }
                    }
                }
            }
            if (EditPicture.BkgdShow) {
                // first, draw the background on visual
                //// (remember that IPicture objects are upside-down DIBs, so we need to flip BkgdImage vertically-
                //BkgdImage.Render picVisual.hDC, tgtX * picVisual.Width / 320, tgtY * picVisual.Height / 168, tgtW * picVisual.Width / 320, tgtH * picVisual.Height / 168, PixToMets(srcX), BkgdImage.Height - PixToMets(srcY, true), PixToMets(srcW), -PixToMets(srcH, true), 0&
                // then add visual/priority images with appropriate transparency

                /* TEMP */
                ShowAGIBitmap(picVisual, EditPicture.VisualBMP, ScaleFactor);
                ShowAGIBitmap(picPriority, EditPicture.PriorityBMP, ScaleFactor);
                /* TEMP */
            }
            else {
                ShowAGIBitmap(picVisual, EditPicture.VisualBMP, ScaleFactor);
                ShowAGIBitmap(picPriority, EditPicture.PriorityBMP, ScaleFactor);
            }

            // add test cel if in preview mode, and a test view is loaded
            bool DrawCel = (PicMode == EPicMode.pmViewTest && TestView != null && ShowTestCel);
            if (DrawCel) {
                // with appropriate transparency if Bkgd is being used
                //AddCelToPic();
            }
        }

        public void RefreshPic() {
            // update palette and redraw
            if (InGame) {
                EditPicture.Palette = EditGame.Palette.CopyPalette();
            }
            else {
                EditPicture.Palette = DefaultPalette.CopyPalette();
            }
            EditPalette = EditPicture.Palette.CopyPalette();
            EditPicture.ResetPicture();
            DrawPicture();
        }

        private void SelectCmd(int CmdPos, bool DontUpdate = true) {
            // ensures cmd list selection is cleared, and selects the desired cmd
            // if DontUpdate is true, the item is selected in the list without 
            // updating the coordinate list and the drawn images

            // disable painting of listbox until all done
            _ = SendMessage(lstCommands.Handle, WM_SETREDRAW, false, 0);
            for (int i = lstCommands.SelectedItems.Count - 1; i > 0; i--) {
                lstCommands.SelectedItems[i].Selected = false;
            }
            lstCommands.Items[CmdPos].Selected = true;
            lstCommands.FocusedItem = lstCommands.Items[CmdPos];
            lstCommands.Items[CmdPos].EnsureVisible();
            // restore painting of listbox
            _ = SendMessage(lstCommands.Handle, WM_SETREDRAW, true, 0);
            lstCommands.Refresh();
            if (!DontUpdate) {
                UpdateCmdSelection();
            }
            else {
                SelectedCmdIndex = CmdPos;
                SelectedCmdType = (DrawFunction)EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag];
            }
            lstCommands.Focus();
        }

        private void HighlightCoords() {
            /*
      Dim i As Long, lngCount As Long
      Dim tmpPT As Point, LineType As DrawFunction
      Dim cOfX As Single, cOfY As Single, cSzX As Single, cSzY As Single

      // if using original highlight mode OR if not in edit mode, just exit
      if (CursorMode = pcmWinAGI || SelectedTool != ttEdit) {
        return;
      }

      cOfX = 1.5 / ScaleFactor ^ 0.5
      cOfY = cOfX * 2 // 3 / ScaleFactor ^ 0.5
      cSzX = cOfX * 2 // 3 / ScaleFactor ^ 0.5
      cSzY = cOfY * 2 // 6 / ScaleFactor ^ 0.5

      // if any coords are in the list highlight them
      //   (lines need all coords highlighted all the time;
      //   plots and fills only highlight up to selected
      //   coord when step-draw is true)

      // get Type of line command
      LineType = EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag)
      if (lstCoords.ListIndex >= 0 && (LineType = dfFill || LineType = dfPlotPen) && EditPicture.StepDraw) {
        lngCount = lstCoords.ListIndex
      } else {
        lngCount = lstCoords.ListCount - 1
      }

      // is this selected coord?
      if (lstCoords.ListIndex != -1) {
        tmpPT = ExtractCoordinates(lstCoords.Text)
      } else {
        // set X to invalid value so it'll never match
        tmpPT.X = 255
      }

      if (lngCount >= 0) {
        for (int i = 0; i <= lngCount; i++) {
          if (CoordPT(i).X = tmpPT.X && CoordPT(i).Y = tmpPT.Y) {
            // draw a box
            if (SelectedPen.VisColor < agNone) {
              picVisual.Line ((CoordPT(i).X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), VCColor, B
            }
            if (SelectedPen.PriColor < agNone) {
              picPriority.Line ((CoordPT(i).X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), VCColor, B
            }
          } else {
            // highlight this coord with an X
            if (SelectedPen.VisColor < agNone) {
              picVisual.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (-cSzY - 0.3) * ScaleFactor), VCColor
              picVisual.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 - cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (cSzY + 0.3) * ScaleFactor), VCColor
            }
            if (SelectedPen.PriColor < agNone) {
              picPriority.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (-cSzY - 0.3) * ScaleFactor), PCColor
              picPriority.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 - cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (cSzY + 0.3) * ScaleFactor), PCColor
            }
          }
        }
      }

            */
        }

        private void RefreshPens(int ForcePen = 0) {
            /*
      // if the current command matches a pen that is changed,
      // we need to change it; otherwise, a new command
      // will be inserted
      // ForcePen=1 means force the visual pen
      //         =2 means force the priority pen
      //         =3 means force plot pen

      // this function assumes selected command is the location to refresh

      Dim VisCmdIndex As Long, PriCmdIndex As Long, PenCmdIndex As Long
      Dim bytData() As Byte
      Dim strBrush As String
      Dim NextUndo As PictureUndo

      // if not forcing
      if (ForcePen = 0) {
        // if current pen and selected pen already match
        if (CurrentPen.PlotShape = SelectedPen.PlotShape &&
           CurrentPen.PlotSize = SelectedPen.PlotSize &&
           CurrentPen.PlotStyle = SelectedPen.PlotStyle &&
           CurrentPen.PriColor = SelectedPen.PriColor &&
           CurrentPen.VisColor = SelectedPen.VisColor) {
            // no action required
          return;
        }
      }

      // determine if current command is a pen command, and if so,
      // what type

      // set indices for set cmds to -1 as default; a positive value
      // will mean that type is currently theselected command
      VisCmdIndex = -1
      PriCmdIndex = -1
      PenCmdIndex = -1

      // check for set commands
      switch (CmdType(lstCommands.List(SelectedCmd))) {
      case 0:
        // skip- command is a draw command

      case 1:
            // visual
        VisCmdIndex = SelectedCmd

      case 2:
            // priority
        PriCmdIndex = SelectedCmd

      case 3:
            // pen
        PenCmdIndex = SelectedCmd

      }

      // now check to see what changed; could be one or more
      // visual, priority or pen setting, so we check them all

      // if visual colors are different update or insert a visual pen command
      if (CurrentPen.VisColor != SelectedPen.VisColor || ForcePen = 1) {
        // if selected cmd is NOT vispen OR forcing a new pen,
        if (VisCmdIndex = -1 || ForcePen = 1) {
          // if pen is being turned off
          if (SelectedPen.VisColor = agNone) {
            // insert a visual disable cmd
            ReDim bytData(0)
            bytData(0) = CByte(dfDisableVis)
            InsertCommand bytData, SelectedCmd, "Vis: Off", gInsertBefore
          } else {
            // insert a visual enable cmd
            ReDim bytData(1)
            bytData(0) = CByte(dfEnableVis)
            bytData(1) = CByte(SelectedPen.VisColor)
            InsertCommand bytData, SelectedCmd, "Vis: " & LoadResString(COLORNAME + SelectedPen.VisColor), gInsertBefore
          }
          // change selection to this command
          SelectedCmd = lstCommands.NewIndex
        } else {
          // update existing vis cmd
          ChangeColor VisCmdIndex, SelectedPen.VisColor
        }

        // set CurrentPen to new Value
        CurrentPen.VisColor = SelectedPen.VisColor
      }

      // if priority colors are different, update or insert a priority pen command
      if (CurrentPen.PriColor != SelectedPen.PriColor || ForcePen = 2) {
        // if selected cmd is NOT pripen OR forcing a new pen,
        if (PriCmdIndex = -1 || ForcePen = 2) {
          // if pen is being turned off
          if (SelectedPen.PriColor = agNone) {
            // insert a priority disable cmd
            ReDim bytData(0)
            bytData(0) = CByte(dfDisablePri)
            InsertCommand bytData, SelectedCmd, "Pri: Off", gInsertBefore
          } else {
            // insert a Priority enable command
            ReDim bytData(1)
            bytData(0) = CByte(dfEnablePri)
            bytData(1) = CByte(SelectedPen.PriColor)
            InsertCommand bytData, SelectedCmd, "Pri: " & LoadResString(COLORNAME + SelectedPen.PriColor), gInsertBefore
          }
          // change selection to this command
          SelectedCmd = lstCommands.NewIndex
        } else {
          // update existing pri cmd
          ChangeColor PriCmdIndex, SelectedPen.PriColor
        }

        // set CurrentPen to new color
        CurrentPen.PriColor = SelectedPen.PriColor
      }

      // if selected pen different from the current pen
      // we need to update or insert a pen
      // (currently nothing ever forces pen for plot settings,
      // but leave the check in anyway)
      if (CurrentPen.PlotShape != SelectedPen.PlotShape ||
         CurrentPen.PlotSize != SelectedPen.PlotSize ||
         CurrentPen.PlotStyle != SelectedPen.PlotStyle || ForcePen = 3) {

        // dynamic array so it can be passed as a variable
        ReDim bytData(1)

        // build set pen command byte
        if (SelectedPen.PlotStyle = psSolid) {
          strBrush = "Solid "
          bytData(1) = 0
        } else {
          strBrush = "Splatter "
          bytData(1) = 0x20
        }
        if (SelectedPen.PlotShape = psCircle) {
          strBrush = strBrush & "Circle "
        } else {
          strBrush = strBrush & "Rectangle "
          bytData(1) = bytData(1) | 0x10
        }
        strBrush = strBrush & CStr(SelectedPen.PlotSize)
        bytData(1) = bytData(1) + SelectedPen.PlotSize

        // if selected cmd is NOT a plotpen or forcing a new pen
        // (currently nothing ever forces pen for plot settings,
        // but leave the check in anyway)
        if (PenCmdIndex = -1 || ForcePen = 3) {
          // insert a set plot pen command
          bytData(0) = dfChangePen

          // if plot style is different
          if (CurrentPen.PlotStyle != SelectedPen.PlotStyle) {
            // adjust plot commands
            ReadjustPlotCoordinates SelectedCmd, SelectedPen.PlotStyle
          }

          // then add the new 'set pen' command
          InsertCommand bytData, SelectedCmd, "Set Pen: " & strBrush, gInsertBefore
          // change selection to this command
          SelectedCmd = lstCommands.NewIndex
        } else {
          // update the pen command at this location

          // if plot style is different
          if (CurrentPen.PlotStyle != SelectedPen.PlotStyle) {
            // adjust plot commands
            ReadjustPlotCoordinates PenCmdIndex + 1, SelectedPen.PlotStyle
          }

          if (Settings.PicUndo != 0) {
            // add undo object
            Set NextUndo = New PictureUndo
              NextUndo.UDAction = ChangePlotPen
              NextUndo.UDPicPos = lstCommands.Items[PenCmdIndex].Tag
              NextUndo.UDCmdIndex = PenCmdIndex
              bytData(0) = EditPicture.Resource.Data(NextUndo.UDPicPos + 1)
              NextUndo.UDData = bytData()
              NextUndo.UDText = lstCommands.List(PenCmdIndex)
            AddUndo NextUndo
          }

          // change existing command byte
          EditPicture.Resource.Data(NextUndo.UDPicPos + 1) = bytData(1)
          // and text
          lstCommands.List(PenCmdIndex) = "Set Pen: " & strBrush
        }

        // set pen values
        CurrentPen.PlotShape = SelectedPen.PlotShape
        CurrentPen.PlotSize = SelectedPen.PlotSize
        CurrentPen.PlotStyle = SelectedPen.PlotStyle
      }

      // re-select cmd to force everything to update
      SelectCmd SelectedCmd, false
            */
        }

        private Rectangle GetSelectionBounds(int SelCmd, int SelCount, bool ShowBox = false) {

            // determines the starting (upper left corner) and the
            // size of selected cmds, and sets selection box to match
            // optionally draws a selection box around the commands

            int lngPos;
            byte bytX, bytY, bytCmd;
            int xdisp, ydisp;
            bool blnRelX = false;
            Rectangle retval = new(-1, -1, 0, 0);

            // go through each cmd; check it for coordinates
            // if coordinates are found, step through them to
            // determine if any coords expand the selected area

            // NOTE: for plots, need to be aware of pen status
            // so coordinate values get extracted correctly
            PlotStyle tmpPlotStyle = GetPenStyle((int)lstCommands.Items[SelCmd - SelCount + 1].Tag);
            byte[] bytData = EditPicture.Data;
            for (int i = SelCount - 1; i >= 0; i--) {
                // set starting pos for this cmd
                lngPos = (int)lstCommands.Items[SelCmd - i].Tag;
                bytCmd = bytData[lngPos++];
                // parse coords based on cmdtype
                switch (bytCmd) {
                // case dfEnableVis or dfDisableVis or dfEnablePri or dfDisablePri:
                //    break;
                // ignore cmds that have no coordinates
                case (byte)DrawFunction.ChangePen:
                    // update plot parameters
                    bytCmd = bytData[lngPos++];
                    tmpPlotStyle = (PlotStyle)((bytCmd & 0x20) / 0x20);
                    break;
                case (byte)DrawFunction.YCorner or (byte)DrawFunction.XCorner:
                    // set initial direction
                    blnRelX = (bytCmd == 0xF5);
                    // get coordinates
                    bytX = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    bytY = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    ComparePoints(ref retval, bytX, bytY);
                    bytCmd = bytData[lngPos++];
                    while (bytCmd < 0xF0) {
                        if (blnRelX) {
                            bytX = bytCmd;
                        }
                        else {
                            bytY = bytCmd;
                        }
                        blnRelX = !blnRelX;
                        ComparePoints(ref retval, bytX, bytY);
                        bytCmd = bytData[lngPos++];
                    }
                    break;
                case (byte)DrawFunction.AbsLine or (byte)DrawFunction.Fill:
                    do {
                        bytX = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        bytY = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        // compare to start/end
                        ComparePoints(ref retval, bytX, bytY);
                    } while (true);
                    break;
                case (byte)DrawFunction.RelLine:
                    // get coordinates
                    bytX = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    bytY = bytData[lngPos++];
                    if (bytY >= 0xF0) {
                        break;
                    }
                    ComparePoints(ref retval, bytX, bytY);
                    bytCmd = bytData[lngPos++];
                    while (bytCmd < 0xF0) {
                        // if horizontal negative bit set
                        if ((bytCmd & 0x80) > 0) {
                            xdisp = -((bytCmd & 0x70) / 0x10);
                        }
                        else {
                            xdisp = (bytCmd & 0x70) / 0x10;
                        }
                        // if vertical negative bit is set
                        if ((bytCmd & 0x8) > 1) {
                            ydisp = -(bytCmd & 0x7);
                        }
                        else {
                            ydisp = bytCmd & 0x7;
                        }
                        bytX = (byte)(bytX + xdisp);
                        bytY = (byte)(bytY + ydisp);
                        // compare to start/end
                        ComparePoints(ref retval, bytX, bytY);
                        // read in next command
                        bytCmd = bytData[lngPos++];
                    }
                    break;
                case (byte)DrawFunction.PlotPen:
                    do {
                        // if brush is splatter
                        if (tmpPlotStyle > 0) {
                            // skip splatter byte
                            bytCmd = bytData[lngPos++];
                            if (bytCmd >= 0xF0) {
                                break;
                            }
                        }
                        // get coordinates
                        bytX = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        bytY = bytData[lngPos++];
                        if (bytY >= 0xF0) {
                            break;
                        }
                        // compare to start/end
                        ComparePoints(ref retval, bytX, bytY);
                    } while (true);
                    break;
                }
            }
            // if no cmds found that have coordinates
            if (Selection.X == -1) {
                Debug.Assert(false);
            }
            // if optionally drawing selection box around the commands,
            if (ShowBox) {
                Selection = retval;
                ShowCmdSelection();
            }
            return retval;
        }

        private void ClearCoordList() {
            // clear lstCoord, clear and reset coordpt list
            lstCoords.Items.Clear();
            CoordPT = new Point[100];
            // always set CurPt to impossible value so clicking any coord will reselect correctly
            CurPt.X = 255;
            CurPt.Y = 255;
        }

        private PlotStyle GetPenStyle(int lngPos) {

            // determines pen status for a given position
            byte[] bytData = EditPicture.Data;

            // default is solid
            PlotStyle retval = PlotStyle.Solid;
            for (int i = 0; i <= lngPos; i++) {
                if (bytData[i] == (byte)DrawFunction.ChangePen) {
                    // set new pen status
                    retval = (PlotStyle)((bytData[i + 1] & 0x20) / 0x20);
                }
            }
            return retval;
        }

        private void ComparePoints(ref Rectangle rect, byte bytX, byte bytY) {
            // expands the rectangle to include the referenced point
            if (bytX < rect.X || rect.X < 0) {
                rect.X = bytX;
            }
            if (bytY < rect.Y || rect.Y < 0) {
                rect.Y = bytY;
            }
            if (bytX > rect.X + rect.Width) {
                rect.Width = bytX - rect.X;
            }
            if (bytY > rect.Y + rect.Height) {
                rect.Height = bytY - rect.Y;
            }
        }

        private void BuildCoordList(int ListPos) {

            // build coord list, based on selected cmd

            byte bytCmd;
            byte[] bytData;
            int lngPos;
            byte bytX, bytY;
            byte xdisp, ydisp;
            bool blnRelX;
            byte PatternNum;
            string strBrush;
            bool blnSplat;

            // clear coords list
            ClearCoordList();

            if (lstCommands.SelectedItems.Count != 1) {
                return;
            }

            // if end selected
            if (SelectedCmdIndex == lstCommands.Items.Count - 1) {
                return;
            }

            bytData = EditPicture.Data;

            // set starting pos for the selected cmd
            lngPos = (int)lstCommands.Items[ListPos].Tag;

            // get command Type
            bytCmd = bytData[lngPos];
            if (bytCmd <= 0xf3) {
                lblCoords.Text = "Parameter";
            }
            else if (bytCmd == 0xf9) {
                lblCoords.Text = "Parameters";
            }
            else {
                lblCoords.Text = "Coordinates";
            }
            // add command based on Type
            switch (bytCmd) {
            //case 0xF0 or 0xF1 or 0xF2 or 0xF3 or 0xF9:
            //    // pen functions; no coords.
            //    break;
            case 0xF0 or 0xF2:
                // get color
                lngPos++;
                bytX = bytData[lngPos];
                // RARE, but check for color out of bounds
                ListViewItem item;
                if (bytX > 15) {
                    item = lstCoords.Items.Add("ERR(0x" + bytX.ToString("x2") + ")");
                }
                else {
                    item = lstCoords.Items.Add(Editor.Base.LoadResString(COLORNAME + bytX));
                }
                item.Tag = lngPos;
                break;
            case 0xF1 or 0xF3:
                // no parameters
                break;
            case 0xF9:
                // Change pen size and style.
                lngPos++;
                bytX = bytData[lngPos];
                if ((bytX & 0x20) / 0x20 == 0) {
                    item = lstCoords.Items.Add("Style: Solid");
                }
                else {
                    item = lstCoords.Items.Add("Style: Splatter");
                }
                item.Tag = lngPos;
                if ((bytX & 0x10) / 0x10 == 0) {
                    item = lstCoords.Items.Add("Shape: Circle");
                }
                else {
                    item = lstCoords.Items.Add("Shape: Rectangle");
                }
                item.Tag = lngPos;
                item = lstCoords.Items.Add("Size: " + (bytX & 0x7).ToString());
                item.Tag = lngPos;
                break;
            case 0xF4 or 0xF5:
                // Draw an X or Y corner.
                // set initial direction
                blnRelX = (bytCmd == 0xF5);
                // get coordinates
                lngPos++;
                bytX = bytData[lngPos];
                if (bytX >= 0xF0) {
                    return;
                }
                lngPos++;
                bytY = bytData[lngPos];
                if (bytX >= 0xF0) {
                    return;
                }

                // add start (adjust by 1 so first byte of coordinate is stored as position)
                AddCoordToList(bytX, bytY, lngPos - 1);

                // get next byte as potential command
                lngPos++;
                bytCmd = bytData[lngPos];

                while (bytCmd < 0xF0) {
                    if (blnRelX) {
                        bytX = bytCmd;
                    }
                    else {
                        bytY = bytCmd;
                    }
                    blnRelX = !blnRelX;

                    // add coordinate node, and set position
                    AddCoordToList(bytX, bytY, lngPos);

                    // get next coordinate or command
                    lngPos++;
                    bytCmd = bytData[lngPos];
                }
                break;
            case 0xF6:
                // Absolute line (long lines).
                // get coordinates
                lngPos++;
                bytX = bytData[lngPos];
                if (bytX >= 0xF0) {
                    return;
                }
                lngPos++;
                bytY = bytData[lngPos];
                if (bytY >= 0xF0) {
                    return;
                }

                // add start (adjust by 1 so first byte of coordinate is stored as position)
                AddCoordToList(bytX, bytY, lngPos - 1);

                // get next byte as potential command
                lngPos++;
                bytCmd = bytData[lngPos];

                while (bytCmd < 0xF0) {
                    bytX = bytCmd;
                    lngPos++;
                    bytY = bytData[lngPos];

                    // add coordinate node, and set position
                    AddCoordToList(bytX, bytY, lngPos - 1);

                    // read in next command
                    lngPos++;
                    bytCmd = bytData[lngPos];
                }
                break;
            case 0xF7:
                // Relative line (short lines).
                // get coordinates
                lngPos++;
                bytX = bytData[lngPos];
                if (bytX >= 0xF0) {
                    return;
                }
                lngPos++;
                bytY = bytData[lngPos];
                if (bytY >= 0xF0) {
                    return;
                }

                // add start (adjust by 1 so first byte of coordinate is stored as position)
                AddCoordToList(bytX, bytY, lngPos - 1);

                // get next byte as potential command
                lngPos++;
                bytCmd = bytData[lngPos];

                while (bytCmd < 0xF0) {
                    // if horizontal negative bit set
                    if ((bytCmd & 0x80) == 0x80) {
                        xdisp = (byte)-((bytCmd & 0x70) / 0x10);
                    }
                    else {
                        xdisp = (byte)((bytCmd & 0x70) / 0x10);
                    }
                    // if vertical negative bit is set
                    if ((bytCmd & 0x8) == 0x8) {
                        ydisp = (byte)-(bytCmd & 0x7);
                    }
                    else {
                        ydisp = (byte)(bytCmd & 0x7);
                    }
                    bytX = (byte)(bytX + xdisp);
                    bytY = (byte)(bytY + ydisp);

                    // add coordinate node, and set position
                    AddCoordToList(bytX, bytY, lngPos);

                    // read in next command
                    lngPos++;
                    bytCmd = bytData[lngPos];
                }
                break;
            case 0xF8:
                // Fill.
                // get next byte as potential command
                lngPos++;
                bytCmd = bytData[lngPos];

                while (bytCmd < 0xF0) {
                    // get coordinates
                    bytX = bytCmd;
                    lngPos++;
                    bytY = bytData[lngPos];

                    // add coord
                    AddCoordToList(bytX, bytY, lngPos - 1);

                    // read in next command
                    lngPos++;
                    bytCmd = bytData[lngPos];
                }
                break;
            case 0xFA:
                // Plot with pen.
                // get next byte as potential command
                lngPos++;
                bytCmd = bytData[lngPos];
                while (bytCmd < 0xF0) {
                    // if brush is splatter
                    if (CurrentPen.PlotStyle == PlotStyle.Splatter) {
                        PatternNum = (byte)(bytCmd / 2);
                        // strBrush = "[Pattern " + PatternNum + "] ";
                        strBrush = PatternNum + " -- ";
                        // get next byte
                        lngPos++;
                        bytCmd = bytData[lngPos];
                        // set offset to 2 (to account for pattern number and x coord)
                        xdisp = 2;
                    }
                    else {
                        strBrush = "";
                        // set offset to 1 (to account for x coord)
                        xdisp = 1;
                    }

                    // get coordinates
                    bytX = bytCmd;
                    lngPos++;
                    bytY = bytData[lngPos];
                    // add coord
                    AddCoordToList(bytX, bytY, lngPos - xdisp, strBrush);

                    // read in next command
                    lngPos++;
                    bytCmd = bytData[lngPos];
                }
                break;
            }

            // highlight the coords, if in edit mode
            if (PicMode == EPicMode.pmEdit) {
                HighlightCoords();
            }
            CoordColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void AddCoordToList(byte bytX, byte bytY, int CmdPos, string Prefix = "", int InsertPos = -1) {
            // add to listbox (normally at end, but if a position is passed, insert it there
            ListViewItem item;
            if (InsertPos == -1) {
                item = lstCoords.Items.Add(Prefix + CoordText(bytX, bytY));
            }
            else {
                item = lstCoords.Items.Insert(InsertPos, Prefix + CoordText(bytX, bytY));
            }
            item.Tag = CmdPos;

            // add to coord list (make sure there is room)
            // (always add to end - order doesn't matter)
            if (lstCoords.Items.Count > CoordPT.Length) {
                Array.Resize(ref CoordPT, CoordPT.Length + 100);
            }
            CoordPT[lstCoords.Items.Count - 1].X = bytX;
            CoordPT[lstCoords.Items.Count - 1].Y = bytY;
        }

        public string CoordText(byte X, byte Y) {
            // this function creates the coordinate text in the form
            //     (X, Y)
            return "(" + X + ", " + Y + ")";
        }

        public void ChangeScale(int Dir, bool useanchor = false) {
            double oldscale = 0;
            if (useanchor) {
                oldscale = ScaleFactor;
            }
            switch (Dir) {
            case > 0:
                if (ScaleFactor < 3) {
                    ScaleFactor += 0.25;
                }
                else if (ScaleFactor < 8) {
                    ScaleFactor += 0.5;
                }
                else if (ScaleFactor < 20) {
                    ScaleFactor += 1;
                }
                break;
            case < 0:
                if (ScaleFactor > 8) {
                    ScaleFactor -= 1;
                }
                else if (ScaleFactor > 3) {
                    ScaleFactor -= 0.5;
                }
                else if (ScaleFactor > 1) {
                    ScaleFactor -= 0.25;
                }
                break;
            }
            if (oldscale == ScaleFactor) {
                return;
            }
            _ = SendMessage(splitImages.Panel1.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picVisual.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picPriority.Handle, WM_SETREDRAW, false, 0);
            // resize images
            picVisual.Width = picPriority.Width = (int)(320 * ScaleFactor);
            picVisual.Height = picPriority.Height = (int)(168 * ScaleFactor);
            // then set the scrollbars
            SetScrollbars(oldscale);
            // redraw pictures at new scale
            DrawPicture();
            _ = SendMessage(splitImages.Panel1.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picVisual.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picPriority.Handle, WM_SETREDRAW, false, 0);

            // visual
            if (CurCmdIsLine) {
                // draw temp line based on current node
                DrawTempLine(false, 0, 0);
            }
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";

            if (Selection.Width > 0 && Selection.Height > 0) {
                // need to redraw selection shapes
                ShowCmdSelection();
            }
            // if only one selected command AND it has coords AND tool is 'none' AND in edit mode
            if (lstCommands.SelectedItems.Count == 1 &&
                  lstCoords.Items.Count > 0 &&
                  SelectedTool == TPicToolTypeEnum.ttEdit &&
                  PicMode == EPicMode.pmEdit) {
                HighlightCoords();
            }
            _ = SendMessage(picPriority.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(picVisual.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(splitImages.Panel1.Handle, WM_SETREDRAW, true, 0);
            splitImages.Refresh();
            //picVisual.Refresh();
            //picPriority.Refresh();
            //MDIMain.UseWaitCursor = false;
        }

        private void SetScrollbars(double oldscale = 0) {
            bool showHSB = false, showVSB = false;

            if (!splitImages.Visible) {
                return;
            }
            if (splitImages.Panel1.Height > 16) {
                // determine if scrollbars are necessary
                showHSB = picVisual.Width > (splitImages.Panel1.ClientSize.Width - 2 * PE_MARGIN);
                showVSB = picVisual.Height > (splitImages.Panel1.ClientSize.Height - 2 * PE_MARGIN - (showHSB ? hsbVisual.Height : 0));
                // check horizontal again(in case addition of vert scrollbar forces it to be shown)
                showHSB = picVisual.Width > (splitImages.Panel1.ClientSize.Width - 2 * PE_MARGIN - (showVSB ? vsbVisual.Width : 0));
                // initial positions
                hsbVisual.Top = splitImages.Panel1.ClientSize.Height - hsbVisual.Height;
                vsbVisual.Left = splitImages.Panel1.ClientSize.Width - vsbVisual.Width;
                hsbVisual.Width = splitImages.Panel1.ClientSize.Width;
                vsbVisual.Height = splitImages.Panel1.ClientSize.Height;
                if (showHSB && showVSB) {
                    // allow for corner
                    picCornerVis.Left = vsbVisual.Left;
                    picCornerVis.Top = hsbVisual.Top;
                    vsbVisual.Height -= hsbVisual.Height;
                    hsbVisual.Width -= vsbVisual.Width;
                    picCornerVis.Visible = true;
                }
                else {
                    picCornerVis.Visible = false;
                }
                hsbVisual.Visible = showHSB;
                vsbVisual.Visible = showVSB;
            }

            if (splitImages.Panel2.Height > 16) {
                // determine if scrollbars are necessary
                showHSB = picPriority.Width > (splitImages.Panel2.ClientSize.Width - 2 * PE_MARGIN);
                showVSB = picPriority.Height > (splitImages.Panel2.ClientSize.Height - 2 * PE_MARGIN - (showHSB ? hsbPriority.Height : 0));
                // check horizontal again(in case addition of vert scrollbar forces it to be shown)
                showHSB = picPriority.Width > (splitImages.Panel2.ClientSize.Width - 2 * PE_MARGIN - (showVSB ? vsbPriority.Width : 0));
                // initial positions
                hsbPriority.Top = splitImages.Panel2.ClientSize.Height - hsbPriority.Height;
                vsbPriority.Left = splitImages.Panel2.ClientSize.Width - vsbPriority.Width;
                hsbPriority.Width = splitImages.Panel2.ClientSize.Width;
                vsbPriority.Height = splitImages.Panel2.ClientSize.Height;
                if (showHSB && showVSB) {
                    // allow for corner
                    picCornerPri.Left = vsbPriority.Left;
                    picCornerPri.Top = hsbPriority.Top;
                    vsbPriority.Height -= hsbPriority.Height;
                    hsbPriority.Width -= vsbPriority.Width;
                    picCornerPri.Visible = true;
                }
                else {
                    picCornerPri.Visible = false;
                }
                hsbPriority.Visible = showHSB;
                vsbPriority.Visible = showVSB;
            }

            Point anchorpt = new(-1, -1);
            if (oldscale > 0) {
                // if using anchor point need to determine which image the
                // cursor is in
                Point cp = splitImages.Panel1.PointToClient(Cursor.Position);
                if (splitImages.Panel1.ClientRectangle.Contains(cp)) {
                    // use this anchor
                    anchorpt = cp;
                }
                else {
                    cp = splitImages.Panel2.PointToClient(Cursor.Position);
                    if (splitImages.Panel2.ClientRectangle.Contains(cp)) {
                        // use this anchor
                        anchorpt = cp;
                    }
                    else {
                        // not a valid anchor
                        oldscale = 0;
                    }
                }
            }
            // now adjust all scrollbar parameters as needed
            AdjustScrollbars(oldscale, anchorpt, hsbVisual, vsbVisual, picVisual, splitImages.Panel1);
            AdjustScrollbars(oldscale, anchorpt, hsbPriority, vsbPriority, picPriority, splitImages.Panel2);
        }

        private void AdjustScrollbars(double oldscale, Point anchor, HScrollBar hsb, VScrollBar vsb, PictureBox image, SplitterPanel panel) {
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
                hsb.LargeChange = (int)(panel.ClientSize.Width * LG_SCROLL);
                hsb.SmallChange = (int)(panel.ClientSize.Width * SM_SCROLL);
                // calculate actual max (when image is fully scrolled to right)
                int SV_MAX = image.Width - (panel.ClientSize.Width - (vsb.Visible ? vsb.Width : 0)) + PE_MARGIN;
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
                    newscroll = (int)(hsb.Value + (hsb.Value + anchor.X - PE_MARGIN) * (ScaleFactor / oldscale - 1));
                }
                else {
                    newscroll = hsb.Value;
                }
                if (newscroll < -PE_MARGIN) {
                    hsb.Value = -PE_MARGIN;
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
                hsb.Value = -PE_MARGIN;
            }
            // readjust picture position
            image.Left = -hsb.Value;

            // repeat for vertical scrollbar
            if (vsb.Visible) {
                vsb.Maximum = image.ClientSize.Height;
                vsb.LargeChange = (int)(panel.ClientSize.Height * LG_SCROLL);
                vsb.SmallChange = (int)(panel.ClientSize.Height * SM_SCROLL);
                int SV_MAX = image.Height - (panel.ClientSize.Height - (hsb.Visible ? hsb.Height : 0)) + PE_MARGIN;
                vsb.Maximum = SV_MAX + vsb.LargeChange - 1;
                int newscroll;
                if (oldscale > 0) {
                    newscroll = (int)(vsb.Value + (vsb.Value + anchor.Y - PE_MARGIN) * (ScaleFactor / oldscale - 1));
                }
                else {
                    newscroll = vsb.Value;
                }
                if (newscroll < -PE_MARGIN) {
                    vsb.Value = -PE_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    vsb.Value = SV_MAX;
                }
                else {
                    vsb.Value = newscroll;
                }
            }
            else {
                vsb.Value = -PE_MARGIN;
            }
            image.Top = -vsb.Value;
        }

        private void DrawTempLine(bool Editing, byte NewX, byte NewY) {
            /*
      // this command draws the line defined by current command
      // it is used to draw temporary lines when point by point editing
      // is desired

      // if Editing is false, no change to current list of coordinates is needed

      // this routine will validate whether NewX and/or NewY is valid based on line command
      // being edited

      Dim i As Long
      Dim CoordCount As Long
      Dim LineType As DrawFunction
      Dim CornerLine As Boolean, XFirst As Boolean
      Dim StartPt As Point, EndPT As Point, tmpPT As Point

      // get coord Count
      CoordCount = lstCoords.ListCount

      // if no coordinates,
      if (CoordCount = 0) {
        return;
      // if only one coordinate
      } else if (CoordCount = 1) {
        // if editing this coord
        if (Editing) {
          // draw new coordinate point
          DrawLine NewX, NewY, NewX, NewY
        } else {
          // get coordinate
          StartPt = ExtractCoordinates(lstCoords.Text)
          DrawLine StartPt.X, StartPt.Y, StartPt.X, StartPt.Y
        }
        return;
      }

      // get Type of line command
      LineType = EditPicture.Resource.Data(lstCommands.Items[SelectedCmd].Tag)

      // if not editing,
      if (!Editing) {
        // draw all lines normally
        if (EditCoordNum = 0) {
          // start at beginning
          StartPt = ExtractCoordinates(lstCoords.List(0))
        } else {
          // start at endpoint
          StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 1))
        }

        // get starting point
        for (int i = EditCoordNum; i < CoordCount; i++) {
          // if editing first pt, skip first iteration
          if (i = 0) {
            i = i + 1
          }

          // get reference to next coord
          EndPT = ExtractCoordinates(lstCoords.List(i))
          // draw the line
          DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
          // set end point as new start point
          StartPt = EndPT
        }
      } else {
        // if an x or Y corner is being edited
        if ((LineType = dfXCorner) || (LineType = dfYCorner)) {
          // enable corner editing
          CornerLine = true
          // determine if x or Y is changed first (at EditCoordNum - 1)
          XFirst = (Int(EditCoordNum / 2) != EditCoordNum / 2)
          // if command is Ycorner,
          if (LineType = dfYCorner) {
            // invert xfirst
            XFirst = !XFirst
          }

          // if edit coord is first point
          if (EditCoordNum = 1) {
            StartPt = ExtractCoordinates(lstCoords.List(0))
            if (XFirst) {
              StartPt.Y = NewY
            } else {
              StartPt.X = NewX
            }

            EndPT.X = NewX
            EndPT.Y = NewY

            DrawLine EndPT.X, EndPT.Y, StartPt.X, StartPt.Y
          } else if (EditCoordNum != 0) {
            // need to draw existing line in front of editcoord to newpooint
            StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 2))
            EndPT = StartPt
            if (XFirst) {
              EndPT.Y = NewY
            } else {
              EndPT.X = NewX
            }

            DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
            StartPt = EndPT
          }

        } else {
          // no corner editing
          CornerLine = false

          // if not first coordinate
          if (EditCoordNum != 0) {
            // extract starting x and Y from coord just in front of edited coord
            StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 1))
          }
        }

        // now draw line

        // step through rest of coordinates
        for (int i = EditCoordNum; i < lstCoords.ListCount; i++) {
          // get next point
          EndPT = ExtractCoordinates(lstCoords.List(i))

          // if this coord is the edited one
          if (i = EditCoordNum) {
            switch (LineType) {
            case dfRelLine:
              // need to validate x and Y first
              // if not first point
              if (i > 0) {
                // validate x and Y against next pt
                // (note that delta x is limited to -6 to avoid
                // values above 0xF0, which would mistakenly be interpreted
                // as a new command)
                if (NewX > StartPt.X + 7) {
                  NewX = StartPt.X + 7
                } else if (NewX < StartPt.X - 6) {
                  NewX = StartPt.X - 6
                }
                if (NewY > StartPt.Y + 7) {
                  NewY = StartPt.Y + 7
                } else if (NewY < StartPt.Y - 7) {
                  NewY = StartPt.Y - 7
                }
              }
              // if not last point
              if (i < CoordCount - 1) {
                // validate against next point
                // note that delta x is limited to -6 (swapped because we are
                // comparing against NEXT vs. PREVIOUS coordinate)
                // for same reason as given above
                tmpPT = ExtractCoordinates(lstCoords.List(i + 1))
                if (NewX > tmpPT.X + 6) {
                  NewX = tmpPT.X + 6
                } else if (NewX < tmpPT.X - 7) {
                  NewX = tmpPT.X - 7
                }
                if (NewY > tmpPT.Y + 7) {
                  NewY = tmpPT.Y + 7
                } else if (NewY < tmpPT.Y - 7) {
                  NewY = tmpPT.Y - 7
                }
              }
            case dfXCorner or dfYCorner:
              if (i = 0) {
                // set start equal to endpt
                StartPt.X = NewX
                StartPt.Y = NewY
              }

            }

            // use new x and Y
            EndPT.X = NewX
            EndPT.Y = NewY

            if (i = 0) {
              // start pt= endpt
              StartPt = EndPT
            }

          // if editing corner
          } else if (CornerLine) {
            // if this coord is directly in front of edited one
            if (i = EditCoordNum - 2) {
              // if xfirst
              if (XFirst) {
                EndPT.X = NewX
              } else {
                EndPT.Y = NewY
              }
            // if this coord is directly after edited one
            } else if (i = EditCoordNum + 1) {
              // if xfirst
              if (XFirst) {
                EndPT.X = NewX
              } else {
                EndPT.Y = NewY
              }
            }
          }

          // draw the line
          DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
          // set end point as new start point
          StartPt = EndPT
        }
      }
            */
        }

        private void ClearCmdSelection() {

            Selection.X = 0;
            Selection.Y = 0;
            Selection.Width = 0;
            Selection.Height = 0;
            //// hide the shapes; the selected cmds dont include cooordinates
            //tmrSelect.Enabled = false;
            //shpVis.Visible = false;
            //shpPri.Visible = false;
        }

        private void ShowCmdSelection() {
            /*
      // positions and displays a flashing selection outline around the current selection box

      // if selection is more than a single pixel
      if (Selection.Width > 0 && Selection.Height > 0) {
        if (!shpVis.Visible) {
          shpVis.Visible = true;
          shpPri.Visible = true;
        }

        // only reposition if cursor has moved
        if ((shpVis.Left != Selection.X * ScaleFactor * 2 - 1) || (shpVis.Top != Selection.Y * ScaleFactor - 1) || (shpVis.Width != Selection.Width * ScaleFactor * 2 + 2) || (shpVis.Height != Selection.Height * ScaleFactor + 2)) {
          // position the shapes around the selection area
          shpVis.Move( Selection.X * ScaleFactor * 2 - 1, Selection.Y * ScaleFactor - 1, Selection.Width * ScaleFactor * 2 + 2, Selection.Height * ScaleFactor + 2);
          // check if off edge
          if (shpVis.Left = -1) {
            shpVis.Left = 0;
            shpVis.Width = shpVis.Width - 1;
          }
          if (shpVis.Top = -1) {
            shpVis.Top = 0;
            shpVis.Height = shpVis.Height - 1;
          }
          if (Selection.X + Selection.Width = 160) {
            shpVis.Width = shpVis.Width - 1;
          }
          if (Selection.Y + Selection.Height = 168) {
            shpVis.Height = shpVis.Height - 1;
          }

          // move priority screen shape to match visual screen
          shpPri.Move (shpVis.Left, shpVis.Top, shpVis.Width, shpVis.Height);
          // timer is used to create 'flashing' of line types
          tmrSelect.Enabled = true;
    //       shpVis.Visible = true;
    //       shpPri.Visible = true;
        }

        // force tool to select if drawing something
        // (if  tool is selectArea, let it be)
        if (SelectedTool != ttEdit && SelectedTool != ttSelectArea) {
          // select it
          SelectedTool = ttEdit;
          Toolbar1.Buttons("select").Value = tbrPressed;
        }

      } else {
        // hide the shapes; the selected cmds dont include cooordinates
        tmrSelect.Enabled = false;
        shpVis.Visible = false;
        shpPri.Visible = false;
      }

      // if tool is select edit, update menu if necessary
      if (SelectedTool = ttSelectArea) {
        // enable copy command if selection is >0
        frmMDIMain.mnuECopy.Enabled = (Selection.Width > 0 && Selection.Height > 0)
      }
            */
        }

        private void UpdateCmdSelection() {
            // always set InsertBefore to TRUE whenever a new
            // command is selected so that the next command
            // added will bump currently selected command down
            gInsertBefore = true;

            if (lstCommands.SelectedItems.Count > 1) {
                // the selected command is always the last of the list
                SelectedCmdIndex = lstCommands.SelectedIndices[^1];
                SelectedCmdCount = lstCommands.SelectedIndices.Count;
                SelectedCmdType = (DrawFunction)EditPicture.Data[(int)lstCommands.SelectedItems[^1].Tag];
                // get start and end coords of selection
                // (sets Value of selstart and selsize)
                GetSelectionBounds(SelectedCmdIndex, SelectedCmdCount, true);
            }
            else if (lstCommands.SelectedItems.Count == 1) {
                SelectedCmdIndex = lstCommands.SelectedIndices[0];
                SelectedCmdType = (DrawFunction)EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag];
                SelectedCmdCount = 1;
                ClearCmdSelection();
            }
            else {
                // nuthin? can only happen if clicking on blank space
                // when the list is too short to fill the box
                // treat as if selecting the End command
                SelectedCmdIndex = lstCommands.Items.Count - 1;
                SelectedCmdType = DrawFunction.End;
                SelectedCmdCount = 1;
                ClearCmdSelection();
            }
            //spTool.Text = SelectedCmdType.ToString();
            // always cancel any drawing operation
            PicDrawMode = TPicDrawOpEnum.doNone;

            // set CurCmdIsLine to false until proven otherwise
            CurCmdIsLine = false;

            // always set cursor highlighting to match selection status
            //tmrSelect.Enabled = (Selection.Width > 0 && Selection.Height > 0);

            // get current tool status
            CurrentPen = EditPicture.CurrentToolStatus;

            // set selected tools to match current
            SelectedPen = CurrentPen;

            if (SelectedCmdCount == 1) {
                BuildCoordList(SelectedCmdIndex);
            }
            else {
                ClearCoordList();
            }
            if (EditPicture.StepDraw) {
                if (SelectedCmdIndex == lstCommands.Items.Count - 1) {
                    EditPicture.DrawPos = -1;
                }
                else {
                    EditPicture.DrawPos = (int)lstCommands.Items[SelectedCmdIndex + 1].Tag;
                }
                // redraw picture
                DrawPicture();
                picVisual.Refresh();
                picPriority.Refresh();
            }

            // update toolbar
            UpdateToolBar();
        }

        public bool MatchPoints(int ListPos) {

            // returns true if coord data for previous command's last coord equals this commands first coord

            byte bytCmd;
            byte[] bytData;
            byte bytX = 0, bytY = 0;
            int xdisp, ydisp;

            bytData = EditPicture.Data;

            // set starting pos for the previous cmd
            int lngPos = (int)lstCommands.Items[ListPos - 1].Tag;

            // get previous command Type
            bytCmd = bytData[lngPos++];
            switch (bytCmd) {
            case 0xF4 or 0xF5:
                // Draw an X or Y corner.
                // set initial direction
                bool blnRelX = bytCmd == 0xF5;
                bool nextIsX = bytData[(int)lstCommands.Items[ListPos].Tag] == 0xF5;
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    if (blnRelX) {
                        bytX = bytCmd;
                    }
                    else {
                        bytY = bytCmd;
                    }
                    blnRelX = !blnRelX;
                    bytCmd = bytData[lngPos++];
                }
                // ending direction must match direction of 
                // next command
                if (blnRelX != nextIsX) {
                    return false;
                }
                break;
            case 0xF6:
                // Absolute line (long lines).
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    bytX = bytCmd;
                    bytY = bytData[lngPos++];
                    bytCmd = bytData[lngPos++];
                }
                break;
            case 0xF7:
                // Relative line (short lines).
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    if ((bytCmd & 0x80) == 0x80) {
                        xdisp = -((bytCmd & 0x70) / 0x10);
                    }
                    else {
                        xdisp = ((bytCmd & 0x70) / 0x10);
                    }
                    if ((bytCmd & 0x8) == 0x8) {
                        ydisp = -(bytCmd & 0x7);
                    }
                    else {
                        ydisp = bytCmd & 0x7;
                    }
                    bytX = (byte)((int)bytX + xdisp);
                    bytY = (byte)((int)bytY + ydisp);
                    bytCmd = bytData[lngPos++];
                }
                break;
            }
            // bytx and byty are now set to last coord of previous cmd
            //  if they match first coordinate value of current command
            //  (current pos +1/+2) then return true
            int pos = (int)lstCommands.Items[ListPos].Tag;
            return bytData[pos + 1] == bytX && bytData[pos + 2] == bytY;
        }

        private void GetTextOptions() {
            /*

      // show print options dialog
      Load frmPicPrintPrev
        frmPicPrintPrev.SetForm TextMode, MaxCol
        frmPicPrintPrev.txtCol.Text = MsgLeft
        frmPicPrintPrev.txtRow.Text = MsgTop
        frmPicPrintPrev.txtMW.Text = MsgMaxW
        if (PowerPack) {
          frmPicPrintPrev.cmbBG.ListIndex = MsgBG
        } else {
          if (MsgBG = 0) {
            frmPicPrintPrev.cmbBG.ListIndex = 0
          } else {
            frmPicPrintPrev.cmbBG.ListIndex = 1
          }
        }
        frmPicPrintPrev.cmbFG.ListIndex = MsgFG
        frmPicPrintPrev.cmbFG.Enabled = PowerPack || MsgBG = 0
        frmPicPrintPrev.txtMessage.Text = MsgText
        frmPicPrintPrev.Show vbModal

      if (!frmPicPrintPrev.Canceled) {
        // display the preview text
        ShowTextPreview
      }
      Unload frmPicPrintPrev
            */
        }


        private bool AskClose() {
            if (EditPicture.ErrLevel < 0) {
                // if exiting due to error on form load
                return true;
            }
            if (PictureNumber == -1) {
                // force shutdown
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this picture resource?",
                    "Save Picture Resource",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SavePicture();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Resource not saved. Continue closing anyway?",
                            "Save Picture Resource",
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
            Text = sPICED + ResourceName(EditPicture, InGame, true);
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }

        private void picVisual_Click(object sender, EventArgs e) {

        }

        private void picPriority_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            // if showing bands
            if (ShowBands) {
                // draw bands in matching priority color one pixel high
                for (int rtn = 5; rtn <= 14; rtn++) {
                    int yp = (int)((int)(Math.Ceiling((rtn - 5) / 10.0 * (168 - EditPicture.PriBase)) + EditPicture.PriBase) * ScaleFactor - 1);
                    g.DrawLine(new(EditPalette[rtn]), 0, yp, picVisual.Width, yp);
                }
            }

            // text marks indicate where text characters are drawn
            if (ShowTextMarks) {
                for (int j = 1; j <= 21; j++) {
                    for (int i = 0; i <= MaxCol; i++) {
                        int x = (int)(i * CharWidth * ScaleFactor);
                        int y = (int)(j * 8 * ScaleFactor - 1);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, (int)(x + ScaleFactor), y);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, x, (int)(y - ScaleFactor));
                    }
                }
            }
        }
    }
}
