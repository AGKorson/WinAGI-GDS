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
using static WinAGI.AGIGame;
using System.Diagnostics;

namespace WinAGI_GDS
{
  public partial class frmMDIMain : Form
  {
    public frmMDIMain()
    {
      InitializeComponent();

      //attach events
      CompileGameStatus += GameEvents_CompileGameStatus;
      CompileLogicStatus += GameEvents_CompileLogicStatus;
      LoadGameStatus += GameEvents_LoadGameStatus;

      //
      WinAGI.WinAGI.InitWinAGI();
      LogicSourceSettings.ShowAllMessages = true;
      LogicSourceSettings.UseReservedNames = true;
      LogicSourceSettings.SpecialSyntax = true;
      LogicSourceSettings.ReservedAsText = true;
      LogicSourceSettings.ErrorLevel = LogicErrorLevel.leMedium;
      LogicSourceSettings.ElseAsGoto = false;
      

    }

    private void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e)
    {

    }

    private void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e)
    {
      Debug.Print($"Loading Status: {e.lStatus} - Type: {e.ResType} - Number: {e.ResNum} Msg: {e.ErrString}");
    }

    private void GameEvents_CompileLogicStatus(object sender, CompileLogicEventArgs e)
    {

    }

    private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void btnOpenGame_Click(object sender, EventArgs e)
    {
      int retval;
      //MessageBox.Show("need to fully test Read/Write settings functions; since VB is 1 based, the functions may not be working correctly in all cases, especially for bad format input and edge cases");
      //ok, let's try to open a game!
      if (SystemInformation.UserName == "agkor")
      {
        // at home:
        retval = OpenGameWAG(@"C:\Users\Andy\OneDrive\AGI Stuff\AGI Test Games\GRm\gr.wag");
      }
      else
      {
        // at work:
        retval = OpenGameWAG(@"C:\Users\d3m294\OneDrive - PNNL\Desktop\WinAGI\GR-IIGS\GR.wag");
      }
      if (retval == 0)
      {
        MessageBox.Show("Game opened with no errors or warnings.");
      }
      else if (retval == WINAGI_ERR + 636)
      {
        MessageBox.Show("Game opened, with warnings.");
      }
      else
      {
        MessageBox.Show($"opengame result: {(retval - WINAGI_ERR).ToString()}");
      }
    }

    private void mnuWCascade_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.Cascade);
    }

    private void mnuWArrange_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.ArrangeIcons);
    }

    private void mnuWTileH_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileHorizontal);
    }

    private void mnuWTileV_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileVertical);
    }

    private void mnuWMinimize_Click(object sender, EventArgs e)
    {
      foreach (Form childForm in MdiChildren)
      {
        childForm.WindowState = FormWindowState.Minimized;
      }
    }

    private void mnuWClose_Click(object sender, EventArgs e)
    {
      //only close if window is close-able
      this.ActiveMdiChild.Close();
    }

    private void mnuWindow_DropDownOpening(object sender, EventArgs e)
    {
      // disable the close item if no windows
      mnuWClose.Enabled = (this.MdiChildren.Length != 0);
    }

    private void btnNewRes_DropDownOpening(object sender, EventArgs e)
    {
      // cancel it? and do whatever is shown?
      System.Windows.Forms.ToolStripDropDownButton btnSender = (System.Windows.Forms.ToolStripDropDownButton)sender;
      if (btnSender.IsOnDropDown)
      {
        MessageBox.Show("on dropdown");
      }
    }

    private void frmMDIMain_Load(object sender, EventArgs e)
    {

    }

    private void btnNewLogic_Click(object sender, EventArgs e)
    {

      if (!GameLoaded) return;

      //lets try to load a logic!
      Logics[5].Load();
      MessageBox.Show($"Logic 0 ({Logics[5].ID}) is loaded: {Logics[5].Loaded}");
      // assign it to new logic form!
      frmLogicEdit frmNew = new frmLogicEdit
      {
        MdiParent = this
      };
      frmNew.rtfLogic.Text = Logics[5].SourceText;
      frmNew.Show();
    }

    private void btnNewPicture_Click(object sender, EventArgs e)
    {
      frmPicEdit frmNew = new frmPicEdit
      {
        MdiParent = this
      };
      frmNew.Show();
    }

    private void btnOjects_Click(object sender, EventArgs e)
    {
      //let's test object list
      InvObjects.Add("test object 1", 44);
      InvObjects.Add("test object 1", 44);
      InvObjects.Add("test object 1", 44);
      InvObjects.Add("test object 2", 44);

      // change some
      InvObjects[99].Room = 44;
      InvObjects[99].ItemName = "ta da!";
      InvObjects[100].ItemName = "ta da!";
      InvObjects[99].ItemName = "?";

      //then remove some
      InvObjects.Remove(134);
      InvObjects.Remove(132);
      InvObjects.Remove(133);
      InvObjects.Remove(132);
      InvObjects.Remove(131);
    }
    private void btnWords_Click(object sender, EventArgs e)
    {
      // now let's test words
      //for (int i = 0; i <WordList.GroupCount; i++)
      //{
      //  Debug.Print($"Group {WordList.GroupN(i).GroupNum}: {WordList.GroupN(i).GroupName} ({WordList.GroupN(i).WordCount} words)");
      //}

      WordList.AddWord("hoohaa", 12345);
      for (int i = 0; i < WordList.WordCount; i++)
        Debug.Print($"group: {WordList[i].Group}  -- {WordList[i].WordText}");

//      WordList.Clear();
    }
  }
}
