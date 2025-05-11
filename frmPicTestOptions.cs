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
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.PictureUndo.ActionType;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.API;
using static WinAGI.Editor.frmPicEdit;

namespace WinAGI.Editor {
    public partial class frmPicTestOptions : Form {
        private int LoopCount;
        private int[] CelCount;
        public PicTestInfo TestInfo;

        public frmPicTestOptions(Engine.View testview, PicTestInfo testinfo) {
            InitializeComponent();
            LoopCount = testview.Loops.Count;
            CelCount = new int[LoopCount];
            TestInfo = testinfo.Clone();

            for (int i = 0; i < LoopCount; i++) {
                CelCount[i] = testview[i].Cels.Count;
            }
            // build loop and cel dropdown lists
            cmbLoop.Items.Add("Auto");
            for (int i = 0; i < LoopCount; i++) {
                cmbLoop.Items.Add("Loop " + i.ToString());
            }
            cmbCel.Items.Add("Auto");
            if (testinfo.TestLoop < testview.Loops.Count && testinfo.TestLoop != -1) {
                for (int i = 0; i < CelCount[testinfo.TestLoop]; i++) {
                    cmbCel.Items.Add("Cel " + i.ToString());
                }
            }
            else {
                // loop is invalid; reset to auto
                testinfo.TestLoop = -1;
            }
            cmbLoop.SelectedIndex = testinfo.TestLoop + 1;
            if (testinfo.TestLoop >= 0 && testinfo.TestCel >= 0) {
                if (testinfo.TestCel >= CelCount[testinfo.TestLoop]) {
                    // cel is invalid; reset to auto
                    testinfo.TestCel = -1;
                }
                cmbCel.SelectedIndex = testinfo.TestCel + 1;
            }
            cmbCel.Enabled = testinfo.TestLoop != -1;
            // set form controls
            cmbSpeed.SelectedIndex = testinfo.ObjSpeed.Value;
            cmbPriority.SelectedIndex = 16 - testinfo.ObjPriority.Value;
            optAnything.Checked = testinfo.ObjRestriction.Value == 0;
            optWater.Checked = testinfo.ObjRestriction.Value == 1;
            optLand.Checked = testinfo.ObjRestriction.Value == 2;
            txtHorizon.Text = testinfo.Horizon.Value.ToString();
            chkIgnoreHorizon.Checked = testinfo.IgnoreHorizon.Value;
            chkIgnoreBlocks.Checked = testinfo.IgnoreBlocks.Value;
            chkCycleAtRest.Checked = testinfo.CycleAtRest.Value;
            chkCycleAtRest.Enabled = testinfo.TestCel == -1;
        }

        #region temp
        /*
private void Form_KeyDown(int KeyCode, int Shift) {
  // check for help key
  If Shift = 0 And KeyCode = vbKeyF1 Then
    // help with picoptions
    Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\winagi\PictureOptions.htm");
    KeyCode = 0
  End If
}
        */
        #endregion

        #region Event Handlers
        private void cmbLoop_SelectedIndexChanged(object sender, EventArgs e) {
            if (cmbLoop.SelectedIndex == 0) {
                // reset cel to 0
                cmbCel.SelectedIndex = 0;
            }
            else {
                // reload cel dropdown list
                cmbCel.Items.Clear();
                cmbCel.Items.Add("Auto");
                for (int i = 0; i < CelCount[cmbLoop.SelectedIndex - 1]; i++) {
                    cmbCel.Items.Add("Cel " + i.ToString());
                }
            }
            cmbCel.Enabled = cmbLoop.SelectedIndex != 0;
            cmbCel.SelectedIndex = 0;
            chkCycleAtRest.Enabled = true;
        }

        private void cmbCel_SelectedIndexChanged(object sender, EventArgs e) {
            // enable cycle checkbox
            chkCycleAtRest.Enabled = cmbCel.SelectedIndex == 0;
        }

        private void cmdOK_Click(object sender, EventArgs e) {
            TestInfo.ObjSpeed.Value = cmbSpeed.SelectedIndex;
            TestInfo.ObjPriority.Value = 16 - cmbPriority.SelectedIndex;
            // PicTest.ObjRestriction: 0 = no restriction, 1 = restrict to water, 2 = restrict to land
            if (optAnything.Checked) {
                TestInfo.ObjRestriction.Value = 0;
            }
            else if (optLand.Checked) {
                TestInfo.ObjRestriction.Value = 2;
            }
            else {
                TestInfo.ObjRestriction.Value = 1;
            }
            TestInfo.Horizon.Value = int.Parse(txtHorizon.Text);
            TestInfo.IgnoreHorizon.Value = chkIgnoreHorizon.Checked;
            TestInfo.IgnoreBlocks.Value = chkIgnoreBlocks.Checked;
            TestInfo.CycleAtRest.Value = chkCycleAtRest.Checked;
            TestInfo.TestLoop = cmbLoop.SelectedIndex - 1;
            TestInfo.TestCel = cmbCel.SelectedIndex - 1;

            DialogResult = DialogResult.OK;
            Hide();
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void txtHorizon_Validating(object sender, CancelEventArgs e) {
            if (txtHorizon.Text.Length == 0) {
                txtHorizon.Value = PicEditTestSettings.Horizon.Value;
            }
        }
        #endregion

        #region Methods
        #endregion
    }
}
