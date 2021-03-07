using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor
{
  public partial class frmSplash : Form
  {
    public frmSplash()
    {
      InitializeComponent();

      // set version information
      lblVersion.Text = "Version " + Application.ProductVersion;
      lblCopyright.Text += Common.Base.COPYRIGHT_YEAR;
  }
  }
}
