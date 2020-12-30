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

      // load the picture
      Pictures[1].Load();

      //to scale the picture without blurring, need to use NearestNeighbor interpolation
      // that can't be set directly, so a graphics object is needed to draw the
      // the picture

      // first, create new image in the picture box that is desired size
      picVisual.Image = new Bitmap(640, 336);
      // intialize a graphics object for the image just created
      using (Graphics g = Graphics.FromImage(picVisual.Image))
      {
        // set correct interpolation mode
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        // draw the bitmap, at correct resolution
        g.DrawImage(Pictures[1].VisualBMP, 0, 0, 640, 336);
      }

    }

    private void frmPicEdit_Load(object sender, EventArgs e)
    {

    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {

      //resize our picture on the fly

      // convert trackbar value to a zoom factor
      float zoom = (float)(trackBar1.Value / 2f + 1);

      // first, create new image in the picture box that is desired size
      picVisual.Image = new Bitmap((int)(320 * zoom), (int)(168 * zoom));
      // intialize a graphics object for the image just created
      using (Graphics g = Graphics.FromImage(picVisual.Image))
      {
        // set correct interpolation mode
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        // draw the bitmap, at correct resolution
        g.DrawImage(Pictures[1].VisualBMP, 0, 0, 320 * zoom, 168 * zoom);
      }
    }
  } 
}
