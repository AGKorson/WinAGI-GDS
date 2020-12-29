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

namespace WinAGI_GDS
{
  public partial class frmPicEdit : Form
  {
    public frmPicEdit()
    {
      InitializeComponent();
      //can we draw a picture?

      // copy to the image
      Pictures[1].Load();

      using (Graphics g = Graphics.FromImage(picVisual.Image))
      {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        picVisual.SizeMode = PictureBoxSizeMode.Normal;
        g.DrawImage(Pictures[1].VisualBMP, new Rectangle(0,0,640,336));
      }
      picVisual.Image = Pictures[1].VisualBMP;
      picVisual.Refresh();
    }
  }
}
