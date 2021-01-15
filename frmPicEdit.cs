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
using WinAGI;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI_GDS.ResMan;

namespace WinAGI_GDS
{
  public partial class frmPicEdit : Form
  {
    Bitmap thisBMP;
    float zoom;
    bool picMode = false;
    internal bool InGame;

    public frmPicEdit()
    {
      InitializeComponent();
    }

    private void frmPicEdit_Load(object sender, EventArgs e)
    {
      //load combobox with AGI color indices
      cmbTransCol.Items.Add(AGIColors.agBlack);
      cmbTransCol.Items.Add(AGIColors.agBlue);
      cmbTransCol.Items.Add(AGIColors.agGreen);
      cmbTransCol.Items.Add(AGIColors.agCyan);
      cmbTransCol.Items.Add(AGIColors.agRed);
      cmbTransCol.Items.Add(AGIColors.agMagenta);
      cmbTransCol.Items.Add(AGIColors.agBrown);
      cmbTransCol.Items.Add(AGIColors.agLtGray);
      cmbTransCol.Items.Add(AGIColors.agDkGray);
      cmbTransCol.Items.Add(AGIColors.agLtBlue);
      cmbTransCol.Items.Add(AGIColors.agLtGreen);
      cmbTransCol.Items.Add(AGIColors.agLtCyan);
      cmbTransCol.Items.Add(AGIColors.agLtRed);
      cmbTransCol.Items.Add(AGIColors.agLtMagenta);
      cmbTransCol.Items.Add(AGIColors.agYellow);
      cmbTransCol.Items.Add(AGIColors.agWhite);
      cmbTransCol.Items.Add("None");
      cmbTransCol.SelectedIndex = 16;

      // load the picture
      Pictures[1].Load();
      thisBMP = Pictures[1].VisualBMP;
      // show it with NO transparency
      ShowAGIBitmap(picVisual, thisBMP);
    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {

      //resize our picture on the fly

      // convert trackbar value to a zoom factor
      zoom = (float)(trackBar1.Value / 2f + 1);

      // first, create new image in the picture box that is desired size
      //picVisual.Size = new Size((int)(320 * zoom), (int)(168 * zoom));
      picVisual.Width = (int)(320 * zoom);
      picVisual.Height = (int)(168 * zoom);

      showPic();
    }
    private void picVisual_Click(object sender, EventArgs e)
    {
      //swap visual/priority
      picMode = !picMode;
      showPic();
    }

    void showPic()
    {
      if (picMode)
      {
        thisBMP = (Bitmap)Pictures[1].PriorityBMP.Clone();
      }
      else
      {
        thisBMP = (Bitmap)Pictures[1].VisualBMP.Clone();
      }
      if (cmbTransCol.SelectedIndex < 16)
      {
        thisBMP.MakeTransparent(EGAColor[cmbTransCol.SelectedIndex]);
      }
      ShowAGIBitmap(picVisual, thisBMP, zoom);

    }

    private void cmbTransCol_SelectionChangeCommitted(object sender, EventArgs e)
    {
      //redraw, with the selected transparent image
      showPic();
    }
  } 
}
