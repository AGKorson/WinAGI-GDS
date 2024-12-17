using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Sound;
using System.Media;
using static WinAGI.Common.Base;
using FastColoredTextBoxNS;

namespace WinAGI.Editor {
    public partial class frmPreview : Form {
        private ComboBox[] cmbInst = new ComboBox[3];
        private CheckBox[] chkTrack = new CheckBox[4];
        const int MIN_HEIGHT = 100;
        const int MIN_WIDTH = 100;

        int intOffsetX, intOffsetY;
        AGIResType PrevResType;

        Logic agLogic;
        Picture agPic;
        int PicScale;
        bool blnDraggingPic;
        Sound agSound;
        long lngStart;
        Engine.View agView;
        int CurLoop, CurCel;
        int ViewScale, VTopMargin;
        bool blnDraggingView;
        int lngVAlign, lngHAlign;
        bool blnTrans = false, DontDraw;

        //use local variables to hold visible status for scrollbars
        //because their visible property remains false as long as
        //the picturebox that holds them is false, even though they
        //are set to true
        bool blnViewHSB, blnViewVSB;
        bool blnPicVSB, blnPicHSB;
        const int PW_MARGIN = 4;

        public frmPreview() {
            InitializeComponent();
            // logic controls
            rtfLogPrev.ShowLineNumbers = WinAGISettings.ShowLineNumbers.Value;
            rtfLogPrev.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            // picture controls
            hsbPic.Minimum = -PW_MARGIN;
            vsbPic.Minimum = -PW_MARGIN;
            hsbPic.LargeChange = (int)(pnlPicture.Width * LG_SCROLL);
            vsbPic.LargeChange = (int)(pnlPicImage.Height * LG_SCROLL);
            hsbPic.SmallChange = (int)(pnlPicImage.Width * SM_SCROLL);
            vsbPic.SmallChange = (int)(pnlPicImage.Height * SM_SCROLL);
            hsbPic.Top = pnlPicImage.Height - hsbPic.Height;
            vsbPic.Left = pnlPicImage.Width - vsbPic.Width;
            fraPCorner.Width = vsbPic.Width;
            fraPCorner.Left = vsbPic.Left;
            fraPCorner.Height = hsbPic.Height;
            fraPCorner.Top = hsbPic.Top;
            PicScale = WinAGISettings.PicScalePreview.Value;
            udPZoom.Value = PicScale;
            // sound controls
            cmbInst[0] = cmbInst0;
            cmbInst[1] = cmbInst1;
            cmbInst[2] = cmbInst2;
            chkTrack[0] = chkTrack0;
            chkTrack[1] = chkTrack1;
            chkTrack[2] = chkTrack2;
            chkTrack[3] = chkTrack3;
            for (int i = 0; i < 128; i++) {
                cmbInst0.Items.Add(InstrumentName(i));
                cmbInst1.Items.Add(InstrumentName(i));
                cmbInst2.Items.Add(InstrumentName(i));
            }
            // view controls
            tsViewPrev.ImageList = imageList1;
            hsbView.LargeChange = (int)(pnlCel.Width * LG_SCROLL);
            vsbView.LargeChange = (int)(pnlCel.Height * LG_SCROLL);
            hsbView.SmallChange = (int)(pnlCel.Width * SM_SCROLL);
            vsbView.SmallChange = (int)(pnlCel.Height * SM_SCROLL);
            hsbView.Top = pnlCel.Height - hsbView.Height;
            vsbView.Left = pnlCel.Width - vsbView.Width;
            lngHAlign = WinAGISettings.ViewAlignH.Value;
            lngVAlign = WinAGISettings.ViewAlignV.Value;
            fraVCorner.Width = vsbView.Width;
            fraVCorner.Left = vsbView.Left;
            fraVCorner.Height = hsbView.Height;
            fraVCorner.Top = hsbView.Top;
            VTopMargin = 50;
            lngVAlign = 2;
            ViewScale = WinAGISettings.ViewScalePreview.Value;
            cmbMotion.SelectedIndex = 0;
            sldSpeed.Value = 5;
            hsbView.Minimum = -PW_MARGIN;
            vsbView.Minimum = -PW_MARGIN;

            MdiParent = MDIMain;
            SelResNum = -1;
        }

        #region Event Handlers
        #region Form Event Handlers
        private void frmPreview_Activated(object sender, EventArgs e) {
            
            //if findform is visible,
            if (FindingForm.Visible) {
                //hide it it
                FindingForm.Visible = false;
            }
        }

        private void frmPreview_Deactivate(object sender, EventArgs e) {
            // stop sound playback
            if (SelResType == AGIResType.Sound) {
                StopSoundPreview();
            }
            //stop cycling
            if (tmrMotion.Enabled) {
                //show play
                tmrMotion.Enabled = false;
                cmdVPlay.BackgroundImage = imageList1.Images[9];
            }
        }

        private void frmPreview_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            Debug.Print($"Main - KeyDown: {e.KeyCode}; KeyData: {e.KeyData}; KeyModifiers: {e.Modifiers}");
        }

        private void frmPreview_KeyPress(object sender, KeyPressEventArgs e) {

            KeyHandler(e);
            e.Handled = true;
        }

        private void frmPreview_KeyDown(object sender, KeyEventArgs e) {
            // check keys based on SHIFT/CTRL/ALT status:
            if (!e.Shift && !e.Control && !e.Alt) {
                // none (no SHIFT, CTRL, ALT)
                switch (e.KeyCode) {
                case Keys.Delete:
                    //if a resource is selected
                    if (SelResType == AGIResType.Logic ||
                          SelResType == AGIResType.Picture ||
                          SelResType == AGIResType.Sound ||
                          SelResType == AGIResType.View) {
                        //call remove from game method
                        MDIMain.RemoveSelectedResource();
                        e.Handled = true;
                    }
                    break;
                case Keys.F1:
                    MenuClickHelp();
                    e.Handled = true;
                    break;
                case Keys.F3:
                    // nothing?
                    break;
                }
            }
            else if (e.Shift && e.Control && !e.Alt) {
                // SHIFT + CTRL
                //switch (e.KeyCode) {
                //}
            }
            else if (!e.Shift && e.Control && !e.Alt) {
                // CTRL
                switch (e.KeyCode) {
                case Keys.F: //Ctrl+F (Find)
                    if (SelResType == AGIResType.Logic ||
                          SelResType == AGIResType.Picture ||
                          SelResType == AGIResType.Sound ||
                          SelResType == AGIResType.View) {
                        //find this resid
                        MDIMain.SearchForID();
                    }
                    e.Handled = true;
                    break;
                }
            }
        }

        private void frmPreview_VisibleChanged(object sender, EventArgs e) {
            // if now visible, need to force position to correct value
            if (Visible) {
                PositionPreview();
            }
            else {
                SavePreviewPos();
            }
        }

        private void frmPreview_FormClosing(object sender, FormClosingEventArgs e) {
            //ensure preview resources are cleared,
            if (agLogic is not null) {
                //unload it
                agLogic.Unload();
                agLogic = null;
            }

            if (agPic is not null) {
                //unload it
                agPic.Unload();
                //delete it
                agPic = null;
            }
            if (agView is not null) {
                //unload it
                agView.Unload();
                //delete it
                agView = null;
            }
            if (agSound is not null) {
                //unload it
                agSound.Unload();
                //delete it
                agSound = null;
            }
            SavePreviewPos();
        }

        internal void SetResourceMenu() {
            mnuRExportAll.Visible = (SelResType == AGIResType.Game);
            if (SelResType == AGIResType.View && SelResNum != -1) {
                bool err = EditGame.Views[SelResNum].ErrLevel < 0;
                mnuRExportGIF.Visible = true;
                mnuRExportGIF.Enabled = !err;
            }
            else {
                mnuRExportGIF.Visible = false;
            }
        }

