using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinAGI.Editor {
    /// <summary>
    /// A customized version of DataGridView that allows cells in a row to simulate
    /// being merged
    /// </summary>
    public class WinAGIGrid : DataGridView {
        private readonly Dictionary<int, Color> mergedRows = [];
        
        public WinAGIGrid() {

        }

        public void MergeCells(int row, Color rowcolor) {
            if (row < 0 || row >= Rows.Count) {
                return;
            }
            mergedRows[row] = rowcolor;
        }

        public void UnMergeCells(int row) {
            if (row < 0 || row >= Rows.Count) {
                return;
            }
            mergedRows.Remove(row);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0) {
                if (mergedRows.TryGetValue(e.RowIndex, out Color bg)) {
                    using (SolidBrush fillBrush = new(bg))
                    using (Pen gridPenColor = new(GridColor)) {
                        Rectangle rect = e.CellBounds;
                        // fill cell background
                        e.Graphics.FillRectangle(fillBrush, rect);
                        // draw top and bottom borders
                        e.Graphics.DrawLine(gridPenColor, rect.Left, rect.Top, rect.Right - 1, rect.Top);
                        e.Graphics.DrawLine(gridPenColor, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
                        if (e.ColumnIndex == 0) {
                            // draw left border
                            e.Graphics.DrawLine(gridPenColor, rect.Left, rect.Top, rect.Left, rect.Bottom - 1);
                        }
                        else if (e.ColumnIndex == Columns.Count - 1) {
                            // draw right border
                            e.Graphics.DrawLine(gridPenColor, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
                        }
                    }
                    // output cell text
                    e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
                    e.Handled = true;
                    return;
                }
            }
            base.OnCellPainting(e);
        }
    }
}
