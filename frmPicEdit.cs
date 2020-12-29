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

      // toss current image
      if (picVisual.Image != null) picVisual.Image.Dispose();
      //create a new image to draw on, sized to hold the zoom image
      Bitmap newVis = new Bitmap(640, 336);
      // graphics object lets us draw the image with correct scaling
      using (Graphics g = Graphics.FromImage(newVis))
      {
        // this is the trick- interpolation = NearestNeighbor
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.DrawImage(Pictures[1].VisualBMP, 0, 0, 640, 336);// new Rectangle(Point.Empty, new Size(320, 168)));
      }
      // now copy the correctly scaled image to our picbox
      picVisual.Image = newVis;
    }

    private void frmPicEdit_Load(object sender, EventArgs e)
    {

    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {

      //resize by creating a new bitmap object that we draw the
      // the image on with correct scaling

      // get rid of existing image
      if (picVisual.Image != null) picVisual.Image.Dispose();

      // convert trackbar value to a zoom factor
      float zoom = (float)(trackBar1.Value / 2f + 1);

      // create new bitmap to hold zoomed image
      Bitmap imgZoom = new Bitmap( (int)(320 * zoom), (int)(168 * zoom));

      // use grahics object to draw the image scaled correctly
      using (Graphics g = Graphics.FromImage(imgZoom))
      {
        // secret is using 'NearestNeighbor mode
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        //g.DrawImage(bmp, new Rectangle(0, 0, (int)(320 * zoom), (int)(168 * zoom)));
        g.DrawImage(Pictures[1].VisualBMP, 0, 0, 320 * zoom, 168 * zoom);
      }
      // now copy the new zoomed image to our picturebox
      picVisual.Image = imgZoom;
    }
  } 
}
