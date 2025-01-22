using System;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using WinAGI.Engine;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Editor.Base.GetRes;
using static WinAGI.Common.Base;
using System.Windows.Forms.Design;
using WinAGI.Common;

namespace WinAGI.Editor {
    public partial class frmGetResourceNum : Form {
        public AGIResType ResType;
        public byte NewResNum;
        public byte OldResNum;
        public bool DontImport = false;
        public GetRes WindowFunction;

        class ListItemData {
            public byte ResNum = 0;
            public string ID = "";
            public override string ToString() { return ID; }
        }

        #region Constructors
        public frmGetResourceNum(GetRes function, AGIResType restype) {
            InitializeComponent();
            ResType = restype;
            OldResNum = 0;
            FormSetup(function);
        }

        public frmGetResourceNum(GetRes function, AGIResType restype, byte resnum) {
            InitializeComponent();
            ResType = restype;
            OldResNum = resnum;
            FormSetup(function);
        }
        #endregion

        #region Event Handlers
        private void frmGetResourceNum_FormClosed(object sender, FormClosedEventArgs e) {
            // save the current 'opennew' value
            WinAGISettingsFile.WriteSetting(sGENERAL, "OpenNew", (chkOpenRes.Checked == true));
        }

        private void btnOK_Click(object sender, EventArgs e) {
            string strErrMsg = "";
            // validate resnum
            NewResNum = ((ListItemData)lstResNum.SelectedItem).ResNum;
            switch (WindowFunction) {
            case Open:
                // nothing else do to
                DialogResult = DialogResult.OK;
                break;
            case Renumber or GetRes.RenumberRoom:
                if (OldResNum == NewResNum) {
                    // same as canceling
                    DialogResult = DialogResult.Cancel;
                    Hide();
                }
                break;
            case AddNew or AddInGame or Import:
                // validate resourceID (use impossible value for old ID to avoid matching
                DefineNameCheck rtn = ValidateID(txtID.Text, 255.ToString());
                switch (rtn) {
                case DefineNameCheck.OK:
                    break;
                case DefineNameCheck.Empty:
                    strErrMsg = "Resource ID cannot be blank.";
                    break;
                case DefineNameCheck.Numeric:
                    strErrMsg = "Resource IDs cannot be numeric.";
                    break;
                case DefineNameCheck.ActionCommand:
                    strErrMsg = "'" + txtID.Text + "' is an AGI command, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.TestCommand:
                    strErrMsg = "'" + txtID.Text + "' is an AGI test command, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.KeyWord:
                    strErrMsg = "'" + txtID.Text + "' is a compiler reserved word, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.ArgMarker:
                    strErrMsg = "Resource IDs cannot be argument markers";
                    break;
                case DefineNameCheck.BadChar:
                    strErrMsg = "Invalid character in resource ID";
                    break;
                case DefineNameCheck.ResourceID:
                    strErrMsg = "'" + txtID.Text + "' is already in use as a resource ID.";
                    break;
                }
                if (rtn != DefineNameCheck.OK) {
                    // error - show msgbox
                    MessageBox.Show(strErrMsg, "Invalid Resource ID", MessageBoxButtons.OK, MessageBoxIcon.Information); // vbMsgBoxHelpButton, WinAGIHelp, "htm\winagi\Managing Resources.htm#resourceids"
                    // send user back to the form to try again
                    txtID.Select();
                    return;
                }
                else {
                    // OK to exit
                    DialogResult = DialogResult.OK;
                }
                break;
            default:
                // TODO: hmm, what would get us here?
                DialogResult = DialogResult.Cancel;
                break;
            }
            // done
            Hide();
        }

