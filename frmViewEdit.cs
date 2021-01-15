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
using static WinAGI_GDS.ResMan;

namespace WinAGI_GDS
{
  public partial class frmViewEdit : Form
  {
    readonly double zoom = 4;
    AGIView thisView;
    private int CurLoop = 0;
    private int CurCel = 0;
    internal bool InGame;

    public frmViewEdit()
    {
      InitializeComponent();
    }

    private void cmbView_SelectionChangeCommitted(object sender, EventArgs e)
    {
      // unload current view, and load selected view
      if (thisView != null)
        thisView.Unload();

      thisView = (AGIView)cmbView.SelectedItem;
      try
      {
        thisView.Load();
      }
      catch (Exception)
      {
        //ignore error
        thisView.Unload();
        thisView = null;
      }
      //clear loop list
      cmbLoop.Items.Clear();
      //add loops to the loop box
      foreach (AGILoop tmpLoop in thisView.Loops)
      {
        cmbLoop.Items.Add($"Loop {tmpLoop.Index}");
      }
      //select first loop
      cmbLoop.SelectedIndex = 0;
    }


    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      // redraw 
      // set transparency
      thisView[CurLoop][CurCel].Transparency = chkTrans.Checked;
      ShowAGIBitmap(picCel, thisView[CurLoop][CurCel].CelBMP, zoom);
    }

    private void frmViewEdit_Load(object sender, EventArgs e)
    {
      // load views
      foreach (AGIView tmpView in Views.Col.Values)
      {
        cmbView.Items.Add(tmpView);
      }
    }

    private void cmbLoop_SelectedIndexChanged(object sender, EventArgs e)
    {
      // loop changing; select a new cel
      CurLoop = cmbLoop.SelectedIndex;
      // clear cel list
      cmbCel.Items.Clear();
      // add all cels
      foreach (AGICel tmpCel in thisView[CurLoop].Cels)
      {
        cmbCel.Items.Add($"Cel {tmpCel.Index}");
      }
      // select first cel
      cmbCel.SelectedIndex = 0;
    }

    private void cmbCel_SelectedIndexChanged(object sender, EventArgs e)
    {
      CurCel = cmbCel.SelectedIndex;
      // clear the picture box
      //using Graphics = picCel.Image.    Graphics.FromImage(pic.Image);
      picCel.CreateGraphics().Clear(BackColor);
      picCel.Refresh();
      // set transparency
      thisView[CurLoop][CurCel].Transparency = chkTrans.Checked;
      // show the cel
      ShowAGIBitmap(picCel, thisView[CurLoop][CurCel].CelBMP, zoom);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      // cycle the cel
      CurCel++;
      if (CurCel >= thisView[CurLoop].Cels.Count)
      {
        CurCel = 0;
        // show the cel
      }
      // set transparency
      thisView[CurLoop][CurCel].Transparency = chkTrans.Checked;
      ShowAGIBitmap(picCel, thisView[CurLoop][CurCel].CelBMP, zoom);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      //toggle cycle state
      if (timer1.Enabled)
      {
        //stop cycling; change button name to allow starting
        button1.Text = "Start";
      }
      else
      {
        button1.Text = "Stop";
      }
      timer1.Enabled = !timer1.Enabled;
    }

    private void frmViewEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
      // make sure view is unloaded
      thisView?.Unload();
    }
  }
}
