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
using static WinAGI.AGIGame;

namespace WinAGI_GDS
{
  public partial class frmMDIMain : Form
  {
    public frmMDIMain()
    {
      InitializeComponent();

      //attach events
      CompileGameStatus += GameEvents_CompileGameStatus;
      CompileLogicStatus += GameEvents_CompileLogicStatus;
      LoadGameStatus += GameEvents_LoadGameStatus;
    }

    private void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e)
    {

    }

    private void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e)
    {
      MessageBox.Show("game is loading!\n\n" + e.ResNum);
    }

    private void GameEvents_CompileLogicStatus(object sender, CompileLogicEventArgs e)
    {

    }

    private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void btnOpenGame_Click(object sender, EventArgs e)
    {
      GameAbout = "test";
    }

    private void mnuWCascade_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.Cascade);
    }

    private void mnuWArrange_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.ArrangeIcons);
    }

    private void mnuWTileH_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileHorizontal);
    }

    private void mnuWTileV_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileVertical);
    }

    private void mnuWMinimize_Click(object sender, EventArgs e)
    {
      foreach (Form childForm in MdiChildren)
      {
        childForm.WindowState = FormWindowState.Minimized;
      }
    }

    private void mnuWClose_Click(object sender, EventArgs e)
    {
      //only close if window is close-able
      this.ActiveMdiChild.Close();
    }

    private void mnuWindow_DropDownOpening(object sender, EventArgs e)
    {
      // disable the close item if no windows
      mnuWClose.Enabled = (this.MdiChildren.Length != 0);
    }

    private void btnNewRes_DropDownOpening(object sender, EventArgs e)
    {
      //can we tell difference between clicking on arrow?
      string sTest = "am123";
      if ("vfmoisc".Any(sTest.ToLower().StartsWith))
        MessageBox.Show("sTest is a marker");
      else
      {
        MessageBox.Show("sTest is NOT a marker");
      }
    }

    private void frmMDIMain_Load(object sender, EventArgs e)
    {

    }
  }
}
