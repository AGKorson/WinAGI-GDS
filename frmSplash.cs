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

namespace WinAGI_GDS
{
  public partial class frmSplash : Form
  {
    public frmSplash()
    {
      InitializeComponent();

      // set version information
      lblVersion.Text = "Version " + Application.ProductVersion;
      lblCopyright.Text += COPYRIGHT_YEAR;
  }
  }
}
