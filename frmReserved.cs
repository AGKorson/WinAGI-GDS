using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Common.Base;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using WinAGI.Common;

namespace WinAGI.Editor {
    public partial class frmReserved : Form {
        Font defaultfont, boldfont;
        ReservedDefineList EditList;
        private char[] invalidall, invalid1st;
        private string s_invalidall, s_invalid1st;

        #region Constructors
        public frmReserved() {
            InitializeComponent();
            if (EditGame == null || !EditGame.SierraSyntax) {
                invalid1st = INVALID_FIRST_CHARS;
                invalidall = INVALID_DEFINE_CHARS;
            }
            else {
                invalidall = INVALID_SIERRA_CHARS;
                invalid1st = INVALID_SIERRA_1ST_CHARS;
            }
            s_invalid1st = new string(invalid1st);
            s_invalidall = new string(invalidall);
        }
        #endregion

        #region Event Handlers
        private void frmReserved_Load(object sender, EventArgs e) {
            defaultfont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            reservedgrid.Font = defaultfont;
            reservedgrid.ColumnHeadersDefaultCellStyle.Font = defaultfont;
            reservedgrid.AlternatingRowsDefaultCellStyle.Font = defaultfont;
            reservedgrid.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            boldfont = new(defaultfont, FontStyle.Bold);
            DataGridViewRow template = new();
            template.CreateCells(reservedgrid);
            template.Cells[0].Value = "";
            template.Cells[1].Value = "";
            reservedgrid.RowTemplate = template;
            if (EditGame == null) {
                EditList = Engine.Base.DefaultReservedDefines;
            }
            else {
                EditList = EditGame.ReservedDefines;
            }
            LoadGrid(EditList);
        }

