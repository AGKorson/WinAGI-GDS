using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI.Editor {
    /// <summary>
    /// A custom message box class that includes a checkbox that the calling function can
    /// inspect after the dialog box closes.
    /// </summary>
    public static class MsgBoxEx {

        /// <summary>
        /// Displays a custom message box with checkbox field with a help button.
        /// </summary>
        public static DialogResult Show(Form owner, string Prompt, string Title, MessageBoxButtons Buttons, MessageBoxIcon Icon, string CheckString, ref bool Checked, string HelpFile, string HelpTopic) {
            DialogResult dlgResult = DialogResult.None;
            using (frmDialog msgboxex = new frmDialog(owner, Prompt, Title, Buttons, Icon, CheckString, ref Checked, HelpFile, HelpTopic)) {
                dlgResult = msgboxex.ShowDialog();
                Checked = msgboxex.Check1.Checked;
            }
            return dlgResult;
        }

        /// <summary>
        /// Displays a custom message box with a checkbox field.
        /// </summary>
        public static DialogResult Show(Form owner, string Prompt, string Title, MessageBoxButtons Buttons, MessageBoxIcon Icon, string CheckString, ref bool Checked) {
            return Show(owner, Prompt, Title, Buttons, Icon, CheckString, ref Checked, "", "");
        }
    }
}
