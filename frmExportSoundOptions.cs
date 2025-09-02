using System;
using System.Windows.Forms;
using WinAGI.Engine;

namespace WinAGI.Editor {
    public partial class frmExportSoundOptions : Form {
        // available export formats:
        //    0 = AGI Native Sound Resource (all restypes)
        //    1 = MIDI Compatible Conversion (pcjr/IIgs midi)
        //    2 = PCM/Wave Compatible Conversion (pcjr/IIgs pcm)
        //    3 = AGI Sound Script Format (all restypes)

            public frmExportSoundOptions(SoundFormat resformat) {
            // resformat = 0: pcjr
            // resformat = 1: IIgs midi
            // resformat = 2: IIgs pcm wave
            InitializeComponent();
            switch (resformat) {
            case SoundFormat.AGI:
                // pcjr (most common)
                // allow all
                break;
            case SoundFormat.MIDI:
                // IIgs midi
                // no pcm/wav
                optWAV.Enabled = false;
                break;
            case SoundFormat.WAV:
                // IIgs pcm/wav
                // no midi
                optMidi.Enabled = false;
                break;
            }
        }

        #region Event Handlers
        private void OKButton_Click(object sender, EventArgs e) {
            // ok!
            DialogResult = DialogResult.OK;
            this.Visible = false;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            // canceled..
            DialogResult = DialogResult.Cancel;
            this.Visible = false;
        }
        #endregion
    }
}
