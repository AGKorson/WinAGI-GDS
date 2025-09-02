using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmImportSoundOptions : Form {

        internal SoundImportOptions Options = new SoundImportOptions();
        private SoundImportFormat Format;

        public frmImportSoundOptions(SoundImportFormat format) {
            InitializeComponent();
            Format = format;
        }

        #region Event Handlers
        private void OKButton_Click(object sender, EventArgs e) {
            // validate channels
            List<int> channels = [];
            if (txtChannel1.Value != -1) {
                channels.Add(txtChannel1.Value);
            }
            if (txtChannel2.Value != -1) {
                channels.Add(txtChannel2.Value);
            }
            if (txtChannel3.Value != -1) {
                channels.Add(txtChannel3.Value);
            }
            if (txtChannel4.Value != -1) {
                channels.Add(txtChannel4.Value);
            }
            if (channels.Count > 0) {
                Options.Channels = channels.ToArray();
            }
            switch (Format) {
            case SoundImportFormat.IT:
            case SoundImportFormat.MOD:
                // validate instrument forced notes
                if (txtForcedNotes.Text.Trim().Length > 0) {
                    string[] fn = txtForcedNotes.Text.Split(',');
                    if (fn.Length.IsOdd()) {
                        // not valid
                        MessageBox.Show(MDIMain,
                            "Instrument forced notes must be in comma separated  pairs",
                            "Invalid Parameters",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        txtForcedNotes.Focus();
                        return;
                    }
                    // each string in fn must be a number
                    for (int i = 0; i < fn.Length; i++) {
                        if (!fn[i].IsInt()) {
                            // not valid
                            MessageBox.Show(MDIMain,
                                "Instrument/note values must be integers.",
                                "Invalid Parameters",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                            txtForcedNotes.Focus();
                            return;
                        }
                    }
                    Options.InstrNote = new();
                    for (int i = 0; i < fn.Length; i += 2) {
                        int k = int.Parse(fn[i]);
                        int v = int.Parse(fn[i + 1]);
                        Options.InstrNote.Add(k, v);
                    }
                }
                // validate instrument shifts
                if (txtInstShifts.Text.Trim().Length > 0) {
                    string[] shifts = txtInstShifts.Text.Split(',');
                    if (shifts.Length.IsOdd()) {
                        // not valid
                        MessageBox.Show(MDIMain,
                            "Instrument note shifts must be in comma separated  pairs",
                            "Invalid Parameters",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        txtForcedNotes.Focus();
                        return;
                    }
                    // each string in shifts must be a number
                    for (int i = 0; i < shifts.Length; i++) {
                        if (!shifts[i].IsInt()) {
                            // not valid
                            MessageBox.Show(MDIMain,
                                "Instrument/note values must be integers.",
                                "Invalid Parameters",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                            txtInstShifts.Focus();
                            return;
                        }
                    }
                    Options.InstrShift = new();
                    for (int i = 0; i < shifts.Length; i += 2) {
                        int k = int.Parse(shifts[i]);
                        int v = int.Parse(shifts[i + 1]);
                        Options.InstrShift.Add(k, v);
                    }
                }
                // auto drum off
                Options.AutoDrumOffs = txtAutoDrum.Value;
                // tempo
                Options.TempoExact = chkExactTempo.Checked;
                break;
            case SoundImportFormat.MIDI:
                Options.MidiRemap = chkRemap.Checked;
                Options.PolyMode = chkPoly.Checked;
                break;
            }
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            // canceled..
            DialogResult = DialogResult.Cancel;
            Hide();
        }
        #endregion

        private void frmImportSoundOptions_Load(object sender, EventArgs e) {
            switch (Format) {
            case SoundImportFormat.IT:
            case SoundImportFormat.MOD:
                if (Format == SoundImportFormat.IT) {
                    Text = "Impulse Tracker File Import Options";
                }
                else {
                    Text = "Protracker File Import Options";
                }
                Height = 272;
                txtChannel1.Value = 1;
                txtChannel2.Value = 2;
                txtChannel3.Value = 3;
                txtChannel4.Value = 4;
                chkPoly.Visible = false;
                chkRemap.Visible = false;
                break;
            case SoundImportFormat.MIDI:
                            chkPoly.Checked = false;
                Text = "MIDI File Import Options";
                Height = 163;
                txtChannel1.MinValue = -1;
                txtChannel1.MaxValue = 15;
                txtChannel1.Value = 0;
                txtChannel2.MinValue = -1;
                txtChannel2.MaxValue = 15;
                txtChannel2.Value = 1;
                txtChannel3.MinValue = -1;
                txtChannel3.MaxValue = 15;
                txtChannel3.Value = 2;
                txtChannel4.MinValue = -1;
                txtChannel4.MaxValue = 15;
                txtChannel4.Value = 9;
                chkExactTempo.Visible = false;
                label2.Visible = false;
                txtAutoDrum.Visible = false;
                label3.Visible = false;
                txtForcedNotes.Visible = false;
                label4.Visible = false;
                txtInstShifts.Visible = false;
                break;
            }
        }
    }
}
