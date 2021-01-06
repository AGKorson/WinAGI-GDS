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
using static WinAGI.AGISound;
using static WinAGI.WinAGI;

namespace WinAGI_GDS
{
  public partial class frmSoundEdit : Form
  {
    public frmSoundEdit()
    {
      InitializeComponent();
    }

    private void frmSoundEdit_Load(object sender, EventArgs e)
    {
        // if  a game is loaded, list all sounds by id
        if (GameLoaded)
      {
        //foreach (AGILogic tmpLog in agLogs.Col.Values)
        //{
        //  listBox1.Items.Add(tmpLog.ToString());
        //}
        //foreach (AGIPicture tmpPic in agPics.Col.Values)
        //{
        //  listBox1.Items.Add(tmpPic.ToString());
        //}
        foreach (AGISound tmpSound in agSnds.Col.Values)
        {
          listBox1.Items.Add(tmpSound);
        }
        //foreach (AGIView tmpView in agViews.Col.Values)
        //{
        //  listBox1.Items.Add(tmpView.ToString());
        //}
      }
    }

    private void listBox1_DoubleClick(object sender, EventArgs e)
    {
      // let's load it

      AGISound tmpSnd = (AGISound)listBox1.SelectedItem;
      tmpSnd.Load();
      tmpSnd.SoundComplete += This_SoundComplete;
      tmpSnd.PlaySound();
    }
    private void This_SoundComplete(object sender, SoundCompleteEventArgs e)
    {
      MessageBox.Show("all done!");
    }
  }
}
