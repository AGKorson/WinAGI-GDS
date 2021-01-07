using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGIResType;
using static WinAGI_GDS.ResMan;

namespace WinAGI_GDS
{
  public partial class frmPreview : Form
  {
    int CalcWidth, CalcHeight;
    const int MIN_HEIGHT = 100;
    const int MIN_WIDTH = 100;

    bool blnNoUpdate;
    double sngOffsetX, sngOffsetY;
    AGIResType PrevResType;

    //logic preview
    AGILogic agLogic;

    //picture preview
    AGIPicture agPic;
    int PicScale;
    bool blnDraggingPic;

    //sound preview
    AGISound agSound;
    string strMIDIFile;
    int lngStart;

    //view preview
    AGIView agView;
    int CurLoop, CurCel;
    int ViewScale, VTopMargin;
    bool blnDraggingView;
    int lngVAlign, lngHAlign, lngMotion;
    bool blnTrans, DontDraw;

    //use local variables to hold visible status for scrollbars
    //because their visible property remains false as long as
    //the picturebox that holds them is false, even though they
    //are set to true
    bool blnViewHSB, blnViewVSB;
    bool blnPicVSB, blnPicHSB;

    const int PW_MARGIN = 4;
    public frmPreview()
    {
      InitializeComponent();
    }

    private void udPZoom_ValueChanged(object sender, EventArgs e)
    {
      //set zoom
      PicScale = (int)udPZoom.Value;
      //force update
      DisplayPicture();
    }

    public void LoadPreview(AGIResType ResType, int ResNum)
    {
      //show wait cursor
      this.UseWaitCursor = true;
      //clear resources
      ClearPreviewWin();
      //if one of the four main resource types and not a header
      if ((int)ResType >= 0 && (int)ResType <= 3 && ResNum >= 0)
      {
        UpdateCaption(ResType, (byte)ResNum);
        //get size, show preview
        switch (ResType)
        {
        case rtLogic:
          if (PreviewLogic((byte)ResNum))
          {
            //show it
            pnlLogic.Visible = true;
          }
          break;
        case rtPicture:
          if (PreviewPic((byte)ResNum))
          {
            //show it
            pnlPicture.Visible = true;
          }
          break;
        case rtSound:
          if (PreviewSound((byte)ResNum))
          {
            //show it
            pnlSound.Visible = true;
          }
          break;
        case rtView: //VIEW
          if (PreviewView((byte)ResNum))
          {
            //show it
            pnlView.Visible = true;
          }
          break;
        }
      }
      //restore mouse pointer
      this.UseWaitCursor = false;

      //set restype
      PrevResType = ResType;

      ////if previewwin has focus, always clear statusbar
      //if (MainStatusBar.Tag = "") {
      //  MainStatusBar.Panels(1).Text = ""
      //}
    }
    public void ClearPreviewWin()
    {
      //if the resource being cleared has recently been deleted
      //we don't need to unload it, just dereference it

      //unload view
      if (agView != null)
      {
        if (tmrMotion.Enabled)
        {
          tmrMotion.Enabled = false;
        }
        //if resource exists, it should still be loaded
        if (agView.Loaded)
        {
          agView.Unload();
        }
        agView = null;
      }
      //unload picture,
      if (agPic != null)
      {
        //if resource exists, it should still be loaded
        if (agPic.Loaded)
        {
          agPic.Unload();
        }
        agPic = null;
      }
      //unload sound
      if (agSound != null)
      {
        //if resource exists, it should still be loaded
        if (agSound.Loaded)
        {
          //stop sound, if it is playing
          agSound.StopSound();
          //unload sound
          agSound.Unload();
        }
        agSound = null;
        //ensure timer is off
        Timer1.Enabled = false;
        //reset progress bar
        //        pgbSound.Value = 0;
        //        cmdStop.Enabled = false;
      }
      //unload logic
      if (agLogic != null)
      {
        //if resource exists, it should still be loaded
        if (agLogic.Loaded)
        {
          agLogic.Unload();
        }
        agLogic = null;
      }
      pnlLogic.Visible = false;
      pnlPicture.Visible = false;
      pnlSound.Visible = false;
      pnlView.Visible = false;

      PrevResType = rtNone;

      // default caption
      this.Text = "Preview";
    }
    public void UpdateCaption(AGIResType ResType, byte ResNum)
    {
      string strID = "";

      // update window caption
      this.Text = "Preview - " + ResTypeName[(int)ResType] + " " + ResNum;
      // if not showing by number in the prvieww list
      if (!Settings.ShowResNum)
      {
        //also include the resource ID
        switch (ResType)
        {
        case rtLogic:
          strID = Logics[ResNum].ID;
          break;
        case rtPicture:
          strID = Pictures[ResNum].ID;
          break;
        case rtSound:
          strID = Sounds[ResNum].ID;
          break;
        case rtView:
          strID = Views[ResNum].ID;
          break;
        }
        strID = "   (" + strID + ")";
        this.Text += strID;
      }
    }
    bool PreviewLogic(byte LogNum)
    {
      //get the logic
      agLogic = Logics[LogNum];
      try
      {
        if (!agLogic.Loaded)
        {
          //load the logic to access source code
          agLogic.Load();
        }
        //success - load the source code
        //need to set text to nothing first in order to force scrollbars back to top
        //    rtfLogPrev.Text = "";
        rtfLogPrev.Text = agLogic.SourceText;
        //rtfLogPrev.Dirty = false;

        //if logic is compiled
        if (agLogic.Compiled)
        {
          //use current background
          rtfLogPrev.BackColor = Color.FromArgb(0xE0, 0xFF, 0xE0);
        }
        else
        {
          //use pink background
          rtfLogPrev.BackColor = Color.FromArgb(0xff, 0xE0, 0xe0);
        }

        //always unload
        agLogic.Unload();
        return true;
      }
      catch (Exception e)
      {
        //check for error
        //if errors occurred,
        //Select Case Err.Number
        //Case vbObjectError + 688
        //  ErrMsgBox "No source code found: ", "Unable to decode the logic resource.", "Preview Logic Error"

        //Case Else
        //  ErrMsgBox "Error while loading logic resource", "", "Preview Logic Error"
        //End Select
        //clear the error
        //always unload
        agLogic.Unload();
        return false;
      }
    }

    private void optVisual_CheckedChanged(object sender, EventArgs e)
    {
      //force a redraw
      DisplayPicture();
    }

