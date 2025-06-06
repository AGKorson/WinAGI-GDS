﻿using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.frmPicEdit;

namespace WinAGI.Editor {
    public partial class frmSettings : Form {
        private agiSettings NewSettings = WinAGISettings.Clone();
        private PicTestInfo NewPicTestSettings = PicEditTestSettings.Clone();
        private bool blnChangeCmtCol;
        private bool blnResetWarnings;
        private TextStyle prevCommentStyle;
        private TextStyle prevStringStyle;
        private TextStyle prevKeyWordStyle;
        private TextStyle prevTestCmdStyle;
        private TextStyle prevActionCmdStyle;
        private TextStyle prevInvalidCmdStyle;
        private TextStyle prevNumberStyle;
        private TextStyle prevArgIdentifierStyle;
        private TextStyle prevDefIdentifierStyle;

        public frmSettings(int starttab = 0, string startprop = "") {
            InitializeComponent();
            MDIMain.UseWaitCursor = true;
            // I don't like doing this, but for now, it's needed- without calling
            // DoEvents, the wait cursor never shows up
            Application.DoEvents();
            //    0,1,2,3=lower, 4,5,6,7=upper, 8,9,10,11=proper
            for (int i = 0; i <= 3; i++) {
                cmbResFormat.Items.Add("logic");
            }
            for (int i = 4; i <= 7; i++) {
                cmbResFormat.Items.Add("LOGIC");
            }
            for (int i = 8; i <= 11; i++) {
                cmbResFormat.Items.Add("Logic");
            }
            for (int i = 0; i <= 11; i++) {
                switch (i % 4) {
                case 0:
                    // .001
                    cmbResFormat.Items[i] += ".001";
                    break;
                case 1:
                    //  001
                    cmbResFormat.Items[i] += " 001";
                    break;
                case 2:
                    // .1
                    cmbResFormat.Items[i] += ".1";
                    break;
                case 3:
                    //  1
                    cmbResFormat.Items[i] += " 1";
                    break;
                }
            }
            // TODO: this takes awhile, maybe move this to app startup, which will also
            // help avoid the need to use DoEvents
            FontFamily[] fontFamilies;
            InstalledFontCollection installedFontCollection = new();
            fontFamilies = installedFontCollection.Families;
            int count = fontFamilies.Length;
            for (int j = 0; j < count; ++j) {
                if (Common.Base.FontIsMonospace(fontFamilies[j])) {
                    cmbEditFont.Items.Add(fontFamilies[j].Name);
                    cmbPrevFont.Items.Add(fontFamilies[j].Name);
                }
            }

            for (int i = 8; i <= 12; i++) {
                cmbEditSize.Items.Add(i);
                cmbPrevSize.Items.Add(i);
            }
            for (int i = 14; i <= 24; i += 2) {
                cmbEditSize.Items.Add(i);
                cmbPrevSize.Items.Add(i);
            }
            for (int i = 0; i < 16; i++) {
                cmbViewDefCol1.Items.Add((AGIColorIndex)i);
                cmbViewDefCol2.Items.Add((AGIColorIndex)i);
            }
            cmbInst0.Items.Clear();
            cmbInst0.Items.AddRange(SoundEditMTrack.InstrumentsConverter.CustomNames);
            cmbInst1.Items.Clear();
            cmbInst1.Items.AddRange(SoundEditMTrack.InstrumentsConverter.CustomNames);
            cmbInst2.Items.Clear();
            cmbInst2.Items.AddRange(SoundEditMTrack.InstrumentsConverter.CustomNames);

            RefreshCodeExample();
            if (MDIMain.ActiveMdiChild == PreviewWin) {
                if (PreviewWin.tmrSound.Enabled) {
                    // stop preview sound
                    PreviewWin.StopSoundPreview();
                }
            }

            prevCommentStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[1].Color.Value), null, WinAGISettings.SyntaxStyle[1].FontStyle.Value);
            prevStringStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[2].Color.Value), null, WinAGISettings.SyntaxStyle[2].FontStyle.Value);
            prevKeyWordStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[3].Color.Value), null, WinAGISettings.SyntaxStyle[3].FontStyle.Value);
            prevTestCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[4].Color.Value), null, WinAGISettings.SyntaxStyle[4].FontStyle.Value);
            prevActionCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[5].Color.Value), null, WinAGISettings.SyntaxStyle[5].FontStyle.Value);
            prevInvalidCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[6].Color.Value), null, WinAGISettings.SyntaxStyle[6].FontStyle.Value);
            prevNumberStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[7].Color.Value), null, WinAGISettings.SyntaxStyle[7].FontStyle.Value);
            prevArgIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[8].Color.Value), null, WinAGISettings.SyntaxStyle[8].FontStyle.Value);
            prevDefIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[9].Color.Value), null, WinAGISettings.SyntaxStyle[9].FontStyle.Value);

            // setup form to match current settings
            InitForm();
            tabControl1.SelectedTab = tabControl1.TabPages[starttab];
            if (startprop.Length > 0) {
                try {
                    tabControl1.SelectedTab.Controls[startprop].Select();
                }
                catch {
                    // ignore errors
                    Debug.Assert(false);
                }
            }
            MDIMain.UseWaitCursor = false;
        }

        #region Form Event Handlers
        private void btnDefault_Click(object sender, EventArgs e) {
            // restore all default values for settings
            // and flag warnings to be reset (only if ok button is pressed)

            // General
            NewSettings.ShowSplashScreen.Reset();
            NewSettings.ShowPreview.Reset();
            NewSettings.ShiftPreview.Reset();
            NewSettings.HidePreview.Reset();
            NewSettings.ResListType.Reset();
            NewSettings.AutoExport.Reset();
            NewSettings.BackupResFile.Reset();
            NewSettings.DefMaxSO.Reset();
            NewSettings.DefMaxVol0.Reset();
            NewSettings.DefCP.Reset();
            NewSettings.DefResDir.Reset();
            // ResFormat
            NewSettings.ShowResNum.Reset();
            NewSettings.IncludeResNum.Reset();
            NewSettings.ResFormatNameCase.Reset();
            NewSettings.ResFormatSeparator.Reset();
            NewSettings.ResFormatNumFormat.Reset();
            // Logics
            NewSettings.AutoWarn.Reset();
            NewSettings.HighlightLogic.Reset();
            NewSettings.HighlightText.Reset();
            NewSettings.LogicTabWidth.Reset();
            NewSettings.MaximizeLogics.Reset();
            NewSettings.AutoQuickInfo.Reset();
            NewSettings.ShowDefTips.Reset();
            NewSettings.ShowDocMap.Reset();
            NewSettings.ShowLineNumbers.Reset();
            NewSettings.DefaultExt.Reset();
            NewSettings.EditorFontName.Reset();
            NewSettings.EditorFontSize.Reset();
            NewSettings.PreviewFontName.Reset();
            NewSettings.PreviewFontSize.Reset();
            NewSettings.ErrorLevel.Reset();
            NewSettings.DefIncludeReserved.Reset();
            NewSettings.UseSnippets.Reset();
            // Syntax Highlights
            NewSettings.EditorBackColor.Reset();
            for (int i = 0; i < 10; i++) {
                NewSettings.SyntaxStyle[i].Color.Reset();
                NewSettings.SyntaxStyle[i].FontStyle.Reset();
            }
            // decompiler
            NewSettings.MsgsByNumber.Reset();
            NewSettings.IObjsByNumber.Reset();
            NewSettings.WordsByNumber.Reset();
            NewSettings.ShowAllMessages.Reset();
            NewSettings.SpecialSyntax.Reset();
            NewSettings.ReservedAsText.Reset();
            NewSettings.CodeStyle.Reset();
            // Pictures
            NewSettings.ShowBands.Reset();
            NewSettings.SplitWindow.Reset();
            NewSettings.PicScalePreview.Reset();
            NewSettings.PicScaleEdit.Reset();
            NewSettings.CursorMode.Reset();
            // Pic Test
            NewPicTestSettings.ObjSpeed.Reset();
            NewPicTestSettings.ObjPriority.Reset();
            NewPicTestSettings.ObjRestriction.Reset();
            NewPicTestSettings.Horizon.Reset();
            NewPicTestSettings.IgnoreHorizon.Reset();
            NewPicTestSettings.IgnoreBlocks.Reset();
            NewPicTestSettings.CycleAtRest.Reset();
            // Sounds
            NewSettings.ShowKeyboard.Reset();
            NewSettings.NoKeyboardSound.Reset();
            NewSettings.PlaybackMode.Reset();
            NewSettings.ShowNotes.Reset();
            NewSettings.OneTrack.Reset();
            for (int i = 0; i < 2; i++) {
                NewSettings.DefInst[i].Reset();
                NewSettings.DefMute[i].Reset();
            }
            NewSettings.DefMute[3].Reset();
            NewSettings.SndZoom.Reset();
            // Views
            NewSettings.ShowGrid.Reset();
            NewSettings.ShowVEPreview.Reset();
            NewSettings.DefPrevPlay.Reset();
            NewSettings.ViewAlignH.Reset();
            NewSettings.ViewAlignV.Reset();
            NewSettings.DefCelH.Reset();
            NewSettings.DefCelW.Reset();
            NewSettings.ViewScalePreview.Reset();
            NewSettings.ViewScaleEdit.Reset();
            NewSettings.DefVColor1.Reset();
            NewSettings.DefVColor2.Reset();
            // Layout
            NewSettings.DefUseLE.Reset();
            NewSettings.LEPages.Reset();
            NewSettings.LEShowPics.Reset();
            NewSettings.LEUseGrid.Reset();
            NewSettings.LEGrid.Reset();
            NewSettings.LESync.Reset();
            NewSettings.LEZoom.Reset();
            NewSettings.RoomEdgeColor.Reset();
            NewSettings.RoomFillColor.Reset();
            NewSettings.TransPtEdgeColor.Reset();
            NewSettings.TransPtFillColor.Reset();
            NewSettings.CmtEdgeColor.Reset();
            NewSettings.CmtFillColor.Reset();
            NewSettings.ErrPtEdgeColor.Reset();
            NewSettings.ErrPtFillColor.Reset();
            NewSettings.ExitEdgeColor.Reset();
            NewSettings.ExitOtherColor.Reset();
            // Globals
            NewSettings.GEShowComment.Reset();
            NewSettings.GENameFrac.Reset();
            NewSettings.GEValFrac.Reset();
            // Menus
            NewSettings.AutoAlignHotKey.Reset();
            // Platform defaults
            NewSettings.AutoFill.Reset();
            NewSettings.PlatformType.Reset();
            NewSettings.PlatformFile.Reset();
            NewSettings.DOSExec.Reset();
            NewSettings.PlatformOpts.Reset();
            // re-init form
            InitForm();

            // force msg reset
            blnResetWarnings = true;
            chkResetWarnings.Checked = true;

            /*
            // redraw colors
            picLEColor_Paint();
            picLESample_Paint();
            picColor_Paint();
            */
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            // no changes made, form no longer needed
            MDIMain.UseWaitCursor = false;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e) {
            // hide form and apply visual settings, then save the settings by writing to settings list
            Hide();

            //  some of the settings changes can take awhile
            MDIMain.UseWaitCursor = true;

            // several settings ned to track changes 
            bool blnChangeNotes = (NewSettings.ShowNotes.Value != WinAGISettings.ShowNotes.Value) || (WinAGISettings.OneTrack.Value != NewSettings.OneTrack.Value);
            bool blnChangeResName = (NewSettings.IncludeResNum.Value != WinAGISettings.IncludeResNum.Value && !NewSettings.ShowResNum.Value) ||
                               (NewSettings.ShowResNum.Value != WinAGISettings.ShowResNum.Value) ||
                               NewSettings.ShowResNum.Value && (NewSettings.ResFormatNameCase.Value != WinAGISettings.ResFormatNameCase.Value ||
                               NewSettings.ResFormatNumFormat.Value != WinAGISettings.ResFormatNumFormat.Value ||
                               NewSettings.ResFormatSeparator.Value != WinAGISettings.ResFormatSeparator.Value);
            bool blnChangeShowPics = NewSettings.LEShowPics.Value != WinAGISettings.LEShowPics.Value;
            bool changePicPrevZoom = NewSettings.PicScalePreview.Value != WinAGISettings.PicScalePreview.Value;
            bool changeViewPrevZoom = NewSettings.ViewScalePreview.Value != WinAGISettings.ViewScalePreview.Value;

            blnChangeCmtCol = NewSettings.GEShowComment.Value != WinAGISettings.GEShowComment.Value;
            EResListType lngResList = WinAGISettings.ResListType.Value;

            // copy new settings back to game settings (this does NOT save them to
            // the settings list; that is done in the save new settings function)
            WinAGISettings = NewSettings.Clone();
            PicEditTestSettings = NewPicTestSettings.Clone();

            // for each section, handle any unique issues associated with the change

            // GENERAL
            Engine.Base.DefMaxSO = WinAGISettings.DefMaxSO.Value;
            Engine.Base.DefMaxVol0Size = WinAGISettings.DefMaxVol0.Value;
            Engine.Base.DefResDir = WinAGISettings.DefResDir.Value;

            // update fonts
            MDIMain.tvwResources.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            MDIMain.lstResources.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            MDIMain.cmbResType.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);

            // if a game is loaded, changes may need to be made immediately:
            if (EditGame != null) {
                switch (WinAGISettings.ResListType.Value) {
                case EResListType.None:
                    // if visible, hide it
                    if (MDIMain.pnlResources.Visible) {
                        MDIMain.HideResTree();
                    }
                    ResetQueue();
                    break;
                default:
                    // if not visible, show it
                    if (!MDIMain.pnlResources.Visible) {
                        SelResType = AGIResType.Game;
                        SelResNum = -1;
                        BuildResourceTree();
                        MDIMain.ShowResTree();
                    }
                    else if (NewSettings.ResListType.Value != lngResList) {
                        SelResType = AGIResType.Game;
                        SelResNum = -1;
                        BuildResourceTree();
                        MDIMain.ShowResTree();
                    }
                    break;
                }

                // set preview font
                PreviewWin.rtfLogPrev.Font = new Font(NewSettings.PreviewFontName.Value, NewSettings.PreviewFontSize.Value);

                // if showing preview window (and using reslist)
                if (WinAGISettings.ShowPreview.Value && WinAGISettings.ResListType.Value != EResListType.None) {
                    // if not visible, show it
                    if (!PreviewWin.Visible) {
                        PreviewWin.Show();
                        if (MDIMain.ActiveMdiChild == PreviewWin) {
                            PreviewWin.LoadPreview(SelResType, SelResNum);
                        }
                    }
                    else {
                        // if it is already visible, update caption
                        if (SelResType >= 0 && SelResType <= AGIResType.View && SelResNum >= 0) {
                            PreviewWin.UpdateCaption(SelResType, (byte)SelResNum);
                        }
                    }
                }
                else {
                    PreviewWin.Hide();
                    if (MDIMain.ActiveMdiChild == null) {
                        switch (WinAGISettings.ResListType.Value) {
                        case 0:
                            MDIMain.LastNodeName = "";
                            break;
                        case EResListType.TreeList:
                            // listtree
                            //// force update (why?)
                            //MDIMain.SelectResource(SelResType, SelResNum);
                            break;
                        case EResListType.ComboList:
                            // listbox
                            //// force update (why?)
                            //MDIMain.SelectResource(SelResType, SelResNum);
                            break;
                        }
                    }
                }
                // if now showing resource list as numbers OR if number format has changed
                if (blnChangeResName || NewSettings.ResListType.Value != EResListType.None) {
                    switch (NewSettings.ResListType.Value) {
                    case EResListType.TreeList:
                        // step through all resources, and reassign tree list caption
                        foreach (TreeNode node in HdrNode[0].Nodes) {
                            node.Text = ResourceName(EditGame.Logics[(byte)node.Tag], true);
                        }
                        foreach (TreeNode node in HdrNode[1].Nodes) {
                            node.Text = ResourceName(EditGame.Pictures[(byte)node.Tag], true);
                        }
                        foreach (TreeNode node in HdrNode[2].Nodes) {
                            node.Text = ResourceName(EditGame.Sounds[(byte)node.Tag], true);
                        }
                        foreach (TreeNode node in HdrNode[3].Nodes) {
                            node.Text = ResourceName(EditGame.Views[(byte)node.Tag], true);
                        }
                        break;
                    case EResListType.ComboList:
                        // combo list - update list for resources being listed
                        switch (MDIMain.cmbResType.SelectedIndex) {
                        case 1:
                            foreach (ListViewItem listitem in MDIMain.lstResources.Items) {
                                listitem.Text = ResourceName(EditGame.Logics[(byte)listitem.Tag], true);
                            }
                            break;
                        case 2:
                            foreach (ListViewItem listitem in MDIMain.lstResources.Items) {
                                listitem.Text = ResourceName(EditGame.Pictures[(byte)listitem.Tag], true);
                            }
                            break;
                        case 3:
                            foreach (ListViewItem listitem in MDIMain.lstResources.Items) {
                                listitem.Text = ResourceName(EditGame.Sounds[(byte)listitem.Tag], true);
                            }
                            break;
                        case 4:
                            foreach (ListViewItem listitem in MDIMain.lstResources.Items) {
                                listitem.Text = ResourceName(EditGame.Logics[(byte)listitem.Tag], true);
                            }
                            break;
                        }
                        break;
                    }
                    // update any open editors
                    foreach (frmLogicEdit frm in LogicEditors) {
                        if (frm.FormMode == LogicFormMode.Logic) {
                            if (frm.InGame) {
                                frm.Text = sLOGED + ResourceName(frm.EditLogic, true, true);
                                if (frm.IsChanged) {
                                    frm.Text = sDM + frm.Text;
                                }
                            }
                        }
                    }
                    foreach (frmPicEdit frm in PictureEditors) {
                        if (frm.InGame) {
                            frm.Text = sPICED + ResourceName(frm.EditPicture, true, true);
                            if (frm.IsChanged) {
                                frm.Text = sDM + frm.Text;
                            }
                        }
                    }
                    foreach (frmSoundEdit frm in SoundEditors) {
                        if (frm.InGame) {
                            frm.Text = sSNDED + ResourceName(frm.EditSound, true, true);
                            if (frm.IsChanged) {
                                frm.Text = sDM + frm.Text;
                            }
                        }
                    }
                    foreach (frmViewEdit frm in ViewEditors) {
                        if (frm.InGame) {
                            frm.Text = sVIEWED + ResourceName(frm.EditView, true, true);
                            if (frm.IsChanged) {
                                frm.Text = sDM + frm.Text;
                            }
                        }
                    }
                    if (LEInUse) {
                        LayoutEditor.DrawLayout(true);
                    }
                }
                if (changePicPrevZoom) {
                    PreviewWin.SetPictureScale();
                    PreviewWin.RefreshPic();
                }
                if (changeViewPrevZoom) {
                    PreviewWin.SetViewScale();
                    PreviewWin.RefreshView();
                }
            }
            foreach (Form frm in MDIMain.MdiChildren) {
                switch (frm.Name) {
                case "frmLogicEdit":
                case "frmGlobals":
                case "frmLayout":
                case "frmObjectEdit":
                case "frmViewEdit":
                case "frmWordsEdit":
                    dynamic dfrm = frm;
                    dfrm.InitFonts();
                    break;
                }
            }
            LogicDecoder.DefaultSrcExt = NewSettings.DefaultExt.Value.ToLower();
            LogicCompiler.ErrorLevel = NewSettings.ErrorLevel.Value;

            // DECOMPILE
            LogicDecoder.MsgsByNumber = WinAGISettings.MsgsByNumber.Value;
            LogicDecoder.IObjsByNumber = WinAGISettings.IObjsByNumber.Value;
            LogicDecoder.WordsByNumber = WinAGISettings.WordsByNumber.Value;
            LogicDecoder.ShowAllMessages = WinAGISettings.ShowAllMessages.Value;
            LogicDecoder.SpecialSyntax = WinAGISettings.SpecialSyntax.Value;
            LogicDecoder.ReservedAsText = WinAGISettings.ReservedAsText.Value;
            LogicDecoder.CodeStyle = WinAGISettings.CodeStyle.Value;
            LogicDecoder.IndentSize = (byte)WinAGISettings.LogicTabWidth.Value;

            // PICTURES
            foreach (frmPicEdit frm in PictureEditors) {
                frm.CursorMode = (CoordinateHighlightType)WinAGISettings.CursorMode.Value;
                frm.InitFonts();
            }

            // SOUNDS
            NewSettings.SndZoom.Value = txtSoundZoom.Value;
            foreach (frmSoundEdit frm in SoundEditors) {
                // if setting for showing/hiding notes has changed, redraw sound editors
                if (blnChangeNotes) {
                    // force redraw
                    frm.Invalidate();
                }
            }

            // VIEWS
            // no updates needed

            // WORDS
            foreach (Form frm in MDIMain.MdiChildren) {
                if (frm.GetType() == typeof(frmWordsEdit)) {
                    ((frmWordsEdit)frm).InitFonts();
                }
            }

            // OBJECTS
            foreach (Form frm in MDIMain.MdiChildren) {
                if (frm.GetType() == typeof(frmObjectEdit)) {
                    ((frmObjectEdit)frm).InitFonts();
                }
            }

            // LAYOUT
            // no updates needed

            // RESET WARNINGS
            if (chkResetWarnings.Checked || blnResetWarnings) {
                WinAGISettings.RenameWAG.Reset(WinAGISettingsFile);
                WinAGISettings.OpenNew.Reset(WinAGISettingsFile);
                WinAGISettings.AskExport.Reset(WinAGISettingsFile);
                WinAGISettings.AskRemove.Reset(WinAGISettingsFile);
                WinAGISettings.AutoUpdateDefines.Reset(WinAGISettingsFile);
                WinAGISettings.AutoUpdateResDefs.Reset(WinAGISettingsFile);
                WinAGISettings.AutoUpdateWords.Reset(WinAGISettingsFile);
                WinAGISettings.AutoUpdateObjects.Reset(WinAGISettingsFile);
                WinAGISettings.WarnDupGName.Reset(WinAGISettingsFile);
                WinAGISettings.WarnDupGVal.Reset(WinAGISettingsFile);
                WinAGISettings.WarnInvalidStrVal.Reset(WinAGISettingsFile);
                WinAGISettings.WarnInvalidCtlVal.Reset(WinAGISettingsFile);
                WinAGISettings.WarnResOverride.Reset(WinAGISettingsFile);
                WinAGISettings.WarnDupObj.Reset(WinAGISettingsFile);
                WinAGISettings.WarnCompile.Reset(WinAGISettingsFile);
                WinAGISettings.DelBlankG.Reset(WinAGISettingsFile);
                WinAGISettings.NotifyCompSuccess.Reset(WinAGISettingsFile);
                //WinAGISettings.NotifyCompWarn.Reset(WinAGISettingsFile);
                WinAGISettings.NotifyCompFail.Reset(WinAGISettingsFile);
                WinAGISettings.WarnItem0.Reset(WinAGISettingsFile);
                WinAGISettings.OpenOnErr.Reset(WinAGISettingsFile);
                WinAGISettings.SaveOnCompile.Reset(WinAGISettingsFile);
                WinAGISettings.CompileOnRun.Reset(WinAGISettingsFile);
                WinAGISettings.WarnMsgs.Reset(WinAGISettingsFile);
                WinAGISettings.WarnEncrypt.Reset(WinAGISettingsFile);
                WinAGISettings.LEDelPicToo.Reset(WinAGISettingsFile);
                //WinAGISettings.WarnPlotPaste.Reset(WinAGISettingsFile);
                for (int i = 1; i <= LogicCompiler.WARNCOUNT; i++) {
                    LogicCompiler.SetIgnoreWarning(5000 + i, false);
                }
                for (int i = 0; i <= 3; i++) {
                    //  (int)((WARNCOUNT - 1) / 30)
                    WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn" + i, 0);
                }
            }

            // save changes to settings list and update the file
            SaveNewSettings();
            Close();
            MDIMain.UseWaitCursor = false;
        }
        #endregion

        #region General Tab Event Handlers
        private void chkDisplayByNum_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowResNum.Value = chkDisplayByNum.Checked;
            cmbResFormat.Enabled = NewSettings.ShowResNum.Value;
            lblResNameFormat.Enabled = NewSettings.ShowResNum.Value;
            chkIncludeResNum.Enabled = !NewSettings.ShowResNum.Value;
        }

        private void cmbResFormat_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.ResFormatNameCase.Value = cmbResFormat.SelectedIndex / 4;
            if (cmbResFormat.Text.Contains('.')) {
                NewSettings.ResFormatSeparator.Value = ".";
            }
            else {
                NewSettings.ResFormatSeparator.Value = " ";
            }
            if (cmbResFormat.Text.Contains('0')) {
                NewSettings.ResFormatNumFormat.Value = "000";
            }
            else {
                NewSettings.ResFormatNumFormat.Value = "";
            }
        }

        private void chkIncludeResNum_CheckedChanged(object sender, EventArgs e) {
            NewSettings.IncludeResNum.Value = chkIncludeResNum.Checked;
        }

        private void cmbResTree_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.ResListType.Value = (EResListType)cmbResTree.SelectedIndex;
            if (NewSettings.ResListType.Value == EResListType.None) {
                chkDisplayByNum.Enabled = false;
                chkIncludeResNum.Enabled = false;
                cmbResFormat.Enabled = false;
                lblResNameFormat.Enabled = false;
            }
            else {
                chkDisplayByNum.Enabled = true;
                chkIncludeResNum.Enabled = !NewSettings.ShowResNum.Value;
                cmbResFormat.Enabled = NewSettings.ShowResNum.Value;
                lblResNameFormat.Enabled = NewSettings.ShowResNum.Value;
            }


            // preview checkboxes depends on tree setting
            Frame17.Enabled = (NewSettings.ResListType.Value != EResListType.None);
            if (Frame17.Enabled) {
                chkPreview.Enabled = (NewSettings.ResListType.Value != EResListType.None);
                chkShiftPreview.Enabled = NewSettings.ShowPreview.Value;
                chkHidePreview.Enabled = NewSettings.ShowPreview.Value;
            }
            else {
                chkPreview.Enabled = false;
                chkShiftPreview.Enabled = false;
                chkHidePreview.Enabled = false;
            }
        }

        private void chkPreview_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowPreview.Value = chkPreview.Checked;
            chkShiftPreview.Enabled = NewSettings.ShowPreview.Value;
            chkHidePreview.Enabled = NewSettings.ShowPreview.Value;
        }

        private void chkShiftPreview_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShiftPreview.Value = chkShiftPreview.Checked;
            if (NewSettings.ShiftPreview.Value) {
                chkHidePreview.Checked = false;
            }
        }

        private void chkHidePreview_CheckedChanged(object sender, EventArgs e) {
            NewSettings.HidePreview.Value = chkHidePreview.Checked;
            if (NewSettings.HidePreview.Value) {
                chkShiftPreview.Checked = false;
            }
        }

        private void chkAutoOpen_CheckedChanged(object sender, EventArgs e) {
            NewSettings.AutoOpen.Value = chkAutoOpen.Checked;
        }

        private void chkSplash_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowSplashScreen.Value = chkSplash.Checked;
        }

        private void chkBackupRes_CheckedChanged(object sender, EventArgs e) {
            NewSettings.BackupResFile.Value = chkBackupRes.Checked;
        }

        private void chkAutoExport_CheckedChanged(object sender, EventArgs e) {
            NewSettings.AutoExport.Value = chkAutoExport.Checked;
        }

        private void txtResDirName_KeyPress(object sender, KeyPressEventArgs e) {
            if ((" \\/:*?\"<>|").Contains(e.KeyChar)) {
                e.KeyChar = '\0';
                e.Handled = true;
            }
            if ((int)e.KeyChar > 124) {
                e.KeyChar = '\0';
                e.Handled = true;
            }
        }

        private void txtResDirName_Validating(object sender, CancelEventArgs e) {
            txtResDirName.Text = txtResDirName.Text.Trim();
            if (txtResDirName.Text.Length == 0) {
                txtResDirName.Text = NewSettings.DefResDir.Value;
            }
            else {
                if ((" \\/:*?\"<>|").Any(txtResDirName.Text.Contains)) {
                    // invalid character; reset to default
                    txtResDirName.Text = NewSettings.DefResDir.Value;
                }
                else if (txtResDirName.Text.Any(ch => ch > 127 || ch < 32)) {
                    // invalid character; reset to default
                    txtResDirName.Text = NewSettings.DefResDir.Value;
                }
            }
            NewSettings.DefResDir.Value = txtResDirName.Text;

        }

        private void txtMaxSO_Validating(object sender, CancelEventArgs e) {
            if (txtMaxSO.Text.Length == 0) {
                txtMaxSO.Value = txtMaxSO.MinValue;
            }
            NewSettings.DefMaxSO.Value = (byte)txtMaxSO.Value;
        }

        private void txtMaxVol0_Validating(object sender, CancelEventArgs e) {
            if (txtMaxVol0.Text.Length == 0) {
                txtMaxVol0.Value = txtMaxVol0.MinValue;
            }
            NewSettings.DefMaxVol0.Value = txtMaxVol0.Value * 1024;
        }

        #endregion

        #region Logics1 Tab Event Handlers
        private void chkSnippets_CheckedChanged(object sender, EventArgs e) {
            NewSettings.UseSnippets.Value = chkSnippets.Checked;
        }

        private void chkAutoQuickInfo_CheckedChanged(object sender, EventArgs e) {
            NewSettings.AutoQuickInfo.Value = chkAutoQuickInfo.Checked;
        }

        private void chkDefTips_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowDefTips.Value = chkDefTips.Checked;
        }

        private void chkDocMap_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowDocMap.Value = chkDocMap.Checked;
        }

        private void chkLineNumbers_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowLineNumbers.Value = chkLineNumbers.Checked;
        }

        private void chkMaxWindow_CheckedChanged(object sender, EventArgs e) {
            NewSettings.MaximizeLogics.Value = chkMaxWindow.Checked;
        }

        private void chkHighlightLogic_CheckedChanged(object sender, EventArgs e) {
            NewSettings.HighlightLogic.Value = chkHighlightLogic.Checked;
            if (!NewSettings.HighlightLogic.Value) {
                chkHighlightText.Checked = false;
            }
            chkHighlightText.Enabled = NewSettings.HighlightLogic.Value;
            // if highlighting is disabled, only normal and background are available
            // otherwise all are available
            if (!NewSettings.HighlightLogic.Value) {
                lstColors.SelectedIndex = 0;
                lstColors.Items.RemoveAt(9);
                lstColors.Items.RemoveAt(8);
                lstColors.Items.RemoveAt(7);
                lstColors.Items.RemoveAt(6);
                lstColors.Items.RemoveAt(5);
                lstColors.Items.RemoveAt(4);
                lstColors.Items.RemoveAt(3);
                lstColors.Items.RemoveAt(2);
                lstColors.Items.RemoveAt(1);
                fraHighlighting.Text = "Text and Background Colors";
            }
            else {
                lstColors.Items.Insert(1, "Comments");
                lstColors.Items.Insert(2, "Strings");
                lstColors.Items.Insert(3, "Keywords");
                lstColors.Items.Insert(4, "Test Cmds");
                lstColors.Items.Insert(5, "Action Cmds");
                lstColors.Items.Insert(6, "Invalid Cmds");
                lstColors.Items.Insert(7, "Numbers");
                lstColors.Items.Insert(8, "Arg Identifiers");
                lstColors.Items.Insert(9, "Define Names");
                fraHighlighting.Text = "Syntax Highlight Styles";
            }
        }

        private void chkHighlightText_CheckedChanged(object sender, EventArgs e) {
            NewSettings.HighlightText.Value = chkHighlightText.Checked;
        }

        private void chkBold_CheckedChanged(object sender, EventArgs e) {
            // should never happen, but...
            if (lstColors.SelectedIndex == -1) {
                return;
            }

            // in case we are switching from non-highlight to highlight,
            // we need to check for index of 10 (background) which has
            // no bold or italic value
            if (lstColors.SelectedIndex == 10) {
                return;
            }
            NewSettings.SyntaxStyle[lstColors.SelectedIndex].FontStyle.Value &= FontStyle.Italic;
            if (chkBold.Checked) {
                NewSettings.SyntaxStyle[lstColors.SelectedIndex].FontStyle.Value |= FontStyle.Bold;
            }
        }

        private void chkItalic_CheckedChanged(object sender, EventArgs e) {
            // should never happen, but...
            if (lstColors.SelectedIndex == -1) {
                return;
            }

            // in case we are switching from non-highlight to highlight,
            // we need to check for index of 10 (background) which has
            // no bold or italic value
            if (lstColors.SelectedIndex == 10) {
                return;
            }
            NewSettings.SyntaxStyle[lstColors.SelectedIndex].FontStyle.Value &= FontStyle.Bold;
            if (chkItalic.Checked) {
                NewSettings.SyntaxStyle[lstColors.SelectedIndex].FontStyle.Value |= FontStyle.Italic;
            }
        }

        private void cmdColor_Click(object sender, EventArgs e) {
            int lngIndex;

            // should never happen, but...
            if (lstColors.SelectedIndex == -1) {
                return;
            }
            if (NewSettings.HighlightLogic.Value) {
                // the index value is taken from listbox as normal
                lngIndex = lstColors.SelectedIndex;
            }
            else {
                // if not highlighting, listbox only has foreground
                //  and background; so need to choose the correct
                //  index value
                if (lstColors.SelectedIndex == 1) {
                    lngIndex = 5;
                }
                else {
                    lngIndex = 0;
                }
            }

            //cdColors.Caption = "Choose Font Highlight Color";
            if (lngIndex < 10) {
                cdColors.Color = NewSettings.SyntaxStyle[lngIndex].Color.Value;
            }
            else {
                cdColors.Color = NewSettings.EditorBackColor.Value;
            }
            cdColors.SolidColorOnly = true;
            cdColors.AnyColor = true;
            cdColors.FullOpen = true;
            if (cdColors.ShowDialog(MDIMain) == DialogResult.OK) {
                if (lngIndex < 10) {
                    NewSettings.SyntaxStyle[lngIndex].Color.Value = cdColors.Color;
                }
                else {
                    NewSettings.EditorBackColor.Value = cdColors.Color;
                }
                // update picture
                lstColors_SelectedIndexChanged(null, null);
                picColor.Select();
            }
        }

        private void cmdRTFPrev_MouseDown(object sender, MouseEventArgs e) {
            // display preview rtf view

            rtfPreview.ForeColor = NewSettings.SyntaxStyle[0].Color.Value;
            rtfPreview.BackColor = NewSettings.EditorBackColor.Value;
            rtfPreview.Font = new Font(NewSettings.EditorFontName.Value, NewSettings.EditorFontSize.Value, NewSettings.SyntaxStyle[0].FontStyle.Value);
            rtfPreview.DefaultStyle = new TextStyle(rtfPreview.DefaultStyle.ForeBrush, rtfPreview.DefaultStyle.BackgroundBrush, NewSettings.SyntaxStyle[0].FontStyle.Value);
            rtfPreview.Range.ClearStyle(StyleIndex.All);
            if (NewSettings.HighlightLogic.Value) {
                RefreshPreviewSyntaxStyles();
                PreviewSyntaxHighlight();
            }
            rtfPreview.Visible = true;
        }

        private void cmdRTFPrev_MouseUp(object sender, MouseEventArgs e) {
            // hide the preview
            rtfPreview.Visible = false;
        }

        private void lstColors_SelectedIndexChanged(object sender, EventArgs e) {
            // redraw selected color in the color picturebox
            // if not highlighting, index 1 is background, not keyword!
            int lngIndex;

            // shouldn't happen, but...
            if (lstColors.SelectedIndex == -1) {
                return;
            }

            if (NewSettings.HighlightLogic.Value) {
                lngIndex = lstColors.SelectedIndex;
            }
            else {
                if (lstColors.SelectedIndex == 1) {
                    lngIndex = 10;
                }
                else {
                    lngIndex = 0;
                }
            }
            if (lngIndex < 10) {
                picColor.BackColor = NewSettings.SyntaxStyle[lngIndex].Color.Value;
            }
            else {
                picColor.BackColor = NewSettings.EditorBackColor.Value;
            }
            // if text setting chosen also set bold/italic status
            // text is 0-9 when highlighting is enabled, 0 when highlighting is disabled
            if (lngIndex == 0 || (lngIndex < 10 && lstColors.Items.Count > 2)) {
                chkBold.Enabled = true;
                chkItalic.Enabled = true;
                chkBold.Checked = (NewSettings.SyntaxStyle[lngIndex].FontStyle.Value & FontStyle.Bold) == FontStyle.Bold;
                chkItalic.Checked = (NewSettings.SyntaxStyle[lngIndex].FontStyle.Value & FontStyle.Italic) == FontStyle.Italic;
            }
            else {
                chkBold.Checked = false;
                chkItalic.Checked = false;
                chkBold.Enabled = false;
                chkItalic.Enabled = false;
            }
        }

        private void lstColors_DoubleClick(object sender, EventArgs e) {
            // same as clicking button
            cmdColor_Click(null, null);
        }

        private void txtTabWidth_Validating(object sender, CancelEventArgs e) {
            if (txtTabWidth.Text.Length == 0) {
                txtTabWidth.Value = txtTabWidth.MinValue;
            }
            NewSettings.LogicTabWidth.Value = txtTabWidth.Value;
            RefreshCodeExample();
        }

        private void txtExtension_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == (char)13) {
                e.KeyChar = '\0';
                e.Handled = true;
                chkSnippets.Select();
            }
            // only backspace, delete, letters, numbers
            switch ((int)e.KeyChar) {
            case < 32:
            case >= 48 and <= 57:
            case >= 65 and <= 90:
            case >= 97 and <= 122:
                // ok
                break;
            default:
                // not allowed
                e.KeyChar = '\0';
                e.Handled = true;
                break;
            }
        }

        private void txtExtension_Validating(object sender, CancelEventArgs e) {
            // only upper case, only letters, only 4 characters
            string val = txtExtension.Text.Trim();

            if (val.Length == 0) {
                val = NewSettings.DefaultExt.Value;
            }
            else {
                string newval = "";
                for (int i = 0; i < val.Length; i++) {
                    int ichar = val[i];
                    switch (ichar) {
                    case >= 97 and <= 122:
                        ichar -= 32;
                        break;
                    case >= 65 and <= 90:
                    case >= 48 and <= 57:
                        // allowed
                        break;
                    default:
                        // not allowed
                        ichar = 0;
                        break;
                    }
                    if (ichar != 0) {
                        newval += (char)ichar;
                    }
                }
                if (newval.Length == 0) {
                    val = NewSettings.DefaultExt.Value;
                }
                else {
                    val = newval;
                }
            }
            // only four characters
            if (val.Length > 4) {
                val = val[..3];
            }
            txtExtension.Text = val;
            NewSettings.DefaultExt.Value = val;
        }

        private void cmbPrevFont_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.PreviewFontName.Value = cmbPrevFont.Text;
        }

        private void cmbPrevSize_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.PreviewFontSize.Value = int.Parse(cmbPrevSize.Text);
        }

        private void cmbEditFont_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.EditorFontName.Value = cmbEditFont.Text;
        }

        private void cmbEditSize_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.EditorFontSize.Value = int.Parse(cmbEditSize.Text);
        }
        #endregion

        #region Logics2 Tab Event Handlers
        private void cmbErrorLevel_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.ErrorLevel.Value = (LogicErrorLevel)cmbErrorLevel.SelectedIndex;
        }

        private void chkAutoWarn_CheckedChanged(object sender, EventArgs e) {
            NewSettings.AutoWarn.Value = chkAutoWarn.Checked;
        }

        private void chkIncludeIDs_CheckedChanged(object sender, EventArgs e) {
            NewSettings.DefIncludeIDs.Value = chkIncludeIDs.Checked;
        }

        private void chkIncludeResDefs_CheckedChanged(object sender, EventArgs e) {
            NewSettings.DefIncludeReserved.Value = chkIncludeResDefs.Checked;
        }

        private void chkIncludeGlobals_CheckedChanged(object sender, EventArgs e) {
            NewSettings.DefIncludeGlobals.Value = chkIncludeGlobals.Checked;
        }

        private void chkShowComment_CheckedChanged(object sender, EventArgs e) {
            NewSettings.GEShowComment.Value = chkShowComment.Checked;
        }

        private void chkShowMsg_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowAllMessages.Value = chkShowMsg.Checked;
        }

        private void chkSpecialSyntax_CheckedChanged(object sender, EventArgs e) {
            NewSettings.SpecialSyntax.Value = chkSpecialSyntax.Checked;
        }

        private void chkResVarText_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ReservedAsText.Value = chkResVarText.Checked;
        }

        private void chkMsgsByNum_CheckedChanged(object sender, EventArgs e) {
            NewSettings.MsgsByNumber.Value = chkMsgsByNum.Checked;
            if (chkMsgsByNum.Checked) {
                chkShowMsg.Checked = true;
                chkShowMsg.Enabled = false;
            }
            else {
                chkShowMsg.Enabled = true;
            }
        }

        private void chkObjsByNum_CheckedChanged(object sender, EventArgs e) {
            NewSettings.IObjsByNumber.Value = chkObjsByNum.Checked;
        }

        private void chkWordsByNum_CheckedChanged(object sender, EventArgs e) {
            NewSettings.WordsByNumber.Value = chkWordsByNum.Checked;
        }

        private void cmbCodeStyle_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.CodeStyle.Value = (LogicDecoder.AGICodeStyle)cmbCodeStyle.SelectedIndex;
            RefreshCodeExample();
        }
        #endregion

        #region Pictures Tab Event Handlers
        private void udPPZoom_SelectedItemChanged(object sender, EventArgs e) {
            NewSettings.PicScalePreview.Value = float.Parse(((string)udPPZoom.SelectedItem)[..^1]) / 100;
        }

        private void udPEZoom_SelectedItemChanged(object sender, EventArgs e) {
            NewSettings.PicScaleEdit.Value = float.Parse(((string)udPEZoom.SelectedItem)[..^1]) / 100;
        }

        private void optPicSplit_CheckedChanged(object sender, EventArgs e) {
            NewSettings.SplitWindow.Value = optPicSplit.Checked;
        }

        private void optPicFull_CheckedChanged(object sender, EventArgs e) {
            NewSettings.SplitWindow.Value = !optPicFull.Checked;
        }

        private void chkBands_Click(object sender, EventArgs e) {
            NewSettings.ShowBands.Value = chkBands.Checked;
        }

        private void optCoordW_CheckedChanged(object sender, EventArgs e) {
            NewSettings.CursorMode.Value = 0;
        }

        private void optCoordX_CheckedChanged(object sender, EventArgs e) {
            NewSettings.CursorMode.Value = 1;
        }

        private void chkIgnoreBlocks_Click(object sender, EventArgs e) {
            // save changed Value
            NewPicTestSettings.IgnoreBlocks.Value = chkIgnoreBlocks.Checked;
        }

        private void chkIgnoreHorizon_Click(object sender, EventArgs e) {
            // save changed Value
            NewPicTestSettings.IgnoreHorizon.Value = chkIgnoreHorizon.Checked;
        }

        private void optAnything_CheckedChanged(object sender, EventArgs e) {
            NewPicTestSettings.ObjRestriction.Value = 0;
        }

        private void optLand_CheckedChanged(object sender, EventArgs e) {
            // PicTest.ObjRestriction: 0 = no restriction, 1 = restrict to water, 2 = restrict to land
            NewPicTestSettings.ObjRestriction.Value = 2;
        }

        private void optWater_CheckedChanged(object sender, EventArgs e) {
            NewPicTestSettings.ObjRestriction.Value = 1;
        }

        private void chkCycleAtRest_Click(object sender, EventArgs e) {
            // save changed Value
            NewPicTestSettings.CycleAtRest.Value = chkCycleAtRest.Checked;
        }

        private void txtHorizon_Validating(object sender, CancelEventArgs e) {
            if (txtHorizon.Text.Length == 0) {
                txtHorizon.Value = txtHorizon.MinValue;
            }
            NewPicTestSettings.Horizon.Value = txtHorizon.Value;
        }

        private void cmbPriority_SelectionIndexChanged(object sender, EventArgs e) {
            NewPicTestSettings.ObjPriority.Value = 16 - cmbPriority.SelectedIndex;
        }

        private void cmbSpeed_SelectionIndexChanged(object sender, EventArgs e) {
            NewPicTestSettings.ObjSpeed.Value = cmbSpeed.SelectedIndex;
        }
        #endregion

        #region Sounds Tab Event Handlers

        private void optPlaybackMode_CheckedChanged(object sender, EventArgs e) {
            if (optPCSpeaker.Checked) {
                NewSettings.PlaybackMode.Value = 0;
            }
            else if (optPCjr.Checked) {
                NewSettings.PlaybackMode.Value = 1;
            }
            else if (optMIDI.Checked) {
                NewSettings.PlaybackMode.Value = 2;
            }
        }

        private void chkMute_CheckedChanged(object sender, EventArgs e) {
            bool newval = (sender as CheckBox).Checked;
            int index = (int)((sender as CheckBox).Tag ?? -1);
            if (index == -1) {
                return;
            }
            NewSettings.DefMute[index].Value = newval;
        }

        private void cmbInst_SelectedIndexChanged(object sender, EventArgs e) {
            byte newval = (byte)(sender as ComboBox).SelectedIndex;
            int index = (int)((sender as ComboBox).Tag ?? -1);
            if (index == -1) {
                return;
            }
            NewSettings.DefInst[index].Value = newval;
        }

        private void cmdInstReset_Click(object sender, EventArgs e) {
            // resets all instruments to current default values

            foreach (Sound tmpSound in EditGame.Sounds) {
                bool blnLoaded = tmpSound.Loaded;
                if (!blnLoaded) {
                    tmpSound.Load();
                }
                for (int i = 0; i < 3; i++) {
                    tmpSound[0].Instrument = NewSettings.DefInst[0].Value;
                    tmpSound[0].Muted = NewSettings.DefMute[0].Value;
                }
                tmpSound.Save();
                if (!blnLoaded) {
                    tmpSound.Unload();
                }
            }
            if (SelResType == AGIResType.Sound && PreviewWin.Visible) {
                // adjust instrument settings
                PreviewWin.cmbInst0.SelectedIndex = NewSettings.DefInst[0].Value;
                PreviewWin.cmbInst1.SelectedIndex = NewSettings.DefInst[1].Value;
                PreviewWin.cmbInst2.SelectedIndex = NewSettings.DefInst[2].Value;
                PreviewWin.chkTrack0.Checked = !NewSettings.DefMute[0].Value;
                PreviewWin.chkTrack1.Checked = !NewSettings.DefMute[1].Value;
                PreviewWin.chkTrack2.Checked = !NewSettings.DefMute[2].Value;
                PreviewWin.chkTrack3.Checked = !NewSettings.DefMute[3].Value;
            }
        }

        private void chkKeybd_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowKeyboard.Value = chkKeybd.Checked;
        }

        private void chkOneTrack_CheckedChanged(object sender, EventArgs e) {
            NewSettings.OneTrack.Value = chkOneTrack.Checked;
        }

        private void chkNoKybdSound_CheckedChanged(object sender, EventArgs e) {
            NewSettings.NoKeyboardSound.Value = !chkNoKybdSound.Checked;
        }


        private void chkNotes_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowNotes.Value = chkNotes.Checked;
        }
        #endregion

        #region Views Tab Event Handlers
        private void chkShowGrid_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowGrid.Value = chkShowGrid.Checked;
        }

        private void chkShowPrev_CheckedChanged(object sender, EventArgs e) {
            NewSettings.ShowVEPreview.Value = chkShowPrev.Checked;
        }

        private void chkDefPrevPlay_CheckedChanged(object sender, EventArgs e) {
            NewSettings.DefPrevPlay.Value = chkDefPrevPlay.Checked;
        }

        private void cmbHAlign_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.ViewAlignH.Value = cmbHAlign.SelectedIndex;
        }

        private void cmbVAlign_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.ViewAlignV.Value = cmbVAlign.SelectedIndex;
        }

        private void cmbViewDefCol1_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.DefVColor1.Value = cmbViewDefCol1.SelectedIndex;
        }

        private void cmbViewDefCol2_SelectedIndexChanged(object sender, EventArgs e) {
            NewSettings.DefVColor2.Value = cmbViewDefCol2.SelectedIndex;
        }

        private void udVPZoom_SelectedItemChanged(object sender, EventArgs e) {
            NewSettings.ViewScalePreview.Value = float.Parse(((string)udVPZoom.SelectedItem)[..^1]) / 100;
        }

        private void udVEZoom_SelectedItemChanged(object sender, EventArgs e) {
            NewSettings.ViewScaleEdit.Value = float.Parse(((string)udVEZoom.SelectedItem)[..^1]) / 100;
        }

        private void txtDefCelH_Validating(object sender, CancelEventArgs e) {
            if (txtDefCelH.Text.Length == 0) {
                txtDefCelH.Value = txtDefCelH.MinValue;
            }
            NewSettings.DefCelH.Value = (byte)txtDefCelH.Value;
        }

        private void txtDefCelW_Validating(object sender, CancelEventArgs e) {
            if (txtDefCelH.Text.Length == 0) {
                txtDefCelH.Value = txtDefCelH.MinValue;
            }
            NewSettings.DefCelH.Value = (byte)txtDefCelH.Value;
        }
        #endregion

        #region Layout Tab Event Handlers
        /*
        private void chkDisplayPics_Click(object sender, EventArgs e) {
            NewSettings.LEShowPics.Value = chkDisplayPics.Checked;
        }

        private void chkLEGrid_Click(object sender, EventArgs e) {
            NewSettings.LEUseGrid = chkLEGrid.Checked;
            txtGrid.Enabled = NewSettings.LEUseGrid;
        }

        private void chkPages_Click(object sender, EventArgs e) {
            NewSettings.LEPages.Value = chkPages.Checked;
        }

        private void chkShowGrid_Click(object sender, EventArgs e) {
            NewSettings.ShowGrid.Value = chkShowGrid.Checked;
        }

        private void chkSynchronize_Click(object sender, EventArgs e) {
            NewSettings.LESync.Value = chkSynchronize.Checked;
        }

        private void chkUseLE_Click(object sender, EventArgs e) {
          NewSettings.DefUseLE.Value = chkUseLE.Checked;
        }

    private void cmdLEColor_Click() {
        NewSettings.DialogTitle = "Choose Layout Editor Colors"
        switch (lstLEColors.SelectedIndex) {
        case 0:
            // room edge
          cdColors.Color = NewSettings.RoomEdgeColor.Value;
            break;
        case 1:
            // room fill
          cdColors.Color = NewSettings.RoomFillColor.Value;
            break;
        case 2:
            // top edge
          cdColors.Color = NewSettings.TransPtEdgeColor.Value;
            break;
        case 3:
            // tp fill
          cdColors.Color = NewSettings.TransPtFillColor.Value;
            break;
        case 4:
            // cmt edge
          cdColors.Color = NewSettings.CmtEdgeColor.Value;
            break;
        case 5:
            // cmt fill
          cdColors.Color = NewSettings.CmtFillColor.Value;
            break; 
        case 6:
            // errpt edge
          cdColors.Color = NewSettings.ErrPtEdgeColor.Value;
            break;
        case 7:
            // errpt fill
          cdColors.Color = NewSettings.ErrPtFillColor.Value;
            break;
        case 8:
            // exit edge
          cdColors.Color = NewSettings.ExitEdgeColor.Value;
            break;
        case 9:
            // exit other
          cdColors.Color = NewSettings.ExitOtherColor.Value;
        default:
          return;
        }
        cdColors.Flags = cdlCCRGBInit Or cdlCCFullOpen;
        if (cdColors.ShowColor() == DialogResult.Cancel) {
            return;
        }

        // special case: color vbWhite-1 is the background color
        // so it can't be chosen; it's close to white, so just
        // change it
        if (cdColors.Color == Color.White - 1) {
          cdColors.Color = Color.White;
        }
        switch (lstLEColors.SelectedIndex) {
        case 0:
            // room edge
          NewSettings.RoomEdgeColor = cdColors.Color;
            break;
        case 1:
            // room fill
          NewSettings.RoomFillColor = cdColors.Color;
            break;
        case 2:
            // tp edge
          NewSettings.TransPtEdgeColor = cdColors.Color;
            break;
        case 3:
            // tp fill
          NewSettings.TransPtFillColor = cdColors.Color;
            break;
        case 4:
            // cmt edge
          NewSettings.CmtEdgeColor = cdColors.Color;
            break;
        case 5:
            // cmt fill
          NewSettings.CmtFillColor = cdColors.Color;
            break;
        case 6:
            // errpt edge
          NewSettings.ErrPtEdgeColor = cdColors.Color;
            break;
        case 7;
            // errpt fill
          NewSettings.ErrPtFillColor = cdColors.Color;
            break;
        case 8:
            // exit edge
          NewSettings.ExitEdgeColor = cdColors.Color;
            break;
        case 9:
            // exit other
          NewSettings.ExitOtherColor = cdColors.Color;
        }
        // update picture
        //lstLEColors_Click
        picLESample.Invalidate();
      picLEColor.Select();
    }

    private void txtLEZoom_Validating(object sender, CancelEventArgs e) {
      if (!double.TryParse(txtLEZoom.Text, out double var) || var < 1) {
        txtLEZoom.Text = "1";
      }
            else if (var > 8) {
        txtLEZoom.Text = "8";
      }
else {
            txtLEZoom.Text = ((int)var).ToString();
      }
      NewSettings.LEZoom = (int)var;
    }

    private void txtLEZoom_KeyPress(object sender, KeyPressEventArgs e) {
      // enter is same as tabbing to next control
      if ( e.KeyChar == 13) {
         e.KeyChar = '\0';
            e.Handled = true;
        lstLEColors.Select();
            return;
      }
      // only numbers or control keys
      switch ( e.KeyChar) {
      case  < 32:
            case >=48 and <= 57:
            break;
      default:
         e.KeyChar = '\0';
            e.Handled = true;
            break;
      }
    }

    private void txtGrid_Validating(object sender, CancelEventArgs e) {
      // >=.05; <=1; increments of 0.05
      if (!double.TryParse(txtGrid.Text, out double var) || var < 0.05) {
            var = 0.05;
        txtGrid.Text = "0.05";
      }
            else if (var > 1) {
        txtGrid.Text = "1.00";
            var = 1;
      }
else {

       var = ((double)((int)(val * 20)) / 20);
            }
      txtGrid.Text = var.ToString("0.00");
            NewSettings.LEGrid = var;
    }

    private void txtGrid_KeyPress(object sender, KeyPressEventArgs e) {
      // enter is same as tabbing to next control
      if ( e.KeyChar == 13) {
         e.KeyChar = '\0';
            e.Handled = true;
        txtLEZoom.Select();
            return;
      }
      // only numbers, period,  or control keys
      switch ( e.KeyChar) {
      case  < 32:
            case >=48 and <= 57:
            case 46:
            break;
      default:
         e.KeyChar = '\0';
                        e.Handled = true;
            break;
      }
    }

    private void picLESample_Paint() {
////      Dim rtn As Long, v(2) As POINTAPI
//      Dim hBrush As Long, hRgn As Long

//      // sets the sample objects to match current color selections
//      picLESample.Cls
//      picLESample.DrawWidth = 1
//      picLESample.FillColor = vbWhite
//      picLESample.Line (5, 5)-Step(picLESample.ScaleWidth - 10, picLESample.ScaleHeight - 10), vbBlack, B
//      picLESample.DrawWidth = 2

//        // rooms
//        picLESample.FillColor = NewSettings.RoomFillColor
//        picLESample.ForeColor = NewSettings.RoomEdgeColor
//        picLESample.Line (38, 60)-Step(33, 33), NewSettings.RoomEdgeColor, B
//        picLESample.Line (94, 60)-Step(33, 33), NewSettings.RoomEdgeColor, B
//        picLESample.CurrentX = 42
//        picLESample.CurrentY = 73
//        picLESample.Print "Rm 1"
//        picLESample.CurrentX = 97
//        picLESample.CurrentY = 73
//        picLESample.Print "Rm 2"

//       // transfer points
//        picLESample.FillColor = NewSettings.TransPtFillColor
//        picLESample.ForeColor = NewSettings.TransPtEdgeColor
//        picLESample.Circle (148.5, 37.5), 8.5, NewSettings.TransPtEdgeColor
//        picLESample.Circle (54.5, 118.5), 8.5, NewSettings.TransPtEdgeColor
//        picLESample.CurrentX = 146
//        picLESample.CurrentY = 31
//        picLESample.Print "1"
//        picLESample.CurrentX = 51
//        picLESample.CurrentY = 112
//        picLESample.Print "1"

//        // comments
//        // create region
//        hRgn = CreateRoundRectRgn(16, 16, 97, 41, 3, 3)

//        // create brush
//        hBrush = CreateSolidBrush(.CmtFillColor)

//        // fill region
//        rtn = FillRgn(picLESample.hDC, hRgn, hBrush)

//        // delete fill brush; create edge brush
//        rtn = DeleteObject(hBrush)
//        hBrush = CreateSolidBrush(.CmtEdgeColor)

//        // draw outline
//        rtn = FrameRgn(picLESample.hDC, hRgn, hBrush, 2, 2)

//        // delete brush and region
//        rtn = DeleteObject(hBrush)
//        rtn = DeleteObject(hRgn)
//        picLESample.ForeColor = NewSettings.CmtEdgeColor
//        picLESample.CurrentX = 30
//        picLESample.CurrentY = 21
//        picLESample.Print "Comment"

//        // exit lines
//        picLESample.Line (70, 76)-(94, 76), NewSettings.ExitEdgeColor
//        picLESample.Line (110, 92)-(110, 116), NewSettings.ExitEdgeColor
//        picLESample.Line (54, 92)-(54, 110), NewSettings.ExitOtherColor
//        picLESample.Line (126, 60)-(142, 44), NewSettings.ExitOtherColor


//        // errpoints
//        // use polygon drawing function
//        picLESample.FillColor = NewSettings.ErrPtFillColor
//        picLESample.ForeColor = NewSettings.ErrPtEdgeColor
//        v(0).X = 110
//        v(0).Y = 108
//        v(1).X = 100
//        v(1).Y = 124
//        v(2).X = 120
//        v(2).Y = 124
//        rtn = Polygon(picLESample.hDC, v(0), 3)
////
    }

    private void picLESample_MouseDown() {
        switch (picLESample.Point(X, Y)) {
        case NewSettings.RoomEdgeColor
          lstLEColors.SelectedIndex = 0;
            break;
        case NewSettings.RoomFillColor:
          lstLEColors.SelectedIndex = 1;
            break;
        case NewSettings.TransPtEdgeColor:
          lstLEColors.SelectedIndex = 2;
                        break;
        case NewSettings.TransPtFillColor:
          lstLEColors.SelectedIndex = 3;
            break;
        case NewSettings.CmtEdgeColor:
          lstLEColors.SelectedIndex = 4;
            break;
        case NewSettings.CmtFillColor:
          lstLEColors.SelectedIndex = 5;
            break;
        case NewSettings.ErrPtEdgeColor:
          lstLEColors.SelectedIndex = 6;
            break;
        case NewSettings.ErrPtFillColor:
          lstLEColors.SelectedIndex = 7;
            break;
        case NewSettings.ExitEdgeColor:
          lstLEColors.SelectedIndex = 8;
            break;
        case NewSettings.ExitOtherColor:
          lstLEColors.SelectedIndex = 9;
            break;
        }
    }

    private void picColor_DblClick() {
      // same as clicking on the color button
      cmdColor.DoClick();
    }

    private void picLEColor_DblClick() {
      // same as clicking on LEColor button
      cmdLEColor.DoClick();
    }

    private void picLEColor_Paint() {
      Color lngColor = Color.Black;

      switch (lstLEColors.SelectedIndex) {
      case 0:
        lngColor = NewSettings.RoomEdgeColor;
            break;
      case 1:
        lngColor = NewSettings.RoomFillColor;
            break;
      case 2:
        lngColor = NewSettings.TransPtEdgeColor;
            break;
      case 3:
        lngColor = NewSettings.TransPtFillColor;
            break;
      case 4:
        lngColor = NewSettings.CmtEdgeColor;
            break;
      case 5:
        lngColor = NewSettings.CmtFillColor;
            break;
      case 6:
        lngColor = NewSettings.ErrPtEdgeColor;
            break;
      case 7:
        lngColor = NewSettings.ErrPtFillColor;
            break;
      case 8:
        lngColor = NewSettings.ExitEdgeColor;
            break;
      case 9:
        lngColor = NewSettings.ExitOtherColor;
            break;
      }
      picLEColor.Line(); // (30, 30)-Step(1320, 180), lngColor, BF
    }

    private void picLESample_DblClick() {
      // same as clicking the cmdLEColor button
      cmdLEColor.DoClick();
    }

    private void lstLEColors_Click() {
      Color lngColor = Color.Black;

      picLEColor.Clear();
      switch (lstLEColors.SelectedIndex) {
      case 0:
        lngColor = NewSettings.RoomEdgeColor;
            break;
      case 1:
        lngColor = NewSettings.RoomFillColor;
            break;
      case 2:
        lngColor = NewSettings.TransPtEdgeColor;
            break;
      case 3:
        lngColor = NewSettings.TransPtFillColor;
            break;
      case 4:
        lngColor = NewSettings.CmtEdgeColor;
            break;
      case 5:
        lngColor = NewSettings.CmtFillColor;
            break;
      case 6:
        lngColor = NewSettings.ErrPtEdgeColor;
            break;
      case 7:
        lngColor = NewSettings.ErrPtFillColor;
            break;
      case 8:
        lngColor = NewSettings.ExitEdgeColor;
            break;
      case 9:
        lngColor = NewSettings.ExitOtherColor;
            break;
      }
      picLEColor.Line(); // (30, 30)-Step(1320, 180), lngColor, BF
    }

    private void lstLEColors_DblClick() {
      // same as clicking on button
      cmdLEColor.DoClick();
    }

        */
        #endregion

        #region temp code
        void temp() {
            /*
    private void Form_KeyDown(KeyCode As Integer, Shift As Integer)

      Dim strHelp As String

      // check for help key
      if (Shift = 0 And KeyCode = vbKeyF1) {
        switch (TabStrip1.SelectedItem.Index) {
        case 1 // general
          strHelp = "generalsettings.htm"
        case 2 // logics
          strHelp = "logicsettings.htm"
        case 3 // decompiler
          strHelp = "decompilersettings.htm"
        case 4 // pictures
          strHelp = "picturesettings.htm"
        case 5 // sounds
          strHelp = "soundsettings.htm"
        case 6 // views
          strHelp = "viewsettings.htm"
        case 7 // layout
          strHelp = "layoutsettings.htm"
        default:
          // generic setting help
          strHelp = "Settings.htm"
        }

        Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\" + strHelp);
        KeyCode = 0
      }

    }
            */
        }
        #endregion

        private void InitForm() {
            // move preview rtf to front
            Controls.SetChildIndex(rtfPreview, 0);

            //***********************
            // GENERAL SETTINGS
            //***********************
            chkDisplayByNum.Checked = NewSettings.ShowResNum.Value;
            chkIncludeResNum.Checked = NewSettings.IncludeResNum.Value;
            chkIncludeResNum.Enabled = !NewSettings.ShowResNum.Value;
            int intIndex = NewSettings.ResFormatNameCase.Value * 4;
            if (NewSettings.ResFormatNumFormat.Value == "") {
                intIndex += 2;
            }
            if (NewSettings.ResFormatSeparator.Value == " ") {
                intIndex++;
            }
            cmbResFormat.SelectedIndex = intIndex;
            cmbResFormat.Enabled = NewSettings.ShowResNum.Value;
            cmbResTree.SelectedIndex = (int)NewSettings.ResListType.Value;
            lblResNameFormat.Enabled = NewSettings.ShowResNum.Value;
            chkPreview.Checked = NewSettings.ShowPreview.Value;
            chkPreview.Enabled = (NewSettings.ResListType.Value != EResListType.None);
            chkShiftPreview.Checked = NewSettings.ShiftPreview.Value;
            chkShiftPreview.Enabled = NewSettings.ShowPreview.Value;
            chkHidePreview.Checked = NewSettings.HidePreview.Value;
            chkHidePreview.Enabled = NewSettings.ShowPreview.Value;
            chkAutoOpen.Checked = NewSettings.AutoOpen.Value;
            chkSplash.Checked = NewSettings.ShowSplashScreen.Value;
            chkBackupRes.Checked = NewSettings.BackupResFile.Value;
            chkAutoExport.Checked = NewSettings.AutoExport.Value;
            txtResDirName.Text = NewSettings.DefResDir.Value;
            txtMaxSO.Text = NewSettings.DefMaxSO.Value.ToString();
            txtMaxVol0.Text = (NewSettings.DefMaxVol0.Value / 1024).ToString();

            //***********************
            // LOGICS SETTINGS TAB 1
            //***********************
            chkSnippets.Checked = NewSettings.UseSnippets.Value;
            chkAutoQuickInfo.Checked = NewSettings.AutoQuickInfo.Value;
            chkDocMap.Checked = NewSettings.ShowDocMap.Value;
            chkLineNumbers.Checked = NewSettings.ShowLineNumbers.Value;
            chkDefTips.Checked = NewSettings.ShowDefTips.Value;
            chkMaxWindow.Checked = NewSettings.MaximizeLogics.Value;
            chkHighlightLogic.Checked = NewSettings.HighlightLogic.Value;
            chkHighlightText.Checked = NewSettings.HighlightText.Value;
            lstColors.SelectedIndex = 0;
            txtTabWidth.Text = NewSettings.LogicTabWidth.Value.ToString();
            cmbEditFont.Text = NewSettings.EditorFontName.Value;
            cmbEditSize.Text = NewSettings.EditorFontSize.Value.ToString();
            cmbPrevFont.Text = NewSettings.PreviewFontName.Value;
            cmbPrevSize.Text = NewSettings.PreviewFontSize.Value.ToString();
            txtExtension.Text = NewSettings.DefaultExt.Value.ToLower();

            //***********************
            // LOGICS SETTINGS TAB 2
            //***********************
            cmbErrorLevel.SelectedIndex = NewSettings.ErrorLevel.IntValue;
            chkAutoWarn.Checked = NewSettings.AutoWarn.Value;
            chkShowComment.Checked = NewSettings.GEShowComment.Value;
            chkMsgsByNum.Checked = NewSettings.MsgsByNumber.Value;
            // if msgs by number, showmsg must always be true
            if (NewSettings.MsgsByNumber.Value) {
                chkShowMsg.Checked = false;
                // TODO: if not listing all msgs at end of logic, should
                // just unused msgs be shown? (if msgs bynum are referenced(print.v?)
                // could label them 'unreferenced' or something like that?
                NewSettings.ShowAllMessages.Value = true;
                chkShowMsg.Enabled = false;
            }
            else {
                chkShowMsg.Enabled = true;
            }
            chkObjsByNum.Checked = NewSettings.IObjsByNumber.Value;
            chkWordsByNum.Checked = NewSettings.WordsByNumber.Value;
            chkShowMsg.Checked = NewSettings.ShowAllMessages.Value;
            chkSpecialSyntax.Checked = NewSettings.SpecialSyntax.Value;
            chkResVarText.Checked = NewSettings.ReservedAsText.Value;
            cmbCodeStyle.SelectedIndex = NewSettings.CodeStyle.IntValue;
            chkIncludeIDs.Checked = NewSettings.DefIncludeIDs.Value;
            chkIncludeResDefs.Checked = NewSettings.DefIncludeReserved.Value;
            chkIncludeGlobals.Checked = NewSettings.DefIncludeGlobals.Value;

            //***********************
            // PICTURE SETTINGS
            //***********************
            double scale = NewSettings.PicScaleEdit.Value;
            // convert scale to percentage vlues
            if (scale < 1) {
                scale = 100;
            }
            else if (scale <= 3) {
                scale = (int)(scale * 4) / 0.04;
            }
            else if (scale < 8) {
                scale = (int)(scale * 2) / 0.02;
            }
            else if (scale < 20) {
                scale = (int)(scale) / 0.01;
            }
            else {
                scale = 2000;
            }
            for (int i = 0; i < udPEZoom.Items.Count; i++) {
                if (scale.ToString() == ((string)udPEZoom.Items[i])[..^1]) {
                    udPEZoom.SelectedIndex = i;
                    break;
                }
            }
            if (udPEZoom.SelectedIndex == -1) {
                udPEZoom.SelectedIndex = 0;
            }
            scale = NewSettings.PicScalePreview.Value;
            // convert to percentage vlues
            if (scale < 1) {
                scale = 100;
            }
            else if (scale <= 3) {
                scale = (int)(scale * 4) / 0.04;
            }
            else if (scale < 8) {
                scale = (int)(scale * 2) / 0.02;
            }
            else if (scale < 10) {
                scale = (int)(scale) / 0.01;
            }
            else {
                scale = 1000;
            }
            for (int i = 0; i < udPPZoom.Items.Count; i++) {
                if (scale.ToString() == ((string)udPPZoom.Items[i])[..^1]) {
                    udPPZoom.SelectedIndex = i;
                    break;
                }
            }
            if (udPPZoom.SelectedIndex == -1) {
                udPPZoom.SelectedIndex = 0;
            }
            chkBands.Checked = (NewSettings.ShowBands.Value);
            if (NewSettings.SplitWindow.Value) {
                optPicSplit.Checked = true;
            }
            else {
                optPicFull.Checked = true;
            }
            if (NewSettings.CursorMode.Value == (int)CoordinateHighlightType.FlashBox) {
                optCoordW.Checked = true;
            }
            else {
                optCoordX.Checked = true;
            }

            // re-check pictest settings as they may have been modified by the user
            NewPicTestSettings.ObjSpeed.ReadSetting(WinAGISettingsFile);
            if (NewPicTestSettings.ObjSpeed.Value < 0) {
                NewPicTestSettings.ObjSpeed.Value = 0;
            }
            if (NewPicTestSettings.ObjSpeed.Value > 3) {
                NewPicTestSettings.ObjSpeed.Value = 3;
            }
            NewPicTestSettings.ObjPriority.ReadSetting(WinAGISettingsFile);
            if (NewPicTestSettings.ObjPriority.Value < 4) {
                NewPicTestSettings.ObjPriority.Value = 4;
            }
            if (NewPicTestSettings.ObjPriority.Value > 16) {
                NewPicTestSettings.ObjPriority.Value = 16;
            }
            NewPicTestSettings.ObjRestriction.ReadSetting(WinAGISettingsFile);
            if (NewPicTestSettings.ObjRestriction.Value < 0) {
                NewPicTestSettings.ObjRestriction.Value = 0;
            }
            if (NewPicTestSettings.ObjRestriction.Value > 2) {
                NewPicTestSettings.ObjRestriction.Value = 2;
            }
            NewPicTestSettings.Horizon.ReadSetting(WinAGISettingsFile);
            if (NewPicTestSettings.Horizon.Value < 0) {
                NewPicTestSettings.Horizon.Value = 0;
            }
            if (NewPicTestSettings.Horizon.Value > 166) {
                NewPicTestSettings.Horizon.Value = 166;
            }
            NewPicTestSettings.IgnoreHorizon.ReadSetting(WinAGISettingsFile);
            NewPicTestSettings.IgnoreBlocks.ReadSetting(WinAGISettingsFile);
            NewPicTestSettings.CycleAtRest.ReadSetting(WinAGISettingsFile);
            txtHorizon.Text = NewPicTestSettings.Horizon.Value.ToString();
            cmbSpeed.SelectedIndex = NewPicTestSettings.ObjSpeed.Value;
            cmbPriority.SelectedIndex = 16 - NewPicTestSettings.ObjPriority.Value;
            switch (NewPicTestSettings.ObjRestriction.Value) {
            case 0:
                optAnything.Checked = true;
                break;
            case 1:
                optWater.Checked = true;
                break;
            case 2:
                optLand.Checked = true;
                break;
            }
            chkIgnoreBlocks.Checked = NewPicTestSettings.IgnoreBlocks.Value;
            chkIgnoreHorizon.Checked = NewPicTestSettings.IgnoreHorizon.Value;
            chkCycleAtRest.Checked = NewPicTestSettings.CycleAtRest.Value;

            //***********************
            // SOUND SETTINGS
            //***********************
            if (NewSettings.PlaybackMode.Value == 2) {
                optMIDI.Checked = true;
            }
            //else if (NewSettings.PlaybackMode.Value == 0) {
            //    optPCSpeaker.Checked = true;
            //}
            else {
                optPCjr.Checked = true;
            }
            txtSoundZoom.Value = NewSettings.SndZoom.Value;
            chkKeybd.Checked = NewSettings.ShowKeyboard.Value;
            chkNoKybdSound.Checked = NewSettings.NoKeyboardSound.Value;
            chkNotes.Checked = NewSettings.ShowNotes.Value;
            chkOneTrack.Checked = NewSettings.OneTrack.Value;
            cmbInst0.SelectedIndex = NewSettings.DefInst[0].Value;
            cmbInst0.Tag = 0;
            cmbInst1.SelectedIndex = NewSettings.DefInst[1].Value;
            cmbInst1.Tag = 1;
            cmbInst2.SelectedIndex = NewSettings.DefInst[2].Value;
            cmbInst2.Tag = 2;
            chkMute0.Checked = NewSettings.DefMute[0].Value;
            chkMute0.Tag = 0;
            chkMute1.Checked = NewSettings.DefMute[1].Value;
            chkMute1.Tag = 1;
            chkMute2.Checked = NewSettings.DefMute[2].Value;
            chkMute2.Tag = 2;
            chkMute3.Checked = NewSettings.DefMute[3].Value;
            chkMute3.Tag = 3;

            // views
            for (int i = 0; i < udVPZoom.Items.Count; i++) {
                if (NewSettings.ViewScalePreview.Value * 100 >= float.Parse(((string)udVPZoom.Items[i])[..^1])) {
                    udVPZoom.SelectedIndex = i;
                    break;
                }
            }
            if (udVPZoom.SelectedIndex == -1) {
                udVPZoom.SelectedIndex = 0;
            }
            for (int i = 0; i < udVEZoom.Items.Count; i++) {
                if (NewSettings.ViewScaleEdit.Value * 100 >= float.Parse(((string)udVEZoom.Items[i])[..^1])) {
                    udVEZoom.SelectedIndex = i;
                    break;
                }
            }
            if (udVEZoom.SelectedIndex == -1) {
                udVEZoom.SelectedIndex = 0;
            }
            txtDefCelH.Value = NewSettings.DefCelH.Value;
            txtDefCelW.Value = NewSettings.DefCelW.Value;
            cmbHAlign.SelectedIndex = NewSettings.ViewAlignH.Value;
            cmbVAlign.SelectedIndex = NewSettings.ViewAlignV.Value;
            cmbViewDefCol1.SelectedIndex = NewSettings.DefVColor1.Value;
            cmbViewDefCol2.SelectedIndex = NewSettings.DefVColor2.Value;
            chkShowPrev.Checked = NewSettings.ShowVEPreview.Value;
            chkDefPrevPlay.Checked = NewSettings.DefPrevPlay.Value;
            chkShowGrid.Checked = NewSettings.ShowGrid.Value;

            /*
            // layout
            chkUseLE.Checked = NewSettings.DefUseLE;
            chkPages.Checked = NewSettings.LEPages;
            chkDisplayPics.Checked = NewSettings.LEShowPics;
            chkSynchronize.Checked = NewSettings.LESync;
            chkLEGrid.Checked = NewSettings.LEUseGrid;
            txtGrid.Enabled = NewSettings.LEUseGrid;
            Label16.Enabled = NewSettings.LEUseGrid;
            txtGrid.Text = NewSettings.LEGrid.Format("0.00");
            lstLEColors.SelectedIndex = 0;
            txtLEZoom.Text = NewSettings.LEZoom.Value.ToString();
            */
        }

        private void SaveNewSettings() {
            // general
            WinAGISettings.ShowSplashScreen.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowPreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShiftPreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.HidePreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ResListType.WriteSetting(WinAGISettingsFile);
            WinAGISettings.AutoExport.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowResNum.WriteSetting(WinAGISettingsFile);
            WinAGISettings.IncludeResNum.WriteSetting(WinAGISettingsFile);
            WinAGISettings.BackupResFile.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefMaxSO.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefMaxVol0.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefCP.WriteSetting(WinAGISettingsFile);
            Engine.Base.CodePage = WinAGISettings.DefCP.Value;
            WinAGISettings.DefResDir.WriteSetting(WinAGISettingsFile);

            // resource format
            WinAGISettings.ResFormatNameCase.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ResFormatSeparator.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ResFormatNumFormat.WriteSetting(WinAGISettingsFile);

            // logics settings
            WinAGISettings.AutoWarn.WriteSetting(WinAGISettingsFile);
            WinAGISettings.HighlightLogic.WriteSetting(WinAGISettingsFile);
            WinAGISettings.HighlightText.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LogicTabWidth.WriteSetting(WinAGISettingsFile);
            WinAGISettings.MaximizeLogics.WriteSetting(WinAGISettingsFile);
            WinAGISettings.AutoQuickInfo.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowDefTips.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowDocMap.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowLineNumbers.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefaultExt.WriteSetting(WinAGISettingsFile);
            WinAGISettings.EditorFontName.WriteSetting(WinAGISettingsFile);
            WinAGISettings.EditorFontSize.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PreviewFontName.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PreviewFontSize.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ErrorLevel.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeIDs.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeReserved.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeGlobals.WriteSetting(WinAGISettingsFile);
            WinAGISettings.UseSnippets.WriteSetting(WinAGISettingsFile);

            // syntax highlighting styles
            WinAGISettings.EditorBackColor.WriteSetting(WinAGISettingsFile);
            for (int i = 0; i < 10; i++) {
                WinAGISettings.SyntaxStyle[i].Color.WriteSetting(WinAGISettingsFile);
                WinAGISettings.SyntaxStyle[i].FontStyle.WriteSetting(WinAGISettingsFile);
            }

            // logic decompiler
            WinAGISettings.MsgsByNumber.WriteSetting(WinAGISettingsFile);
            WinAGISettings.IObjsByNumber.WriteSetting(WinAGISettingsFile);
            WinAGISettings.WordsByNumber.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowAllMessages.WriteSetting(WinAGISettingsFile);
            WinAGISettings.SpecialSyntax.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ReservedAsText.WriteSetting(WinAGISettingsFile);
            WinAGISettings.CodeStyle.WriteSetting(WinAGISettingsFile);

            // pictures
            WinAGISettings.ShowBands.WriteSetting(WinAGISettingsFile);
            WinAGISettings.SplitWindow.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PicScalePreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PicScaleEdit.WriteSetting(WinAGISettingsFile);
            WinAGISettings.CursorMode.WriteSetting(WinAGISettingsFile);

            // sounds
            WinAGISettings.PlaybackMode.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowKeyboard.WriteSetting(WinAGISettingsFile);
            WinAGISettings.NoKeyboardSound.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowNotes.WriteSetting(WinAGISettingsFile);
            WinAGISettings.OneTrack.WriteSetting(WinAGISettingsFile);
            for (int i = 0; i < 3; i++) {
                WinAGISettings.DefInst[i].WriteSetting(WinAGISettingsFile);
                WinAGISettings.DefMute[i].WriteSetting(WinAGISettingsFile);
            }
            WinAGISettings.DefMute[3].WriteSetting(WinAGISettingsFile);
            WinAGISettings.SndZoom.WriteSetting(WinAGISettingsFile);

            // view settings
            WinAGISettings.ShowGrid.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ShowVEPreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefPrevPlay.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ViewAlignH.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ViewAlignV.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefCelH.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefCelW.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ViewScalePreview.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ViewScaleEdit.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefVColor1.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DefVColor2.WriteSetting(WinAGISettingsFile);

            // layout settings
            WinAGISettings.DefUseLE.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LEPages.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LEShowPics.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LEUseGrid.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LEGrid.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LESync.WriteSetting(WinAGISettingsFile);
            WinAGISettings.LEZoom.WriteSetting(WinAGISettingsFile);
            WinAGISettings.RoomEdgeColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.RoomFillColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.TransPtEdgeColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.TransPtFillColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.CmtEdgeColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.CmtFillColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ErrPtEdgeColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ErrPtFillColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ExitEdgeColor.WriteSetting(WinAGISettingsFile);
            WinAGISettings.ExitOtherColor.WriteSetting(WinAGISettingsFile);

            // global editor settings
            WinAGISettings.GEShowComment.WriteSetting(WinAGISettingsFile);
            if (blnChangeCmtCol) {
                //  reset split fractions
                WinAGISettings.GENameFrac.Value = 0;
                WinAGISettings.GEValFrac.Value = 0;
            }
            WinAGISettings.GENameFrac.WriteSetting(WinAGISettingsFile);
            WinAGISettings.GEValFrac.WriteSetting(WinAGISettingsFile);

            // mru list options
            WinAGISettings.AutoOpen.WriteSetting(WinAGISettingsFile);

            // platform defaults
            WinAGISettings.AutoFill.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PlatformType.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PlatformFile.WriteSetting(WinAGISettingsFile);
            WinAGISettings.DOSExec.WriteSetting(WinAGISettingsFile);
            WinAGISettings.PlatformOpts.WriteSetting(WinAGISettingsFile);

            // pictest
            PicEditTestSettings.ObjSpeed.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.ObjPriority.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.ObjRestriction.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.Horizon.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.IgnoreHorizon.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.IgnoreBlocks.WriteSetting(WinAGISettingsFile);
            PicEditTestSettings.CycleAtRest.WriteSetting(WinAGISettingsFile);

            // save settings to file
            WinAGISettingsFile.Save();
        }

        public void RefreshPreviewSyntaxStyles() {
            rtfPreview.ClearStylesBuffer();
            prevCommentStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[1].Color.Value);
            prevCommentStyle.FontStyle = NewSettings.SyntaxStyle[1].FontStyle.Value;
            prevStringStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[2].Color.Value);
            prevStringStyle.FontStyle = NewSettings.SyntaxStyle[2].FontStyle.Value;
            prevKeyWordStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[3].Color.Value);
            prevKeyWordStyle.FontStyle = NewSettings.SyntaxStyle[3].FontStyle.Value;
            prevTestCmdStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[4].Color.Value);
            prevTestCmdStyle.FontStyle = NewSettings.SyntaxStyle[4].FontStyle.Value;
            prevActionCmdStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[5].Color.Value);
            prevActionCmdStyle.FontStyle = NewSettings.SyntaxStyle[5].FontStyle.Value;
            prevInvalidCmdStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[6].Color.Value);
            prevInvalidCmdStyle.FontStyle = NewSettings.SyntaxStyle[6].FontStyle.Value;
            prevNumberStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[7].Color.Value);
            prevNumberStyle.FontStyle = NewSettings.SyntaxStyle[7].FontStyle.Value;
            prevArgIdentifierStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[8].Color.Value);
            prevArgIdentifierStyle.FontStyle = NewSettings.SyntaxStyle[8].FontStyle.Value;
            prevDefIdentifierStyle.ForeBrush = new SolidBrush(NewSettings.SyntaxStyle[9].Color.Value);
            prevDefIdentifierStyle.FontStyle = NewSettings.SyntaxStyle[9].FontStyle.Value;
        }

        public void PreviewSyntaxHighlight() {
            rtfPreview.Range.SetStyle(prevCommentStyle, CommentStyleRegEx1, RegexOptions.Multiline);
            rtfPreview.Range.SetStyle(prevCommentStyle, CommentStyleRegEx2, RegexOptions.Multiline);
            rtfPreview.Range.SetStyle(prevStringStyle, StringStyleRegEx);
            rtfPreview.Range.SetStyle(prevKeyWordStyle, KeyWordStyleRegEx);
            rtfPreview.Range.SetStyle(prevTestCmdStyle, TestCmdStyleRegex);
            rtfPreview.Range.SetStyle(prevActionCmdStyle, @"\b(set\.view|toggle|print)\b"); // ActionCmdStyleRegEx);
            rtfPreview.Range.SetStyle(prevInvalidCmdStyle, @"\ballow.menu\b"); // InvalidCmdStyleRegEx);
            rtfPreview.Range.SetStyle(prevNumberStyle, NumberStyleRegEx);
            rtfPreview.Range.SetStyle(prevArgIdentifierStyle, ArgIdentifierStyleRegEx);
            rtfPreview.Range.SetStyle(prevDefIdentifierStyle, DefIdentifierStyleRegEx);
            rtfPreview.Range.ClearFoldingMarkers();
            rtfPreview.Range.SetFoldingMarkers("{", "}");
        }

        private void RefreshCodeExample() {
            string tab = "".PadRight(NewSettings.LogicTabWidth.Value);
            switch (NewSettings.CodeStyle.Value) {
            case LogicDecoder.AGICodeStyle.cstDefaultStyle:
                rtfPreview.Text = "[ CodeStyle: Default\n" +
                                  "\n" +
                                  "[ local defines\n" +
                                  "#define aVariable  v40\n" +
                                  "#define aNumber    199\n" +
                                  "if (aVariable == aNumber && said(\"fall\")\n" +
                                tab + "{\n" +
                                tab + "set.view(ego, vw.EgoFalling);\n" +
                                tab + "toggle(f200);\n" +
                                tab + "print(\"You are now Falling!\");\n" +
                                tab + "}\n" +
                                  "else\n" +
                                tab + "{\n" +
                                tab + "[ not valid in version 2 games\n" +
                                tab + "allow.menu(0);\n" +
                                tab + "}\n";
                break;
            case LogicDecoder.AGICodeStyle.cstAltStyle1:
                rtfPreview.Text = "[ CodeStyle: Alternate 1\n" +
                                  "\n" +
                                  "[ local defines\n" +
                                  "#define aVariable  v40\n" +
                                  "#define aNumber    199\n" +
                                  "if (aVariable == aNumber && said(\"fall\") {\n" +
                                tab + "set.view(ego, vw.EgoFalling);\n" +
                                tab + "toggle(f200);\n" +
                                tab + "print(\"You are now Falling!\");\n" +
                                  "}\n" +
                                  "else {\n" +
                                tab + "[ not valid in version 2 games\n" +
                                tab + "allow.menu(0);\n" +
                                  "}";
                break;
            case LogicDecoder.AGICodeStyle.cstAltStyle2:
                rtfPreview.Text = "[ CodeStyle: Alternate 2\n" +
                                  "\n" +
                                  "[ local defines\n" +
                                  "#define aVariable  v40\n" +
                                  "#define aNumber    199\n" +
                                  "if (aVariable == aNumber && said(\"fall\") {\n" +
                                tab + "set.view(ego, vw.EgoFalling);\n" +
                                tab + "toggle(f200);\n" +
                                tab + "print(\"You are now Falling!\");\n" +
                                  "} else {\n" +
                                tab + "[ not valid in version 2 games\n" +
                                tab + "allow.menu(0);\n" +
                                  "}";
                break;
            }
        }
    }
}
