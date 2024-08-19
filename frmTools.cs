using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmTools : Form {
        int sourcerow;
        bool dragging = false;

        public frmTools() {
            InitializeComponent();
        }

        private void frmTools_Load(object sender, EventArgs e) {
            int i;
            // set font
            fgTools.Font = new Font(WinAGISettings.EFontName, WinAGISettings.EFontSize);

            //set form height so all rows are visible
            //this.Height = ?;

            for (i = 1; i <= 6; i++) {
                fgTools.Rows.Add(
                    i.ToString() + ".",
                    WinAGISettingsList.GetSetting(sTOOLS, "Caption" + i, ""),
                    WinAGISettingsList.GetSetting(sTOOLS, "Source" + i, "")
                );
            }
            // select column 1
            fgTools.CurrentCell = fgTools[1, 0];
            fgTools.Select();
            //set browse button so it fits inside a row
            btnBrowse.Height = fgTools.Rows[0].Height - 1;
        }

        private void fgTools_KeyDown(object sender, KeyEventArgs e) {
            if (fgTools.SelectedRows.Count == 1) {
                // entire row can be deleted
                if (e.KeyCode == Keys.Delete) {
                    //erase this row
                    fgTools[1, fgTools.CurrentCell.RowIndex].Value = "";
                    fgTools[2, fgTools.CurrentCell.RowIndex].Value = "";
                }
            }
            else {
                // individual cells can be edited
                if (e.KeyCode == Keys.Return) {
                    // if not already editing, do so
                    if (!fgTools.IsCurrentCellInEditMode && fgTools.CurrentCell.ColumnIndex > 0) {
                        e.Handled = true;
                        fgTools.BeginEdit(true);
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            bool blnTools = false;
            string caption, target;

            // first item to add will be row 1
            int lngRow = 1;
            //step through all rows
            for (int i = 1; i <= 6; i++) {
                // if both columns are NON blank
                caption = ((string)fgTools[1, i - 1].Value).Trim();
                target = ((string)fgTools[2, i - 1].Value).Trim();
                if (caption.Length > 0 && target.Length > 0) {
                    // add this item
                    WinAGISettingsList.WriteSetting(sTOOLS, "Caption" + lngRow, caption);
                    WinAGISettingsList.WriteSetting(sTOOLS, "Source" + lngRow, target);
                    // update tools menu
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" +lngRow].Visible = true;
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" + lngRow].Text = caption;
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" + lngRow].Tag = target;
                    blnTools = true;
                    lngRow++;
                }
            }

            // erase any remaining rows
            for (int i = lngRow; i <= 6; i++) {
                WinAGISettingsList.WriteSetting(sTOOLS, "Caption" + i, "");
                WinAGISettingsList.WriteSetting(sTOOLS, "Source" + i, "");
                // hide this tool menu
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Visible = false;
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Text = "";
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Tag = "";
            }

            // if no tools, hide separator
            MDIMain.mnuTSep2.Visible = blnTools;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void btnBrowse_MouseDown(object sender, MouseEventArgs e) {
            // get target command
            dlgTool.ShowPinnedPlaces = true;
            dlgTool.ShowReadOnly = false;
            dlgTool.FileName = Path.GetFileName((string)fgTools.CurrentCell.Value);
            dlgTool.Filter = "Programs (*.exe)|*.exe|URLs (*.url)|*.url|All Files (*.*)|*.*";
            bool isfile;
            // check for urls vs file
            try {
                isfile = new Uri(Path.GetFullPath((string)fgTools.CurrentCell.Value)).IsFile;
            }
            catch {
                isfile = false;
            }
            if (isfile) {
                // file
                switch (Path.GetExtension((string)fgTools.CurrentCell.Value).ToLower()) {
                case ".exe":
                    dlgTool.FilterIndex = 1;
                    break;
                case ".url":
                    dlgTool.FilterIndex = 2;
                    break;
                default:
                    dlgTool.FilterIndex = 3;
                    break;
                }
                string dir = Path.GetDirectoryName((string)fgTools.CurrentCell.Value);
                if (Path.Exists(dir)) {
                    try {
                        dlgTool.InitialDirectory = dir;
                    }
                    catch {
                        dlgTool.InitialDirectory = ProgramDir;
                    }
                }
                else {
                    dlgTool.InitialDirectory = ProgramDir;
                }
            }
            else {
                // url (or some other file mash)
                dlgTool.InitialDirectory = ProgramDir;
                dlgTool.FilterIndex = 2;
            }
            if (dlgTool.ShowDialog() == DialogResult.OK) {
                fgTools.CurrentCell.Value = dlgTool.FileName;
            }
        }

        private void fgTools_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            if (fgTools.CurrentCell.ColumnIndex == 2) {
                btnBrowse.Visible = true;
                btnBrowse.Left = fgTools.Left + fgTools.Width - btnBrowse.Width - 1;
                btnBrowse.Top = fgTools.Top + (fgTools.CurrentCell.RowIndex + 1) * fgTools.Rows[0].Height - 5;
            }
            else {
                btnBrowse.Visible = false;
            }
        }

        private void fgTools_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            btnBrowse.Visible = false;
        }

        private void fgTools_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex > 0) {
                fgTools.BeginEdit(true);
            }
        }

        private void fgTools_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 0) {
                // select only the cell
                fgTools.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                fgTools.CurrentRow.Selected = true;
            }
            else {
                // select only the cell
                fgTools.SelectionMode = DataGridViewSelectionMode.CellSelect;
                fgTools.CurrentCell.Selected = true;
            }
        }

        private void fgTools_MouseDown(object sender, MouseEventArgs e) {
            // if a full row is selected, begin dragging

            if (fgTools.SelectedRows.Count == 1) {
                if (e.Button == MouseButtons.Left) {
                    sourcerow = fgTools.HitTest(e.X, e.Y).RowIndex;
                    fgTools.Rows[sourcerow].Selected = true;
                    dragging = true;
                    //fgTools.Cursor = Cursors.HSplit;
                }
            }
        }

        private void fgTools_MouseUp(object sender, MouseEventArgs e) {
            if (dragging) {
                int targetrow = fgTools.HitTest(e.X, e.Y).RowIndex;
                // validate drop location
                if (targetrow >= 0 && targetrow != sourcerow) {
                    DataGridViewRow sr = fgTools.Rows[sourcerow];
                    fgTools.Rows.RemoveAt(sourcerow);
                    fgTools.Rows.Insert(targetrow, sr);
                    fgTools.Rows[targetrow].Selected = true;
                    // reset numbers
                    for (int i = 0; i < 6; i++) {
                        fgTools[0, i].Value = (i + 1) + ".";
                    }
                }
                fgTools.Cursor = Cursors.Default;
                dragging = false;
            }
        }

        private void fgTools_MouseMove(object sender, MouseEventArgs e) {
            if (dragging) {
                int targetrow = fgTools.HitTest(e.X, e.Y).RowIndex;
                if (targetrow >= 0 && targetrow != sourcerow) {
                    fgTools.Cursor = Cursors.HSplit;
                }
                else {
                    fgTools.Cursor = Cursors.No;
                }
            }
        }
    }
}