        private void btnDont_Click(object sender, EventArgs e) {
            // not canceled, but not importing
            DontImport = true;
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void lstResNum_SelectedIndexChanged(object sender, EventArgs e) {
            byte resnum = ((ListItemData)lstResNum.SelectedItem).ResNum;
            switch (WindowFunction) {
            case Renumber:
            case Open:
            case TestView:
            case ShowRoom:
            case GetRes.Menu:
            case MenuBkgd:
            case GetRes.RenumberRoom:
                btnOK.Enabled = lstResNum.SelectedIndex != -1;
                break;
            default:
                break;
            }
            switch (WindowFunction) {
            case AddLayout or AddNew or AddInGame or Import:
                btnOK.Enabled = txtID.Text.Length != 0 && lstResNum.SelectedIndex != -1;
                // if format is 'restype & resnum' (i.e., "Logic0") OR is blank
                if (txtID.Text.Length == 0 || txtID.Text.Left(ResType.ToString().Length).Equals(ResType.ToString(), StringComparison.OrdinalIgnoreCase)) {
                    txtID.Text = ResType.ToString() + lstResNum.Text;
                }
                // if adding a room (logic, with matching pic) check for existing pic
                if (WindowFunction == AddLayout || ((WindowFunction == AddNew || WindowFunction == Import) && ResType == AGIResType.Logic)) {
                    if (EditGame.Pictures.Contains(resnum)) {
                        // change checkbox text
                        chkIncludePic.Text = "Replace existing Picture";
                        // UNCHECK, this forces user to be certain they want this option
                        chkIncludePic.Checked = false;
                    }
                    else {
                        chkIncludePic.Text = "Create matching Picture";
                    }
                }
                // never allow adding pic for room 0!
                if (resnum == 0) {
                    chkIncludePic.Enabled = false;
                    chkIncludePic.Checked = false;
                    chkRoom.Enabled = false;
                    chkRoom.Checked = false;
                }
                else {
                    chkIncludePic.Enabled = chkRoom.Checked;
                    chkRoom.Enabled = (WindowFunction != AddLayout);
                }
                break;
            default:
            //case grRenumber:
            //case grOpen:
            //case grTestView:
            //case grShowRoom:
            //case grMenu:
            //case grMenuBkgd:
            //case grRenumberRoom:
                btnOK.Enabled = lstResNum.SelectedIndex != -1;
                break;
            }
        }

        private void chkRoom_CheckedChanged(object sender, EventArgs e) {
            chkIncludePic.Enabled = chkRoom.Checked;
            if (!chkIncludePic.Enabled) {
                chkIncludePic.Checked = false;
            }
        }

        private void chkIncludePic_CheckedChanged(object sender, EventArgs e) {
            // no action
        }

        private void txtID_TextChanged(object sender, EventArgs e) {
            // enable ok only if id is NOT null AND a valid number is selected
            btnOK.Enabled = txtID.Text.Length != 0 && lstResNum.SelectedIndex != -1;
        }

        private void txtID_KeyPress(object sender, KeyPressEventArgs e) {
            // some characters not allowed:
            // NOT OK  !"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~
            //     OK    #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    

            switch ((int)e.KeyChar) {
            case 8 or 13:
                // backspace or enter OK
                break;
            case <= 34 or (>= 38 and <= 45) or 47 or (>= 58 and <= 63) or (>= 91 and <= 94) or 96 or >= 123:
                e.Handled = true;
                break;
            }
        }
        #endregion

        public void FormSetup(GetRes function) {
            int i;
            ListItemData tmpItem;
            WindowFunction = function;
            //clear the listbox
            lstResNum.Items.Clear();

            switch (WindowFunction) {
            case AddNew:
            case AddInGame:
            case Renumber:
            case GetRes.RenumberRoom:
            case AddLayout:
            case Import:
                // if adding rooms in layout, always SUGGEST include pic
                if (WindowFunction == AddLayout) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Enabled = true;
                }
                // only add  resource numbers that are NOT in use
                for (i = 0; i < 256; i++) {
                    tmpItem = new ListItemData {
                        ResNum = (byte)i,
                        ID = i.ToString()
                    };
                    switch (ResType) {
                    case AGIResType.Logic:
                        // if exists, don't add
                        // if grAddLayout AND i == 0, don't add
                        // if grRenumberRoom AND Picture exists, don't add
                        if (!EditGame.Logics.Contains((byte)i) &&
                            (WindowFunction != AddLayout || i != 0) &&
                            (WindowFunction != GetRes.RenumberRoom || !EditGame.Pictures.Contains((byte)i))) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.View:
                        if (!EditGame.Views.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Picture:
                        if (!EditGame.Pictures.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (!EditGame.Sounds.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case Open or TestView or GetRes.Menu or MenuBkgd:
                // only add resources that are in game
                for (i = 0; i < 256; i++) {
                    tmpItem = new() {
                        ResNum = (byte)i
                    };
                    switch (ResType) {
                    case AGIResType.Logic:
                        if (EditGame.Logics.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }

                        break;
                    case AGIResType.Picture:
                        if (EditGame.Pictures.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Pictures[i], true)
                                : i + " - " + EditGame.Pictures[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (EditGame.Sounds.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Sounds[i], true)
                                : i + " - " + EditGame.Sounds[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.View:
                        if (EditGame.Views.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Views[i], true)
                                : i + " - " + EditGame.Views[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case ShowRoom:
                // displays all logics in game which do NOT have InRoom=true
                for (i = 1; i < 256; i++) {
                    tmpItem = new() {
                        ResNum = (byte)i
                    };
                    if (EditGame.Logics.Contains((byte)i)) {
                        if (!EditGame.Logics[i].IsRoom) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                    }
                }
                if (lstResNum.Items.Count == 0) {
                    MessageBox.Show(MDIMain, "All logics in the game are currently tagged as rooms and are visible.", "Show Room", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0, WinAGIHelp, "htm\\winagi\\Managing_Resources.htm#resourceids");
                    Hide();
                }
                break;
            }
            AdjustControls();
            lstResNum.Select();
        }

        private void AdjustControls() {

            // shows/hides form controls and adjusts
            // form height and control positions based on window function

            // defaults when form is initially loaded:
            //    size wide (6645)
            //    Label2/lblCurrent are visible
            //    Label1 and lstResNum in lower position (825/1065/1425)
            //    chkRoom, chkPic, chkOpenRes are not visible
            //    cmdDont is not visible
            //    cmdCancel is visible at left position, with caption of 'cancel' (1620)
            //    form caption needs to be set!
            //    list caption (Label1) needs to be set!
            //    cmdOK disabled

            switch (WindowFunction) {
            case AddNew:
                base.Text = "Add New " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnDont.Text = "New Not in Game";
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Visible = true;
                    chkIncludePic.Enabled = false;
                    chkRoom.Checked = true;
                    chkRoom.Visible = true;
                    chkRoom.Enabled = true;
                    chkRoom.Text = "Include Room Template Code";
                }
                else {
                    chkIncludePic.Visible = false;
                    chkRoom.Visible = false;
                }
                chkOpenRes.Visible = true;
                chkOpenRes.Checked = WinAGISettings.OpenNew.Value;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case Renumber:
                Width = 244;
                base.Text = "Renumber ";
                switch (ResType) {
                case AGIResType.Logic:
                    base.Text += ResourceName(EditGame.Logics[OldResNum], true, true);
                    break;
                case AGIResType.Picture:
                    base.Text += ResourceName(EditGame.Pictures[OldResNum], true, true);
                    break;
                case AGIResType.Sound:
                    base.Text += ResourceName(EditGame.Sounds[OldResNum], true, true);
                    break;
                case AGIResType.View:
                    base.Text += ResourceName(EditGame.Views[OldResNum], true, true);
                    break;
                }
                lblCurrent.Text = OldResNum.ToString();
                Label1.Text = "Select a new number for this " + ResType.ToString() + ":";
                lstResNum.Height = 105;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case Open:
                Width = 244;
                base.Text = "Open " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select the " + ResType.ToString() + " to open:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case TestView:
                Width = 244;
                base.Text = "Select Test View";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a view for testing pictures:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // select current test view, if there is one
                lstResNum.SelectedIndex = -1;
                for (int i = 0; i < lstResNum.Items.Count; i++) {
                    if (((ListItemData)lstResNum.Items[i]).ResNum == OldResNum) {
                        lstResNum.SelectedIndex = i;
                        break;
                    }
                }
                break;
            case AddLayout:
                base.Text = "Add New Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new room:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                chkIncludePic.Visible = true;
                chkIncludePic.Checked = true;
                chkRoom.Visible = true;
                chkRoom.Checked = true;
                chkRoom.Enabled = false;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case ShowRoom:
                Width = 244;
                base.Text = "Show Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a room to show:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case GetRes.Menu:
                Width = 244;
                base.Text = "Extract Menu";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select the logic with the game's menu:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case Import:
                base.Text = "Import " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this imported " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic) {
                    chkIncludePic.Visible = false;
                    chkRoom.Checked = true;
                    chkRoom.Visible = true;
                    chkRoom.Enabled = false;
                    chkRoom.Top = 156;
                    chkRoom.Text = "Import as a Room";
                    txtDescription.Height = 70;
                }
                else {
                    chkIncludePic.Visible = false;
                    chkRoom.Visible = false;
                }
                chkOpenRes.Visible = true;
                chkOpenRes.Checked = WinAGISettings.OpenNew.Value;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case MenuBkgd:
                Width = 244;
                base.Text = "Menu Background";
                lblCurrent.Text = OldResNum.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select picture to use as background:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // select the current pic
                lstResNum.SelectedIndex = -1;
                for (int i = 0; i < lstResNum.Items.Count; i++) {
                    if (((ListItemData)lstResNum.Items[i]).ResNum == OldResNum) {
                        lstResNum.SelectedIndex = i;
                        break;
                    }
                }
                break;
            case AddInGame:
                base.Text = "Add " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this " + ResType.ToString() + ":";
                lstResNum.Top = 32;
                lstResNum.Height = 135;
                btnDont.Visible = false;
                btnCancel.Left = 382;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case GetRes.RenumberRoom:
                Width = 244;
                base.Text = "Renumber " + ResourceName(EditGame.Logics[OldResNum], true, true);
                lblCurrent.Text = OldResNum.ToString();
                Label1.Text = "New number for this Room:";
                chkIncludePic.Left = 8;
                chkIncludePic.Top = 161;
                chkIncludePic.Visible = true;
                chkIncludePic.Checked = true;
                chkIncludePic.Text = "Also renumber room picture";
                lstResNum.Height = 90;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            }
        }

    }
}