        private void btnSave_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                // replace any changed defines with new names
                DialogResult rtn = DialogResult.No;
                bool blnDontAsk = false;
                switch (WinAGISettings.AutoUpdateResDefs.Value) {
                case AskOption.Ask:
                    MDIMain.UseWaitCursor = false;
                    // get user's response
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to update all logics with any reserved define names that have been changed?",
                        "Update Logics?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        "Always take this action when saving the reserved defines list.", ref blnDontAsk,
                        WinAGIHelp, "htm\\winagi\\editingdefines.htm#edit");
                    if (blnDontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.AutoUpdateDefines.Value = AskOption.Yes;
                        }
                        else {
                            WinAGISettings.AutoUpdateDefines.Value = AskOption.No;
                        }
                        WinAGISettings.AutoUpdateDefines.WriteSetting(WinAGISettingsFile);
                    }
                    break;
                case AskOption.No:
                    rtn = DialogResult.No;
                    break;
                case AskOption.Yes:
                    rtn = DialogResult.Yes;
                    break;
                }
                string FindText, replacetext, pattern;
                if (rtn == DialogResult.Yes) {
                    MDIMain.UseWaitCursor = true;
                    // step through all defines; if the current name is different than
                    // the original name, use replaceall to make the change
                    ProgressWin = new() {
                        Text = "Updating Reserved Defines",
                    };
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + LogicEditors.Count + 1;
                    ProgressWin.pgbStatus.Value = 0;
                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                    ProgressWin.Show(MDIMain);
                    ProgressWin.Refresh();
                    foreach (frmLogicEdit loged in LogicEditors) {
                        if (loged.FormMode == LogicFormMode.Logic && loged.InGame) {
                            bool textchanged = false;
                            for (int i = 0; i < reservedgrid.RowCount - 1; i++) {
                                // skip header and blank lines
                                if (reservedgrid.Rows[i].Tag != null) {
                                    continue;
                                }
                                ResDefGroup group = (ResDefGroup)reservedgrid[0, i].Tag;
                                int index = (int)reservedgrid[1, i].Tag;
                                // check for name change
                                FindText = EditList.ByGroup(group)[index].Name;
                                replacetext = (string)reservedgrid[0, i].Value;
                                if (FindText != replacetext) {
                                    pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + Regex.Escape(FindText) + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                                    MatchCollection mc = Regex.Matches(loged.fctb.Text, pattern);
                                    if (mc.Count > 0) {
                                        ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                        ProgressWin.Refresh();
                                        for (int m = mc.Count - 1; m >= 0; m--) {
                                            Place pl = loged.fctb.PositionToPlace(mc[m].Index);
                                            AGIToken token = loged.fctb.TokenFromPos(pl);
                                            if (token.Type == AGITokenType.Identifier && token.Text.Length == FindText.Length) {
                                                loged.fctb.ReplaceToken(token, replacetext);
                                                textchanged = true;
                                            }
                                        }
                                        ProgressWin.lblProgress.Text = "Locating modified define names...";
                                        ProgressWin.Refresh();
                                    }
                                }
                                // check for standard arg identifier vfos grp=
                                if (group == ResDefGroup.Variable || group == ResDefGroup.Flag ||
                                    group == ResDefGroup.Object || group == ResDefGroup.String) {
                                    FindText = (string)reservedgrid[1, i].Value;
                                    pattern = $@"\b" + FindText + $@"\b";
                                    MatchCollection mc = Regex.Matches(loged.fctb.Text, pattern);
                                    if (mc.Count > 0) {
                                        ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                        ProgressWin.Refresh();
                                        for (int m = mc.Count - 1; m >= 0; m--) {
                                            Place pl = loged.fctb.PositionToPlace(mc[m].Index);
                                            AGIToken token = loged.fctb.TokenFromPos(pl);
                                            if (token.Type == AGITokenType.Identifier && token.Text.Length == FindText.Length) {
                                                loged.fctb.ReplaceToken(token, replacetext);
                                                textchanged = true;
                                            }
                                        }
                                        ProgressWin.lblProgress.Text = "Locating modified define names...";
                                        ProgressWin.Refresh();
                                    }
                                }
                            }
                            if (textchanged) {
                                // ? who cares for editors...
                            }
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }
                    foreach (Logic logic in EditGame.Logics) {
                        bool unload = !logic.Loaded;
                        bool textchanged = false;
                        // check for deleted define names
                        for (int i = 0; i < reservedgrid.RowCount - 1; i++) {
                            // ignore headers and blanks
                            if (reservedgrid.Rows[i].Tag != null) {
                                continue;
                            }
                            ResDefGroup group = (ResDefGroup)reservedgrid[0, i].Tag;
                            int index = (int)reservedgrid[1, i].Tag;
                            // check for name change
                            FindText = EditList.ByGroup(group)[index].Name;
                            replacetext = (string)reservedgrid[0, i].Value;
                            if (FindText != replacetext) {
                                // load first
                                if (unload) {
                                    logic.Load();
                                }
                                // this pattern ensures only whole tokens are found
                                pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + Regex.Escape(FindText) + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                                MatchCollection mc = Regex.Matches(logic.SourceText, pattern);
                                if (mc.Count > 0) {
                                    ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                    ProgressWin.Refresh();
                                    for (int m = mc.Count - 1; m >= 0; m--) {
                                        AGIToken token = WinAGIFCTB.TokenFromPos(logic.SourceText, mc[m].Index);
                                        if (token.Type == AGITokenType.Identifier && token.Text.Length == FindText.Length) {
                                            logic.SourceText = logic.SourceText.ReplaceFirst(FindText, replacetext, mc[m].Index);
                                            textchanged = true;
                                        }
                                    }
                                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                                    ProgressWin.Refresh();
                                }
                            }
                            // check for standard arg identifier
                            if (group == ResDefGroup.Variable || group == ResDefGroup.Flag ||
                                group == ResDefGroup.Object || group == ResDefGroup.String) {
                                if (unload) {
                                    logic.Load();
                                }
                                FindText = (string)reservedgrid[1, i].Value;
                                pattern = $@"\b" + FindText + $@"\b";
                                MatchCollection mc = Regex.Matches(logic.SourceText, pattern);
                                if (mc.Count > 0) {
                                    ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                    ProgressWin.Refresh();
                                    for (int m = mc.Count - 1; m >= 0; m--) {
                                        AGIToken token = WinAGIFCTB.TokenFromPos(logic.SourceText, mc[m].Index);
                                        if (token.Type == AGITokenType.Identifier && token.Text.Length == FindText.Length) {
                                            logic.SourceText = logic.SourceText.ReplaceFirst(FindText, replacetext, mc[m].Index);
                                            textchanged = true;
                                        }
                                    }
                                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                                    ProgressWin.Refresh();
                                }
                            }
                        }
                        if (textchanged) {
                            logic.SaveSource();
                            //update reslist
                            RefreshTree(AGIResType.Logic, logic.Number);
                        }
                        if (unload) {
                            logic.Unload();
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }
                    ProgressWin.Close();
                    ProgressWin.Dispose();
                    MDIMain.UseWaitCursor = false;
                }
            }
            for (int i = 0; i < reservedgrid.Rows.Count; i++) {
                // skip header and blank rows
                if (reservedgrid.Rows[i].Tag != null) {
                    continue;
                }
                ResDefGroup group = (ResDefGroup)reservedgrid[0, i].Tag;
                int index = (int)reservedgrid[1, i].Tag;
                EditList.ByGroup(group)[index].Name = (string)reservedgrid[0, i].Value;
            }
            EditList.SaveResDefOverrides();
            EditList.SaveList(true);
            Hide();
        }

        private void btnReset_Click(object sender, EventArgs e) {
            for (int i = 0; i < reservedgrid.Rows.Count; i++) {
                // skip header and blank rows
                if (reservedgrid.Rows[i].Tag != null) {
                    continue;
                }
                ResDefGroup group = (ResDefGroup)reservedgrid[0, i].Tag;
                int index = (int)reservedgrid[1, i].Tag;
                if ((string)reservedgrid[0, i].Value != EditList.ByGroup(group)[index].Default) {
                    reservedgrid[0, i].Value = EditList.ByGroup(group)[index].Default;
                }
            }
            btnSave.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            // do nothing
            Hide();
        }

        private void reservedgrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.ColumnIndex != 0 || reservedgrid[0, e.RowIndex].Tag == null) {
                return;
            }
            ResDefGroup group = (ResDefGroup)reservedgrid[0, e.RowIndex].Tag;
            int index = (int)reservedgrid[1, e.RowIndex].Tag;
            if ((string)reservedgrid[0, e.RowIndex].Value != EditList.ByGroup(group)[index].Default) {
                e.CellStyle.Font = boldfont;
                e.CellStyle.ForeColor = Color.Red;
            }
            else {
                e.CellStyle.Font = defaultfont;
                e.CellStyle.ForeColor = Color.Black;
            }
        }

        private void reservedgrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (reservedgrid.IsCurrentCellInEditMode) {
                string checkname = (string)e.FormattedValue;
                string message = "";
                // empty cell means reset to default (this will happen in CellValidated event)
                if (string.IsNullOrEmpty(checkname)) {
                    return;
                }
                // basic checks
                bool sierrasyntax = EditGame != null && EditGame.SierraSyntax;
                DefineNameCheck retval = Common.Base.BaseNameCheck(checkname, sierrasyntax);
                switch (retval) {
                case DefineNameCheck.OK:
                    break;
                //case DefineNameCheck.Empty:
                //    // empty is already handled above
                //    break;
                case DefineNameCheck.Numeric:
                    message = "Define names cannot be numeric.";
                    break;
                case DefineNameCheck.ActionCommand:
                    message = "'" + checkname + "' is an AGI command, and cannot be redefined.";
                    break;
                case DefineNameCheck.TestCommand:
                    message = "'" + checkname + "' is an AGI test command, and cannot be redefined.";
                    break;
                case DefineNameCheck.KeyWord:
                    message = "'" + checkname + "' is a compiler reserved word, and cannot be redefined.";
                    break;
                case DefineNameCheck.ArgMarker:
                    message = "Invalid define name - define names cannot be argument markers.";
                    break;
                case DefineNameCheck.BadChar:
                    string bad1st = "", badchars = "";
                    // build list of the bad chars in the name
                    for (int i = 0; i < invalid1st.Length; i++) {
                        if (invalid1st[i] == checkname[0]) {
                            bad1st = invalid1st[i].ToString();
                            break;
                        }
                    }
                    for (int i = 0; i < invalidall.Length; i++) {
                        for (int j = 1; j < checkname.Length; j++) {
                            if (invalidall[i] == checkname[j]) {
                                if (!badchars.Contains(invalidall[i])) {
                                    badchars += invalidall[i].ToString();
                                }
                                break;
                            }
                        }
                    }
                    if (bad1st.Length > 0) {
                        message = "Invalid starting character in this define name.";
                    }
                    if (badchars.Length > 0) {
                        if (message.Length > 0) {
                            message += "\n\n";
                        }
                        message += "One or more invalid characters in this define ({badchars}).";
                    }
                    if (message.Length == 0) {
                        message = "One or more invalid characters in this define (<= 32 or > 127)";
                    }
                    break;
                }
                if (retval != DefineNameCheck.OK) {
                    // dislay message and cancel
                    MessageBox.Show(MDIMain,
                        message,
                        "Invalid Name",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0 //,
                                                   //WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax"
                    );
                    e.Cancel = true;
                    return;
                }
                // check against existing names in this list
                for (int i = 0; i < reservedgrid.Rows.Count; i++) {
                    // skip header and blank rows
                    if (reservedgrid.Rows[i].Tag != null) {
                        continue;
                    }
                    // skip current row
                    if (i == e.RowIndex) {
                        continue;
                    }
                    if (i != reservedgrid.CurrentRow.Index && reservedgrid[0, i].Value.ToString() == checkname) {
                        message = "'" + checkname + "' is already in use as a global define.";
                        MessageBox.Show(MDIMain,
                            message,
                            "Invalid Name",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, 0, 0 //,
                                                       //WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax"
                        );
                        e.Cancel = true;
                        return;
                    }
                }
                // enable save button if value has changed
                if (checkname != (string)reservedgrid[0, e.RowIndex].Value) {
                    btnSave.Enabled = true;
                    btnReset.Enabled = true;
                }
            }
        }

        private void reservedgrid_CellValidated(object sender, DataGridViewCellEventArgs e) {
            // skip if not editing
            if (reservedgrid.IsCurrentCellInEditMode) {
                // force blanks to default
                if (string.IsNullOrEmpty((string)reservedgrid[0, e.RowIndex].Value)) {
                    reservedgrid[0, e.RowIndex].Value = EditList.ByGroup((ResDefGroup)reservedgrid[0, e.RowIndex].Tag)[(int)reservedgrid[1, e.RowIndex].Tag].Default;
                }
            }
        }

        private void reservedgrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                // force selection
                if (e.RowIndex != -1) {
                    reservedgrid.CurrentCell = reservedgrid[0, e.RowIndex];
                    reservedgrid.CurrentRow.Selected = true;
                    reservedgrid.Refresh();
                }
            }
        }

        private void cmCopy_Click(object sender, EventArgs e) {
            if (reservedgrid.CurrentRow.Tag == null) {
                Clipboard.SetText((string)reservedgrid.CurrentCell.Value);
            }
        }

        private void cmReset_Click(object sender, EventArgs e) {
            // if name is different from default, reset to default
            if ((string)reservedgrid[0, reservedgrid.CurrentRow.Index].Value != EditList.ByGroup((ResDefGroup)reservedgrid[0, reservedgrid.CurrentRow.Index].Tag)[(int)reservedgrid[1, reservedgrid.CurrentRow.Index].Tag].Default) {
                reservedgrid[0, reservedgrid.CurrentRow.Index].Value = EditList.ByGroup((ResDefGroup)reservedgrid[0, reservedgrid.CurrentRow.Index].Tag)[(int)reservedgrid[1, reservedgrid.CurrentRow.Index].Tag].Default;
                btnSave.Enabled = true;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            // ignore if on header or blank row

            if (reservedgrid.CurrentRow.Tag != null) {
                e.Cancel = true;
                return;
            }
            // enable reset if value is different from default
            cmReset.Enabled = (string)reservedgrid[0, reservedgrid.CurrentRow.Index].Value != EditList.ByGroup((ResDefGroup)reservedgrid[0, reservedgrid.CurrentRow.Index].Tag)[(int)reservedgrid[1, reservedgrid.CurrentRow.Index].Tag].Default;
        }
        #endregion

        #region Methods
        private void LoadGrid(ReservedDefineList gridlist) {
            TDefine[] tmpDefines;
            int currentrow;
            bool modified = false;

            EditList = gridlist;
            // load by group, not by data type

            // RESERVED VARIABLES
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Reserved Variables";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ReservedVariables;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.Variable;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // RESERVED FLAGS
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Reserved Flags";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ReservedFlags;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.Flag;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // EDGECODES
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Edge Code Values";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.EdgeCodes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.EdgeCode;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // OBJ DIRECTION
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Obj Direction Values";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ObjDirections;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.ObjectDir;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // VIDEO MODES
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Video Modes";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.VideoModes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.VideoMode;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // COMPUTER TYPES
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Computer Types";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ComputerTypes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.ComputerType;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // COLORS
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Colors";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ColorNames;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.Color;
                reservedgrid[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            reservedgrid.Rows[currentrow].ReadOnly = true;
            // OTHER RESERVED DEFINES
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, Color.Wheat);
            reservedgrid[0, currentrow].Value = "Other Reserved Defines";
            reservedgrid[0, currentrow].Style.Font = boldfont;
            reservedgrid[0, currentrow].ReadOnly = true;
            //ego
            tmpDefines = gridlist.ReservedObjects;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.Object;
                reservedgrid[1, currentrow].Tag = i;
            }
            // input prompt
            tmpDefines = gridlist.ReservedStrings;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.String;
                reservedgrid[1, currentrow].Tag = i;
            }

            // game info (id, version, about, invcount)
            tmpDefines = gridlist.GameInfo;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = reservedgrid.Rows.Add();
                reservedgrid[0, currentrow].Value = tmpDefines[i].Name;
                reservedgrid[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name != tmpDefines[i].Default) {
                    modified = true;
                }
                reservedgrid[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                reservedgrid[0, currentrow].Tag = ResDefGroup.GameInfo;
                reservedgrid[1, currentrow].Tag = i;
            }

            // add a blank row
            currentrow = reservedgrid.Rows.Add();
            reservedgrid.MergeCells(currentrow, reservedgrid.DefaultCellStyle.BackColor);
            // select first item
            reservedgrid[0, 1].Selected = true;
            btnReset.Enabled = modified;
        }

        /*
Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\reservednames.htm#editor"
End Sub
        */
        #endregion
    }

    /// <summary>
    /// A customized version of DataGridView that allows cells in a row to simulate
    /// being merged
    /// </summary>
    public class WinAGIGrid : DataGridView {
        public WinAGIGrid() {

        }

        public void MergeCells(int row, Color rowcolor) {
            if (row < 0 || row >= Rows.Count) {
                return;
            }
            Rows[row].Tag = rowcolor;
            Rows[row].Height = Rows[row].Height + 1;
        }

        public void UnMergeCells(int row) {
            if (row < 0 || row >= Rows.Count) {
                return;
            }
            Rows[row].Tag = null;
            Rows[row].Height = Rows[row].Height - 1;
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0) {
                if (Rows[e.RowIndex].Tag is Color bg) {
                    using (SolidBrush fillBrush = new SolidBrush(bg))
                    using (Pen gridPenColor = new Pen(this.GridColor)) {
                        Rectangle rect2 = new Rectangle(e.CellBounds.Location, e.CellBounds.Size);
                        rect2.X += 1;
                        rect2.Width += 1;
                        rect2.Height -= 1;
                        e.Graphics.FillRectangle(fillBrush, rect2);
                        // draw top and bottom borders
                        Point p1, p2, p3, p4;
                        p1 = p2 = p3 = p4 = e.CellBounds.Location;
                        p1.Y -= 1;
                        p2.Offset(e.CellBounds.Size.Width - 1, -1);
                        p3.Offset(0, e.CellBounds.Size.Height - 1);
                        p4.Offset(e.CellBounds.Size.Width - 1, e.CellBounds.Size.Height - 1);
                        e.Graphics.DrawLine(gridPenColor, p1, p2);
                        e.Graphics.DrawLine(gridPenColor, p3, p4);
                        if (e.ColumnIndex == 0) {
                            // draw left border
                            e.Graphics.DrawLine(gridPenColor, p1, p3);
                        }
                        else if (e.ColumnIndex == 1) {
                            // draw right border
                            e.Graphics.DrawLine(gridPenColor, p2, p4);
                        }
                    }
                    // output cell text
                    e.PaintContent(e.CellBounds);
                    e.Handled = true;
                    return;
                }
            }
            base.OnCellPainting(e);
        }
    }
}
