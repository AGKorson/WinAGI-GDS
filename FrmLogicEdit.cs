using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinAGI;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI_GDS
{
  public partial class frmLogicEdit : Form
  {
    public AGILogic ThisLogic = new AGILogic { };
    public AGILogics aglogs = new AGILogics { };
    public frmLogicEdit()
    {
      InitializeComponent();
    }

  }
}