        private void mnuRExportGIF_Click(object sender, EventArgs e) {
            ExportLoop(EditGame.Views[SelResNum], CurLoop);
        }
        #endregion

        #region Preview Logic Event Handlers
        private void rtfLogPrev_DoubleClick(object sender, EventArgs e) {
            if (OpenGameLogic((byte)SelResNum)) {
                try {
                    WinAGIFCTB fctb = ((frmLogicEdit)LogicEditors[LogicEditors.Count - 1]).fctb;
                    Place start = new(0, rtfLogPrev.Selection.Start.iLine);
                    fctb.Selection.Start = start;
                    fctb.DoSelectionVisible();
                }
                catch {
                    //ignore errors
                }
            };
        }

        private void rtfLogPrev_KeyDown(object sender, KeyEventArgs e) {
            if (e.Handled == true) {
                return;
            }
            switch (e.Modifiers) {
            case Keys.None:
                switch (e.KeyCode) {
                case Keys.Delete:
                    //it should be caught by Form_KeyDown
                    // but just in case, ignore it
                    e.Handled = true;
                    break;
                }
                break;
            case Keys.Control://    vbCtrlMask
                switch (e.KeyCode) {
                case Keys.A:
                    //*//  rtfLogPrev.Range.SelectRange();
                    break;
                case Keys.C:
                    //*//  rtfLogPrev.Selection.Range.Copy();
                    break;
                }
                break;
            }
            e.Handled = true;
        }

        private void rtfLogPrev_MouseDown(object sender, MouseEventArgs e) {
            //with right mouse click, show the context menu, but only
            //allow user to copy text, if some was selected
            if (e.Button == MouseButtons.Right) {
                //*//
                /*   if (rtfLogPrev.Selection.Range.Length > 0) {
                         MDIMain.mnuLPCopy.Enabled = true;
                     } else {
                         MDIMain.mnuLPCopy.Enabled = false;
                     }
                     MDIMain.mnuLPSelectAll.Visible = true;
                     PopupMenu(mnuLPPopup, 0, X, Y);
                */
            }
        }

        private void cmiSelectAll_Click(object sender, EventArgs e) {
            rtfLogPrev.SelectAll();
        }

        private void cmiCopy_Click(object sender, EventArgs e) {
            rtfLogPrev.Copy();
        }

        private void cmiLineNumbers_Click(object sender, EventArgs e) {
            rtfLogPrev.ShowLineNumbers = !rtfLogPrev.ShowLineNumbers;
            cmiLineNumbers.Text = (rtfLogPrev.ShowLineNumbers ? "Hide" : "Show") + " Line Numbers";
        }
        #endregion

        #region Preview Picture Event Handlers
        private void udPZoom_ValueChanged(object sender, EventArgs e) {
            //set zoom
            PicScale = (int)udPZoom.Value;
            //force update
            DisplayPicture();
        }

        private void optVisual_CheckedChanged(object sender, EventArgs e) {
            //force a redraw
            DisplayPicture();
        }

        private void pnlPicImage_Resize(object sender, EventArgs e) {
            //position scrollbars
            hsbPic.Width = pnlPicImage.Bounds.Width;
            vsbPic.Height = pnlPicImage.Bounds.Height;
            SetPScrollbars();
        }

        private void vsbPic_Scroll(object sender, ScrollEventArgs e) {
            // position image
            imgPicture.Top = -vsbPic.Value;
        }

        private void hsbPic_Scroll(object sender, ScrollEventArgs e) {
            //position picture
            imgPicture.Left = -hsbPic.Value;
        }

        private void imgPicture_DoubleClick(object sender, EventArgs e) {
            //open picture for editing
            OpenGamePicture((byte)SelResNum);
        }

        private void imgPicture_Validated(object sender, EventArgs e) {
            Debug.Print("validate - did it work?");
        }

        private void imgPicture_MouseWheel(object sender, MouseEventArgs e) {
            switch (e.Delta) {
            case < 0:
                // wheel down
                if (udPZoom.Value > udPZoom.Minimum) {
                    udPZoom.Value--;
                }
                break;
            case > 0:
                // wheel up
                if (udPZoom.Value < udPZoom.Maximum) {
                    udPZoom.Value++;
                }
                break;
            }
        }

        private void imgPicture_MouseLeave(object sender, EventArgs e) {
            if (this != MDIMain.ActiveMdiChild) {
                return;
            }
            MainStatusBar.Items[nameof(spStatus)].Text = "";
        }

        private void imgPicture_MouseDown(object sender, MouseEventArgs e) {
            //if either scrollbar is visible,
            if (hsbPic.Visible || vsbPic.Visible) {
                //set dragpic mode
                blnDraggingPic = true;
                //set pointer to custom
                imgPicture.Cursor = Cursors.Hand;
                //save x and Y offsets
                intOffsetX = e.X;
                intOffsetY = e.Y;
            }
        }

        private void imgPicture_MouseUp(object sender, MouseEventArgs e) {
            //if dragging
            if (blnDraggingPic) {
                //cancel dragmode
                blnDraggingPic = false;
                // reset cursor
                imgPicture.Cursor = Cursors.Default;
            }
        }

        private void imgPicture_MouseMove(object sender, MouseEventArgs e) {
            if (PreviewWin != MDIMain.ActiveMdiChild) {
                return;
            }
            // if left mouse button is down and either or both scrollbars are
            // visible, scroll the image;
            // if mouse button is up, show coordinates

            if (e.Button == MouseButtons.Left) {
                int tmpHVal = 0, tmpVVal = 0;
                int DX = 0, DY = 0;

                //if not active form
                if (MDIMain.ActiveMdiChild != this) {
                    return;
                }

                //if dragging picture
                if (blnDraggingPic) {
                    // always clear statusbar
                    MainStatusBar.Items[nameof(spStatus)].Text = "";
                    if (vsbPic.Visible) {
                        // adjust scroll bars by amount of delta from 
                        // starting offset (subract from scrollbar value, since
                        // scrollbar value = - picture offset)
                        tmpVVal = vsbPic.Value - (e.Y - intOffsetY);
                        //limit to valid values
                        if (tmpVVal < vsbPic.Minimum) {
                            tmpVVal = vsbPic.Minimum;
                        }
                        else if (tmpVVal > (vsbPic.Maximum - vsbPic.LargeChange)) {
                            tmpVVal = vsbPic.Maximum - vsbPic.LargeChange;
                        }
                        // adjust if it changed
                        DY = tmpVVal - vsbPic.Value;
                        if (DY != 0) {
                            //set vertical scrollbar
                            vsbPic.Value = tmpVVal;
                        }
                    }
                    //repeat for horizontal scrollbar
                    if (hsbPic.Visible) {
                        tmpHVal = hsbPic.Value - (e.X - intOffsetX);
                        //limit positions to valid values
                        if (tmpHVal < hsbPic.Minimum) {
                            tmpHVal = hsbPic.Minimum;
                        }
                        else if (tmpHVal > (hsbPic.Maximum - hsbPic.LargeChange)) {
                            tmpHVal = hsbPic.Maximum - hsbPic.LargeChange;
                        }
                        DX = tmpHVal - hsbPic.Value;
                        if (DX != 0) {
                            //set horizontal scrollbar
                            hsbPic.Value = tmpHVal;
                        }
                    }
                    //move the image to this location
                    if (DX != 0 || DY != 0) {
                        imgPicture.Location = new Point(-tmpHVal, -tmpVVal);
                    }
                    // NEVER update offset; it's always relative to original starting point of the drag
                }
            }
            else if (e.Button == MouseButtons.Right) {

            }
            else if (e.Button == MouseButtons.None) {
                MainStatusBar.Items[nameof(spStatus)].Text = $"X: {e.X / 2 / PicScale}    Y: {e.Y / PicScale}";
            }
        }