    bool PreviewPic(byte PicNum)
    {
      try
      {
        //get new picture
        agPic = Pictures[PicNum];

        if (!agPic.Loaded)
        {
          //load resource for this view
          agPic.Load();
        }

        //draw picture
        DisplayPicture();
        return true;
      }
      catch (Exception)
      {
        ////error occurred,
        //ErrMsgBox("Error while loading picture resource: ", "", "Preview Picture Error");
        return false;
      }
    }
    void DisplayPicture()
    {
      //resize picture Image holder
      imgPicture.Width = 320 * PicScale;
      imgPicture.Height = 168 * PicScale;

      //if visual picture is being displayed
      if (optVisual.Checked == true)
      {
        //load visual Image
        ShowAGIBitmap(imgPicture, agPic.VisualBMP, PicScale);
      }
      else
      {
        //load priority Image
        ShowAGIBitmap(imgPicture, agPic.PriorityBMP, PicScale);
      }
      //set scrollbars if necessary
      SetPScrollbars();
    }
    void SetPScrollbars()
    {
      /*

  //determine if scrollbars are necessary
  blnPicHSB = (imgPicture.Width > (pnlPicture.ScaleWidth - 2 * PW_MARGIN))
  blnPicVSB = (imgPicture.Height > (pnlPicture.ScaleHeight - fraPHeader.Height - PW_MARGIN) + blnPicHSB * hsbPic.Height)
  //check horizontal again(incase addition of vert scrollbar forces it to be shown)
  blnPicHSB = (imgPicture.Width > (pnlPicture.ScaleWidth - 2 * PW_MARGIN + blnPicVSB * vsbPic.Width))
  
  //if both are visibile
  if (blnPicHSB && blnPicVSB) {
    //move back from corner
    hsbPic.Width = pnlPicture.ScaleWidth - vsbPic.Width
    vsbPic.Height = pnlPicture.ScaleHeight - fraPHeader.Height - hsbPic.Height
    //show corner
    fraPCorner.Top = hsbPic.Top
    fraPCorner.Left = vsbPic.Left
    fraPCorner.Visible = true
  Else
    fraPCorner.Visible = false
  }
  
  //set Max values
  if (blnPicHSB) {
    hsbPic.Max = PW_MARGIN + imgPicture.Width - (pnlPicture.ScaleWidth + blnPicVSB * vsbPic.Width)
  }
  if (blnPicVSB) {
    vsbPic.Max = imgPicture.Height - (pnlPicture.ScaleHeight - fraPHeader.Height + blnPicHSB * hsbPic.Height - PW_MARGIN)
  }
  
  //adjust scroll bar values - base them on size of pnlPicture (the visible part)
  hsbPic.LargeChange = pnlPicture.Width * LG_SCROLL  //80% for big jump
  vsbPic.LargeChange = pnlPicture.Height * LG_SCROLL //80% for big jump
  hsbPic.SmallChange = pnlPicture.Width * SM_SCROLL  //20% for small jump
  vsbPic.SmallChange = pnlPicture.Height * SM_SCROLL //20% for small jump
  
  //set visible properties for scrollbars
  hsbPic.Visible = blnPicHSB
  vsbPic.Visible = blnPicVSB
  
  //position picture holder
  imgPicture.Left = PW_MARGIN
  imgPicture.Top = this.fraPHeader.Height
  
  //set flag so scrollbar events don't cause recursion
  blnNoUpdate = true
  
  if (blnPicHSB) {
    hsbPic.Value = -imgPicture.Left
  }
  if (blnPicVSB) {
    vsbPic.Value = fraPHeader.Height - imgPicture.Top
  }
  
  //reset updating flag
  blnNoUpdate = false
      */
    }
    bool PreviewSound(byte SndNum)
    {
      return false;
      /*
      On Error GoTo ErrHandler

      Dim i As Long

      //get new sound
      Set agSound = Sounds(SndNum)

      On Error Resume Next
      if (!agSound.Loaded) {
        //load the resource
        agSound.Load
      }

      //check for error
      if (Err.Number != 0) {
          //error occurred,
        ErrMsgBox "Error while loading sound resource", "", "Preview Sound Error"
        Err.Clear
        Exit Function
      }

      On Error GoTo ErrHandler

      Select Case agSound.SndFormat
      Case 1  //standard agi
        //set instrument values
        For i = 0 To 2
          cmbInst(i).Enabled = true
          cmbInst(i).ListIndex = agSound.Track(i).Instrument
          chkTrack(i).Enabled = true
          chkTrack(i).Value = vbChecked && !agSound.Track(i).Muted
        Next i
        //add noise track
        chkTrack(3).Value = vbChecked && !agSound.Track(3).Muted
        chkTrack(3).Enabled = true

        //set length (which loads mididata)
        lblFormat.Caption = "PC/PCjr Standard Sound"

      Case 2    //IIgs sampled sound
        //disable tracks and play
        For i = 0 To 2
          cmbInst(i).Enabled = false
          cmbInst(i).ListIndex = -1
          chkTrack(i).Enabled = false
          chkTrack(i).Value = vbUnchecked
        Next i
        chkTrack(3).Enabled = false
        chkTrack(3).Value = vbUnchecked

        lblFormat.Caption = "Apple IIgs PCM Sound"

      Case 3    //IIgs midi
        //disable tracks and play
        For i = 0 To 2
          cmbInst(i).Enabled = false
          cmbInst(i).ListIndex = -1
          chkTrack(i).Enabled = false
          chkTrack(i).Value = vbUnchecked
        Next i
        chkTrack(3).Enabled = false
        chkTrack(3).Value = vbUnchecked

        lblFormat.Caption = "Apple IIgs MIDI Sound"
      End Select

      //set length
      this.lblLength = "Sound clip length: " + format$(agSound.Length, "0.0") + " seconds"

      cmdPlay.Enabled = !Settings.NoMIDI

      //return true
      PreviewSound = true

    Exit Function

    ErrHandler:
      //Debug.Assert false
      Resume Next
*/
    }
    bool PreviewView(byte ViewNum)
    {
      return false;
      /*
  On Error GoTo ErrHandler
  
  //get the view
  Set agView = Views(ViewNum)
  
  On Error Resume Next
  if (!agView.Loaded) {
    //load resource for this view
    agView.Load
  }
  
  if (Err.Number = 0) {
    On Error GoTo ErrHandler
    //ensure picturebox is resized
    With pnlView
      if (.Width != CalcWidth || .Height != CalcHeight) {
        .Move 0, 0, CalcWidth, CalcHeight
      }
    End With
    
    //show correct toolbars for alignment
    Toolbar1.Buttons("HAlign").Image = lngHAlign + 3
    Toolbar1.Buttons("VAlign").Image = lngVAlign + 6
    
    //success-disable updating until
    //loop updowns is set
    blnNoUpdate = true
    
    //set updown for Loops
    udLoop.Max = agView.Loops.Count - 1
    udLoop.Value = 0
    lblLoop.Caption = "0"
    lblLoopCount.Caption = "/ " + CStr(agView.Loops.Count - 1)
    
    CurLoop = 0
    
    //reenable updates
    blnNoUpdate = false
    
    //display the first loop (which will display the first cel!)
    DisplayLoop
    
    //return true
    PreviewView = true
  Else
    //error occurred,
    ErrMsgBox "Error while loading view resource", "", "Preview View Error"
    Err.Clear
  }
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
      */
    }

