using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.ResMan;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGIGame;

namespace WinAGI.Editor
{
  public partial class frmViewGifOptions : Form
  {
    public bool Canceled = true;
    public int FormMode = 0;

    AGILoop ExportLoop;
    AGIPicture ExportPic;
    GifOptions ThisGifOps;
    bool DontDraw;
    bool blnVisOn, blnXYDraw;
    byte bytCel;
    int lngPos;
    int MaxW, MaxH;
    const int VG_MARGIN = 4;
    public frmViewGifOptions()
    {
      InitializeComponent();
    }
    void CheckScrollbars()
    {
      DontDraw = true;
      HScroll1.Visible = (picCel.Width > 272); //picGrid.Width - 2 * VG_MARGIN);
      if (HScroll1.Visible) {
        HScroll1.Width = 280; //picGrid.Width;
        HScroll1.Maximum = HScroll1.Minimum + picCel.Width + 2 * VG_MARGIN - 280; // picGrid.Width
      } else {
        //reset it, reposition cel frame
        HScroll1.Value = -VG_MARGIN;
        picCel.Left = VG_MARGIN;
      }

      VScroll1.Visible = (picCel.Height > 224); //picGrid.Height - 2 * VG_MARGIN);
      if (VScroll1.Visible) {
        VScroll1.Height = 232; //picGrid.Height;
        VScroll1.Maximum = VScroll1.Minimum + picCel.Height + 2 * VG_MARGIN - 232; //picGrid.Height
      } else {
        //reset it, reposition cel frame
        VScroll1.Value = -VG_MARGIN;
        picCel.Top = VG_MARGIN;
      }

      //shrink grid if cel is small
      if (picCel.Width + 2 * VG_MARGIN < 280) {
        picGrid.Width = picCel.Width + 2 * VG_MARGIN;
      } else {
        picGrid.Width = 280;
      }
      if (picCel.Height + 2 * VG_MARGIN < 232) {
        picGrid.Height = picCel.Height + 2 * VG_MARGIN;
      } else {
        picGrid.Height = 232;
      }

      DontDraw = false;
      return;
    }
    void DisplayCel()
    {
      //this function copies the bitmap Image
      //from bytLoop.bytCel into the view Image box,
      //and resizes it to be correct size
      int tgtX, tgtY, tgtH, tgtW;
      picCel.BackColor = EditGame.EGAColor[(int)ExportLoop.Cels[bytCel].TransColor];
      //copy view Image
      tgtW = ExportLoop.Cels[bytCel].Width * 2 * ThisGifOps.Zoom;
      tgtH = ExportLoop.Cels[bytCel].Height * ThisGifOps.Zoom;
      if (ThisGifOps.HAlign == 0) {
        //align left
        tgtX = 0;
      } else {
        //align right
        tgtX = picCel.Width - tgtW;
      }
      if (ThisGifOps.VAlign == 0) {
        //align top
        tgtY = 0;
      } else {
        //align bottom
        tgtY = picCel.Height - tgtH;
      }
      ShowAGIBitmap(picCel, ExportLoop.Cels[bytCel].CelBMP,tgtX, tgtY, tgtW, tgtH);
    }
    public void InitForm(int Mode, object GifObj)
    {
      //display the loop, and begin animating it using default export settings
      int i;
      //assume cancel unless user explicitly selects OK
      Canceled = true;
      //set up form according to what//s being exported
      FormMode = Mode;

      switch (Mode) {
      case 0:  //export a loop
        ExportLoop = (AGILoop)GifObj;
        //use the global options as starting point
        ThisGifOps = VGOptions;
        if (ThisGifOps.Cycle) {
          chkLoop.Checked = true;
        } else {
          chkLoop.Checked = false;
        }
        udDelay.Text = ThisGifOps.Delay.ToString();
        timer1.Interval = 10 * ThisGifOps.Delay;
        timer1.Enabled = true;
        UpdateAlignmentLabel();
        if (ThisGifOps.Transparency) {
          chkTrans.Checked = true;
        } else {
          chkTrans.Checked = false;
        }
        udScale.Text = ThisGifOps.Zoom.ToString(); ;

        //determine size of holding pic
        MaxW = 0;
        MaxH = 0;
        for (i = 0; i < ExportLoop.Cels.Count; i++) {
          if (ExportLoop.Cels[i].Width > MaxW) {
            MaxW = ExportLoop.Cels[i].Width;
          }
          if (ExportLoop.Cels[i].Height > MaxH) {
            MaxH = ExportLoop.Cels[i].Height;
          }
        }

        //set size of view holder
        picCel.Width = MaxW * 2 * ThisGifOps.Zoom;
        picCel.Height = MaxH * ThisGifOps.Zoom;
        //force back to upper, left
        picCel.Top = VG_MARGIN;
        picCel.Left = VG_MARGIN;

        CheckScrollbars();
        break;
      case 1: //export a picture
        ExportPic = (AGIPicture)GifObj;
        //hide the alignment toolbar, scrollbars and transparency options
        toolStrip1.Visible = false;
        chkTrans.Visible = false;
        lblAlign.Visible = false;
        VScroll1.Visible = false;
        HScroll1.Visible = false;
        //picGrid.Cls
        picCel.Visible = false;
        //default to continous cycle
        chkLoop.Checked = true;
        //reset preview position
        lngPos = -1;
        break;
      }
    }
    void UpdateAlignmentLabel()
    {
      lblAlign.Text = "Align: ";
      if (ThisGifOps.VAlign == 0) {
        lblAlign.Text += "Top, ";
      } else {
        lblAlign.Text += "Bottom, ";
      }
      if (ThisGifOps.HAlign == 0) {
        lblAlign.Text += "Left";
      } else {
        lblAlign.Text += "Right";
      }
    }
    void tmpForm()
    {
      /*





  void CancelButton_Click()
    //canceled..
    Canceled = true
    Hide
  }

  void chkLoop_Click()


    //set autoloop option
    ThisGifOps.Cycle = (chkLoop.Checked);

  }
  void chkTrans_Click()

    //set transparency option
    ThisGifOps.Transparency = (chkTrans.Checked);

  }
  void Form_Unload()

    //drop reference to loop
    ExportLoop = null;
  }


  void HScroll1_Change()

    if (DontDraw) {
      return;
    }

    picCel.Left = -HScroll1.Value;
  }
  void OKButton_Click()
    if (FormMode == 0) {
      //copy new options back to global options
      VGOptions = ThisGifOps;
    }

    //ok!
    Canceled = false;
    this.Hide();
  }
  void timer1_Timer()

    //advance cel number for this loop
    byte bytCmd;
    switch (FormMode) {
    case 0: //loop
      bytCel = bytCel + 1
      if (bytCel = ExportLoop.Cels.Count) {
        //reset to start
        bytCel = 0
        //if cycling to end, insert a pause
        if (!ThisGifOps.Cycle) {
          timer1.Interval = timer1.Interval + 1000
        }
      } else {
        timer1.Interval = 10 * ThisGifOps.Delay
      }

      DisplayCel

    case 1: //picture
      do {
        //increment data pointer
        lngPos++;

        //if end is reached
        if (lngPos >= ExportPic.Size) {
          //reset to beginning of picture
          lngPos = -1
          Exit Do
        }

        //get the value at this pos, and determine if it's
        //a draw command
        bytCmd = ExportPic.Data(lngPos)

        switch (bytCmd) {
        case 240:
          blnXYDraw = false
          blnVisOn = true
          lngPos++;
        case 241:
          blnXYDraw = false
          blnVisOn = false
        case 242:
          blnXYDraw = false
          lngPos++;
        case 243:
          blnXYDraw = false
        case 244:
      case 245:
          blnXYDraw = true
          lngPos = lngPos + 2
        case 246:
      case 247:
      case 248:
      case 250:
          blnXYDraw = false
          lngPos = lngPos + 2
        case 249:
          blnXYDraw = false
          lngPos++;
        default:
          //skip second coordinate byte, unless
          //currently drawing X or Y lines
          if (!blnXYDraw) {
            lngPos++;
          }
        }

      // exit if non-pen cmd found, and vis pen is active
      }
      while ((bytCmd >= 240 && bytCmd <= 244) || bytCmd == 249 || !blnVisOn);

      //show pic drawn up to this point
      ExportPic.DrawPos = lngPos;
      ShowAGIBitmap(picGrid, ExportPic.VisualBMP, 1);
    }
  }
  void tlbHAlign_ButtonClick(object sender, EventArgs e)

    //set vertical alignment
    ThisGifOps.HAlign = Button.Index - 1
    UpdateAlignmentLabel

    //update main toolbar
    toolStrip1.Buttons("HAlign").Image = ThisGifOps.HAlign + 1

    //hide toolbar
    tlbHAlign.Visible = false

    //force redraw
    DisplayCel
    //reset timer
    timer1.Enabled = false
    timer1.Enabled = true
  }

  void tlbVAlign_ButtonClick(object sender, EventArgs e)

    //set vertical alignment
    ThisGifOps.VAlign = Button.Index - 1
    UpdateAlignmentLabel

    //update main toolbar
    toolStrip1.Buttons("VAlign").Image = ThisGifOps.VAlign + 3

    //hide toolbar
    tlbVAlign.Visible = false

    //force redraw
    DisplayCel
    //reset timer
    timer1.Enabled = false
    timer1.Enabled = true
  }


  void toolStrip1_ButtonClick(object sender, EventArgs e)

    switch (Button.Key) {
      case "VAlign"
        //show valign toolbar
        tlbVAlign.Visible = true
      case "HAlign"
        tlbHAlign.Visible = true
    }
  }

  void toolStrip1_ButtonDropDown(object sender, EventArgs e)

    switch (Button.Key) {
    case "VAlign"
      //show valign toolbar
      tlbVAlign.Visible = true
    case "HAlign"
      tlbHAlign.Visible = true
    }
  }


  void udDelay_Change()

    //limit Value
    if (Val(udDelay.Text) < 1) {
      udDelay.Text = "1"
    }
    if (Val(udDelay.Text) > 100) {
      udDelay.Text = "100"
    }

    //adjust delay time
    ThisGifOps.Delay = Val(udDelay.Text)
    timer1.Interval = 10 * Val(udDelay.Text)
  }

  void udDelay_KeyPress(object sender, EventArgs e)

    //numbers, only...

    switch (KeyAscii) {
    case 8, 46, 48 To 57
      //backspace, delete, digits
    default:
      //not ok
      KeyAscii = 0
    }
  }

  void udScale_Change()

    //limit Value
    if (Val(udScale.Text) < 1) {
      udScale.Text = "1"
    }
    if (Val(udScale.Text) > 100) {
      udScale.Text = "100"
    }

    //only update preview if exporting a loop
    if (FormMode = 0) {
      ThisGifOps.Zoom = Val(udScale.Text)
      picCel.Width = MaxW * ThisGifOps.Zoom * 2
      picCel.Height = MaxH * ThisGifOps.Zoom
      //force redraw
      CheckScrollbars
      DisplayCel
    }
  }

  void udScale_KeyPress(object sender, EventArgs e)

    //numbers, only...

    switch (KeyAscii) {
    case 8, 46, 48 To 57
      //backspace, delete, digits
    default:
      //not ok
      KeyAscii = 0
    }

  }


   VScroll1_Change()

    if (DontDraw) {
      return;
    }

    picCel.Top = -VScroll1.Value
  }
      */
    }
  }
}