        #endregion

        #region Preview Sound Event Handlers
        private void btnPlay_Click(object sender, EventArgs e) {
            // if nothing to play
            if (agSound.Length == 0) {
                //this could happen if the sound has no notes in any tracks
                return;
            }
            btnStop.Enabled = true;
            btnPlay.Enabled = false;
            // disable other controls while sound is playing
            SetMIDIControls(false);
            try {
                switch (agSound.SndFormat) {
                case SoundFormat.AGI:
                    // wav or midi, depending on button option
                    agSound.PlaySound(optPCjr.Checked ? SoundPlaybackMode.WAV : SoundPlaybackMode.MIDI);
                    break;
                case SoundFormat.MIDI:
                    // MIDI only
                    agSound.PlaySound();
                    break;
                case SoundFormat.WAV:
                    // WAV only
                    agSound.PlaySound();
                    break;
                }
            }
            catch (Exception) {
                tmrSound.Enabled = false;
                //reset buttons
                btnStop.Enabled = false;
                picProgress.Width = 0;
            }
            //save current time
            lngStart = DateTime.Now.Ticks;

            //enable timer
            tmrSound.Enabled = true;
        }

        private void Timer1_Tick(object sender, EventArgs e) {
            //update progress bar

            long lngNow;
            double dblPos = 0;

            lngNow = DateTime.Now.Ticks;
            if (agSound.Length != 0) {
                //fraction of sound played
                dblPos = (lngNow - lngStart) / 10000000f / agSound.Length;
            }

            if (dblPos >= 1 || agSound.Length == 0) {
                tmrSound.Enabled = false;
                picProgress.Width = pnlProgressBar.Width;
            }
            else {
                picProgress.Width = (int)(pnlProgressBar.Width * dblPos);
            }
        }

        private void chkTrack0_CheckedChanged(object sender, EventArgs e) {
            chkTrack_Click(0);
        }

        private void chkTrack1_CheckedChanged(object sender, EventArgs e) {
            chkTrack_Click(1);
        }

        private void chkTrack2_CheckedChanged(object sender, EventArgs e) {
            chkTrack_Click(2);
        }

        private void chkTrack3_CheckedChanged(object sender, EventArgs e) {
            chkTrack_Click(3);
        }

        private void chkTrack_Click(int Index) {
            //if disabled, just exit
            if (!chkTrack[Index].Enabled) {
                return;
            }
            //if changing
            if (agSound.Tracks[Index].Muted == chkTrack[Index].Checked) {
                agSound.Tracks[Index].Muted = !chkTrack[Index].Checked;
            }
            //redisplay length (it may have changed)
            lblLength.Text = "Sound clip length: " + agSound.Length.ToString("0.0") + " seconds";
            //enable play button if at least one track is NOT muted AND midi not disabled AND length>0
            btnPlay.Enabled = (chkTrack[0].Checked || chkTrack[1].Checked || chkTrack[2].Checked || chkTrack[3].Checked) && (agSound.Length > 0);
        }

        private void cmbInst0_SelectionChangeCommitted(object sender, EventArgs e) {
            cmbInst_Click(0);
        }

        private void cmbInst1_SelectionChangeCommitted(object sender, EventArgs e) {
            cmbInst_Click(1);
        }

        private void cmbInst2_SelectionChangeCommitted(object sender, EventArgs e) {
            cmbInst_Click(2);
        }

        private void cmbInst_Click(int Index) {
            if (agSound.Tracks[Index].Instrument != cmbInst[Index].SelectedIndex) {
                agSound.Tracks[Index].Instrument = (byte)cmbInst[Index].SelectedIndex;
            }
        }

        private void cmdReset_Click(object sender, EventArgs e) {
            //reset instruments to default
            cmbInst[0].SelectedIndex = 80;
            cmbInst[1].SelectedIndex = 80;
            cmbInst[2].SelectedIndex = 80;
        }

        private void btnStop_Click(object sender, EventArgs e) {
            // stop sounds
            StopSoundPreview();
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            tmrSound.Enabled = false;
            picProgress.Width = pnlProgressBar.Width;
            picProgress.Refresh();
            // if playing MIDI, re-enable track controls
            if (optMIDI.Checked) {
                SetMIDIControls(true);
            }
            picProgress.Width = 0;
        }

        void pnlSound_DoubleClick(object sender, EventArgs e) {
            //open sound for editing, if standard agi
            OpenGameSound((byte)SelResNum);
        }

        private void optPCjr_CheckedChanged(object sender, EventArgs e) {
            SetMIDIControls(false);
        }

        private void optMIDI_CheckedChanged(object sender, EventArgs e) {
            SetMIDIControls(true);
        }

        private void This_SoundComplete(object sender, SoundCompleteEventArgs e) {
            // disable stop and enable play

            // need to check if invoke is required- accessing the UI elements
            // is done differently if that's the case
            if (btnPlay.InvokeRequired) {
                btnPlay.Invoke(new Action(() => { btnPlay.Enabled = true; }));
                btnStop.Invoke(new Action(() => { btnStop.Enabled = false; }));
                tmrSound.Enabled = false;
                picProgress.Invoke(new Action(() => { picProgress.Width = pnlProgressBar.Width; }));
                picProgress.Invoke(new Action(() => { picProgress.Refresh(); }));
                // if playing MIDI sound, re-enable track controls
                if (optMIDI.Checked) {
                    for (int i = 0; i < 3; i++) {
                        chkTrack[i].Invoke(new Action(() => { chkTrack[i].Enabled = optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[i].Muted; }));
                        cmbInst[i].Invoke(new Action(() => { cmbInst[i].Enabled = optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[i].Muted; }));
                    }
                    chkTrack[3].Invoke(new Action(() => { chkTrack[3].Enabled = optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[3].Muted; }));
                    cmdReset.Invoke(new Action(() => { cmdReset.Enabled = true; }));
                }
                picProgress.Invoke(new Action(() => { picProgress.Width = 0; }));
            }
            else {
                btnPlay.Enabled = true;
                btnStop.Enabled = false;
                tmrSound.Enabled = false;
                picProgress.Width = pnlProgressBar.Width;
                picProgress.Refresh();
                // if playing MIDI sound, re-enable track controls
                if (optMIDI.Checked) {
                    SetMIDIControls(true);
                }
                picProgress.Width = 0;
            }
        }

        #endregion

        #region Preview View Event Handlers
        private void cmdVPlay_Click(object sender, EventArgs e) {
            //toggle motion
            tmrMotion.Enabled = !tmrMotion.Enabled;

            //set icon to match
            if (tmrMotion.Enabled) {
                cmdVPlay.BackgroundImage = imageList1.Images[9];
                //reset cel, if endofloop or reverseloop motion selected
                //begin cycling the cels, based on motion type
                switch (cmbMotion.SelectedIndex) {
                case 0: //normal
                    break;
                case 1: //reverse
                    break;
                case 2: //end of loop
                        //if already on last cel
                    if (CurCel == agView[CurLoop].Cels.Count - 1) {
                        CurCel = 0;
                        DisplayCel();
                    }
                    break;
                case 3: // reverse loop
                        //if already on first cel
                    if (CurCel == 0) {
                        CurCel = agView[CurLoop].Cels.Count - 1;
                        DisplayCel();
                    }
                    break;
                default:
                    //nothing to do
                    return;
                }
            }
            else {
                cmdVPlay.BackgroundImage = imageList1.Images[8];
            }
            picCel.Select();
        }

