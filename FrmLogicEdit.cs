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
using static WinAGI_GDS.ResMan;

namespace WinAGI_GDS
{
  public partial class frmLogicEdit : Form
  {
    public AGILogic ThisLogic = new AGILogic { };
    public AGILogics aglogs = new AGILogics { };
    internal ELogicFormMode FormMode;
    internal bool InGame;

    public frmLogicEdit()
    {
      InitializeComponent();
    }

    private void rtfLogic_DoubleClick(object sender, EventArgs e)
    {
      // testing logic functions
      Logics[5].SourceText = rtfLogic.Text;
      //Logics[5].Save();
      Logics[5].SaveSource();
    }
  }
}
