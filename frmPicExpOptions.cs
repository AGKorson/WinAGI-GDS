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
using WinAGI_GDS.WinAGI;

namespace WinAGI_GDS
{
  public partial class frmPicExpOptions : Form
  {
    public bool Canceled;
    public int FormMode;
    public frmPicExpOptions(int NewMode)
    {
      InitializeComponent();
      //default is bitmap
      cmbFormat.SelectedIndex = 0;
      // assume cancel, until OK is pressed
      Canceled = true;
       FormMode = NewMode;
      //FormMode=0: allow choice of export
      //FormMode=1: force image export only
      switch (FormMode) {
      case 0:  // allow choice
        fraChoice.Visible = true;
        optResource.Checked = true;
        fraImage.Visible = false;
        Height = 110;
        break;
      case 1:  // image only
        fraChoice.Visible = false;
        fraImage.Top -= 58;
        lblFormat.Top -= 58;
        cmbFormat.Top -= 58;
        lblScale.Top -= 58;
        udZoom.Top -= 58;
        lblBoth.Top -= 58;
        //}
        optImage.Checked = true;
        this.Height = 182;
        break;
      }
   }
    private void OKButton_Click(object sender, EventArgs e)
    {
      //ok!
      Canceled = false;
      this.Visible = false;
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      //canceled..
      Canceled = true;
      this.Visible = false;
    }

    private void optImageFormat(object sender, EventArgs e)
    {
      //shrink/expand form as needed
      if (optResource.Checked) {
        this.Height = 110;
      } else {
        this.Height = 240;
      }
    }

    private void optImageType_CheckedChanged(object sender, EventArgs e)
    {
      //show priority image tip label if both is selected
      lblBoth.Visible = optBoth.Checked;
    }
  }
}
