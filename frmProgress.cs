using System.Drawing;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmProgress : Form {
        public frmProgress(Form owner) {
            InitializeComponent();
            this.Owner = owner;

            if (owner is null) {
                StartPosition = FormStartPosition.CenterScreen;
            }
            else {
                Point offset = new() {
                    X = owner.Width / 2 - Width / 2,
                    Y = owner.Height / 2 - Height / 2
                };
                Point pos = new();
                // child form?
                if (owner.IsMdiChild) {
                    // extracting actual position of form is not easy- 
                    // PointToScreen on the form gives the position of the client area
                    // not the form, so we have to account for the left and top borders
                    // BUT there is no way to get that easily; we use the difference
                    // between client size and form size and make some assumptions...

                    // if we assume bottom border is same as right/left; then
                    // rightborder = leftborder = (frm.Width - client.Width) / 2
                    // topborder = frm.Width - client.Width - rightborder
                    //
                    // this doesn't work though - instead, rightborder needs to be
                    // frm.Width - client.Width (full amount, not halved)
                    // and topborder = 1/2 of rightborder
                    //
                    // NO IDEA why this is so- but it works.
                    // 
                    Point childoffset = new() {
                        X = -(owner.Width - owner.ClientSize.Width), // / 2
                    };
                    childoffset.Y = -(owner.Height - owner.ClientSize.Height + childoffset.X / 2);
                    pos = owner.PointToScreen(childoffset);
                }
                else {
                    pos.X = owner.Left;
                    pos.Y = owner.Top;
                }
                // adjust pos by offset to center the form
                pos.Offset(offset);
                Location = pos;
                Size = new(278, 128);
            }
        }
    }
}
