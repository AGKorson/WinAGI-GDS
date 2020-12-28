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
      //can we draw a picture???


      ////Bitmap bmVis = new Bitmap(160, 168);
      //var bmVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
      //ColorPalette ncp = bmVis.Palette;
      //for (int i = 0; i < 16; i++)
      //{
      //  ncp.Entries[i] = Color.FromArgb(255,
      //  (int)((lngEGARevCol[i] & 0xFF0000) / 0x10000),
      //  (int)((lngEGARevCol[i] & 0xFF00) / 0x100),
      //  (int)(lngEGARevCol[i] % 0x100)
      //    );
      //}
      //bmVis.Palette = ncp;
      //var BoundsRect = new Rectangle(0, 0, 160, 168);
      //BitmapData bmpData = bmVis.LockBits(BoundsRect,
      //                                ImageLockMode.WriteOnly,
      //                                bmVis.PixelFormat);
      //IntPtr ptr = bmpData.Scan0;
      //int bytes = bmpData.Stride * bmVis.Height;
      //byte[] rgbValues = new byte[bytes];
      //for (int i = 0; i < bytes; i++)
      //{
      //  rgbValues[i] = (byte)(i % 16);
      //}

      //// fill in rgbValues, e.g. with a for loop over an input array

      //Marshal.Copy(rgbValues, 0, ptr, bytes);
      //bmVis.UnlockBits(bmpData);

      // copy to the image?
      Pictures[1].Load();
      picVisual.Image = Pictures[1].VisualBMP;
      picVisual.Refresh();
    }
  }
}
