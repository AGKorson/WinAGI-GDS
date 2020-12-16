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
using static WinAGI.AGITestCommands;
using static WinAGI.AGICommands;

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

    private void rtfLogic_DoubleClick(object sender, EventArgs e)
    {
      // try a few functions!
      if (IsValidGameDir( rtfLogic.Text))
      {
        MessageBox.Show("it works");
      }
      else
      {
        MessageBox.Show("not a valid directory");
      }
    }
  }
}