        private void chkTrans_Click(object sender, EventArgs e) {
            //toggle transparency
            blnTrans = !blnTrans;
            //force update
            DisplayCel();
            // show or hide panel grid as needed
            if (blnTrans) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
            else {
                pnlCel.CreateGraphics().Clear(BackColor);
            }
        }

        private void dLoop_Click(object sender, EventArgs e) {
            if (agView.Loops.Count > 1) {
                //stop motion
                tmrMotion.Enabled = false;
                //decrement loop, wrapping around
                CurLoop = ((CurLoop == 0) ? agView.Loops.Count - 1 : CurLoop - 1);
                DisplayLoop();
            }
        }

        private void uLoop_Click(object sender, EventArgs e) {
            if (agView.Loops.Count > 1) {
                //stop motion
                tmrMotion.Enabled = false;
                // increment loop, wrapping around
                CurLoop = ((CurLoop == agView.Loops.Count - 1) ? 0 : CurLoop + 1);
                DisplayLoop();
            }
        }

        private void dCel_Click(object sender, EventArgs e) {
            if (agView[CurLoop].Cels.Count > 1) {
                //stop motion
                tmrMotion.Enabled = false;
                //decrement cel, wrapping around
                CurCel = (CurCel == 0) ? agView[CurLoop].Cels.Count - 1 : CurCel - 1;
                DisplayCel();
            }
        }

        private void uCel_Click(object sender, EventArgs e) {
            if (agView[CurLoop].Cels.Count > 1) {
                // stop motion
                tmrMotion.Enabled = false;
                // increment cel, wrapping around
                CurCel = (CurCel == agView[CurLoop].Cels.Count - 1) ? 0 : CurCel + 1;
                DisplayCel();
            }
        }

        private void tbbZoomIn_Click(object sender, EventArgs e) {
            ZoomPrev(1);
        }

        private void tbbZoomOut_Click(object sender, EventArgs e) {
            ZoomPrev(-1);
        }

        private void tbbAlignLeft_Click(object sender, EventArgs e) {
            lngHAlign = 0;
            HAlign.ImageIndex = lngHAlign + 2;
        }

        private void tbbAlignCenter_Click(object sender, EventArgs e) {
            // update alignment, and redraw
            lngHAlign = 1;
            HAlign.ImageIndex = lngHAlign + 2;
        }

        private void tbbAlignRight_Click(object sender, EventArgs e) {
            lngHAlign = 2;
            HAlign.ImageIndex = lngHAlign + 2;
        }

        private void tbbTop_Click(object sender, EventArgs e) {
            lngVAlign = 0;
            VAlign.ImageIndex = lngVAlign + 5;
        }

        private void tbbMiddle_Click(object sender, EventArgs e) {
            lngVAlign = 1;
            VAlign.ImageIndex = lngVAlign + 5;
        }

        private void tbbBottom_Click(object sender, EventArgs e) {
            lngVAlign = 2;
            VAlign.ImageIndex = lngVAlign + 5;
        }

        private void sldSpeed_ValueChanged(object sender, EventArgs e) {
            tmrMotion.Interval = 600 / sldSpeed.Value - 45;
        }

        private void hsbView_Scroll(object sender, ScrollEventArgs e) {
            //position the cel
            picCel.Left = -hsbView.Value;
            if (blnTrans) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
        }

        private void vsbView_Scroll(object sender, ScrollEventArgs e) {
            //position the cel
            picCel.Top = -vsbView.Value;
            if (blnTrans) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
        }

        private void pnlCel_Resize(object sender, EventArgs e) {
            //position scrollbars
            hsbView.Top = pnlCel.Height - hsbView.Height;
            hsbView.Width = pnlCel.Width;
            vsbView.Left = pnlCel.Width - vsbView.Width;
            vsbView.Height = pnlCel.Height;
            SetVScrollbars();
            if (blnTrans) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
        }

        private void pnlCel_Paint(object sender, PaintEventArgs e) {
            if (chkTrans.Checked) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
        }

        private void picCel_DoubleClick(object sender, EventArgs e) {
            //open view for editing
            OpenGameView((byte)SelResNum);
        }

        private void picCel_MouseDown(object sender, MouseEventArgs e) {
            //if either scrollbar is visible,
            if (hsbView.Visible || vsbView.Visible) {
                //set dragView mode
                blnDraggingView = true;

                //set pointer to custom
                pnlView.Cursor = Cursors.Hand;
                //save x and Y offsets
                intOffsetX = e.X;
                intOffsetY = e.Y;
            }
        }

        private void picCel_MouseUp(object sender, MouseEventArgs e) {
            //if dragging
            if (blnDraggingView) {
                //cancel dragmode
                blnDraggingView = false;
                picCel.Cursor = Cursors.Default;
            }
        }

        private void picCel_MouseWheel(object sender, MouseEventArgs e) {
            switch (e.Delta) {
            case < 0:
                // wheel down
                ZoomPrev(-1);
                break;
            case > 0:
                // wheel up
                ZoomPrev(1);
                break;
            }
        }

        private void picCel_MouseLeave(object sender, EventArgs e) {
            if (this != MDIMain.ActiveMdiChild) {
                return;
            }
            MainStatusBar.Items["spStatus"].Text = "";
        }

        void pnlView_DoubleClick(object sender, EventArgs e) {
            //let user change background color
            frmPalette NewPallete = new(1);
            NewPallete.ShowDialog(MDIMain);
            pnlView.BackColor = PrevWinBColor;
            picCel.BackColor = PrevWinBColor;
            //toolbars stay default gray, but that's OK

            //force redraw of cel
            DisplayCel();
            if (blnTrans) {
                DrawTransGrid(pnlCel, picCel.Left % 10, picCel.Top % 10);
            }
        }

        void picCel_MouseMove(object sender, MouseEventArgs e) {
            if (this != MDIMain.ActiveMdiChild) {
                return;
            }
            // if left mouse button is down and either or both scrollbars are
            // visible, scroll the image;
            // if mouse button is up, show coordinates

            if (e.Button == MouseButtons.Left) {
                int tmpHVal = 0, tmpVVal = 0;
                int DX = 0, DY = 0;

                //if not active form
                if (MDIMain.ActiveMdiChild != this) {
                    return;
                }

                //if dragging view
                if (blnDraggingView) {
                    //if vertical scrollbar is visible
                    if (vsbView.Visible) {
                        // adjust scroll bars by amount of delta from 
                        // starting offset (subract from scrollbar value, since
                        // scrollbar value = - picture offset)
                        tmpVVal = vsbView.Value - (e.Y - intOffsetY);
                        //limit to valid values
                        if (tmpVVal < vsbView.Minimum) {
                            tmpVVal = vsbView.Minimum;
                        }
                        else if (tmpVVal > (vsbView.Maximum - vsbView.LargeChange)) {
                            tmpVVal = vsbView.Maximum - vsbView.LargeChange;
                        }
                        // adjust if it changed
                        DY = tmpVVal - vsbView.Value;
                        if (DY != 0) {
                            //set vertical scrollbar
                            vsbView.Value = tmpVVal;
                        }
                    }
                    //repeat for horizontal scrollbar
                    if (hsbView.Visible) {
                        tmpHVal = hsbView.Value - (e.X - intOffsetX);
                        //limit positions to valid values
                        if (tmpHVal < hsbView.Minimum) {
                            tmpHVal = hsbView.Minimum;
                        }
                        else if (tmpHVal > (hsbView.Maximum - hsbView.LargeChange)) {
                            tmpHVal = hsbView.Maximum - hsbView.LargeChange;
                        }
                        DX = tmpHVal - hsbView.Value;
                        if (DX != 0) {
                            //set horizontal scrollbar
                            hsbView.Value = tmpHVal;
                        }
                    }
                    //move the image to this location
                    if (DX != 0 || DY != 0) {
                        picCel.Location = new Point(-tmpHVal, -tmpVVal);
                    }
                    // NEVER update offset; it's always relative to original starting point of the drag
                }
            }
            else if (e.Button == MouseButtons.Right) {

            }
            else if (e.Button == MouseButtons.None) {
                // show coordinates in statusbar
                MainStatusBar.Items[nameof(spStatus)].Text = $"X: {e.X / 2 / ViewScale}    Y: {e.Y / ViewScale}";
            }
        }

