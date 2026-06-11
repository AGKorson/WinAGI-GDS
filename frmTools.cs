using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmTools : Form {
        #region Fields
        private int sourcerow = -1, targetrow = -1;
        private bool dropping = false;
        # endregion

        #region Constructors
        public frmTools() {
            InitializeComponent();
            toolgrid.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            toolgrid.Columns[0].Width = toolgrid.Font.Height * 2;
        }
        #endregion

        #region Event Handlers
        private void frmTools_Load(object sender, EventArgs e) {
            int pad = Height - ClientRectangle.Height;
            for (int i = 1; i <= 6; i++) {
                toolgrid.Rows.Add(
                    i.ToString() + ".",
                    WinAGISettingsFile.GetSetting(sTOOLS, "Caption" + i, ""),
                    WinAGISettingsFile.GetSetting(sTOOLS, "Source" + i, "")
                );
            }
            toolgrid.Height = toolgrid.Rows.GetRowsHeight(DataGridViewElementStates.None); //  6 * toolgrid.Rows[0].Height;
            toolgrid.Height += toolgrid.ColumnHeadersHeight;
            Height = toolgrid.Height + pad + 10 + btnOK.Height;
            btnOK.Top = btnCancel.Top = toolgrid.Height + 5;

            // select row 1
            toolgrid.CurrentCell = toolgrid[1, 0];
            toolgrid.Select();
            // set browse button so it fits inside a row
            btnBrowse.Height = toolgrid.Rows[0].Height - 1;
        }

        private void frmTools_HelpRequested(object sender, HelpEventArgs hlpevent) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\customtools.htm");
            hlpevent.Handled = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            bool showTools = false;
            string caption, target;

            // first item to add will be row 1
            int row = 1;
            // step through all rows
            for (int i = 1; i <= 6; i++) {
                // if both columns are NON blank
                caption = ((string)toolgrid[1, i - 1].Value).Trim();
                target = ((string)toolgrid[2, i - 1].Value).Trim();
                if (caption.Length > 0 && target.Length > 0) {
                    // add this item
                    WinAGISettingsFile.WriteSetting(sTOOLS, "Caption" + row, caption);
                    WinAGISettingsFile.WriteSetting(sTOOLS, "Source" + row, target);
                    // update tools menu
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" + row].Visible = true;
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" + row].Text = caption;
                    MDIMain.mnuTools.DropDownItems["mnuTCustom" + row].Tag = target;
                    showTools = true;
                    row++;
                }
            }

            // erase any remaining rows
            for (int i = row; i <= 6; i++) {
                WinAGISettingsFile.WriteSetting(sTOOLS, "Caption" + i, "");
                WinAGISettingsFile.WriteSetting(sTOOLS, "Source" + i, "");
                // hide this tool menu
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Visible = false;
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Text = "";
                MDIMain.mnuTools.DropDownItems["mnuTCustom" + i].Tag = "";
            }

            // if no tools, hide separator
            MDIMain.mnuTSep2.Visible = showTools;
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void btnBrowse_MouseDown(object sender, MouseEventArgs e) {
            // get target command
            bool isfile;
            string filename = (string)toolgrid.CurrentCell.Value;

            toolOpen.ShowPinnedPlaces = true;
            toolOpen.ShowReadOnly = false;
            toolOpen.Filter = "Programs (*.exe)|*.exe|URLs (*.url)|*.url|All Files (*.*)|*.*";
            toolOpen.CheckFileExists = true;
            toolOpen.CheckPathExists = true;
            toolOpen.Multiselect = false;
            toolOpen.OkRequiresInteraction = true;
            toolOpen.ValidateNames = true;
            if (filename.Length == 0) {
                toolOpen.FileName = "";
                toolOpen.InitialDirectory = ProgramDir;
                toolOpen.FilterIndex = 2;
            }
            else {
                toolOpen.FileName = Path.GetFileName(filename);
                // check for urls vs file
                try {
                    isfile = new Uri(Path.GetFullPath((string)toolgrid.CurrentCell.Value)).IsFile;
                }
                catch {
                    isfile = false;
                }
                if (isfile) {
                    // file
                    switch (Path.GetExtension((string)toolgrid.CurrentCell.Value).ToLower()) {
                    case ".exe":
                        toolOpen.FilterIndex = 1;
                        break;
                    case ".url":
                        toolOpen.FilterIndex = 2;
                        break;
                    default:
                        toolOpen.FilterIndex = 3;
                        break;
                    }
                    string dir = Path.GetDirectoryName((string)toolgrid.CurrentCell.Value);
                    if (Path.Exists(dir)) {
                        try {
                            toolOpen.InitialDirectory = dir;
                        }
                        catch {
                            toolOpen.InitialDirectory = ProgramDir;
                        }
                    }
                    else {
                        toolOpen.InitialDirectory = ProgramDir;
                    }
                }
                else {
                    // url (or some other file mash)
                    toolOpen.InitialDirectory = ProgramDir;
                    toolOpen.FilterIndex = 2;
                }

            }
            if (toolOpen.ShowDialog() == DialogResult.OK) {
                toolgrid.CurrentCell.Value = toolOpen.FileName;
            }
        }

        private void toolgrid_KeyDown(object sender, KeyEventArgs e) {
            if (toolgrid.SelectedRows.Count == 1) {
                // entire row can be deleted
                if (e.KeyCode == Keys.Delete) {
                    // erase this row
                    toolgrid[1, toolgrid.CurrentCell.RowIndex].Value = "";
                    toolgrid[2, toolgrid.CurrentCell.RowIndex].Value = "";
                }
            }
            else {
                // individual cells can be edited
                if (e.KeyCode == Keys.Return) {
                    // if not already editing, do so
                    if (!toolgrid.IsCurrentCellInEditMode && toolgrid.CurrentCell.ColumnIndex > 0) {
                        e.Handled = true;
                        toolgrid.BeginEdit(true);
                    }
                }
            }
        }

        private void toolgrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            if (toolgrid.CurrentCell.ColumnIndex == 2) {
                btnBrowse.Visible = true;
                btnBrowse.Left = toolgrid.Left + toolgrid.Width - btnBrowse.Width - 1;
                btnBrowse.Top = toolgrid.Top + toolgrid.CurrentCell.RowIndex * toolgrid.Rows[0].Height + toolgrid.ColumnHeadersHeight;
            }
            else {
                btnBrowse.Visible = false;
            }
        }

        private void toolgrid_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            // if cell is empty, clear the row
            if (toolgrid.CurrentCell.Value is null ||
                ((string)toolgrid.CurrentCell.Value).Trim().Length == 0) {
                toolgrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                toolgrid.Rows[e.RowIndex].Cells[3 - e.ColumnIndex].Value = "";
            }
            btnBrowse.Visible = false;
        }

        private void toolgrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex > 0) {
                toolgrid.BeginEdit(true);
            }
        }

        private void toolgrid_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 0) {
                // select only the cell
                toolgrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                toolgrid.CurrentRow.Selected = true;
            }
            else {
                // select only the cell
                toolgrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                toolgrid.CurrentCell.Selected = true;
            }
        }

        private void toolgrid_MouseDown(object sender, MouseEventArgs e) {
            // if a full row is selected, begin dragging if on column 0

            if (toolgrid.SelectedRows.Count == 1) {
                // check for dragging operation
                if (toolgrid.HitTest(e.X, e.Y).RowIndex == toolgrid.CurrentRow.Index &&
                    toolgrid.HitTest(e.X, e.Y).ColumnIndex == 0) {
                    if (e.Button == MouseButtons.Left) {
                        sourcerow = toolgrid.HitTest(e.X, e.Y).RowIndex;
                        toolgrid.Rows[sourcerow].Selected = true;
                        toolgrid.DoDragDrop(toolgrid.Rows[sourcerow], DragDropEffects.Move);

                    }
                }
            }
        }

        private void toolgrid_DragDrop(object sender, DragEventArgs e) {
            Point clientPoint = toolgrid.PointToClient(new Point(e.X, e.Y));
            targetrow = toolgrid.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
            // move the row if not on the curret row, or one below
            if (targetrow >= 0 && targetrow != sourcerow && targetrow != sourcerow + 1) {
                DataGridViewRow sr = toolgrid.Rows[sourcerow];
                toolgrid.Rows.RemoveAt(sourcerow);
                if (targetrow > sourcerow) {
                    toolgrid.Rows.Insert(targetrow - 1, sr);
                }
                else {
                    toolgrid.Rows.Insert(targetrow, sr);
                }
                // reset numbers
                for (int i = 0; i < 6; i++) {
                    toolgrid[0, i].Value = (i + 1) + ".";
                }
                // let the PrePaint event know that the rows
                // need to be re-selected
                dropping = true;
            }
        }

        private void toolgrid_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        private void toolgrid_DragOver(object sender, DragEventArgs e) {
            // allow drop if not on current selection
            Point clientPoint = toolgrid.PointToClient(new Point(e.X, e.Y));
            var hit = toolgrid.HitTest(clientPoint.X, clientPoint.Y);
            if (hit.RowIndex == toolgrid.CurrentRow.Index) {
                e.Effect = DragDropEffects.None;
            }
            else if (hit.RowIndex == -1) {
                e.Effect = DragDropEffects.None;
            }
            else {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void toolgrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
            // This was the only event I could find that reliably occurs after the
            // last SelectionChanged event that happens when a row is moved. Something
            // forces the Selection to change back to the original sourcerow.
            // It happens after all mouse related events. This event happens after
            // that. 

            // The dropping flag is used force the selection to change.
            if (dropping) {
                dropping = false;
                if (targetrow > sourcerow) {
                    targetrow--;
                }
                // IMPORTANT! must set current cell to a cell in the target row,
                // otherwise the selection won't update to the target row
                toolgrid.CurrentCell = toolgrid.Rows[targetrow].Cells[1];
                toolgrid.Rows[targetrow].Selected = true;
                sourcerow = -1;
                targetrow = -1;
            }
        }

        private void toolgrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            DataGridViewCell cell = toolgrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (e.Value is null) {
                // no tooltip
                return;
            }
            // determine if tooltip is needed
            string text = (string)e.Value;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(toolgrid.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }
        }

        private void toolgrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) {
                return;
            }
            DataGridViewCell cell = toolgrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.ToolTipText.Length > 0) {
                toolgrid.ShowCellToolTips = true;
            }
        }

        private void toolgrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            toolgrid.ShowCellToolTips = false;
        }
    }
    #endregion
}
