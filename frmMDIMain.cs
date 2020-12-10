using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI_GDS
{
    public partial class frmMDIMain : Form
    {
        public frmMDIMain()
        {
            InitializeComponent();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // how do you know what was selected???
            //System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            //messageBoxCS.AppendFormat("{0} = {1}", "Clicked Item", e.ClickedItem);
            //messageBoxCS.AppendLine();
            //MessageBox.Show(messageBoxCS.ToString(), "ItemClicked Event");

            switch (e.ClickedItem.Name)
            {
                case "newToolStripButton":
                    MessageBox.Show("you clicked New", "Toolbar Click");
                    //frmLogicEdit

                    frmLogicEdit FormNew = new frmLogicEdit();
                    FormNew.MdiParent = this;
                    FormNew.Show();

                    break;
                default:
                    break;
            }
        }
    }
}
