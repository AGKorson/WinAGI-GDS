using System;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.Base.GetRes;

namespace WinAGI.Editor {
    public partial class frmGetResourceNum : Form {
        #region Fields
        public AGIResType ResType;
        public byte NewResNum;
        public byte OldResNum;
        public bool DontImport = false;
        public GetRes WindowFunction;
        #endregion

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

        public frmGetResourceNum(GetRes function, AGIResType restype, string id) {
            InitializeComponent();
            ResType = restype;
            OldResNum = 0;
            FormSetup(function);
            txtID.Text = id;
        }
        #endregion

        #region Event Handlers
        private void frmGetResourceNum_FormClosed(object sender, FormClosedEventArgs e) {
            // save the current 'opennew' value
            WinAGISettingsFile.WriteSetting(sGENERAL, "OpenNew", (chkOpenRes.Checked == true));
        }

        private void frmGetResourceNum_HelpRequested(object sender, HelpEventArgs hlpevent) {
            string topic = @"htm\winagi\managingresources.htm";
            switch (WindowFunction) {
            case AddNew:
                topic = @"htm\winagi\resource_new.htm";
                break;
            case Renumber:
                topic += "#resnum";
                break;
            case Open:
                topic = @"htm\winagi\resource_open.htm";
                break;
            case TestView:
                topic = @"htm\winagi\viewtestmode.htm#testview";
                break;
            case AddLayout:
                topic = @"htm\winagi\editinglayouts.htm#addrooms";
                break;
            case ShowRoom:
                topic = @"htm\winagi\editinglayouts.htm#showhide";
                break;
            case Import:
                topic = @"htm\winagi\resource_import.htm";
                break;
            case MenuBkgd:
                topic = @"htm\winagi\editor_menu.htm#preview";
                break;
            case TextBkgd:
                topic = @"htm\winagi\editor_textscreen.htm#background";
                break;
            case AddInGame:
                topic = @"htm\winagi\resource_addremove.htm";
                break;
            case GetRes.RenumberRoom:
                topic = @"htm\winagi\managingresources.htm#resnum";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
            hlpevent.Handled = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            string errorMsg = "";
            // validate resnum
            NewResNum = ((ListItemData)lstResNum.SelectedItem).ResNum;
            switch (WindowFunction) {
            case Open:
            case ShowRoom:
                // nothing else do to
                DialogResult = DialogResult.OK;
                break;
            case Renumber:
            case GetRes.RenumberRoom:
                if (OldResNum == NewResNum) {
                    // same as canceling
                    DialogResult = DialogResult.Cancel;
                    Hide();
                }
                DialogResult = DialogResult.OK;
                break;
            case AddNew:
            case AddInGame:
            case AddLayout:
            case Import:
                // validate resourceID (use impossible value for old ID to avoid matching
                DefineNameCheck rtn = ValidateID(txtID.Text, 255.ToString());
                switch (rtn) {
                case DefineNameCheck.OK:
                    break;
                case DefineNameCheck.Empty:
                    errorMsg = "Resource ID cannot be blank.";
                    break;
                case DefineNameCheck.Numeric:
                    errorMsg = "Resource IDs cannot be numeric.";
                    break;
                case DefineNameCheck.ActionCommand:
                    errorMsg = "'" + txtID.Text + "' is an AGI command, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.TestCommand:
                    errorMsg = "'" + txtID.Text + "' is an AGI test command, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.KeyWord:
                    errorMsg = "'" + txtID.Text + "' is a compiler reserved word, and cannot be used as a resource ID.";
                    break;
                case DefineNameCheck.ArgMarker:
                    errorMsg = "Resource IDs cannot be argument markers";
                    break;
                case DefineNameCheck.BadChar:
                    errorMsg = "Invalid character in resource ID";
                    break;
                case DefineNameCheck.ResourceID:
                    errorMsg = "'" + txtID.Text + "' is already in use as a resource ID.";
                    break;
                }
                if (rtn != DefineNameCheck.OK) {
                    MDIMain.MsgBoxWithHelp(
                        errorMsg,
                        "Invalid Resource ID",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\winagi\\Managing Resources.htm#resourceids");
                    // send user back to the form to try again
                    txtID.Select();
                    return;
                }
                else {
                    // OK to exit
                    DialogResult = DialogResult.OK;
                }
                break;
            case TestView:
                // OK to exit
                DialogResult = DialogResult.OK;
                break;
            case MenuBkgd:
                // OK to exit
                DialogResult = DialogResult.OK;
                break;
            case TextBkgd:
                // OK to exit
                DialogResult = DialogResult.OK;
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
            switch (WindowFunction) {
            case AddLayout:
            case AddNew:
            case AddInGame:
            case Import:
                btnOK.Enabled = txtID.Text.Length != 0 && lstResNum.SelectedIndex != -1;
                // if format is 'restype & resnum' (i.e., "Logic0") OR is blank
                if (txtID.Text.Length == 0 || txtID.Text.Left(ResType.ToString().Length).Equals(ResType.ToString(), StringComparison.OrdinalIgnoreCase)) {
                    if (lstResNum.SelectedItem is not null) {
                        txtID.Text = ResType.ToString() + lstResNum.Text;
                    }
                    else {
                        txtID.Text = "";
                    }
                }
                // never allow adding pic for room 0!
                if (lstResNum.SelectedItem is not null && ((ListItemData)lstResNum.SelectedItem).ResNum == 0) {
                    chkIncludePic.Enabled = false;
                    chkIncludePic.Checked = false;
                    chkRoom.Enabled = false;
                    chkRoom.Checked = false;
                }
                else {
                    chkIncludePic.Enabled = chkRoom.Checked;
                    chkRoom.Enabled = WindowFunction != AddLayout;
                }
                break;
            default:
                btnOK.Enabled = lstResNum.SelectedIndex != -1;
                break;
            }
        }

        private void lstResNum_MouseDoubleClick(object sender, MouseEventArgs e) {
            btnOK.PerformClick();
        }

        private void chkRoom_CheckedChanged(object sender, EventArgs e) {
            chkIncludePic.Enabled = chkRoom.Checked;
            if (!chkIncludePic.Enabled) {
                chkIncludePic.Checked = false;
            }
            UpdateSelectionList(chkRoom.Checked);
        }

        private void chkIncludePic_CheckedChanged(object sender, EventArgs e) {
            if (WindowFunction == GetRes.RenumberRoom) {
                UpdateSelectionList(chkIncludePic.Checked);
            }
        }

        private void txtID_TextChanged(object sender, EventArgs e) {
            // enable ok only if id is NOT null AND a valid number is selected
            btnOK.Enabled = txtID.Text.Length != 0 && lstResNum.SelectedIndex != -1;
        }

        private void txtID_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true; // Prevents the 'ding'
                btnOK.PerformClick();      // Triggers the OK button
            }
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

        #region Methods
        public void FormSetup(GetRes function) {
            int i;
            ListItemData tmpItem;
            WindowFunction = function;
            // clear the listbox
            lstResNum.Items.Clear();

            switch (WindowFunction) {
            case AddNew:
            case AddInGame:
            case Renumber:
            case GetRes.RenumberRoom:
            case AddLayout:
            case Import:
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
                        if (!EditGame.Logics.Contains(i) &&
                            (WindowFunction != AddLayout || i != 0) &&
                            (WindowFunction != GetRes.RenumberRoom || !EditGame.Pictures.Contains(i))) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        // (added logics assume picture is NOT being included, i.e. 
                        // chkIncludePic.Checked = false, which is the default state)
                        break;
                    case AGIResType.View:
                        if (!EditGame.Views.Contains(i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Picture:
                        if (!EditGame.Pictures.Contains(i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (!EditGame.Sounds.Contains(i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case Open or TestView or MenuBkgd or TextBkgd:
                // only add resources that are in game
                for (i = 0; i < 256; i++) {
                    tmpItem = new() {
                        ResNum = (byte)i
                    };
                    switch (ResType) {
                    case AGIResType.Logic:
                        if (EditGame.Logics.Contains(i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }

                        break;
                    case AGIResType.Picture:
                        if (EditGame.Pictures.Contains(i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Pictures[i], true)
                                : i + " - " + EditGame.Pictures[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (EditGame.Sounds.Contains(i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Sounds[i], true)
                                : i + " - " + EditGame.Sounds[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.View:
                        if (EditGame.Views.Contains(i)) {
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
                    if (EditGame.Logics.Contains(i)) {
                        if (!EditGame.Logics[i].IsRoom) {
                            tmpItem.ID = WinAGISettings.ShowResNum.Value
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                    }
                }
                if (lstResNum.Items.Count == 0) {
                    MDIMain.MsgBoxWithHelp(
                        "All logics in the game are currently tagged as rooms and are visible.",
                        "Show Room",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\winagi\\editor_layout.htm#isroom");
                    // set NewResNum to a value so the calling function knows 
                    NewResNum = 255;
                    return;
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
                Text = "Add New " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnDont.Text = "New Not in Game";
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic && !EditGame.SierraSyntax) {
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
                Text = "Renumber ";
                switch (ResType) {
                case AGIResType.Logic:
                    Text += ResourceName(EditGame.Logics[OldResNum], true, true);
                    break;
                case AGIResType.Picture:
                    Text += ResourceName(EditGame.Pictures[OldResNum], true, true);
                    break;
                case AGIResType.Sound:
                    Text += ResourceName(EditGame.Sounds[OldResNum], true, true);
                    break;
                case AGIResType.View:
                    Text += ResourceName(EditGame.Views[OldResNum], true, true);
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
                Text = "Open " + ResType.ToString();
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
                Text = "Select Test View";
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
                Text = "Add New Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new room:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                chkIncludePic.Visible = true;
                // change default to include pics; this is most likely what user wants
                chkIncludePic.Checked = true;
                chkRoom.Visible = true;
                chkRoom.Checked = true;
                chkRoom.Enabled = true;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case ShowRoom:
                Width = 244;
                Text = "Show Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a room to show:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case Import:
                Text = "Import " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this imported " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic && !EditGame.SierraSyntax) {
                    chkIncludePic.Visible = true;
                    chkRoom.Checked = true;
                    chkRoom.Visible = true;
                    chkRoom.Enabled = true;
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
                Text = "Menu Background";
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
            case TextBkgd:
                Width = 244;
                Text = "Text Screen Background";
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
                Text = "Add " + ResType.ToString();
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
                Text = "Renumber " + ResourceName(EditGame.Logics[OldResNum], true, true);
                lblCurrent.Text = OldResNum.ToString();
                Label1.Text = "New number for this Room:";
                chkIncludePic.Left = 8;
                chkIncludePic.Top = 161;
                // change include pic checkbox to renumber
                chkIncludePic.Visible = true;
                chkIncludePic.Checked = true;
                chkIncludePic.Text = "Also renumber room picture";
                lstResNum.Height = 90;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            }
        }

        private void UpdateSelectionList(bool rooms) {
            // reload the available numbers list based on selection
            if (rooms) {
                // remove unused logic numbers that have pictures
                for (int i = lstResNum.Items.Count - 1; i >= 0; i--) {
                    if (EditGame.Pictures.Contains(((ListItemData)lstResNum.Items[i]).ResNum)) {
                        lstResNum.Items.RemoveAt(i);
                    }
                }
            }
            else {
                // add unused logic numbers that have pictures
                int pos = 0;
                for (int i = 0; i < 256; i++) {
                    // if this logic is not ingame, and has a picture of 
                    // same index, add it
                    if (!EditGame.Logics.Contains(i)) {
                        if (EditGame.Pictures.Contains(i)) {
                            lstResNum.Items.Insert(pos, new ListItemData() { ResNum = (byte)i, ID = i.ToString() });
                        }
                        pos++;
                    }
                }
            }
        }
        #endregion

        class ListItemData {
            public byte ResNum = 0;
            public string ID = "";
            public override string ToString() {
                return ID;
            }
        }
    }
}
