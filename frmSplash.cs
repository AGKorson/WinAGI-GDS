using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmSplash : Form {
        public frmSplash() {
            InitializeComponent();

            // set version information
            lblVersion.Text = "Version " + Application.ProductVersion;
            lblCopyright.Text += Common.Base.COPYRIGHT_YEAR;
        }
    }
}
