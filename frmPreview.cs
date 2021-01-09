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
using static WinAGI.AGISound;

namespace WinAGI_GDS
{
  public partial class frmPreview : Form
  {
    private ComboBox[] cmbInst = new ComboBox[3];
    private CheckBox[] chkTrack = new CheckBox[4];

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
    long lngStart;

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
      cmbInst[0] = cmbInst0;
      cmbInst[1] = cmbInst1;
      cmbInst[2] = cmbInst2;
      chkTrack[0] = chkTrack0;
      chkTrack[1] = chkTrack1;
      chkTrack[2] = chkTrack2;
      chkTrack[3] = chkTrack3;
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
        // always unhook the event handler
        agSound.SoundComplete -= This_SoundComplete;
        agSound = null;
        //ensure timer is off
        Timer1.Enabled = false;
        //reset progress bar
        pgbSound.Value = 0;
        cmdStop.Enabled = false;
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
        rtfLogPrev.Text = agLogic.SourceText;
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
        //switch (Err.Number
        //case vbObjectError + 688
        //  ErrMsgBox "No source code found: ", "Unable to decode the logic resource.", "Preview Logic Error"
        //default:
        //  ErrMsgBox "Error while loading logic resource", "", "Preview Logic Error"
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
    private void panel2_Resize(object sender, EventArgs e)
    {
      //position scrollbars
      //hsbPic.Top = pnlPicture.Bounds.Height - hsbPic.Height;
      hsbPic.Width = panel2.Bounds.Width;
      //vsbPic.Left = pnlPicture.Bounds.Width - vsbPic.Width;
      vsbPic.Height = panel2.Bounds.Height;
      SetPScrollbars();
    }
    private void vsbPic_Scroll(object sender, ScrollEventArgs e)
    {
      // position image
      imgPicture.Top = -vsbPic.Value;
    }
    private void hsbPic_Scroll(object sender, ScrollEventArgs e)
    {
      //position image
      imgPicture.Left = -hsbPic.Value;
      System.Diagnostics.Debug.Print($"hsb min: {hsbPic.Minimum} hsb value: {hsbPic.Value} hsb max: {hsbPic.Maximum}");
      System.Diagnostics.Debug.Print($"  hsb larg: {hsbPic.LargeChange} hsb small: {hsbPic.SmallChange}");
    }
    private void hsbPic_ValueChanged(object sender, EventArgs e)
    {
    }
    private void frmPreview_FormClosing(object sender, FormClosingEventArgs e)
    {
      //ensure preview resources are cleared,
      if (agLogic != null)
      {
        //unload it
        agLogic.Unload();
        agLogic = null;
      }

      if (agPic != null)
      {
        //unload it
        agPic.Unload();
        //delete it
        agPic = null;
      }
      if (agView != null)
      {
        //unload it
        agView.Unload();
        //delete it
        agView = null;
      }
      if (agSound != null)
      {
        //unload it
        agSound.Unload();
        //delete it
        agSound = null;
      }
      //save preview window pos
      WriteAppSetting(SettingsList, sPOSITION, "PreviewTop", Top);
      WriteAppSetting(SettingsList, sPOSITION, "PreviewLeft", Left);
      WriteAppSetting(SettingsList, sPOSITION, "PreviewWidth", Width);
      WriteAppSetting(SettingsList, sPOSITION, "PreviewHeight", Height);
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
    private void cmdPlay_Click(object sender, EventArgs e)
    {
      int i;
      //if nothing to play
      if (agSound.Length == 0)
      {
        //this could happen if the sound has no notes in any tracks
        return;
      }
      //disable play and enable stop
      cmdStop.Enabled = true;
      cmdPlay.Enabled = false;
      //disable other controls while sound is playing
      for (i = 0; i < 3; i++)
      {
        chkTrack[i].Enabled = false;
        cmbInst[i].Enabled = false;
      }
      chkTrack[3].Enabled = false;
      cmdReset.Enabled = false;
      // set progress bar max to number of milliseconds
      pgbSound.Maximum = (int)(agSound.Length * 1000);
      try
      {
        // hook the sound_complete event and play the sound
        agSound.SoundComplete += This_SoundComplete;
        agSound.PlaySound();
      }
      catch (Exception)
      {
        //ErrMsgBox "An error occurred during playback: ", "Disabling MIDI playback.", "Play Sound Error"
        //disable timer
        Timer1.Enabled = false;
        //reset buttons
        cmdStop.Enabled = false;
        pgbSound.Enabled = false;
        pgbSound.Value = 0;
        //////  Settings.NoMIDI = true
      }
      //save current time
      lngStart = DateTime.Now.Ticks;

      //enable timer
      Timer1.Enabled = true;
    }
    private void Timer1_Tick(object sender, EventArgs e)
    {
      //update progress bar

      long lngNow;
      int dblPos = 0;

      lngNow = DateTime.Now.Ticks;
      if (agSound.Length != 0)
      {
        //time passed, in milliseconds
        dblPos = (int)(lngNow - lngStart) / 10000;
      }

      if (dblPos > pgbSound.Maximum || agSound.Length == 0)
      {
        Timer1.Enabled = false;
        pgbSound.Value = pgbSound.Maximum;
        //don't need to reset butttons; it//s done when the
        // SoundComplete event happens
        ////cmdPlay.Enabled = true
        ////cmdStop.Enabled = false
      }
      else
      {
        pgbSound.Value = dblPos;
      }
    }
    public void StopSoundPreview()
    {
      //disable stop and enable play
      cmdPlay.Enabled = !Settings.NoMIDI;
      //cmdPlay.SetFocus   //DON'T do this - setting focus to a control also
      //sets focus to the form, which creates an unending
      //cycle of getfocus/lostfocus
      cmdStop.Enabled = false;
      if (agSound != null)
      {
        //stop sound
        agSound.StopSound();
      }
      //disable timer
      Timer1.Enabled = false;
      //reset progress bar
      pgbSound.Value = 0;
      //re-enable track/instrument controls when sound is stopped
      for (int i = 0; i < 3; i++)
      {
        chkTrack[i].Enabled = true;
        cmbInst[i].Enabled = true;
      }
      chkTrack[3].Enabled = true;
      cmdReset.Enabled = true;
    }

    void SetPScrollbars()
    {
      //determine if scrollbars are necessary
      blnPicHSB = (imgPicture.Width > (panel2.Bounds.Width - 2 * PW_MARGIN));
      blnPicVSB = (imgPicture.Height > (panel2.Bounds.Height - 2 * PW_MARGIN - (blnPicHSB ? hsbPic.Height : 0)));
      //check horizontal again(incase addition of vert scrollbar forces it to be shown)
      blnPicHSB = (imgPicture.Width > (panel2.Bounds.Width - 2 * PW_MARGIN - (blnPicVSB ? vsbPic.Width : 0)));
      //if both are visibile
      if (blnPicHSB && blnPicVSB) {
        //move back from corner
        hsbPic.Width = panel2.Bounds.Width - vsbPic.Width;
        vsbPic.Height = panel2.Bounds.Height - hsbPic.Height;
        //show corner
        fraPCorner.Visible = true;
      }
      else
      {
        fraPCorner.Visible = false;
      }
      // if visible, set large/small change values, then max values
      // note that in .NET, the actual highest value attainable in a
      // scrollbar is NOT the Maximum value; it's Maximum - LargeChange + 1!!
      // that seems really dumb, but it's what happens...

      //set change and Max values
      if (blnPicHSB) {
        // changes are based on size of the visible panel
        hsbPic.LargeChange = (int)(panel2.Width * LG_SCROLL);  //90% for big jump
        hsbPic.SmallChange = (int)(panel2.Width * SM_SCROLL);  //22.5% for small jump
        // calculate control MAX value - equals desired actual Max + LargeChange - 1
        hsbPic.Maximum = imgPicture.Width - (panel2.Bounds.Width - (blnPicVSB ? vsbPic.Width : 0)) + PW_MARGIN + hsbPic.LargeChange - 1;
      }
      // repeate for vertical bar
      if (blnPicVSB) {
        vsbPic.LargeChange = (int)(panel2.Height * LG_SCROLL); //90% for big jump
        vsbPic.SmallChange = (int)(panel2.Height * SM_SCROLL); //22.5% for small jump
        vsbPic.Maximum = imgPicture.Height - (panel2.Bounds.Height - (blnPicHSB ? hsbPic.Height : 0)) + PW_MARGIN + vsbPic.LargeChange -1;
      }
      //set visible properties for scrollbars
      hsbPic.Visible = blnPicHSB;
      vsbPic.Visible = blnPicVSB;
      //always reposition picture holder back to default
      imgPicture.Left = PW_MARGIN;
      imgPicture.Top = PW_MARGIN;
      if (blnPicHSB) {
        hsbPic.Value = -imgPicture.Left;
      }
      if (blnPicVSB)
      {
        vsbPic.Value = -imgPicture.Top;
      }
    }
    private void chkTrack0_CheckedChanged(object sender, EventArgs e)
    {
      chkTrack_Click(0);
    }
    private void chkTrack1_CheckedChanged(object sender, EventArgs e)
    {
      chkTrack_Click(1);
    }
    private void chkTrack2_CheckedChanged(object sender, EventArgs e)
    {
      chkTrack_Click(2);
    }
    private void chkTrack3_CheckedChanged(object sender, EventArgs e)
    {
      chkTrack_Click(3);
    }
    private void cmbInst0_SelectionChangeCommitted(object sender, EventArgs e)
    {
      cmbInst_Click(0);
    }
    private void cmbInst1_SelectionChangeCommitted(object sender, EventArgs e)
    {
      cmbInst_Click(1);
    }
    private void cmbInst2_SelectionChangeCommitted(object sender, EventArgs e)
    {
      cmbInst_Click(2);
    }
    bool PreviewSound(byte SndNum)
    {
      int i;

      //get new sound
      try
      {
        agSound = Sounds[SndNum];
        if (!agSound.Loaded)
        {
          //load the resource
          agSound.Load();
        }
      }
      catch (Exception)
      {
        //ErrMsgBox "Error while loading sound resource", "", "Preview Sound Error"
        return false;
      }

      switch (agSound.SndFormat)
      {
      case 1:  //standard agi
        //set instrument values
        for (i = 0; i < 3; i++) 
        {
          cmbInst[i].Enabled = true;
          cmbInst[i].SelectedIndex = agSound.Track(i).Instrument;
          chkTrack[i].Enabled = true;
          chkTrack[i].Checked = !agSound.Track(i).Muted;
        }
        //add noise track
        chkTrack[3].Checked = !agSound.Track(3).Muted;
        chkTrack[3].Enabled = true;

        //set length (which loads mididata)
        lblFormat.Text = "PC/PCjr Standard Sound";
        break;
      case 2:    //IIgs sampled sound
        //disable tracks and play
        for (i = 0; i < 3; i++)
        {
          cmbInst[i].Enabled = false;
          cmbInst[i].SelectedIndex = -1;
          chkTrack[i].Enabled = false;
          chkTrack[i].Checked = false;
        }
        chkTrack[3].Enabled = false;
        chkTrack[3].Checked = false;
        lblFormat.Text = "Apple IIgs PCM Sound";
        break;
      case 3:    //IIgs midi
        //disable tracks and play
        for (i = 0; i < 3; i++)
        {
          cmbInst[i].Enabled = false;
          cmbInst[i].SelectedIndex = -1;
          chkTrack[i].Enabled = false;
          chkTrack[i].Checked = false;
        }
        chkTrack[3].Enabled = false;
        chkTrack[3].Checked = false;
        lblFormat.Text = "Apple IIgs MIDI Sound";
        break;
      }
      //set length
      lblLength.Text = "Sound clip length: " + agSound.Length.ToString("0.0") + " seconds";
      cmdPlay.Enabled = !Settings.NoMIDI;
      return true;
    }
    private void cmdReset_Click(object sender, EventArgs e)
    {
      //reset instruments to default
      cmbInst[0].SelectedIndex = 80;
      cmbInst[1].SelectedIndex = 80;
      cmbInst[2].SelectedIndex = 80;
    }
    private void This_SoundComplete(object sender, SoundCompleteEventArgs e)
    {
      //disable stop and enable play
      cmdPlay.Enabled = !Settings.NoMIDI;
      cmdStop.Enabled = false;
      Timer1.Enabled = false;
      pgbSound.Value = pgbSound.Maximum;
      pgbSound.Refresh();
      //long lngNow = DateTime.Now.Ticks;
      //System.Diagnostics.Debug.Print($"Play time: {((double)(lngNow - lngStart) / 10000).ToString("0.0")} seconds");
      //if this is a PC/PCjr sound, re-enable track controls
      //now that sound is done
      if (agSound.SndFormat == 1)
      {
        for (int i = 0; i < 3; i++)
        {
          chkTrack[i].Enabled = true;
          cmbInst[i].Enabled = true;
        }
        chkTrack[3].Enabled = true;
        cmdReset.Enabled = true;
      }
      //now reset the progress bar
      pgbSound.Value = 0;
    }
    bool PreviewView(byte ViewNum)
    {
      //get the view
      agView = Views[ViewNum];
      try
      {
        if (!agView.Loaded)
        {
          //load resource for this view
          agView.Load();
        }
      }
      catch (Exception)
      {
        ////error occurred,
        //ErrMsgBox "Error while loading view resource", "", "Preview View Error"
        return false;
      }
      //show correct toolbars for alignment
      tsViewPrev.Items["HAlign"].ImageIndex = lngHAlign + 3;
      tsViewPrev.Items["VAlign"].ImageIndex = lngVAlign + 6;
      //success-disable updating until
      //loop updowns is set
      blnNoUpdate = true;

      //set updown for Loops
      udLoop.Items.Clear();
      for (int i = agView.Loops.Count - 1; i >= 0; i--)
      {
        udLoop.Items.Add($"Loop {i} / {agView.Loops.Count} ");
      }
      udLoop.SelectedIndex = 0;
      CurLoop = 0;
      //reenable updates
      blnNoUpdate = false;
      //display the first loop (which will display the first cel!)
      DisplayLoop();
      //return true
      return true;
    } 
    void cmbInst_Click(int Index)
    {
      //if changing,
      if (agSound.Track(Index).Instrument != cmbInst[Index].SelectedIndex)
      {
        //set instrument for this sound
        agSound.Track(Index).Instrument = (byte)cmbInst[Index].SelectedIndex;
      }

    }
    void DisplayLoop()
    {
      int i, mW, mH;
      //disable updating while
      //changing loop and cel controls
      blnNoUpdate = true;
      //update updown control for cels
      for (i = 0; i < agView[CurLoop].Cels.Count; i++)
      {
        udCel.Items.Add($"Cel {i} / {agView[CurLoop].Cels.Count - 1} ");
      }
      //default to first cel
      udCel.SelectedIndex = 0;
      CurCel = 0;
      //enable play/stop if more than one cel
      cmdVPlay.Enabled = udCel.Enabled;
      cmdVPlay.Image = imageList1.Images[9];
      //determine size of holding pic
      mW = 0;
      mH = 0;
      for (i = 0; i <= agView[CurLoop].Cels.Count - 1; i++)
      {
        if (agView[CurLoop].Cels[i].Width > mW)
        {
          mW = agView.Loops[CurLoop].Cels[i].Width;
        }
        if (agView.Loops[CurLoop].Cels[i].Height > mH)
        {
          mH = agView.Loops[CurLoop].Cels[i].Height;
        }
      }
      //set size of view holder
      picCel.Width = mW * 2 * ViewScale;
      picCel.Height = mH * ViewScale;
      //force back to upper, left
      picCel.Top = PW_MARGIN;
      picCel.Left = PW_MARGIN;
      //set scroll bars everytime loop is changed
      SetVScrollBars();
      //restore updating, and display the first cel in the loop
      blnNoUpdate = false;
      DisplayCel();
    }
    void SetVScrollBars()
    {
      DontDraw = true;
      //adjust scroll bar values

      // set horizontal scrollbox
      hsbView.LargeChange = (int)(picViewHolder.Width * LG_SCROLL);
      hsbView.SmallChange = (int)(picViewHolder.Width * SM_SCROLL);
      hsbView.Visible = (picCel.Width > picViewHolder.Width - 2 * PW_MARGIN);
      if (hsbView.Visible)
      {
        hsbView.Width = picViewHolder.Width;
        hsbView.Maximum = picCel.Width + PW_MARGIN - picViewHolder.Width + hsbView.LargeChange - 1;
      }
      else
      {
        //reset it, reposition cel frame
        hsbView.Value = -PW_MARGIN;
        picCel.Left = PW_MARGIN;
      }
      // set vertical scrollbar
      vsbView.Visible = (picCel.Height > picViewHolder.Height - 2 * PW_MARGIN);
      if (vsbView.Visible)
      {
        vsbView.Height = picViewHolder.Height;
        vsbView.Maximum = picCel.Height + PW_MARGIN - picViewHolder.Height + vsbView.LargeChange - 1;
        vsbView.LargeChange = (int)(picViewHolder.Height * LG_SCROLL);
        vsbView.SmallChange = (int)(picViewHolder.Height * SM_SCROLL);
      }
      else
      {
        //reset it, reposition cel frame
        vsbView.Value = -PW_MARGIN;
        picCel.Top = PW_MARGIN;
      }
      DontDraw = false;
      return;
    }
    public bool DisplayCel()
    {
      //this function copies the bitmap Image
      //from CurLoop.CurCel into the view Image box,
      //and resizes it to be correct size

      int rtn;
      int tgtX, tgtY, tgtH, tgtW;

      //set transparent color for the holder
      tbbTrans.BackColor = EGAColor[(int)agView[CurLoop][CurCel].TransColor];
      picCel.CreateGraphics().Clear(EGAColor[(int)agView[CurLoop][CurCel].TransColor]);
      //copy view Image
      tgtW = agView[CurLoop][CurCel].Width * 2 * ViewScale;
      tgtH = agView[CurLoop][CurCel].Height * ViewScale;
      switch (lngHAlign)
      {
      case 0:
        tgtX = 0;
        break;
      case 1:
        tgtX = (picCel.Width - tgtW) / 2;
        break;
      case 2:
        tgtX = picCel.Width - tgtW;
        break;
      }
      switch (lngVAlign)
      {
      case 0:
        tgtY = 0;
        break;
      case 1:
        tgtY = (picCel.Height - tgtH) / 2;
        break;
      case 2:
        tgtY = picCel.Height - tgtH;
        break;
      }
      //if transparency
      if (blnTrans)
      {
        agView[CurLoop][CurCel].CelBMP.MakeTransparent(EGAColor[(int)agView[CurLoop][CurCel].TransColor]);
      }
      else
      {
        agView[CurLoop][CurCel].CelBMP.MakeTransparent(EGAColor[(int)agView[CurLoop][CurCel].TransColor]);
//        agView[CurLoop][CurCel].CelBMP.MakeTransparent();
      }
      //load visual Image
      ShowAGIBitmap(picCel, agView[CurLoop][CurCel].CelBMP, ViewScale);
      //picCel.Refresh();
      //pnlView.Refresh();
      //success
      return true;
    }

    void tmpPreview()
    {
      /*

void DrawTransGrid()

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

public void KeyHandler(ByRef KeyAscii As Integer)

  switch (SelResType
  case rtPicture
    switch (KeyAscii
    case 43 //+//
      //zoom in
      if (udPZoom.Value < 4) {
        udPZoom.Value = udPZoom.Value + 1
      }
      KeyAscii = 0
      
    case 45 //-//
      //zoom out
      if (udPZoom.Value > 1) {
        udPZoom.Value = udPZoom.Value - 1
      }
      KeyAscii = 0
    }
  case rtView
    switch (KeyAscii
    case 32 // //
     //toggle play/pause
      cmdVPlay_Click
      
    case 43 //+//
      //zoom in
      ZoomPrev 1
      KeyAscii = 0
    case 45 //-//
      //zoom out
      ZoomPrev -1
      KeyAscii = 0
    case 65, 97 //a//
      if (udCel.Value > 0) {
        udCel.Value = udCel.Value - 1
      }
      KeyAscii = 0
    case 83, 115 //s//
      if (udCel.Value < udCel.Maximum) {
        udCel.Value = udCel.Value + 1
      }
      KeyAscii = 0
    case 81, 113 //q//
      if (udLoop.Value > 0) {
        udLoop.Value = udLoop.Value - 1
      }
      KeyAscii = 0
    case 87, 119 //w//
      if (udLoop.Value < udLoop.Maximum) {
        udLoop.Value = udLoop.Value + 1
      }
      KeyAscii = 0
    }
  }
}

public void MenuClickCustom1()

  //export a loop as a gif
  //export a picture as bmp or gif
  
  Dim blnCanceled As Boolean, rtn As Long
  
  On Error GoTo ErrHandler
  
  switch (PrevResType
  case rtGame
    ExportAllPicImgs
    
  case rtPicture
    ExportOnePicImg agPic
    
  case rtView
    
    ExportLoop agView.Loops(udLoop.Value)
  }
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}

void MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindLogic)

  On Error GoTo ErrHandler
  
  //don't need the find form; just go directly to the find function
  
  //set form defaults
  switch (SelResType) {
  case rtLogic:
    GFindText = Logics(SelResNum).ID
  case rtPicture:
    GFindText = Pictures[SelResNum).ID
  case rtSound:
    GFindText = Sounds(SelResNum).ID
  case rtView:
    GFindText = Views(SelResNum).ID
  }
  
  GFindDir = fdAll
  GMatchWord = true
  GMatchcase = true
  GLogFindLoc = flAll
  GFindSynonym = false
  
  //reset search flags
  FindForm.ResetSearch
  
  SearchForm = MDIMain
  
  FindInLogic GFindText, GFindDir, GMatchWord, GMatchcase, GLogFindLoc
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}
public void MenuClickHelp()
  
  Dim strTopic As String
  
  On Error GoTo ErrHandler
  
  //show preview window help
  strTopic = "htm\winagi\preview.htm"
  switch (SelResType
  case rtLogic, rtPicture, rtSound, rtView
    strTopic = strTopic + "#" + ResTypeName(SelResType)
  }

  //show preview window help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, strTopic
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}

void ZoomPrev(ByVal Dir As Long)

  int mW, mH;
  //get current maxH and maxW (by de-calculating...)
  mW = picCel.Width / ViewScale / 2;
  mH = picCel.Height / ViewScale;

  if (Dir == 1) {
    ViewScale++
    if (ViewScale == 13) {
      ViewScale = 12;
      return;
    }
  } else {
    ViewScale--;
    if (ViewScale == 0) {
      ViewScale = 1;
      return;
    }
  }
  //now rezize cel
  picCel.Width = mW * 2 * ViewScale;
  picCel.Height = mH * ViewScale;
  //set scrollbars
  SetVScrollBars();
  //then redraw the cel
  DisplayCel();
}
void cmdToggleTrans_Click()
  blnTrans = !blnTrans;
  //toggle transparency
  if (blnTrans) {
    DrawTransGrid();
    cmdToggleTrans.Text = "Show";
  } else {
    picViewHolder.Cls;
    pnlView.Cls;
    cmdToggleTrans.Text = "Hide";
  }
 
  DisplayCel();
  picCel.SetFocus();
}

void cmdVPlay_Click()
{  
  //toggle motion
  tmrMotion.Enabled = !tmrMotion.Enabled;
  //set icon to match
  if (tmrMotion.Enabled) {
    cmdVPlay.Picture = ImageList1.ListImages(10).Picture;
    //reset cel, if endofloop or reverseloop motion selected
    switch (cmbMotion.SelectedIndex) {
    case 2:  //endofloop
      //if already on last cel
      if (udCel.Value == udCel.Maximum) {
        udCel.Value = 0;
      }
      
    case 3: //reverseloop
      //if already on first cel
      if (udCel.Value == 0) {
        udCel.Value = udCel.Maximum;
      }
    }
  } else {
    cmdVPlay.Picture = ImageList1.ListImages(9).Picture;
  }
  
  picCel.SetFocus();
}

void Form_Activate()
  //if minimized, exit
  //(to deal with occasional glitch causing focus to lock up)
  if (this.WindowState == WindowState.Minimized) {
    return;
  }
  //if findform is visible,
  if (FindForm.Visible) {
    //hide it it
    FindForm.Visible = false
  }
  cmbMotion.SelectedIndex = 0;
  sldSpeed.Value = 5;
  hsbView.Minimum = -PW_MARGIN;
}
void Form_Deactivate()

  //if previewing a sound,
  if (SelResType = rtSound) {
    StopSoundPreview();
  }
  
    //stop cycling
    if (tmrMotion.Enabled) {
      //show play
      tmrMotion.Enabled = false;
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture;
    }
}

void Form_KeyDown(KeyCode As Integer, Shift As Integer)

  //detect and respond to keyboard shortcuts
  //
  //BLAST! with the rtf window for previewing logics, the key
  //preview feature doesn//t work; so we have to catch the
  //keypress in the rtf control, then send it BACK to the form!
  
  //check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  if (KeyCode = 0) {
    return;
  }
  
  if (Shift = 0) {
    //no shift, ctrl, alt
    switch (KeyCode
    case vbKeyDelete
      //if a resource is selected
      switch (SelResType
      case rtLogic, rtPicture, rtSound, rtView
        //call remove from game method
        MDIMain.RemoveSelectedRes
        KeyCode = 0
      }
      
    case vbKeyF1
      MenuClickHelp
      KeyCode = 0
    
    case vbKeyF3
    
    }
  } else if (Shift = vbShiftMask + vbCtrlMask) {
    switch (KeyCode
    case vbKeyS //Shift+Ctrl+S//
      if (SelResType = rtPicture) {
        //save Image as ...
        MenuClickCustom1
      }
    }
    
  } else if (Shift = vbCtrlMask) {
    switch (KeyCode
    case vbKeyF //Ctrl+F (Find)
      switch (SelResType
      case rtLogic, rtPicture, rtSound, rtView
        //find this resid
        MDIMain.SearchForID
      }
    }
  }
}

void Form_KeyPress(KeyAscii As Integer)

On Error GoTo ErrHandler

  KeyHandler KeyAscii
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}
void hsbView_Change()
{
  //if not updating
  if (!blnNoUpdate) {
    //position viewholder
    picCel.Left = -hsbView.Value;
  }
}
void hsbView_Scroll()

  hsbView_Change();
}
void imgPicture_DblClick()

  //open picture for editing
  OpenPicture(SelResNum);
}

void imgPicture_LostFocus()

  //clear statusbar
  MainStatusBar.Panels(1).Text = ""
}

void imgPicture_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  //if either scrollbar is visible,
  if (hsbPic.Visible || vsbPic.Visible) {
    //set dragpic mode
    blnDraggingPic = true;
    
    //set pointer to custom
    pnlPicture.MousePointer = vbCustom
    rtn = SetCapture(pnlPicture.hWnd)
    //save x and Y offsets
    sngOffsetX = X;
    sngOffsetY = Y;
  }
}


void imgPicture_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //display coords in statusbar
  
  Dim pX As Long, pY As Long
  pX = X / 2 / PicScale
  pY = Y / PicScale
  MainStatusBar.Panels(1).Text = "X: " + pX + "    Y: " + pY
}
void Label6_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
}
void Form_Resize()
  
  if (ScaleWidth < MIN_WIDTH) {
    CalcWidth = MIN_WIDTH
  } else {
    CalcWidth = ScaleWidth
  }
  if (ScaleHeight < MIN_HEIGHT) {
    CalcHeight = MIN_HEIGHT
  } else {
    CalcHeight = ScaleHeight
  }
  
  //if not minimized
  if (this.WindowState != vbMinimized) {
    
    switch (SelResType
    case rtLogic
        pnlLogic.Width = CalcWidth
        pnlLogic.Height = CalcHeight
      
    case rtPicture
        pnlPicture.Width = CalcWidth
        pnlPicture.Height = CalcHeight
      
    case rtSound
        pnlSound.Width = CalcWidth
        pnlSound.Height = CalcHeight
      
    case rtView
        pnlView.Width = CalcWidth
        pnlView.Height = CalcHeight
      
    default:
      //no action needed, as there is no preview
    }
  }
}
void picCel_DblClick()

  //open view for editing
  OpenView SelResNum
}
void picCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

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
void picPicture_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  //always clear statusbar
  MainStatusBar.Panels(1).Text = ""
  
  //if not active form
  if (!MDIMain.ActiveForm Is Me) {
    return;
  }
  
  //if dragging picture
  if (blnDraggingPic) {
    //get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y + fraPHeader.Height
    
    //if vertical scrollbar is visible
    if (vsbPic.Visible) {
      //limit positions to valid values
      if (tmpY < vsbPic.Minimum) {
        tmpY = vsbPic.Minimum
      } else if (tmpY > vsbPic.Maximum) {
        tmpY = vsbPic.Maximum
      }
      //set vertical scrollbar
      vsbPic.Value = tmpY
    }
    
    //if horizontal scrollbar is visible
    if (hsbPic.Visible) {
      //limit positions to valid values
      if (tmpX < hsbPic.Minimum) {
        tmpX = hsbPic.Minimum
      } else if (tmpX > hsbPic.Maximum) {
        tmpX = hsbPic.Maximum
      }
      //set horizontal scrollbar
      hsbPic.Value = tmpX
    }
  }
}
void picPicture_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

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


void picSound_DblClick()

  //open sound for editing, if standard agi
  
  OpenSound SelResNum
}
void picView_DblClick()

  //let user change background color
  Load frmPalette
  frmPalette.SetForm 1
  frmPalette.Show vbModal, MDIMain
  pnlView.BackColor = PrevWinBColor
  picViewHolder.BackColor = PrevWinBColor
  //toolbars stay default gray, but that//s OK
  
  //force redraw of cel
  DisplayCel
  if (blnTrans) {
    DrawTransGrid
  }
}
void picView_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  On Error GoTo ErrHandler
  
  //if not active form
  if (!MDIMain.ActiveForm Is Me) {
    return;
  }
  
  //if dragging picture
  if (blnDraggingView) {
    //get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y + 2 * fraToolbar.Height
    
    //if vertical scrollbar is visible
    if (vsbView.Visible) {
      //limit positions to valid values
      if (tmpY < vsbView.Minimum) {
        tmpY = vsbView.Minimum
      } else if (tmpY > vsbView.Maximum) {
        tmpY = vsbView.Maximum
      }
      //set vertical scrollbar
      vsbView.Value = tmpY
    }
    
    //if horizontal scrollbar is visible
    if (hsbView.Visible) {
      //limit positions to valid values
      if (tmpX < hsbView.Minimum) {
        tmpX = hsbView.Minimum
      } else if (tmpX > hsbView.Maximum) {
        tmpX = hsbView.Maximum
      }
      //set horizontal scrollbar
      hsbView.Value = tmpX
    }
  }
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}

void picView_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

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

void picView_Resize()
  
  Dim rtn As Long, offset As Long
  
  On Error GoTo ErrHandler
  
  if (blnTrans) {
    DrawTransGrid
  }
 
  DontDraw = true
  
  //position toolbars and viewholder
  if (pnlView.Bounds.Width >= 4 * fraCel.Width) {
    //position loop/cel frames on same row
    fraLoop.Move 220, 0
    fraCel.Move 330, 0
    picViewHolder.Top = 26
    picViewHolder.Height = pnlView.Bounds.Height - 26 - fraVMotion.Height - hsbView.Height
  } else {
    //position loop/cel frames on different rows
    fraLoop.Move 0, 24
    fraCel.Move 110, 24
    picViewHolder.Top = 50
    picViewHolder.Height = pnlView.Bounds.Height - 50 - fraVMotion.Height - hsbView.Height
  }
  picViewHolder.Width = pnlView.Bounds.Width - vsbView.Width
  
  //position/size scrollbars
  vsbView.Left = picViewHolder.Width
  vsbView.Height = picViewHolder.Height
  vsbView.Top = picViewHolder.Top
  
  hsbView.Top = picViewHolder.Top + picViewHolder.Height
  hsbView.Width = picViewHolder.Width
  
  //position motion frame
  fraVMotion.Top = pnlView.Bounds.Height - fraVMotion.Height
  
  SetVScrollBars
  
  DontDraw = false
  
return;

ErrHandler:

  //Debug.Assert false
  Resume Next
}

void picViewHolder_DblClick()

  picView_DblClick();
}

void rtfLogPrev_DblClick(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  //open logic for editing
  OpenLogic(SelResNum);
}
void rtfLogPrev_KeyDown(KeyCode As Integer, Shift As Integer)

  //BLAST! with the rtf window for previewing logics, the key
  //preview feature doesn//t work; so we have to catch the
  //keypress in the rtf control, then send it BACK to the form!
  Form_KeyDown KeyCode, Shift
  
  if (KeyCode = 0) {
    return;
  }
  
  switch (Shift
  case 0
    switch (KeyCode
    case vbKeyDelete
      //it should be caught by Form_KeyDown
      // but just in case, ignore it
      KeyCode = 0
    }
    
  case vbCtrlMask
    switch (KeyCode
    case vbKeyA
      rtfLogPrev.Range.SelectRange
    case vbKeyC
      rtfLogPrev.Selection.Range.Copy
    }
  }
  
  KeyCode = 0
  Shift = 0
  
}
void rtfLogPrev_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  //with right mouse click, show the context menu, but only
  //allow user to copy text, if some was selected
  On Error GoTo ErrHandler
  
  if (Button = vbRightButton) {
    With MDIMain
      if (rtfLogPrev.Selection.Range.Length > 0) {
        .mnuLPCopy.Enabled = true
      } else {
        .mnuLPCopy.Enabled = false
      }
      .mnuLPSelectAll.Visible = true
      PopupMenu .mnuLPPopup, 0, X, Y
    endwith
  }
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}

void sldSpeed_Change()
{  
  tmrMotion.Interval = 600 / sldSpeed - 45;
}

void sldSpeed_Click()
{  
  tmrMotion.Interval = 600 / sldSpeed - 45;
}
void tlbHAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

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

void tlbVAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

  //set alignment
  lngVAlign = Button.Index - 1
  
  //hide toolbar
  tlbVAlign.Visible = false
  
  //update main toolbar
  Toolbar1.Buttons("VAlign").Image = lngVAlign + 6
  
  //redraw the cel to update
  DisplayCel
}
void tmrMotion_Timer()
  
  On Error GoTo ErrHandler
  
  //advance to next cel, depending on mode
  
  switch (cmbMotion.SelectedIndex
  case 0  //normal
    if (udCel.Value = udCel.Maximum) {
      udCel.Value = 0
    } else {
      udCel.Value = udCel.Value + 1
    }
  
  case 1 //reverse
    if (udCel.Value = 0) {
      udCel.Value = udCel.Maximum
    } else {
      udCel.Value = udCel.Value - 1
    }
    
  case 2  //end of loop
    if (udCel.Value = udCel.Maximum) {
      //stop motion
      tmrMotion.Enabled = false
      //show play
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture
    } else {
      udCel.Value = udCel.Value + 1
    }
    
  case 3  //reverse loop
    if (udCel.Value = 0) {
      //stop motion
      tmrMotion.Enabled = false
      cmdVPlay.Picture = ImageList1.ListImages(9).Picture
    } else {
      udCel.Value = udCel.Value - 1
    }
  }
return;

ErrHandler:
  //Debug.Assert false
  Resume Next
}

void Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

  switch (Button.Key
  case "ZoomIn"
    ZoomPrev 1
    
  case "ZoomOut"
    ZoomPrev -1
    
    case "VAlign"
      //show valign toolbar
      tlbVAlign.Top = Toolbar1.Height / ScreenTWIPSY
      tlbVAlign.Visible = true
      
    case "HAlign"
      tlbHAlign.Top = Toolbar1.Height / ScreenTWIPSY
      tlbHAlign.Visible = true
  }
}

void Toolbar1_ButtonDropDown(ByVal Button As MSComctlLib.Button)

  switch (Button.Key
  case "VAlign"
    //show valign toolbar
    tlbVAlign.Top = Toolbar1.Height / ScreenTWIPSY
    tlbVAlign.Visible = true
  case "HAlign"
    tlbHAlign.Top = Toolbar1.Height / ScreenTWIPSY
    tlbHAlign.Visible = true
  }
}
void udCel_Change()
{
      //if not updating,
  if (blnNoUpdate) {
    return;
  }
  CurCel = udCel.Value;
  //display this cel
  DisplayCel();
}
void udCel_DownClick()
{
  //stop motion
  tmrMotion.Enabled = false;
}
void udLoop_Change()
{  
  //if updating is disabled,
  if (blnNoUpdate) {
    return;
  }
  //get new loop Value
  CurLoop = udLoop.Value;
  //display the loop
  DisplayLoop
}
void udLoop_DownClick()
{
  //stop motion
  tmrMotion.Enabled = false
}
void vsbView_Scroll()

  vsbView_Change
}
      */
    }
    private void tmrMotion_Tick(object sender, EventArgs e)
    {

    }
    void chkTrack_Click(int Index)
    {
      //if disabled, just exit
      if (!chkTrack[Index].Enabled)
      {
        return;
      }
      //if changing
      if (agSound.Track(Index).Muted == chkTrack[Index].Checked)
      {
        agSound.Track(Index).Muted = !chkTrack[Index].Checked;
      }
      //redisplay length (it may have changed)
      lblLength.Text = "Sound clip length: " + agSound.Length.ToString("0.0") + " seconds";
      //enable play button if at least one track is NOT muted AND midi not disabled AND length>0
      cmdPlay.Enabled = (chkTrack[0].Checked || chkTrack[1].Checked || chkTrack[2].Checked || chkTrack[3].Checked) && !Settings.NoMIDI && (agSound.Length > 0);
    }
    private void frmPreview_Load(object sender, EventArgs e)
    {
      // temp settings
      //set up the form controls
      hsbPic.Minimum = -PW_MARGIN;
      vsbPic.Minimum = -PW_MARGIN;
      cmbMotion.SelectedIndex = 0;
      sldSpeed.Value = 5;
      hsbView.Minimum = -PW_MARGIN;
      vsbView.Minimum = -PW_MARGIN;

      int i;
      int sngLeft, sngTop;
      int sngWidth, sngHeight;
      CalcWidth = MIN_WIDTH;
      CalcHeight = MIN_HEIGHT;

      //get preview window position
      sngWidth = ReadSettingLong(SettingsList, sPOSITION, "PreviewWidth", (int)(0.4 * MDIMain.Bounds.Width));
      if (sngWidth <= MIN_WIDTH)
      {
        sngWidth = MIN_WIDTH;
      }
      else if (sngWidth > 0.75 * Screen.GetWorkingArea(this).Width)
      {
        sngWidth = (int)(0.75 * Screen.GetWorkingArea(this).Width);
      }
      sngHeight = ReadSettingLong(SettingsList, sPOSITION, "PreviewHeight", (int)(0.5 * MDIMain.Bounds.Height));
      if (sngHeight <= MIN_HEIGHT)
      {
        sngHeight = MIN_HEIGHT;
      }
      else if (sngHeight > 0.75 * Screen.GetWorkingArea(this).Height)
      {
        sngHeight = (int)(0.75 * Screen.GetWorkingArea(this).Height);
      }
      sngLeft = ReadSettingLong(SettingsList, sPOSITION, "PreviewLeft", 0);
      if (sngLeft < 0)
      {
        sngLeft = 0;
      }
      else
      {
        if (Settings.ResListType != 0)
        {
          if (sngLeft > MDIMain.Width - MDIMain.pnlResources.Width - 300)
          {
            sngLeft = MDIMain.Width - MDIMain.pnlResources.Width - 300;
          }
        }
        else
        {
          if (sngLeft > MDIMain.Width - 300)
          {
            sngLeft = MDIMain.Width - 300;
          }
        }
      }
      sngTop = ReadSettingLong(SettingsList, sPOSITION, "PreviewTop", 0);
      if (sngTop < 0)
      {
        sngTop = 0;
      }
      else
      {
        if (sngTop > MDIMain.Bounds.Height - 300)
        {
          sngTop = MDIMain.Bounds.Height - 300;
        }
      }
      //now move the form
      this.Bounds = new Rectangle(sngLeft, sngTop, sngWidth, sngHeight);

      ////set flag to skip update of cels + loops during load
      //blnNoUpdate = true;

      //load instrument listboxes
      for (i = 0; i < 128; i++)
      {
        cmbInst0.Items.Add(InstrumentName(i));
        cmbInst1.Items.Add(InstrumentName(i));
        cmbInst2.Items.Add(InstrumentName(i));
      }
      //get default scale values
      ViewScale = Settings.ViewScale.Preview;
      PicScale = Settings.PicScale.Preview;
      //set default view alignment
      lngHAlign = Settings.ViewAlignH;
      lngVAlign = Settings.ViewAlignV;
      ////set view scrollbar values
      //hsbView.LargeChange = picViewHolder.Width * LG_SCROLL;
      //vsbView.LargeChange = picViewHolder.Height * LG_SCROLL;
      //hsbView.SmallChange = picViewHolder.Width * SM_SCROLL;
      //vsbView.SmallChange = picViewHolder.Height * SM_SCROLL;
      VTopMargin = 50;
      lngVAlign = 2;
      //tsViewPrev.Items["VAlign"].ImageIndexImage = 8;
      //set picture scrollbar values
      hsbPic.LargeChange = (int)(pnlPicture.Width * LG_SCROLL);
      vsbPic.LargeChange = (int)(panel2.Height * LG_SCROLL);
      hsbPic.SmallChange = (int)(panel2.Width * SM_SCROLL);
      vsbPic.SmallChange = (int)(panel2.Height * SM_SCROLL);
      //set picture zoom
      udPZoom.Value = PicScale;

      //no resource is selected on load
      SelResNum = -1;

      //set font
      rtfLogPrev.Font = new Font(Settings.PFontName, Settings.PFontSize);
      //rtfLogPrev.HighlightSyntax = false;

      ////restore updating capability
      //blnNoUpdate = false;
    }
  }
}
