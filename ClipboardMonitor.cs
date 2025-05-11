using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class ClipboardMonitor : Form {
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    protected override void WndProc(ref Message m) {
        base.WndProc(ref m);
        if (m.Msg == WM_CLIPBOARDUPDATE) {
            OnClipboardChanged();
        }
    }

    protected virtual void OnClipboardChanged() {
        // Override this method to handle clipboard changes
    }
protected override void OnHandleCreated(EventArgs e) {
        base.OnHandleCreated(e);
        AddClipboardFormatListener(this.Handle);
    }
    
    protected override void OnHandleDestroyed(EventArgs e) {
        RemoveClipboardFormatListener(this.Handle); base.OnHandleDestroyed(e);
    }
}