    void working()
    {
      /*
      */
    }
    void tmpPreview()
    {
      /*

Sub DrawTransGrid()

  Dim i As Long, j As Long, rtn As Long, offset As Long
  
  if (picViewHolder.Top = 26) {
    offset = 4
  }
  picViewHolder.Cls
  pnlView.Cls
  
  //draw the background grid
  For i = 0 To pnlView.Width / 10 + 1
    For j = 0 To pnlView.Height / 10 + 1
      rtn = SetPixelV(pnlView.hDC, i * 10, j * 10, 0)
      rtn = SetPixelV(picViewHolder.hDC, i * 10, j * 10 + offset, 0)
    Next j
  Next i
}

public Sub KeyHandler(ByRef KeyAscii As Integer)

  Select Case SelResType
  Case rtPicture
    Select Case KeyAscii
    Case 43 //+//
      //zoom in
      if (udPZoom.Value < 4) {
        udPZoom.Value = udPZoom.Value + 1
      }
      KeyAscii = 0
      
    Case 45 //-//
      //zoom out
      if (udPZoom.Value > 1) {
        udPZoom.Value = udPZoom.Value - 1
      }
      KeyAscii = 0
      
    End Select
    
  Case rtView
    Select Case KeyAscii
    Case 32 // //
     //toggle play/pause
      cmdVPlay_Click
      
    Case 43 //+//
      //zoom in
      ZoomPrev 1
      KeyAscii = 0
      
    Case 45 //-//
      //zoom out
      ZoomPrev -1
      KeyAscii = 0
      
    Case 65, 97 //a//
      if (udCel.Value > 0) {
        udCel.Value = udCel.Value - 1
      }
      KeyAscii = 0
      
    Case 83, 115 //s//
      if (udCel.Value < udCel.Max) {
        udCel.Value = udCel.Value + 1
      }
      KeyAscii = 0
      
    Case 81, 113 //q//
      if (udLoop.Value > 0) {
        udLoop.Value = udLoop.Value - 1
      }
      KeyAscii = 0
      
    Case 87, 119 //w//
      if (udLoop.Value < udLoop.Max) {
        udLoop.Value = udLoop.Value + 1
      }
      KeyAscii = 0
      
    End Select
  End Select
}

public Sub MenuClickCustom1()

  //export a loop as a gif
  //export a picture as bmp or gif
  
  Dim blnCanceled As Boolean, rtn As Long
  
  On Error GoTo ErrHandler
  
  Select Case PrevResType
  Case rtGame
    ExportAllPicImgs
    
  Case rtPicture
    ExportOnePicImg agPic
    
  Case rtView
    
    ExportLoop agView.Loops(udLoop.Value)
  End Select
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindLogic)

  On Error GoTo ErrHandler
  
  //don't need the find form; just go directly to the find function
  
  //set form defaults
  Select Case SelResType
  Case rtLogic
    GFindText = Logics(SelResNum).ID
  Case rtPicture
    GFindText = Pictures[SelResNum).ID
  Case rtSound
    GFindText = Sounds(SelResNum).ID
  Case rtView
    GFindText = Views(SelResNum).ID
  End Select
  
  GFindDir = fdAll
  GMatchWord = true
  GMatchCase = true
  GLogFindLoc = flAll
  GFindSynonym = false
  
  //reset search flags
  FindForm.ResetSearch
  
  Set SearchForm = frmMDIMain
  
  FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


public Sub MenuClickHelp()
  
  Dim strTopic As String
  
  On Error GoTo ErrHandler
  
  //show preview window help
  strTopic = "htm\winagi\preview.htm"
  Select Case SelResType
  Case rtLogic, rtPicture, rtSound, rtView
    strTopic = strTopic + "#" + ResTypeName(SelResType)
  End Select

  //show preview window help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, strTopic
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


public Function DisplayCel() As Boolean
  //this function copies the bitmap Image
  //from CurLoop.CurCel into the view Image box,
  //and resizes it to be correct size
  
  Dim rtn As Long
  Dim tgtX As Long, tgtY As Long, tgtH As Long, tgtW As Long
  
  On Error GoTo ErrHandler
  
  SendMessage picCel.hWnd, WM_SETREDRAW, 0, 0
  
  //set transparent color
  picTransCol.BackColor = EGAColor(agView.Loops(CurLoop).Cels(CurCel).TransColor)
  picCel.Cls
  
  With agView.Loops(CurLoop).Cels(CurCel)
    //copy view Image
    tgtW = .Width * 2 * ViewScale
    tgtH = .Height * ViewScale
    
    Select Case lngHAlign
    Case 0
      tgtX = 0
    Case 1
      tgtX = (picCel.Width - tgtW) / 2
    Case 2
      tgtX = picCel.Width - tgtW
    End Select
    Select Case lngVAlign
    Case 0
      tgtY = 0
    Case 1
      tgtY = (picCel.Height - tgtH) / 2
    Case 2
      tgtY = picCel.Height - tgtH
    End Select
  
    //if no transparency
    if (!blnTrans) {
    rtn = .CelBMP
      rtn = StretchBlt(picCel.hDC, tgtX, tgtY, tgtW, tgtH, .CelBMP, 0&, 0&, CLng(.Width), CLng(.Height), SRCCOPY)
    
    Else
      //first get background
      rtn = BitBlt(picCel.hDC, 0&, 0&, CLng(picCel.Width), CLng(picCel.Height), picViewHolder.hDC, CLng(picCel.Left), CLng(picCel.Top), SRCCOPY)
      //use transblit
      rtn = TransparentBlt(picCel.hDC, tgtX, tgtY, tgtW, tgtH, .CelBMP, 0&, 0&, CLng(.Width), CLng(.Height), EGAColor(.TransColor))
    }
  End With
  
  SendMessage picCel.hWnd, WM_SETREDRAW, 1, 0
  picCel.Refresh
  pnlView.Refresh
  //success
  DisplayCel = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub DisplayLoop()
    
  Dim i As Long, mW As Long, mH As Long
  
  //disable updating while
  //changing loop and cel controls
  blnNoUpdate = true
  
  //update updown control for cels
  udCel.Max = agView.Loops(CurLoop).Cels.Count - 1
  udCel.Enabled = (agView.Loops(CurLoop).Cels.Count > 1)
  
  //default to first cel
  udCel.Value = 0
  CurCel = 0
  //set cel caption
  lblCel.Caption = "0"
  lblCelCount.Caption = "/ " + CStr(agView.Loops(CurLoop).Cels.Count - 1)
  
  //enable play/stop if more than one cel
  cmdVPlay.Enabled = udCel.Enabled
  cmdVPlay.Picture = ImageList1.ListImages(9).Picture
  
  //determine size of holding pic
  mW = 0
  mH = 0
  With agView.Loops(CurLoop)
    For i = 0 To .Cels.Count - 1
      if (.Cels(i).Width > mW) {
        mW = .Cels(i).Width
      }
      if (.Cels(i).Height > mH) {
        mH = .Cels(i).Height
      }
    Next i
  End With
  
  With picCel
    //set size of view holder
    .Width = mW * 2 * ViewScale
    .Height = mH * ViewScale
    //force back to upper, left
    .Top = PW_MARGIN
    .Left = PW_MARGIN
  End With
  
  //set scroll bars everytime loop is changed
  SetVScrollBars
  
  //restore updating, and display the first cel in the loop
  blnNoUpdate = false
  DisplayCel
}






Sub SetVScrollBars()

On Error GoTo ErrHandler
  
  DontDraw = true
  
  With hsbView
    .Visible = (picCel.Width > picViewHolder.Width - 2 * PW_MARGIN)
    if (.Visible) {
      .Width = picViewHolder.Width
      .Max = .Min + picCel.Width + 2 * PW_MARGIN - picViewHolder.Width
    Else
      //reset it, reposition cel frame
      .Value = -PW_MARGIN
      picCel.Left = PW_MARGIN
    }
  End With
  
  With vsbView
    .Visible = (picCel.Height > picViewHolder.Height - 2 * PW_MARGIN)
    if (.Visible) {
      .Height = picViewHolder.Height
      .Max = .Min + picCel.Height + 2 * PW_MARGIN - picViewHolder.Height
    Else
      //reset it, reposition cel frame
      .Value = -PW_MARGIN
      picCel.Top = PW_MARGIN
    }
  End With
  
  //adjust scroll bar values
  hsbView.LargeChange = picViewHolder.Width * LG_SCROLL
  vsbView.LargeChange = picViewHolder.Height * LG_SCROLL
  hsbView.SmallChange = picViewHolder.Width * SM_SCROLL
  vsbView.SmallChange = picViewHolder.Height * SM_SCROLL
  
  DontDraw = false
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


public Sub StopSoundPreview()

  On Error GoTo ErrHandler
  
  Dim i As Long
  
  //disable stop and enable play
  cmdPlay.Enabled = !Settings.NoMIDI
  //cmdPlay.SetFocus   //DON//T do this - setting focus to a control also
                      //sets focus to the form, which creates an unending
                      //cycle of getfocus/lostfocus
                      
  cmdStop.Enabled = false
  
  if (agSound != null) {
    //stop sound
    agSound.StopSound();
  }
  
  //disable timer
  Timer1.Enabled = false
  
  //reset progress bar
  pgbSound.Value = 0
  
  //re-enable track/instrument controls when sound is stopped
  For i = 0 To 2
    this.chkTrack(i).Enabled = true
    this.cmbInst(i).Enabled = true
  Next i
  this.chkTrack(3).Enabled = true
  this.cmdReset.Enabled = true
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub ZoomPrev(ByVal Dir As Long)

  Dim mW As Long, mH As Long
  
  On Error Resume Next
  
  //get current maxH and maxW (by de-calculating...)
  mW = picCel.Width / ViewScale / 2
  mH = picCel.Height / ViewScale

  if (Dir = 1) {
    ViewScale = ViewScale + 1
    if (ViewScale = 13) {
      ViewScale = 12
      Exit Sub
    }
  Else
    ViewScale = ViewScale - 1
    if (ViewScale = 0) {
      ViewScale = 1
      Exit Sub
    }
  }
  
  //now rezize cel
  picCel.Width = mW * 2 * ViewScale
  picCel.Height = mH * ViewScale
  
  //set scrollbars
  SetVScrollBars
  
  //then redraw the cel
  DisplayCel
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub agSound_SoundComplete(NoError As Boolean)
  
  Dim i As Long
  
  //disable stop and enable play
  cmdPlay.Enabled = !Settings.NoMIDI
  cmdStop.Enabled = false
  Timer1.Enabled = false
  pgbSound.Value = 0
  
  //if this is a PC/PCjr sound, re-enable track controls
  //now that sound is done
  if (agSound.SndFormat = 1) {
    For i = 0 To 2
      this.chkTrack(i).Enabled = true
      this.cmbInst(i).Enabled = true
    Next i
    this.chkTrack(3).Enabled = true
    this.cmdReset.Enabled = true
  }
}

Sub chkTrack_Click(Index As Integer)
  
  //if disabled, just exit
  if (!chkTrack(Index).Enabled) {
    Exit Sub
  }
  
  //if form not visible, just exit
  if (!this.Visible) {
    Exit Sub
  }
  
  //if changing
  if (agSound.Track(Index).Muted != (chkTrack(Index).Value = vbUnchecked)) {
    agSound.Track(Index).Muted = (chkTrack(Index).Value = vbUnchecked)
  }
  
  //redisplay length (it may have changed)
  this.lblLength = "Sound clip length: " + format$(agSound.Length, "0.0") + " seconds"
  
  //enable play button if at least one track is NOT muted AND midi not disabled AND length>0
  cmdPlay.Enabled = ((chkTrack(0).Value = vbChecked) || (chkTrack(1).Value = vbChecked) || (chkTrack(2).Value = vbChecked) || (chkTrack(3).Value = vbChecked)) && !Settings.NoMIDI && agSound.Length > 0
}

Sub cmbInst_Click(Index As Integer)

  //if changing,
  if (agSound.Track(Index).Instrument != cmbInst(Index).ListIndex) {
    //set instrument for this sound
    agSound.Track(Index).Instrument = cmbInst(Index).ListIndex
  }
  
}

Sub cmbMotion_Click()

  On Error GoTo ErrHandler
  //only set focus if visible and enabled
  if (picCel.Visible && picCel.Enabled) {
    picCel.SetFocus
  }
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub cmdPlay_Click()

  On Error GoTo ErrHandler
  Dim i As Integer
  
  //if nothing to play
  if (agSound.Length = 0) {
    //this could happen if the sound has no notes in any tracks
    Exit Sub
  }
  
  //disable play and enable stop
  cmdStop.Enabled = true
  cmdPlay.Enabled = false
  
  //disable other controls while sound is playing
  For i = 0 To 2
    this.chkTrack(i).Enabled = false
    this.cmbInst(i).Enabled = false
  Next i
  this.chkTrack(3).Enabled = false
  this.cmdReset.Enabled = false
  
  //play the sound
  agSound.PlaySound
  
  //save current time
  lngStart = GetTickCount()
  
  //enable timer
  Timer1.Enabled = true
Exit Sub

ErrHandler:
  ErrMsgBox "An error occurred during playback: ", "Disabling MIDI playback.", "Play Sound Error"
  //disable timer
  Timer1.Enabled = false
  //reset buttons
  cmdStop.Enabled = false
  pgbSound.Enabled = false
  pgbSound.Value = 0
//////  Settings.NoMIDI = true
}

Sub cmdReset_Click()

  //reset instruments to default
  cmbInst(0).ListIndex = 80
  cmbInst(1).ListIndex = 80
  cmbInst(2).ListIndex = 80
}

Sub cmdStop_Click()
  //stop sounds
  StopSoundPreview
}


Sub cmdToggleTrans_Click()

  On Error GoTo ErrHandler
  
  blnTrans = !blnTrans
  
  //toggle transparency
  if (blnTrans) {
    DrawTransGrid
    cmdToggleTrans.Caption = "Show"
  Else
    picViewHolder.Cls
    pnlView.Cls
    cmdToggleTrans.Caption = "Hide"
  }
 
  DisplayCel
  picCel.SetFocus
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub cmdToggleTrans_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}

Sub cmdVPlay_Click()
  
  On Error GoTo ErrHandler
  
  //toggle motion
  tmrMotion.Enabled = !tmrMotion.Enabled
  
  //set icon to match
  if (tmrMotion.Enabled) {
    cmdVPlay.Picture = ImageList1.ListImages(10).Picture
    //reset cel, if endofloop or reverseloop motion selected
    Select Case cmbMotion.ListIndex
    Case 2  //endofloop
      //if already on last cel
      if (udCel.Value = udCel.Max) {
        udCel.Value = 0
      }
      
    Case 3 //reverseloop
      //if already on first cel
      if (udCel.Value = 0) {
        udCel.Value = udCel.Max
      }
    End Select
  Else
    cmdVPlay.Picture = ImageList1.ListImages(9).Picture
  }
  
  picCel.SetFocus
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub Form_Activate()

  On Error GoTo ErrHandler
  
  //if minimized, exit
  //(to deal with occasional glitch causing focus to lock up)
  if (this.WindowState = vbMinimized) {
    Exit Sub
  }
  
  //if form not visible
  if (!this.Visible) {
    Exit Sub
  }
  
  //if findform is visible,
  if (FindForm.Visible) {
    //hide it it
    FindForm.Visible = false
  }
  
  //adjust menus
  AdjustMenus SelResType, true, false, false
   
  cmbMotion.ListIndex = 0
  sldSpeed.Value = 5
  hsbView.Min = -PW_MARGIN
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub Form_Deactivate()

  //if previewing a sound,
  if (SelResType = rtSound) {
    StopSoundPreview
  }
  
//  //if previewing a view
//  if (SelResType = rtView) {
    //stop cycling
    if (tmrMotion.Enabled) {
      //Debug.Assert SelResType = rtView
    
      //show play
      tmrMotion.Enabled = false
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture
    }
//  }
}

Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

  //detect and respond to keyboard shortcuts
  //
  //BLAST! with the rtf window for previewing logics, the key
  //preview feature doesn//t work; so we have to catch the
  //keypress in the rtf control, then send it BACK to the form!
  
  //check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  if (KeyCode = 0) {
    Exit Sub
  }
  
  if (Shift = 0) {
    //no shift, ctrl, alt
    Select Case KeyCode
    Case vbKeyDelete
      //if a resource is selected
      Select Case SelResType
      Case rtLogic, rtPicture, rtSound, rtView
        //call remove from game method
        frmMDIMain.RemoveSelectedRes
        KeyCode = 0
      End Select
      
    Case vbKeyF1
      MenuClickHelp
      KeyCode = 0
    
    Case vbKeyF3
    
    End Select
  } else if (Shift = vbShiftMask + vbCtrlMask) {
    Select Case KeyCode
    Case vbKeyS //Shift+Ctrl+S//
      if (SelResType = rtPicture) {
        //save Image as ...
        MenuClickCustom1
      }
    End Select
    
  } else if (Shift = vbCtrlMask) {
    Select Case KeyCode
    Case vbKeyF //Ctrl+F (Find)
      Select Case SelResType
      Case rtLogic, rtPicture, rtSound, rtView
        //find this resid
        frmMDIMain.SearchForID
      End Select
    End Select
  }
}

Sub Form_KeyPress(KeyAscii As Integer)

On Error GoTo ErrHandler

  KeyHandler KeyAscii
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}
Sub Form_Load()
  Dim i As Long
  Dim sngLeft As Single, sngTop As Single
  Dim sngWidth As Single, sngHeight As Single
  
  On Error GoTo ErrHandler
  
  CalcWidth = MIN_WIDTH
  CalcHeight = MIN_HEIGHT
   
  //get preview window position
  sngWidth = ReadSettingLong(SettingsList, sPOSITION, "PreviewWidth", 0.4 * frmMDIMain.ScaleWidth)
    if (sngWidth <= MIN_WIDTH * Screen.TwipsPerPixelX) {
      sngWidth = MIN_WIDTH * Screen.TwipsPerPixelX
    } else if (sngWidth > 0.75 * Screen.Width) {
      sngWidth = 0.75 * Screen.Width
    }

  sngHeight = ReadSettingLong(SettingsList, sPOSITION, "PreviewHeight", 0.5 * frmMDIMain.ScaleHeight)
    if (sngHeight <= MIN_HEIGHT * Screen.TwipsPerPixelY) {
      sngHeight = MIN_HEIGHT * Screen.TwipsPerPixelY
    } else if (sngHeight > 0.75 * Screen.Height) {
      sngHeight = 0.75 * Screen.Height
    }
  
  sngLeft = ReadSettingSingle(SettingsList, sPOSITION, "PreviewLeft", 0)
    if (sngLeft < 0) {
      sngLeft = 0
    Else
      if (Settings.ResListType != 0) {
        //-1
        if (sngLeft > frmMDIMain.ScaleWidth - frmMDIMain.picLeft.Width - 300) {
          sngLeft = frmMDIMain.ScaleWidth - frmMDIMain.picLeft.Width - 300
        }
      Else
        //0
        if (sngLeft > frmMDIMain.ScaleWidth - 300) {
          sngLeft = frmMDIMain.ScaleWidth - 300
        }
      }
    }

  sngTop = ReadSettingLong(SettingsList, sPOSITION, "PreviewTop", 0)
    if (sngTop < 0) {
      sngTop = 0
    Else
      if (sngTop > frmMDIMain.ScaleHeight - 300) {
        sngTop = frmMDIMain.ScaleHeight - 300
      }
    }

  //now move the form
  Move sngLeft, sngTop, sngWidth, sngHeight
    
  //set flag to skip update of cels + loops during load
  blnNoUpdate = true
  
  //load instrument listboxes
  For i = 0 To 127
    cmbInst(0).AddItem InstrumentName(i)
    cmbInst(1).AddItem InstrumentName(i)
    cmbInst(2).AddItem InstrumentName(i)
  Next i
  
  //get default scale values
  ViewScale = Settings.ViewScale.Preview
  PicScale = Settings.PicScale.Preview
  
  //set default view alignment
  lngHAlign = Settings.ViewAlignH
  lngVAlign = Settings.ViewAlignV
  
  //set view scrollbar values
  hsbView.LargeChange = picViewHolder.Width * LG_SCROLL
  vsbView.LargeChange = picViewHolder.Height * LG_SCROLL
  hsbView.SmallChange = picViewHolder.Width * SM_SCROLL
  vsbView.SmallChange = picViewHolder.Height * SM_SCROLL
  VTopMargin = 50
  lngVAlign = 2
  Toolbar1.Buttons("VAlign").Image = 8
  
  //set picture scrollbar values
  hsbPic.LargeChange = pnlPicture.Width * LG_SCROLL  //80% for big jump
  vsbPic.LargeChange = pnlPicture.Height * LG_SCROLL //80% for big jump
  hsbPic.SmallChange = pnlPicture.Width * SM_SCROLL  //20% for small jump
  vsbPic.SmallChange = pnlPicture.Height * SM_SCROLL //20% for small jump
  
  //set picture zoom
  udPZoom.Value = PicScale
  txtPZoom.Text = CStr(PicScale)
  
  //no resource is selected on load
  SelResNum = -1
  
  //set font
  With rtfLogPrev
    .Font.Name = Settings.PFontName
    .Font.Size = Settings.PFontSize
    .HighlightSyntax = false
  End With
  
  //restore updating capability
  blnNoUpdate = false
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub Form_LostFocus()

  //clear statusbar if in picpreview mode
  if (PrevResType = rtPicture) {
    MainStatusBar.Panels(1).Text = ""
  }
}

Sub Form_Unload(Cancel As Integer)
  
  On Error GoTo ErrHandler
  
  //ensure preview resources are cleared,
  if (agLogic != null) {
    //unload it
    agLogic.Unload
    agLogic = null;
  }
  
  if (agPic != null) {
    //unload it
    agPic.Unload
    //delete it
    agPic = null;
  }
  if (agView != null) {
    //unload it
    agView.Unload
    //delete it
    agView = null;
  }
  if (agSound != null) {
    //unload it
    agSound.Unload
    //delete it
    agSound = null;
  }
  
  //save preview window pos
  WriteAppSetting SettingsList, sPOSITION, "PreviewTop", Top
  WriteAppSetting SettingsList, sPOSITION, "PreviewLeft", Left
  WriteAppSetting SettingsList, sPOSITION, "PreviewWidth", Width
  WriteAppSetting SettingsList, sPOSITION, "PreviewHeight", Height
  
  //need to check if this is last form
  LastForm Me
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}
Sub fraCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub fraLoop_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub fraToolbar_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub fraTransCol_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}

Sub hsbPic_Change()

  //if not updating
  if (!blnNoUpdate) {
    //position viewholder
    imgPicture.Left = -hsbPic.Value
    if (imgPicture.Left > PW_MARGIN) {
      imgPicture.Left = PW_MARGIN
    }
  }
}

Sub hsbPic_GotFocus()

  On Error GoTo ErrHandler
  
  //give focus back to picturebox
  pnlPicture.SetFocus
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub hsbPic_Scroll()

  hsbPic_Change
}

Sub hsbView_Change()

  //if not updating
  if (!blnNoUpdate) {
    //position viewholder
    picCel.Left = -hsbView.Value
  }
}

Sub hsbView_GotFocus()
  
  On Error GoTo ErrHandler
  
  //set focus to cel
  picCel.SetFocus
  
  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub hsbView_Scroll()

  hsbView_Change
}

Sub imgPicture_DblClick()

  //open picture for editing
  OpenPicture SelResNum
}

Sub imgPicture_LostFocus()

  //clear statusbar
  MainStatusBar.Panels(1).Text = ""
}

Sub imgPicture_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  //if either scrollbar is visible,
  if (hsbPic.Visible || vsbPic.Visible) {
    //set dragpic mode
    blnDraggingPic = true
    
    //set pointer to custom
    pnlPicture.MousePointer = vbCustom
    rtn = SetCapture(pnlPicture.hWnd)
    //save x and Y offsets
    sngOffsetX = X
    sngOffsetY = Y
  }
}


Sub imgPicture_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //display coords in statusbar
  
  Dim pX As Long, pY As Long
  pX = X / 2 / PicScale
  pY = Y / PicScale
  MainStatusBar.Panels(1).Text = "X: " + pX + "    Y: " + pY
}

Sub Label1_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub Label2_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub Label6_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}


Sub lblTrans_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub lblCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub lblCelCount_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub lblLoop_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub lblLoopCount_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub optPriority_Click()
  
  DisplayPicture
}

Sub optPriority_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}


Sub optVisual_Click()

  DisplayPicture
}

Sub Form_Resize()
  
  if (ScaleWidth < MIN_WIDTH) {
    CalcWidth = MIN_WIDTH
  Else
    CalcWidth = ScaleWidth
  }
  if (ScaleHeight < MIN_HEIGHT) {
    CalcHeight = MIN_HEIGHT
  Else
    CalcHeight = ScaleHeight
  }
  
  //if not minimized
  if (this.WindowState != vbMinimized) {
    
    Select Case SelResType
    Case rtLogic
        pnlLogic.Width = CalcWidth
        pnlLogic.Height = CalcHeight
      
    Case rtPicture
        pnlPicture.Width = CalcWidth
        pnlPicture.Height = CalcHeight
      
    Case rtSound
        pnlSound.Width = CalcWidth
        pnlSound.Height = CalcHeight
      
    Case rtView
        pnlView.Width = CalcWidth
        pnlView.Height = CalcHeight
      
    Case Else
      //no action needed, as there is no preview
    End Select
  }
}

Sub optVisual_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}

Sub picCel_DblClick()

  //open view for editing
  OpenView SelResNum
}


Sub picCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
  
  //if either scrollbar is visible,
  if (hsbView.Visible || vsbView.Visible) {
    //set dragView mode
    blnDraggingView = true
    
    //set pointer to custom
    pnlView.MousePointer = vbCustom
    rtn = SetCapture(pnlView.hWnd)
    //save x and Y offsets
    sngOffsetX = X
    sngOffsetY = Y
  }
}


Sub picLogic_Resize()

  rtfLogPrev.Width = pnlLogic.ScaleWidth
  rtfLogPrev.Height = pnlLogic.ScaleHeight
}


Sub picPicture_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
  
  //if not active form
  if (!frmMDIMain.ActiveForm Is Me) {
    Exit Sub
  }
  
  //if dragging picture
  if (blnDraggingPic) {
    //get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y + fraPHeader.Height
    
    //if vertical scrollbar is visible
    if (vsbPic.Visible) {
      //limit positions to valid values
      if (tmpY < vsbPic.Min) {
        tmpY = vsbPic.Min
      } else if (tmpY > vsbPic.Max) {
        tmpY = vsbPic.Max
      }
      //set vertical scrollbar
      vsbPic.Value = tmpY
    }
    
    //if horizontal scrollbar is visible
    if (hsbPic.Visible) {
      //limit positions to valid values
      if (tmpX < hsbPic.Min) {
        tmpX = hsbPic.Min
      } else if (tmpX > hsbPic.Max) {
        tmpX = hsbPic.Max
      }
      //set horizontal scrollbar
      hsbPic.Value = tmpX
    }
  }
}
Sub picPicture_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  //if dragging
  if (blnDraggingPic) {
    //cancel dragmode
    blnDraggingPic = false
    //release mouse capture
    rtn = ReleaseCapture()
    pnlPicture.MousePointer = vbDefault
  }
}


Sub picPicture_Resize()

  On Error GoTo ErrHandler
  
  //position/size header and footer
  fraPHeader.Width = pnlPicture.ScaleWidth
  
  //position scrollbars
  hsbPic.Top = pnlPicture.ScaleHeight - hsbPic.Height
  hsbPic.Width = pnlPicture.ScaleWidth
  vsbPic.Left = pnlPicture.ScaleWidth - vsbPic.Width
  vsbPic.Height = pnlPicture.ScaleHeight - fraPHeader.Height
      
  SetPScrollbars
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub picSound_DblClick()

  //open sound for editing, if standard agi
  
  OpenSound SelResNum
}

Sub picTransCol_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub picView_DblClick()

  //let user change background color
  Load frmPalette
  frmPalette.SetForm 1
  frmPalette.Show vbModal, frmMDIMain
  pnlView.BackColor = PrevWinBColor
  picViewHolder.BackColor = PrevWinBColor
  //toolbars stay default gray, but that//s OK
  
  //force redraw of cel
  DisplayCel
  if (blnTrans) {
    DrawTransGrid
  }
}

Sub picView_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}

Sub picView_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  On Error GoTo ErrHandler
  
  //if not active form
  if (!frmMDIMain.ActiveForm Is Me) {
    Exit Sub
  }
  
  //if dragging picture
  if (blnDraggingView) {
    //get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y + 2 * fraToolbar.Height
    
    //if vertical scrollbar is visible
    if (vsbView.Visible) {
      //limit positions to valid values
      if (tmpY < vsbView.Min) {
        tmpY = vsbView.Min
      } else if (tmpY > vsbView.Max) {
        tmpY = vsbView.Max
      }
      //set vertical scrollbar
      vsbView.Value = tmpY
    }
    
    //if horizontal scrollbar is visible
    if (hsbView.Visible) {
      //limit positions to valid values
      if (tmpX < hsbView.Min) {
        tmpX = hsbView.Min
      } else if (tmpX > hsbView.Max) {
        tmpX = hsbView.Max
      }
      //set horizontal scrollbar
      hsbView.Value = tmpX
    }
  }
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub picView_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  //if dragging
  if (blnDraggingView) {
    //cancel dragmode
    blnDraggingView = false
    //release mouse capture
    rtn = ReleaseCapture()
    pnlView.MousePointer = vbDefault
  }
}

Sub picView_Resize()
  
  Dim rtn As Long, offset As Long
  
  On Error GoTo ErrHandler
  
  if (blnTrans) {
    DrawTransGrid
  }
 
  DontDraw = true
  
  //position toolbars and viewholder
  if (pnlView.ScaleWidth >= 4 * fraCel.Width) {
    //position loop/cel frames on same row
    fraLoop.Move 220, 0
    fraCel.Move 330, 0
    picViewHolder.Top = 26
    picViewHolder.Height = pnlView.ScaleHeight - 26 - fraVMotion.Height - hsbView.Height
  Else
    //position loop/cel frames on different rows
    fraLoop.Move 0, 24
    fraCel.Move 110, 24
    picViewHolder.Top = 50
    picViewHolder.Height = pnlView.ScaleHeight - 50 - fraVMotion.Height - hsbView.Height
  }
  picViewHolder.Width = pnlView.ScaleWidth - vsbView.Width
  
  //position/size scrollbars
  vsbView.Left = picViewHolder.Width
  vsbView.Height = picViewHolder.Height
  vsbView.Top = picViewHolder.Top
  
  hsbView.Top = picViewHolder.Top + picViewHolder.Height
  hsbView.Width = picViewHolder.Width
  
  //position motion frame
  fraVMotion.Top = pnlView.ScaleHeight - fraVMotion.Height
  
  SetVScrollBars
  
  DontDraw = false
  
Exit Sub

ErrHandler:

  //Debug.Assert false
  Resume Next
}

Sub picViewHolder_DblClick()

  picView_DblClick
}

Sub rtfLogPrev_DblClick(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  //open logic for editing
  OpenLogic SelResNum
}


Sub rtfLogPrev_KeyDown(KeyCode As Integer, Shift As Integer)

  //BLAST! with the rtf window for previewing logics, the key
  //preview feature doesn//t work; so we have to catch the
  //keypress in the rtf control, then send it BACK to the form!
  Form_KeyDown KeyCode, Shift
  
  if (KeyCode = 0) {
    Exit Sub
  }
  
  Select Case Shift
  Case 0
    Select Case KeyCode
    Case vbKeyDelete
      //it should be caught by Form_KeyDown
      // but just in case, ignore it
      KeyCode = 0
    End Select
    
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyA
      rtfLogPrev.Range.SelectRange
    Case vbKeyC
      rtfLogPrev.Selection.Range.Copy
    End Select
  End Select
  
  KeyCode = 0
  Shift = 0
  
}

Sub rtfLogPrev_KeyPress(KeyAscii As Integer)
  //ignore all key events except copy (ctrl+c) in KeyDown event
  KeyAscii = 0
}

Sub rtfLogPrev_KeyUp(KeyCode As Integer, Shift As Integer)
  //ignore all key events except copy (ctrl+c) in KeyDown event
  KeyCode = 0
  Shift = 0
}

Sub rtfLogPrev_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  //with right mouse click, show the context menu, but only
  //allow user to copy text, if some was selected
  On Error GoTo ErrHandler
  
  if (Button = vbRightButton) {
    With frmMDIMain
      if (rtfLogPrev.Selection.Range.Length > 0) {
        .mnuLPCopy.Enabled = true
      Else
        .mnuLPCopy.Enabled = false
      }
      .mnuLPSelectAll.Visible = true
      PopupMenu .mnuLPPopup, 0, X, Y
    End With
  }
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub sldSpeed_Change()
  
  tmrMotion.Interval = 600 / sldSpeed - 45
}

Sub sldSpeed_Click()
  
  tmrMotion.Interval = 600 / sldSpeed - 45
}

Sub Timer1_Timer()
  //update progress bar
  
  Dim lngNow As Long
  
  Dim dblPos As Double
  
  //get current tick Count (milliseconds)
  lngNow = GetTickCount()
  
  if (agSound.Length != 0) {
    //convert to seconds
    dblPos = CDbl(lngNow - lngStart) / 1000
    //then to a fraction of Max progress bar value (include padding at start/end)
    // currently, 4 ticks at start, 4 ticks at end
    dblPos = dblPos * pgbSound.Max / (agSound.Length + CDbl(8 / 60))
  }
  
  if (dblPos > pgbSound.Max || agSound.Length = 0) {
    Timer1.Enabled = false
    pgbSound.Value = pgbSound.Max
    //don't need to reset butttons; it//s done when the
    // SoundComplete event happens
    ////cmdPlay.Enabled = true
    ////cmdStop.Enabled = false
  Else
    pgbSound.Value = dblPos
  }
}


Sub tlbHAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

  Dim i As Long, MaxW As Long
  
  //set alignment
  lngHAlign = Button.Index - 1
  
  //hide toolbar
  tlbHAlign.Visible = false
  
  //update main toolbar
  Toolbar1.Buttons("HAlign").Image = lngHAlign + 3
  
  //redraw the cel to update
  DisplayCel
}

Sub tlbVAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

  //set alignment
  lngVAlign = Button.Index - 1
  
  //hide toolbar
  tlbVAlign.Visible = false
  
  //update main toolbar
  Toolbar1.Buttons("VAlign").Image = lngVAlign + 6
  
  //redraw the cel to update
  DisplayCel
}


Sub tmrMotion_Timer()
  
  On Error GoTo ErrHandler
  
  //advance to next cel, depending on mode
  
  Select Case cmbMotion.ListIndex
  Case 0  //normal
    if (udCel.Value = udCel.Max) {
      udCel.Value = 0
    Else
      udCel.Value = udCel.Value + 1
    }
  
  Case 1 //reverse
    if (udCel.Value = 0) {
      udCel.Value = udCel.Max
    Else
      udCel.Value = udCel.Value - 1
    }
    
  Case 2  //end of loop
    if (udCel.Value = udCel.Max) {
      //stop motion
      tmrMotion.Enabled = false
      //show play
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture
    Else
      udCel.Value = udCel.Value + 1
    }
    
  Case 3  //reverse loop
    if (udCel.Value = 0) {
      //stop motion
      tmrMotion.Enabled = false
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture
    Else
      udCel.Value = udCel.Value - 1
    }
  End Select
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}

Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

  Select Case Button.Key
  Case "ZoomIn"
    ZoomPrev 1
    
  Case "ZoomOut"
    ZoomPrev -1
    
    Case "VAlign"
      //show valign toolbar
      tlbVAlign.Top = Toolbar1.Height / ScreenTWIPSY
      tlbVAlign.Visible = true
      
    Case "HAlign"
      tlbHAlign.Top = Toolbar1.Height / ScreenTWIPSY
      tlbHAlign.Visible = true
  End Select
}

Sub Toolbar1_ButtonDropDown(ByVal Button As MSComctlLib.Button)

  Select Case Button.Key
  Case "VAlign"
    //show valign toolbar
    tlbVAlign.Top = Toolbar1.Height / ScreenTWIPSY
    tlbVAlign.Visible = true
  Case "HAlign"
    tlbHAlign.Top = Toolbar1.Height / ScreenTWIPSY
    tlbHAlign.Visible = true
  End Select
}
Sub Toolbar1_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}

Sub txtPZoom_KeyPress(KeyAscii As Integer)
  //ignore all keys
  KeyAscii = 0
}


Sub txtPZoom_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}


Sub udCel_Change()
  
  //if not updating,
  if (blnNoUpdate) {
    Exit Sub
  }
    
  CurCel = udCel.Value
  
  //display this cel
  DisplayCel
}

Sub udCel_DownClick()

  //stop motion
  tmrMotion.Enabled = false
}

Sub udCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub udLoop_Change()
  
  //if updating is disabled,
  if (blnNoUpdate) {
    Exit Sub
  }
  
  //get new loop Value
  CurLoop = udLoop.Value
  
  //display the loop
  DisplayLoop
}


Sub udLoop_DownClick()

  //stop motion
  tmrMotion.Enabled = false
}

Sub udLoop_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
}


Sub udPZoom_Change()

  //if not updating
  if (blnNoUpdate) {
    Exit Sub
  }
  
  //set zoom
  PicScale = udPZoom.Value
  txtPZoom.Text = CStr(PicScale)
  
  //force update
  DisplayPicture
}

Sub udPZoom_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}


Sub vsbPic_Change()

  //if not updating
  if (!blnNoUpdate) {
    //position viewholder
    imgPicture.Top = -vsbPic.Value + fraPHeader.Height
    if (imgPicture.Top > fraPHeader.Height) {
      imgPicture.Top = fraPHeader.Height
    }
  }
}

Sub vsbPic_GotFocus()

  On Error GoTo ErrHandler
  
  //give focus back to picturebox
  pnlPicture.SetFocus
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub vsbPic_Scroll()

  vsbPic_Change
}

Sub vsbView_Change()

  //if not updating
  if (!blnNoUpdate) {
    //position viewholder
    picCel.Top = -vsbView.Value
  }
}


Sub vsbView_GotFocus()
  
  On Error GoTo ErrHandler
  
  //set focus to view
  picCel.SetFocus
  
  //ensure flyout toolbars are hidden
  tlbHAlign.Visible = false
  tlbVAlign.Visible = false
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
}


Sub vsbView_Scroll()

  vsbView_Change
}

      */
    }

    private void tmrMotion_Tick(object sender, EventArgs e)
    {

    }

    private void frmPreview_Load(object sender, EventArgs e)
    {
      // temp settings
      PicScale = 1;
    }
  }
}
