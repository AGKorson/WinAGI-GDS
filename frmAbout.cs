using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace WinAGI.Editor {
    partial class frmAbout : Form {
        TextStyle blueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Underline);
        public frmAbout() {
            InitializeComponent();
            labelVersion.Text = "Version " + Application.ProductVersion;
            fctbLicense.Text = "This program is free software: you can redistribute " +
                "it and/or modify it under the terms of the GNU General Public License as " +
                "published by the Free Software Foundation, either version 3 of the License, " +
                "or (at your option) any later version.\r\n\r\nThis program is distributed in " +
                "the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the " +
                "implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. " +
                "See the GNU General Public License for more details.\r\n\r\nYou should have " +
                "received a copy of the GNU General Public License along with this program. " +
                "If not, see https://www.gnu.org/licenses/.";
        }

        private void fctbLicense_TextChangedDelayed(object sender, TextChangedEventArgs e) {
            e.ChangedRange.ClearStyle(blueStyle);
            e.ChangedRange.SetStyle(blueStyle, @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
        }

        private void fctbLicense_MouseDown(object sender, MouseEventArgs e) {
            var p = fctbLicense.PointToPlace(e.Location);
            if (CharIsHyperlink(p)) {
                var url = fctbLicense.GetRange(p, p).GetFragment(@"[\S]").Text;
                // open as a url, not a file
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch {
                    // ignore error
                }
            }
        }

        private void fctbLicense_MouseMove(object sender, MouseEventArgs e) {
            var p = fctbLicense.PointToPlace(e.Location);
            if (CharIsHyperlink(p))
                fctbLicense.Cursor = Cursors.Hand;
            else
                fctbLicense.Cursor = Cursors.IBeam;
        }

        bool CharIsHyperlink(Place place) {
            var mask = fctbLicense.GetStyleIndexMask(new Style[] { blueStyle });
            if (place.iChar < fctbLicense.GetLineLength(place.iLine))
                if ((fctbLicense[place].style & mask) != 0)
                    return true;

            return false;
        }
    }
}
