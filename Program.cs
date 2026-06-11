using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To test command line arguments in Visual Studio 2026:
            // 1.	Right-click project in Solution Explorer and select Properties.
            // 2.	Go to the Debug tab.
            // 3.	In the Application arguments (or Command line arguments) field,
            //      enter desired arguments (e.g., --console).
            // 4.	Save and run the project (F5 or Ctrl+F5).

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 &&
                args[1].Equals("--console", StringComparison.OrdinalIgnoreCase)) {
#if DEBUG
                // Display a message box to confirm console mode
                var result = MessageBox.Show(
                    "RUN CONSOLE MODE?",
                    "Confirm Console Mode",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    WAGConsole.RunConsoleMode(args[2..]);
                    return;
                }
#else
                // In Release mode, automatically run console mode
                WAGConsole.RunConsoleMode(args[2..]);
                return;
#endif
            }
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Diagnostics.Debug.Print("begin WinAGI");
            Application.Run(new frmMDIMain());
        }
    }
}
