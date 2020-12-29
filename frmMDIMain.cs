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

      //
      WinAGI.WinAGI.InitWinAGI();
    }

    private void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e)
    {

    }

    private void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e)
    {
      statusStrip1.Items[0].Text = $"Loading Status: {e.lStatus} - Type: {e.ResType} - Number: {e.ResNum}";
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
      int retval;
      //MessageBox.Show("need to fully test Read/Write settings functions; since VB is 1 based, the functions may not be working correctly in all cases, especially for bad format input and edge cases");
      //ok, let's try to open a game!
      if (SystemInformation.UserName == "agkor")
      {
        // at home:
        retval = OpenGameWAG(@"C:\Users\Andy\OneDrive\AGI Stuff\AGI Test Games\GRm\gr.wag");
      }
      else
      {
        // at work:
        retval = OpenGameWAG(@"C:\Users\d3m294\OneDrive - PNNL\Desktop\WinAGI\GR-IIGS\GR.wag");
      }
      MessageBox.Show($"opengame result: {retval.ToString()}");
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
    }

    private void frmMDIMain_Load(object sender, EventArgs e)
    {

    }

    private void btnNewLogic_Click(object sender, EventArgs e)
    {
      //lets try to load a logic!
      Logics[5].Load();
      MessageBox.Show($"Logic 0 ({Logics[5].ID}) is loaded: {Logics[5].Loaded.ToString()}");
      // assign it to new logic form!
      frmLogicEdit frmNew = new frmLogicEdit();
      frmNew.MdiParent = this;
      frmNew.rtfLogic.Text = Logics[5].SourceText;
      frmNew.Show();
    }

    private void btnNewPicture_Click(object sender, EventArgs e)
    {
      frmPicEdit frmNew = new frmPicEdit();
      frmNew.MdiParent = this;
      frmNew.Show();
    }
  }
}
