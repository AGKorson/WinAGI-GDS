using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI_GDS
{
  public partial class frmMDIMain : Form
  {
    public frmMDIMain()
    {
      InitializeComponent();
    }

    private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      // how do you know what was selected???
      //System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
      //messageBoxCS.AppendFormat("{0} = {1}", "Clicked Item", e.ClickedItem);
      //messageBoxCS.AppendLine();
      //MessageBox.Show(messageBoxCS.ToString(), "ItemClicked Event");

    }

    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
      //add a new window
      frmLogicEdit FormNew = new frmLogicEdit
      {
        MdiParent = this
      };
      FormNew.Show();
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

    }
  }
}