        private void tmrMotion_Tick(object sender, EventArgs e) {
            //advance to next cel, depending on mode
            switch (cmbMotion.SelectedIndex) {
            case 0:  //normal
                CurCel = (CurCel == (agView[CurLoop].Cels.Count - 1)) ? 0 : CurCel + 1;
                break;
            case 1: //reverse
                CurCel = (CurCel == 0) ? agView[CurLoop].Cels.Count - 1 : CurCel - 1;
                break;
            case 2:  //end of loop
                if (CurCel == agView[CurLoop].Cels.Count - 1) {
                    //stop motion
                    tmrMotion.Enabled = false;
                    //show play
                    //cmdVPlay.BackgroundImage = imageList1.Images[8];
                    return;
                }
                else {
                    CurCel++;
                }
                break;
            case 3:  //reverse loop
                if (CurCel == 0) {
                    //stop motion
                    tmrMotion.Enabled = false;
                    cmdVPlay.BackgroundImage = imageList1.Images[8];
                    return;
                }
                else {
                    CurCel--;
                }
                break;
            }
            DisplayCel();
        }

        #endregion

        #endregion

        #region Form Methods
        private void PositionPreview() {
            int sngLeft, sngTop;
            int sngWidth, sngHeight;

            // get preview window position
            sngWidth = WinAGISettingsFile.GetSetting(sPOSITION, "PreviewWidth", (int)(0.4 * MDIMain.Bounds.Width));
            if (sngWidth <= MIN_WIDTH) {
                sngWidth = MIN_WIDTH;
            }
            else if (sngWidth > 0.75 * Screen.GetWorkingArea(this).Width) {
                sngWidth = (int)(0.75 * Screen.GetWorkingArea(this).Width);
            }
            sngHeight = WinAGISettingsFile.GetSetting(sPOSITION, "PreviewHeight", (int)(0.5 * MDIMain.Bounds.Height));
            if (sngHeight <= MIN_HEIGHT) {
                sngHeight = MIN_HEIGHT;
            }
            else if (sngHeight > 0.75 * Screen.GetWorkingArea(this).Height) {
                sngHeight = (int)(0.75 * Screen.GetWorkingArea(this).Height);
            }
            sngLeft = WinAGISettingsFile.GetSetting(sPOSITION, "PreviewLeft", 0);
            if (sngLeft < 0) {
                sngLeft = 0;
            }
            else {
                if (WinAGISettings.ResListType.Value != EResListType.None) {
                    if (sngLeft > MDIMain.Width - MDIMain.pnlResources.Width - 300) {
                        sngLeft = MDIMain.Width - MDIMain.pnlResources.Width - 300;
                    }
                }
                else {
                    if (sngLeft > MDIMain.Width - 300) {
                        sngLeft = MDIMain.Width - 300;
                    }
                }
            }
            sngTop = WinAGISettingsFile.GetSetting(sPOSITION, "PreviewTop", 0);
            if (sngTop < 0) {
                sngTop = 0;
            }
            else {
                if (sngTop > MDIMain.Bounds.Height - 300) {
                    sngTop = MDIMain.Bounds.Height - 300;
                }
            }
            //now move the form
            this.Bounds = new Rectangle(sngLeft, sngTop, sngWidth, sngHeight);
        }

        public void ClearPreviewWin() {
            // if the resource being cleared has recently been deleted
            // we don't need to unload it, just dereference it
            // unload view
            if (agView is not null) {
                if (tmrMotion.Enabled) {
                    tmrMotion.Enabled = false;
                }
                //if resource exists, it should still be loaded
                if (agView.Loaded) {
                    agView.Unload();
                }
                agView = null;
            }
            //unload picture,
            if (agPic is not null) {
                //if resource exists, it should still be loaded
                if (agPic.Loaded) {
                    agPic.Unload();
                }
                agPic = null;
            }
            //unload sound
            if (agSound is not null) {
                //if resource exists, it should still be loaded
                if (agSound.Loaded) {
                    //stop sound, if it is playing
                    agSound.StopSound();
                    //unload sound
                    agSound.Unload();
                }
                //stop sound
                agSound.StopSound();
                // always unhook the event handler
                agSound.SoundComplete -= This_SoundComplete;
                agSound = null;
                //ensure timer is off
                tmrSound.Enabled = false;
                //reset progress bar
                picProgress.Width = 0;
                btnStop.Enabled = false;
            }
            //unload logic
            if (agLogic is not null) {
                //if resource exists, it should still be loaded
                if (agLogic.Loaded) {
                    agLogic.Unload();
                }
                agLogic = null;
            }
            if (!Visible) {
                return;
            }
            using Graphics cg = CreateGraphics();
            cg.Clear(base.BackColor);
            pnlLogic.Visible = false;
            pnlPicture.Visible = false;
            pnlSound.Visible = false;
            pnlView.Visible = false;
            PrevResType = None;
            // default caption
            Text = "Preview";
        }

