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
    AGIView SelectedView;

    public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, int scale = 1)
    {
      //to scale the picture without blurring, need to use NearestNeighbor interpolation
      // that can't be set directly, so a graphics object is needed to draw the
      // the picture
      int bWidth = agiBMP.Width * scale * 2, bHeight = agiBMP.Height * scale;
      // first, create new image in the picture box that is desired size
      pic.Image = new Bitmap(bWidth, bHeight);
      // intialize a graphics object for the image just created
      using Graphics g = Graphics.FromImage(pic.Image);
      // set correct interpolation mode
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
      // draw the bitmap, at correct resolution
      g.DrawImage(agiBMP, 0, 0, bWidth, bHeight);
    }
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
    private void btnNewSound_Click(object sender, EventArgs e)
    {
      // show editor form
      frmSoundEdit frmNew = new frmSoundEdit
      {
        MdiParent = this
      };
      frmNew.Show();
    }
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      //fill list box with resources for selected type
      if (GameLoaded)
      {
        //lstResources.Anchor = AnchorStyles.None;

        // clear current list
        lstResources.Items.Clear();
        switch (cmbResType.SelectedIndex)
        {
          case 0: //logics
            foreach (AGILogic tmpRes in Logics.Col.Values)
              lstResources.Items.Add(tmpRes);
            break;
          case 1://pictures
            foreach (AGIPicture tmpRes in Pictures.Col.Values)
              lstResources.Items.Add(tmpRes);
            break;
          case 2: //sounds
            foreach (AGISound tmpRes in Sounds.Col.Values)
              lstResources.Items.Add(tmpRes);
            break;
          case 3: //views
            foreach (AGIView tmpRes in Views.Col.Values)
              lstResources.Items.Add(tmpRes);
            break;
        }
      }

    }

    private void lstResources_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (GameLoaded)
      {
        //try to load the selected item
        switch (cmbResType.SelectedIndex)
        {
          case 0: //logic
            ((AGILogic)lstResources.SelectedItem).Load();
            break;
          case 1: //picture
            ((AGIPicture)lstResources.SelectedItem).Load();
            break;
          case 2: //sound
            ((AGISound)lstResources.SelectedItem).Load();
            //double it = Sounds[1].Track[0].Length;
            break;
          case 3: //view
            SelectedView = ((AGIView)lstResources.SelectedItem);
            SelectedView.Load();
            curCel = 0;
            ShowAGIBitmap(picView, SelectedView[0][0].CelBMP, 5);
            timer1.Enabled = (SelectedView[0].Cels.Count > 1);
            break;
        }
      }
    }
    int curCel = 0;
    private void timer1_Tick(object sender, EventArgs e)
    {
      if (SelectedView != null)
      {
        curCel++;
        if (curCel == SelectedView[0].Cels.Count)
        {
          curCel = 0;
        }
        ShowAGIBitmap(picView, SelectedView[0][curCel].CelBMP, 5);
      }
    }
  }
}
