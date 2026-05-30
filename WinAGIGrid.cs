using System.Drawing;
using System.Windows.Forms;

namespace WinAGI.Editor {
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