        public void LoadPreview(AGIResType ResType, int ResNum) {
            // the desired resource must be loaded before showing its preview

            //show wait cursor
            UseWaitCursor = true;
            //if changing restype, or showing a header
            if (SelResType != ResType || ResNum == -1) {
                //clear resources
                ClearPreviewWin();
            }
            // always unload previous resources
            agLogic?.Unload();
            agPic?.Unload();
            agView?.Unload();
            if (agSound is not null) {
                // if a sound is already being previewed, make sure it gets stopped
                // before switching to the new sound
                StopSoundPreview();
                agSound.Unload();
            }
            // if one of the four main resource types and not a header
            if ((int)ResType >= 0 && (int)ResType <= 3 && ResNum >= 0) {
                UpdateCaption(ResType, (byte)ResNum);
                //get size, show preview
                switch (ResType) {
                case AGIResType.Logic:
                    if (PreviewLogic((byte)ResNum)) {
                        pnlLogic.Visible = true;
                    }
                    else {
                        pnlLogic.Visible = false;
                        using Graphics cg = CreateGraphics();
                        cg.Clear(base.BackColor);
                        string errMsg = "";
                        switch (EditGame.Logics[ResNum].SrcErrLevel) {
                        case -1:
                            errMsg = "SOURCE FILE ERROR: Source file is missing.";
                            break;
                        case -2:
                            errMsg = "SOURCE FILE ERROR: Source file is marked readonly.";
                            break;
                        case -3:
                            errMsg = "SOURCE FILE ERROR: File access error.";
                            break;
                        }
                        cg.DrawString(errMsg, base.Font, new SolidBrush(Color.Black), 0, 0);
                    }
                    break;
                case AGIResType.Picture:
                    if (PreviewPic((byte)ResNum)) {
                        pnlPicture.Visible = true;
                    }
                    else {
                        pnlPicture.Visible = false;
                        using Graphics cg = CreateGraphics();
                        cg.Clear(base.BackColor);
                        string errMsg = "";
                        switch (EditGame.Pictures[ResNum].ErrLevel) {
                        case -1:
                            errMsg = $"PICTURE RESOURCE ERROR: VOL File (VOL.{EditGame.Pictures[ResNum].Volume}) does not exist.";
                            break;
                        case -2:
                            errMsg = $"PICTURE RESOURCE ERROR: VOL File (VOL.{EditGame.Pictures[ResNum].Volume}) is marked readonly.";
                            break;
                        case -3:
                            errMsg = $"PICTURE RESOURCE ERROR: VOL File (VOL.{EditGame.Pictures[ResNum].Volume}) file access error.";
                            break;
                        case -4:
                            errMsg = $"PICTURE RESOURCE ERROR: Invalid Location index ({EditGame.Pictures[ResNum].Loc}) for this resource.";
                            break;
                        case -5:
                            errMsg = "PICTURE RESOURCE ERROR: Invalid resource header.";
                            break;
                        case -6:
                            errMsg = "PICTURE RESOURCE ERROR: Resource decompression error.";
                            break;
                        }
                        cg.DrawString(errMsg, base.Font, new SolidBrush(Color.Black), 0, 0);
                    }
                    break;
                case AGIResType.Sound:
                    if (PreviewSound((byte)ResNum)) {
                        pnlSound.Visible = true;
                        // hook the sound_complete event
                        agSound.SoundComplete += This_SoundComplete;
                    }
                    else {
                        pnlSound.Visible = false;
                        using Graphics cg = CreateGraphics();
                        cg.Clear(base.BackColor);
                        string errMsg = "";
                        switch (EditGame.Sounds[ResNum].ErrLevel) {
                        case -1:
                            errMsg = $"SOUND RESOURCE ERROR: VOL File (VOL.{EditGame.Sounds[ResNum].Volume}) does not exist.";
                            break;
                        case -2:
                            errMsg = $"SOUND RESOURCE ERROR: VOL File (VOL.{EditGame.Sounds[ResNum].Volume}) is marked readonly.";
                            break;
                        case -3:
                            errMsg = $"SOUND RESOURCE ERROR: VOL File (VOL.{EditGame.Sounds[ResNum].Volume}) file access error.";
                            break;
                        case -4:
                            errMsg = $"SOUND RESOURCE ERROR: Invalid Location index ({EditGame.Sounds[ResNum].Loc}) for this resource.";
                            break;
                        case -5:
                            errMsg = "SOUND RESOURCE ERROR: Invalid resource header.";
                            break;
                        case -6:
                            errMsg = "SOUND RESOURCE ERROR: Resource decompression error.";
                            break;
                        }
                        cg.DrawString(errMsg, base.Font, new SolidBrush(Color.Black), 0, 0);
                    }
                    break;
                case AGIResType.View:
                    if (PreviewView((byte)ResNum)) {
                        pnlView.Visible = true;
                    }
                    else {
                        pnlView.Visible = false;
                        using Graphics cg = CreateGraphics();
                        cg.Clear(base.BackColor);
                        string errMsg = "";
                        switch (EditGame.Views[ResNum].ErrLevel) {
                        case -1:
                            errMsg = $"VIEW RESOURCE ERROR: VOL File (VOL.{EditGame.Views[ResNum].Volume}) does not exist.";
                            break;
                        case -2:
                            errMsg = $"VIEW RESOURCE ERROR: VOL File (VOL. {EditGame.Views[ResNum].Volume}) is marked readonly.";
                            break;
                        case -3:
                            errMsg = $"VIEW RESOURCE ERROR: VOL File (VOL.{EditGame.Views[ResNum].Volume}) file access error.";
                            break;
                        case -4:
                            errMsg = $"VIEW RESOURCE ERROR: Invalid Location index ({EditGame.Views[ResNum].Loc}) for this resource.";
                            break;
                        case -5:
                            errMsg = "VIEW RESOURCE ERROR: Invalid resource header.";
                            break;
                        case -6:
                            errMsg = "VIEW RESOURCE ERROR: Resource decompression error.";
                            break;
                        }
                        cg.DrawString(errMsg, base.Font, new SolidBrush(Color.Black), 0, 0);
                    }
                    break;
                }
            }
            //restore mouse pointer
            this.UseWaitCursor = false;
            //set restype
            PrevResType = ResType;
        }

        public void UpdateCaption(AGIResType ResType, byte ResNum) {
            string strID = "";
            // update window caption
            Text = "Preview - " + ResType + " " + ResNum;
            // if not showing by number in the prvieww list
            if (!WinAGISettings.ShowResNum.Value) {
                //also include the resource ID
                switch (ResType) {
                case AGIResType.Logic:
                    strID = EditGame.Logics[ResNum].ID;
                    break;
                case AGIResType.Picture:
                    strID = EditGame.Pictures[ResNum].ID;
                    break;
                case AGIResType.Sound:
                    strID = EditGame.Sounds[ResNum].ID;
                    break;
                case AGIResType.View:
                    strID = EditGame.Views[ResNum].ID;
                    break;
                }
                strID = "   (" + strID + ")";
                this.Text += strID;
            }
        }

        private void SavePreviewPos() {
            //save preview window pos
            WinAGISettingsFile.WriteSetting(sPOSITION, "PreviewTop", Top);
            WinAGISettingsFile.WriteSetting(sPOSITION, "PreviewLeft", Left);
            WinAGISettingsFile.WriteSetting(sPOSITION, "PreviewWidth", Width);
            WinAGISettingsFile.WriteSetting(sPOSITION, "PreviewHeight", Height);
        }

        public void KeyHandler(KeyPressEventArgs e) {
            switch (SelResType) {
            case AGIResType.Picture:
                switch ((int)e.KeyChar) {
                case 43: //+//
                         //zoom in
                    if (udPZoom.Value < 4) {
                        udPZoom.Value++;
                    }
                    break;
                case 45: //-//
                         //zoom out
                    if (udPZoom.Value > 1) {
                        udPZoom.Value--;
                    }
                    break;
                }
                break;
            case AGIResType.View:
                switch ((int)e.KeyChar) {
                case 32: // //
                         //toggle play/pause
                    cmdVPlay_Click(null, null);
                    break;
                case 43: //+//
                         //zoom in
                    ZoomPrev(1);
                    break;
                case 45: //-//
                         //zoom out
                    ZoomPrev(-1);
                    break;
                case 65:
                case 97: //a//
                    dCel_Click(null, null);
                    break;
                case 83:
                case 115: //s//
                    uCel_Click(null, null);
                    break;
                case 81:
                case 113: //q//
                    dLoop_Click(null, null);
                    break;
                case 87:
                case 119: //w//
                    uLoop_Click(null, null);
                    break;
                }
                break;
            }
        }

        void MenuClickFind(FindFormFunction ffValue = FindFormFunction.FindLogic) {
            //don't need the find form; just go directly to the find function

            //set form defaults
            switch (SelResType) {
            case AGIResType.Logic:
                GFindText = EditGame.Logics[(byte)SelResNum].ID;
                break;
            case AGIResType.Picture:
                GFindText = EditGame.Pictures[(byte)SelResNum].ID;
                break;
            case AGIResType.Sound:
                GFindText = EditGame.Sounds[(byte)SelResNum].ID;
                break;
            case AGIResType.View:
                GFindText = EditGame.Views[(byte)SelResNum].ID;
                break;
            }
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = false;

            FindingForm.ResetSearch();
            FindInLogic(MDIMain, GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc);
        }

        void MenuClickHelp() {
            string strTopic;
            //show preview window help
            strTopic = "htm\\winagi\\preview.htm";
            if (SelResType == AGIResType.Logic ||
                SelResType == AGIResType.Picture ||
                SelResType == AGIResType.Sound ||
                SelResType == AGIResType.View) {
                strTopic += "#" + SelResType;
            }
            // show preview window help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, strTopic);
        }

        #endregion

        #region Preview Logic Methods
        bool PreviewLogic(byte LogNum) {
            agLogic = EditGame.Logics[LogNum];
            agLogic.Load();
            // check for errors
            if (agLogic.SrcErrLevel < 0) {
                // logics are different - only source error will
                // cause preview to fail
                return false;
            }
            // get the source code
            rtfLogPrev.Text = agLogic.CodePage.GetString(agLogic.CodePage.GetBytes(agLogic.SourceText));
            //rtfLogPrev.Text = agLogic.SourceText;
            rtfLogPrev.DoCaretVisible();
            // set background
            rtfLogPrev.BackColor = agLogic.Compiled ? Color.FromArgb(0xE0, 0xFF, 0xE0) : Color.FromArgb(0xFF, 0xE0, 0xe0);
            return true;
        }

        #endregion

        #region Preview Picture Methods
        bool PreviewPic(byte PicNum) {
            //get the picture
            agPic = EditGame.Pictures[PicNum];
            agPic.Load();
            // check for errors
            if (agPic.ErrLevel < 0) {
                agPic.Unload();
                return false;
            }
            DisplayPicture();
            return true;
        }

        void DisplayPicture() {
            //resize picture Image holder
            imgPicture.Width = 320 * PicScale;
            imgPicture.Height = 168 * PicScale;

            //if visual picture is being displayed
            if (optVisual.Checked == true) {
                //load visual Image
                ShowAGIBitmap(imgPicture, agPic.VisualBMP, PicScale);
            }
            else {
                // load priority Image
                ShowAGIBitmap(imgPicture, agPic.PriorityBMP, PicScale);
            }
            //set scrollbars if necessary
            SetPScrollbars();
        }

        void SetPScrollbars() {
            //// if panel is not visible, no need to adjust scrollbars
            //if (!panel2.Visible) {
            //  return;
            //}
            //determine if scrollbars are necessary
            blnPicHSB = (imgPicture.Width > (pnlPicImage.Width - 2 * PW_MARGIN));
            blnPicVSB = (imgPicture.Height > (pnlPicImage.Height - 2 * PW_MARGIN - (blnPicHSB ? hsbPic.Height : 0)));
            //check horizontal again(incase addition of vert scrollbar forces it to be shown)
            blnPicHSB = (imgPicture.Width > (pnlPicImage.Width - 2 * PW_MARGIN - (blnPicVSB ? vsbPic.Width : 0)));
            //if both are visibile
            if (blnPicHSB && blnPicVSB) {
                //move back from corner
                hsbPic.Width = pnlPicImage.Width - vsbPic.Width;
                vsbPic.Height = pnlPicImage.Height - hsbPic.Height;
                //show corner
                fraPCorner.Visible = true;
            }
            else {
                fraPCorner.Visible = false;
            }
            // if visible, set large/small change values, then max values
            // note that in .NET, the actual highest value attainable in a
            // scrollbar is NOT the Maximum value; it's Maximum - LargeChange + 1!!
            // that seems really dumb, but it's what happens...

            //set change and Max values for horizontal bar
            if (blnPicHSB) {
                // changes are based on size of the visible panel
                //(LargeChange value can't exceed Max value, so force Max to high enough value;
                // it will be calculated correctly later)
                hsbPic.Maximum = pnlPicImage.Width;
                hsbPic.LargeChange = (int)(pnlPicImage.Width * LG_SCROLL);
                hsbPic.SmallChange = (int)(pnlPicImage.Width * SM_SCROLL);
                // calculate control MAX value - equals desired actual Max + LargeChange - 1
                hsbPic.Maximum = imgPicture.Width - (pnlPicImage.Width - (blnPicVSB ? vsbPic.Width : 0)) + PW_MARGIN + hsbPic.LargeChange - 1;

                //always reposition picture holder back to default
                imgPicture.Left = PW_MARGIN;
                // set value to current 
                hsbPic.Value = -imgPicture.Left;
            }
            // repeat for vertical bar
            if (blnPicVSB) {
                vsbPic.Maximum = pnlPicImage.Height;
                vsbPic.LargeChange = (int)(pnlPicImage.Height * LG_SCROLL); //90% for big jump
                vsbPic.SmallChange = (int)(pnlPicImage.Height * SM_SCROLL); //22.5% for small jump
                vsbPic.Maximum = imgPicture.Height - (pnlPicImage.Bounds.Height - (blnPicHSB ? hsbPic.Height : 0)) + PW_MARGIN + vsbPic.LargeChange - 1;
                imgPicture.Top = PW_MARGIN;
                vsbPic.Value = -imgPicture.Top;
            }
            //set visible properties for scrollbars
            hsbPic.Visible = blnPicHSB;
            vsbPic.Visible = blnPicVSB;
        }

        #endregion

        #region PreviewSound Methods
        bool PreviewSound(byte SndNum) {
            int i;

            //get new sound
            agSound = EditGame.Sounds[SndNum];
            // load the resource
            agSound.Load();
            // check for errors
            if (agSound.ErrLevel < 0) {
                agSound.Unload();
                return false;
            }
            switch (agSound.SndFormat) {
            case SoundFormat.AGI:
                optMIDI.Text = "MIDI Sound";
                optPCjr.Enabled = true;
                for (i = 0; i < 3; i++) {
                    cmbInst[i].Enabled = optMIDI.Checked;
                    cmbInst[i].SelectedIndex = agSound.Tracks[i].Instrument;
                    chkTrack[i].Enabled = optMIDI.Checked;
                    chkTrack[i].Checked = !agSound.Tracks[i].Muted;
                }
                chkTrack[3].Checked = !agSound.Tracks[3].Muted;
                chkTrack[3].Enabled = optMIDI.Checked;
                lblFormat.Text = "PC/PCjr Standard Sound";
                break;
            case SoundFormat.WAV:
                optMIDI.Text = "WAV Sound";
                optMIDI.Checked = true;
                optPCjr.Enabled = false;
                for (i = 0; i < 3; i++) {
                    cmbInst[i].Enabled = false;
                    cmbInst[i].SelectedIndex = -1;
                    chkTrack[i].Enabled = false;
                    chkTrack[i].Checked = false;
                }
                chkTrack[3].Enabled = false;
                chkTrack[3].Checked = false;
                lblFormat.Text = "Apple IIgs PCM Sound";
                break;
            case SoundFormat.MIDI:
                optMIDI.Text = "MIDI Sound";
                optMIDI.Checked = true;
                optPCjr.Enabled = false;
                for (i = 0; i < 3; i++) {
                    cmbInst[i].Enabled = false;
                    cmbInst[i].SelectedIndex = -1;
                    chkTrack[i].Enabled = false;
                    chkTrack[i].Checked = false;
                }
                chkTrack[3].Enabled = false;
                chkTrack[3].Checked = false;
                lblFormat.Text = "Apple IIgs MIDI Sound";
                break;
            }
            //set length
            lblLength.Text = "Sound clip length: " + agSound.Length.ToString("0.0") + " seconds";
            btnPlay.Enabled = true;
            return true;
        }

        public void StopSoundPreview() {
            // stop sound
            agSound?.StopSound();
        }

        void SetMIDIControls(bool enabled) {
            for (int i = 0; i < 3; i++) {
                chkTrack[i].Enabled = enabled && optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[i].Muted;
                cmbInst[i].Enabled = enabled && optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[i].Muted;
            }
            chkTrack[3].Enabled = enabled && optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[3].Muted;
            cmdReset.Enabled = enabled && optMIDI.Checked && agSound.SndFormat == SoundFormat.AGI && !agSound[3].Muted;
        }

        #endregion

        #region Preview View Methods
        bool PreviewView(byte ViewNum) {
            //load resource for this view
            agView = EditGame.Views[ViewNum];
            agView.Load();
            // check for errors
            if (agView.ErrLevel < 0) {
                agView.Unload();
                return false;
            }
            //show correct toolbars for alignment
            HAlign.ImageIndex = lngHAlign + 2;
            VAlign.ImageIndex = lngVAlign + 5;

            // display the first loop (which will display the first cel)
            CurLoop = 0;
            DisplayLoop();
            return true;
        }

        void ZoomPrev(int Dir) {
            int mW, mH;
            //get current maxH and maxW (by de-calculating)
            mW = picCel.Width / ViewScale / 2;
            mH = picCel.Height / ViewScale;

            if (Dir == 1) {
                ViewScale++;
                if (ViewScale == 17) {
                    ViewScale = 16;
                    return;
                }
            }
            else {
                ViewScale--;
                if (ViewScale == 0) {
                    ViewScale = 1;
                    return;
                }
            }
            //now rezize cel
            picCel.Width = mW * 2 * ViewScale;
            picCel.Height = mH * ViewScale;
            //set scrollbars
            SetVScrollbars();
            //then redraw the cel
            DisplayCel();
        }

        void DisplayLoop() {
            int i, mW, mH;
            // update loop label
            udLoop.Text = $"Loop {CurLoop} / {agView.Loops.Count - 1}";
            // reset cel
            CurCel = 0;
            udCel.Text = "Cel 0 / " + (agView[CurLoop].Cels.Count - 1);
            // enable play/stop if more than one cel
            cmdVPlay.Enabled = (agView[CurLoop].Cels.Count > 1);
            cmdVPlay.BackgroundImage = imageList1.Images[8];
            // determine size of holding pic
            mW = 0;
            mH = 0;
            for (i = 0; i <= agView[CurLoop].Cels.Count - 1; i++) {
                if (agView[CurLoop].Cels[i].Width > mW) {
                    mW = agView.Loops[CurLoop].Cels[i].Width;
                }
                if (agView.Loops[CurLoop].Cels[i].Height > mH) {
                    mH = agView.Loops[CurLoop].Cels[i].Height;
                }
            }
            // old image needs to be cleared to avoid ghost images
            // when resizing the cel image
            picCel.Image = new Bitmap(picCel.Width, picCel.Height);
            picCel.Refresh();
            picCel.Width = mW * 2 * ViewScale;
            picCel.Height = mH * ViewScale;
            //force back to upper, left
            picCel.Top = PW_MARGIN;
            picCel.Left = PW_MARGIN;

            //set scroll bars everytime loop is changed
            SetVScrollbars();
            DisplayCel();
        }

        void SetVScrollbars() {
            // determine if scrollbars are necessary
            blnViewHSB = (picCel.Width > (pnlCel.Width - 2 * PW_MARGIN));
            blnViewVSB = (picCel.Height > (pnlCel.Height - 2 * PW_MARGIN - (blnViewHSB ? hsbView.Height : 0)));
            //check horizontal again(in case addition of vert scrollbar forces it to be shown)
            blnViewHSB = (picCel.Width > (pnlCel.Width - 2 * PW_MARGIN - (blnViewVSB ? vsbView.Width : 0)));
            // if both are visibile
            if (blnViewHSB && blnViewVSB) {
                //move back from corner
                hsbView.Width = pnlCel.Width - vsbView.Width;
                vsbView.Height = pnlCel.Height - hsbView.Height;
                //show corner
                fraVCorner.Visible = true;
            }
            else {
                fraVCorner.Visible = false;
            }

            // if visible, set large/small change values, then max values
            // note that in .NET, the actual highest value attainable in a
            // scrollbar is NOT the Maximum value; it's Maximum - LargeChange + 1!!
            // that seems really dumb, but it's what happens...

            if (blnViewHSB) {
                // set change and Max values for horizontal bar
                //(LargeChange value can't exceed Max value, so force Max to high enough value;
                hsbView.Maximum = pnlCel.Width;
                // it will be calculated correctly later)
                hsbView.LargeChange = (int)(pnlCel.Width * LG_SCROLL);
                hsbView.SmallChange = (int)(pnlCel.Width * SM_SCROLL);
                // Max value: = desired actual Max + LargeChange - 1
                hsbView.Maximum = picCel.Width - (pnlCel.Width - (blnViewVSB ? vsbView.Width : 0)) + PW_MARGIN + hsbView.LargeChange - 1;
            }
            // repeeat for vertical scrollbar
            if (blnViewVSB) {
                vsbView.Maximum = pnlCel.Height;
                vsbView.LargeChange = (int)(pnlCel.Height * LG_SCROLL);
                vsbView.SmallChange = (int)(pnlCel.Height * SM_SCROLL);
                vsbView.Maximum = picCel.Height - (pnlCel.Height - (blnViewHSB ? hsbView.Height : 0)) + PW_MARGIN + vsbView.LargeChange - 1;
            }
            // set visible properties
            hsbView.Visible = blnViewHSB;
            vsbView.Visible = blnViewVSB;
            DontDraw = false;
            return;
        }

        public bool DisplayCel() {
            //this function copies the bitmap Image
            //from CurLoop.CurCel into the view Image box,
            //and resizes it to be correct size
            int tgtX = 0, tgtY = 0, tgtH, tgtW;
            // update ud caption
            udCel.Text = $"Cel {CurCel} / {agView[CurLoop].Cels.Count - 1}";
            //set transparent color for the toolbox image
            picTrans.Image = new Bitmap(picTrans.Width, picTrans.Height);
            Graphics.FromImage(picTrans.Image).Clear(EditGame.Palette[(int)agView[CurLoop][CurCel].TransColor]);

            //copy view Image
            tgtW = agView[CurLoop][CurCel].Width * 2 * ViewScale;
            tgtH = agView[CurLoop][CurCel].Height * ViewScale;
            switch (lngHAlign) {
            case 0:
                tgtX = 0;
                break;
            case 1:
                tgtX = (picCel.Width - tgtW) / 2;
                break;
            case 2:
                tgtX = picCel.Width - tgtW;
                break;
            }
            switch (lngVAlign) {
            case 0:
                tgtY = 0;
                break;
            case 1:
                tgtY = (picCel.Height - tgtH) / 2;
                break;
            case 2:
                tgtY = picCel.Height - tgtH;
                break;
            }
            // set transparency
            agView[CurLoop][CurCel].Transparency = blnTrans;

            /*------------------------------------------------------------*/
            // this, along with related mod to ShowAGIBitmap, does not work
            // right-it creates very bad flicker - I can't find a way to
            // stop the flickering
            /*------------------------------------------------------------*/
            /*
            picCel.CreateGraphics().Clear(picCel.BackColor);
            if (blnTrans) {
                DrawTransGrid(picCel, 0, 0);
            }
            ShowAGIBitmap(picCel, agView[CurLoop][CurCel].CelBMP, tgtX, tgtY, tgtW, tgtH);
            */

            /*------------------------------------------------------------*/
            /* this method works with no flicker at all: */
            /*------------------------------------------------------------*/
            // create new image in the picture box that is desired size
            picCel.Image = new Bitmap(picCel.Width, picCel.Height);
            ShowAGIBitmap(picCel, agView[CurLoop][CurCel].CelBMP, tgtX, tgtY, tgtW, tgtH);
            if (blnTrans) {
                // draw single pixel dots spaced 10 pixels apart over transparent pixels only
                using Graphics gc = Graphics.FromImage(picCel.Image);
                Bitmap b = new(picCel.Image);
                for (int i = 0; i < picCel.Width; i += 10) {
                    for (int j = 0; j < picCel.Height; j += 10) {
                        if (b.GetPixel(i, j).ToArgb() == picCel.BackColor.ToArgb()) {
                            gc.FillRectangle(Brushes.Black, new Rectangle(i, j, 1, 1));
                        }
                    }
                }
            }
            /*------------------------------------------------------------*/
            return true;
        }
        #endregion

        private void mnuRExportAll_Click(object sender, EventArgs e) {
            ExportAll(false);
        }
    }
}
