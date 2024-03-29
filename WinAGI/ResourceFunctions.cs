﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.AudioPlayer;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using System.Diagnostics;

namespace WinAGI.Engine {
    public static partial class Base {
        //constants used in extracting compressed resources
        const int TABLE_SIZE = 18041;
        const int START_BITS = 9;

        //variables used in extracting compressed resources
        private static int lngMaxCode;
        private static uint[] intPrefix;
        private static byte[] bytAppend;
        private static int lngBitsInBuffer;
        private static uint lngBitBuffer;
        private static int lngOriginalSize;

        //sound subclassing variables, constants, declarations
        internal static AudioPlayer SndPlayer = new();
        //public Declare int CallWindowProc Lib "user32" Alias "CallWindowProcA" (int lpPrevWndFunc, int hWnd, int uMsg, int wParam, int lParam)
        //public Declare int SetWindowLong Lib "user32" Alias "SetWindowLongA" (int hWnd, int nIndex, int dwNewLong)
        public const int GWL_WNDPROC = (-4);
        //public Declare int mciGetErrorString Lib "winmm.dll" Alias "mciGetErrorStringA" (int dwError, string lpstrBuffer, int uLength)
        //public Declare int mciSendString Lib "winmm.dll" Alias "mciSendStringA" (string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback)
        public const int MM_MCINOTIFY = 0x3B9;
        public const int MCI_NOTIFY_SUCCESSFUL = 0x1;

        internal static bool ExtractResources(AGIGame game) {
            // gets the resources from VOL files, and adds them to the game
            // returns true if resources loaded with warnings
            // returns false if one or more recoverable errors occur during load
            // throws error if critical error enountered that prevents game from loading

            byte bytResNum;
            AGIResType bytResType;
            string strDirFile = "";
            byte[] bytBuffer = [];
            bool blnDirtyDIR = false;
            byte byte1, byte2, byte3;
            int lngDirOffset = 0;  // offset of this resource's directory in Dir file (for v3)
            int lngDirSize = 0, intResCount, i;
            sbyte bytVol;
            int lngLoc;
            bool loadWarnings = false;
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.etInfo,
                InfoType = EInfoType.itResources,
                ID = "",
                Module = "",
                Text = "",
            };
            Raise_LoadGameEvent(warnInfo);
            // set up for warnings
            warnInfo.Type = EventType.etWarning;

            //if version 3
            if (game.agIsVersion3) {
                //get combined dir Volume
                strDirFile = game.agGameDir + game.agGameID + "DIR";
                // verify file exists
                if (!File.Exists(strDirFile)) {
                    WinAGIException wex = new(LoadResString(524).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 524
                    };
                    wex.Data["missingfile"] = strDirFile;
                    throw wex;
                }
                // check for readonly
                if ((File.GetAttributes(strDirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    WinAGIException wex = new(LoadResString(700).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 700,
                    };
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
                try {
                    //open the file, load it into buffer, and close it
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        bytBuffer = new byte[fsDIR.Length];
                        fsDIR.Read(bytBuffer);
                    }
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = Path.GetFileName(strDirFile);
                    throw wex;
                }
                //if not enough bytes to hold at least the 4 dir pointers + 1 resource
                if (bytBuffer.Length < 11) // 11 + bytes
                {
                    WinAGIException wex = new(LoadResString(542).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 542
                };
                    wex.Data["baddir"] = Path.GetFileName(strDirFile);
                    throw wex;
                }
            }
            // step through all four resource types
            for (bytResType = 0; bytResType <= AGIResType.rtView; bytResType++) {
                //if version 3
                if (game.agIsVersion3) {
                    //calculate offset and size of each dir component
                    switch (bytResType) {
                    case AGIResType.rtLogic:
                        lngDirOffset = bytBuffer[0] + (bytBuffer[1] << 8);
                        lngDirSize = bytBuffer[2] + (bytBuffer[3] << 8) - lngDirOffset;
                        break;
                    case AGIResType.rtPicture:
                        lngDirOffset = bytBuffer[2] + (bytBuffer[3] << 8);
                        lngDirSize = bytBuffer[4] + (bytBuffer[5] << 8) - lngDirOffset;
                        break;
                    case AGIResType.rtView:
                        lngDirOffset = bytBuffer[4] + (bytBuffer[5] << 8);
                        lngDirSize = bytBuffer[6] + (bytBuffer[7] << 8) - lngDirOffset;
                        break;
                    case AGIResType.rtSound:
                        lngDirOffset = bytBuffer[6] + (bytBuffer[7] << 8);
                        lngDirSize = bytBuffer.Length - lngDirOffset;
                        break;
                    default:
                        break;
                    }
                }
                else {
                    //no offset for version 2
                    lngDirOffset = 0;
                    //get name of resource dir file
                    strDirFile = game.agGameDir + ResTypeAbbrv[(int)bytResType] + "DIR";
                    //verify it exists
                    if (!File.Exists(strDirFile)) {
                        WinAGIException wex = new(LoadResString(524).Replace(ARG1, strDirFile)) {
                            HResult = WINAGI_ERR + 524
                        };
                        wex.Data["missingfile"] = strDirFile;
                        throw wex;
                    }
                    // readonly not allowed
                    if ((File.GetAttributes(strDirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                        WinAGIException wex = new(LoadResString(700).Replace(ARG1, strDirFile)) {
                            HResult = WINAGI_ERR + 700,
                        };
                        wex.Data["badfile"] = strDirFile;
                        throw wex;
                    }
                    try {
                        //open the file, load it into buffer, and close it
                        using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                            bytBuffer = new byte[fsDIR.Length];
                            fsDIR.Read(bytBuffer);
                        }
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                            HResult = WINAGI_ERR + 502
                        };
                        wex.Data["exception"] = e;
                        wex.Data["badfile"] = strDirFile;
                        throw wex;
                    }

                    //get size
                    lngDirSize = bytBuffer.Length;
                }

                // if invalid dir information, return false
                if ((lngDirOffset < 0) || (lngDirSize < 0)) {
                    WinAGIException wex = new(LoadResString(542).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 542
                    };
                    wex.Data["baddir"] = Path.GetFileName(strDirFile);
                    throw wex;
                }

                // if at least one resource,
                if (lngDirSize >= 3) {
                    if (lngDirOffset + lngDirSize > bytBuffer.Length) {
                        WinAGIException wex = new(LoadResString(542).Replace(ARG1, strDirFile)) {
                            HResult = WINAGI_ERR + 542
                        };
                        wex.Data["baddir"] = Path.GetFileName(strDirFile);
                        throw wex;
                    }
                }

                //max size of useable directory is 768 (256*3)
                if (lngDirSize > 768) {
                    //warning- file might be invalid
                    loadWarnings = true;
                    if (game.agIsVersion3) {
                        warnInfo.ResType = AGIResType.rtGame;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "DW01";
                        warnInfo.Text = ResTypeAbbrv[(int)bytResType] + " portion of DIR file is larger than expected; it may be corrupted";
                        warnInfo.Line = "--";
                        warnInfo.Module = "--";
                        Raise_LoadGameEvent(warnInfo);
                    }
                    else {
                        warnInfo.ResType = AGIResType.rtGame;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "DW02";
                        warnInfo.Text = ResTypeAbbrv[(int)bytResType] + "DIR file is larger than expected; it may be corrupted";
                        warnInfo.Line = "--";
                        warnInfo.Module = "--";
                        Raise_LoadGameEvent(warnInfo);
                    }
                    //assume the max for now
                    intResCount = 256;
                }
                else {
                    intResCount = lngDirSize / 3;
                }

                //if this resource type has entries,
                if (intResCount > 0) {
                    //check for bad resources
                    for (i = 0; i < intResCount; i++) {
                        bytResNum = (byte)i;
                        warnInfo.Type = EventType.etInfo;
                        warnInfo.InfoType = EInfoType.itResources;
                        warnInfo.ResType = bytResType;
                        warnInfo.ResNum = bytResNum;
                        warnInfo.ID = "";
                        warnInfo.Text = "";
                        warnInfo.Line = "--";
                        warnInfo.Module = "--";
                        Raise_LoadGameEvent(warnInfo);
                        warnInfo.Type = EventType.etWarning;

                        //get location data for this resource
                        byte1 = bytBuffer[lngDirOffset + bytResNum * 3];
                        byte2 = bytBuffer[lngDirOffset + bytResNum * 3 + 1];
                        byte3 = bytBuffer[lngDirOffset + bytResNum * 3 + 2];
                        //ignore any 0xFFFFFF sequences,
                        if (byte1 != 0xff) {
                            //extract volume and location
                            bytVol = (sbyte)(byte1 >> 4);
                            lngLoc = ((byte1 & 0xF) << 16) + (byte2 << 8) + byte3;
                            //add a resource of this res type
                            switch (bytResType) {
                            case AGIResType.rtLogic:
                                game.agLogs.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agLogs[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(AGIResType.rtLogic, bytResNum, game.agLogs[bytResNum].ErrLevel, game.agLogs[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agLogs.Exists(bytResNum)) {
                                    game.agViews[bytResNum].WritePropState = false;
                                    game.agLogs[bytResNum].IsDirty = false;
                                    // logic source checks come after all resources loaded so leave it loaded
                                }
                                break;
                            case AGIResType.rtPicture:
                                game.agPics.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agPics[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(AGIResType.rtPicture, bytResNum, game.agPics[bytResNum].ErrLevel, game.agPics[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agPics.Exists(bytResNum)) {
                                    game.agViews[bytResNum].WritePropState = false;
                                    game.agPics[bytResNum].IsDirty = false;
                                    game.agPics[bytResNum].Unload();
                                }
                                break;
                            case AGIResType.rtSound:
                                game.agSnds.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agSnds[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(AGIResType.rtSound, bytResNum, game.agSnds[bytResNum].ErrLevel, game.agSnds[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agSnds.Exists(bytResNum)) {
                                    game.agViews[bytResNum].WritePropState = false;
                                    game.agSnds[bytResNum].IsDirty = false;
                                    game.agSnds[bytResNum].Unload();
                                }
                                break;
                            case AGIResType.rtView:
                                game.agViews.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agViews[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(AGIResType.rtView, bytResNum, game.agViews[bytResNum].ErrLevel, game.agViews[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                //make sure it was added before finishing
                                if (game.agViews.Exists(bytResNum)) {
                                    game.agViews[bytResNum].WritePropState = false;
                                    game.agViews[bytResNum].IsDirty = false;
                                    game.agViews[bytResNum].Unload();
                                }
                                break;
                            }
                        }
                    }
                    //if a v2 DIR was modified, save it
                    if (!game.agIsVersion3 && blnDirtyDIR) {
                        try {
                            //save the new DIR file
                            if (File.Exists(strDirFile + ".OLD")) {
                                File.Delete(strDirFile + ".OLD");
                                File.Move(strDirFile, strDirFile + ".OLD");
                            }
                            using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                                fsDIR.Write(bytBuffer);
                            }
                        }
                        catch (Exception e) {
                            // error!
                            WinAGIException wex = new(LoadResString(703)) {
                                HResult = WINAGI_ERR + 703,
                            };
                            wex.Data["exception"] = e;
                            wex.Data["dirfile"] = strDirFile;
                            throw wex;
                        }
                        //reset the dirty flag
                        blnDirtyDIR = false;
                    }
                }
            }

            //if a V3 DIR file was modified, save it
            if (game.agIsVersion3 && blnDirtyDIR) {
                //save the new DIR file
                try {
                    //save the new DIR file
                    if (File.Exists(strDirFile + ".OLD")) {
                        File.Delete(strDirFile + ".OLD");
                        File.Move(strDirFile, strDirFile + ".OLD");
                    }
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        fsDIR.Write(bytBuffer);
                    }
                }
                catch (Exception e) {
                    // error!
                    WinAGIException wex = new(LoadResString(703)) {
                        HResult = WINAGI_ERR + 703,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = strDirFile;
                    throw wex;
                }
            }
            //return any warning codes
            return loadWarnings;
        }

        internal static void AddLoadWarning(AGIResType resType, byte resNum, int errlevel, string[] errdata) {
            // called when warning encountered while trying to extract resources
            // during game load
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.etWarning,
                ResType = resType,
                ResNum = resNum,
                ID = "",
                Module = "--",
                Text = "",
                Line = "--"
            };

            // include check for positive warning values
            if (errlevel > 0) {
                switch (resType) {
                case AGIResType.rtLogic:
                    // none
                    break;
                case AGIResType.rtPicture:
                    if (errlevel >= 8) {
                        // unhandled error
                        warnInfo.ID = "RW05";
                        warnInfo.Text = $"Unhandled error in Picture {resNum} data- picture may not display correctly ({errlevel})";
                        warnInfo.Module = errdata[0];
                        Raise_LoadGameEvent(warnInfo);
                    }
                    else {
                        // missing EOP marker, bad color or bad cmd
                        warnInfo.ID = "RW06";
                        warnInfo.Text = $"Data anomalies in Picture {resNum} cannot be decompiled ({errlevel})";
                        warnInfo.Module = errdata[0];
                        Raise_LoadGameEvent(warnInfo);
                    }
                    break;
                case AGIResType.rtSound:
                    warnInfo.ID = "RW07";
                    warnInfo.Text = $"Sound {resNum} has an invalid track pointer ({errlevel})";
                    warnInfo.Module = errdata[0];
                    Raise_LoadGameEvent(warnInfo);
                    break;
                case AGIResType.rtView:
                    warnInfo.ID = "RW08";
                    warnInfo.Text = $"View {resNum} has an invalid view description pointer";
                    warnInfo.Module = errdata[0];
                    Raise_LoadGameEvent(warnInfo);
                    break;
                }
                return;
            }
            // resource errors:
            switch (errlevel) {
            case -1: // 606:
                // Can't load resource: file not found (%1)
                warnInfo.ID = "VW03";
                warnInfo.Text = $"{ResTypeName[(int)resType]} {resNum} is in a VOL file ({Path.GetFileName(errdata[0])}) that does not exist";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -2: // 700:
                // file (%1) is readonly
                warnInfo.ID = "VW03";
                warnInfo.Text = $"{ResTypeName[(int)resType]} {resNum} is in a VOL file ({Path.GetFileName(errdata[0])}) marked readonly";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -3: // 502:
                // Error %1 occurred while trying to access %2.
                warnInfo.ID = "VW01";
                warnInfo.Text = $"{ResTypeName[(int)resType]} {resNum} is invalid due to file access error ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -4: // 505:
                //Invalid resource location (%1) in %2.
                warnInfo.ID = "VW02";
                warnInfo.Text = $"{ResTypeName[(int)resType]} {resNum} has an invalid location ({errdata[0]}) in volume file {errdata[2]}";
                warnInfo.Module = errdata[3];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -5: // 506:
                // invalid header
                warnInfo.ID = "RW09";
                warnInfo.Text = $"{ResTypeName[(int)resType]} {resNum} has an invalid resource header at location {errdata[0]} in {errdata[2]}";
                warnInfo.Module = errdata[3];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -6: // 704:
                // sourcefile missing
                // TODO: need new load warning value
                warnInfo.ID = "RW99";
                warnInfo.Text = $"Logic {resNum} source file is missing ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -7: // 700:
                // sourcefile is readonly
                // TODO: need new load warning value
                warnInfo.ID = "RW98";
                warnInfo.Text = $"Logic {resNum} source file is marked readonly ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -8: // 502: 
                // Error %1 occurred while trying to access logic source file(%2).
                // TODO: need new load warning value
                warnInfo.ID = "RW97";
                warnInfo.Text = $"Logic {resNum} is invalid due to file access error ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -9: // 688: 
                // Error %1 occurred while decompiling message section.
                // TODO: need new load warning value
                warnInfo.ID = "RW96";
                warnInfo.Text = $"Logic {resNum} is invalid due to error in message section ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -10: // 688: 
                // Error %1 occurred while decompiling labels.
                // TODO: need new load warning value
                warnInfo.ID = "RW95";
                warnInfo.Text = $"Logic {resNum} is invalid due to error in label search ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -11: // 688: 
                // Error %1 occurred while decompiling if block.
                // TODO: need new load warning value
                warnInfo.ID = "RW94";
                warnInfo.Text = $"Logic {resNum} is invalid due to error in if block section ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -12: // 688: 
                // Error %1 occurred while decompiling - invalid message.
                // TODO: need new load warning value
                warnInfo.ID = "RW93";
                warnInfo.Text = $"Logic {resNum} is invalid due to invalid message value ({errdata[0]})";
                warnInfo.Module = errdata[1];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -13: // 598:
                // invalid sound data
                // TODO: need new load warning value
                warnInfo.ID = "RW92";
                warnInfo.Text = $"Invalid sound data format, unable to load tracks";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -14: // 565:
                // sound invalid data error
                // TODO: need new load warning value
                warnInfo.ID = "RW91";
                warnInfo.Text = $"Error encountered in LoadTracks ({errdata[1]})";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -15: // 595:
                // invalid view data
                // TODO: need new load warning value
                warnInfo.ID = "RW90";
                warnInfo.Text = $"Invalid view data, unable to load view";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -16: // 548:
                // invalid loop pointer
                // TODO: need new load warning value
                warnInfo.ID = "RW89";
                warnInfo.Text = $"Invalid loop data pointer detected (loop {errdata[1]})";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -17: // 539:
                // invalid source loop for mirror/invalid mirror loop number
                // TODO: need new load warning value
                warnInfo.ID = "RW88";
                warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[2]} and/or loop {errdata[3]})";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -18: // 550:
                // invalid mirror data, target loop already mirrored
                // TODO: need new load warning value
                warnInfo.ID = "RW87";
                warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[3]} already mirrored)";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -19: // 551:
                // invalid mirror data, source already a mirror
                // TODO: need new load warning value
                warnInfo.ID = "RW86";
                warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[2]} already mirrored)";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            case -20: // 553:
                // invalid cel data pointer
                // TODO: need new load warning value
                warnInfo.ID = "RW85";
                warnInfo.Text = $"Invalid cel pointer detected (cel {errdata[2]} of loop {errdata[1]})";
                warnInfo.Module = errdata[0];
                Raise_LoadGameEvent(warnInfo);
                break;
            // TODO: remove loadwarning "RW11" (unhandled load error)
            default:
                // should be no others
                Debug.Assert(false);
                break;
            }
        }

        internal static byte[] DecompressPicture(byte[] bytOriginalData) {
            short intPosIn = 0;
            byte bytCurComp, bytBuffer = 0, bytCurUncomp;
            bool blnOffset = false;
            int lngTempCurPos = 0;
            byte[] bytExpandedData = new byte[MAX_RES_SIZE];
            // TODO: should resize array to the uncompressed size; it's 
            // included in the header, so no reason not to use it.

            // decompress the picture
            do {
                //get current compressed byte
                bytCurComp = bytOriginalData[intPosIn];
                intPosIn++;

                //if currently offset,
                if (blnOffset) {
                    //adjust buffer byte
                    bytBuffer += (byte)(bytCurComp >> 4);
                    //extract uncompressed byte
                    bytCurUncomp = bytBuffer;
                    //shift buffer back
                    bytBuffer = (byte)(bytCurComp << 4);
                }
                else {
                    //byte is not compressed
                    bytCurUncomp = bytCurComp;
                }
                //save byte to temp resource
                bytExpandedData[lngTempCurPos] = bytCurUncomp;
                lngTempCurPos++;

                //check if byte sets or restores offset
                if (((bytCurUncomp == 0xF0) || (bytCurUncomp == 0xF2)) && (intPosIn < bytOriginalData.Length)) {
                    //if currently offset
                    if (blnOffset) {
                        //write rest of buffer byte
                        bytExpandedData[lngTempCurPos] = (byte)(bytBuffer >> 4);
                        lngTempCurPos++;
                        //restore offset
                        blnOffset = false;
                    }
                    else {
                        //get next byte
                        bytCurComp = bytOriginalData[intPosIn];
                        intPosIn++;
                        //save the byte, after shifting
                        bytExpandedData[lngTempCurPos] = (byte)(bytCurComp >> 4);
                        lngTempCurPos++;
                        //fill buffer
                        bytBuffer = (byte)(bytCurComp << 4);
                        blnOffset = true;
                    }
                }
            }
            //continue until all original data has been read
            while (intPosIn < bytOriginalData.Length);
            //redim to array to actual size
            Array.Resize(ref bytExpandedData, lngTempCurPos);
            return bytExpandedData;
        }
        internal static byte[] ExpandV3ResData(byte[] bytOriginalData, int lngExpandedSize) {
            int intPosIn, intPosOut, intNextCode, i;
            uint intOldCode, intNewCode;
            string strDat;
            char strChar;
            int intCodeSize;
            byte[] bytTempData;
            //initialize variables
            intPrefix = new uint[TABLE_SIZE]; //remember to correct index by 257
            bytAppend = new byte[TABLE_SIZE]; //remember to correct index by 257

            //set temporary data field
            bytTempData = new byte[lngExpandedSize];
            //original size is determined by array bounds
            lngOriginalSize = bytOriginalData.Length;

            //reset variables used in expansion
            intPosIn = 0;
            intPosOut = 0;

            lngBitBuffer = 0;
            lngBitsInBuffer = 0;

            //set initial Value for code size
            intCodeSize = NewCodeSize(START_BITS);
            //first code is 257
            intNextCode = 257;
            //this seems wrong! first code should be 258, right?
            //isn't 257 the 'end' code?

            //Read in the first code.
            intOldCode = InputCode(ref bytOriginalData, intCodeSize, ref intPosIn);
            //!!!!why is this set???
            strChar = (char)0;
            //first code for  SIERRA resouces should always be 256
            //if first code is NOT 256
            if (intOldCode != 256) {
                intPrefix = [];
                bytAppend = [];
                //TODO: wrong error msg here!
                WinAGIException wex = new(LoadResString(559).Replace(ARG1, "123")) {
                    HResult = WINAGI_ERR + 559,
                };
                throw wex;
            }

            //now begin decompressing actual data
            intNewCode = InputCode(ref bytOriginalData, intCodeSize, ref intPosIn);

            //continue extracting data, until all bytes are read (or end code is reached)
            while ((intPosIn <= lngOriginalSize) && (intNewCode != 0x101)) {
                //if new code is 0x100,(256)
                if (intNewCode == 0x100) {
                    //Restart LZW process (should tables be flushed?)
                    intNextCode = 258;
                    intCodeSize = NewCodeSize(START_BITS);
                    //Read in the first code.
                    intOldCode = InputCode(ref bytOriginalData, intCodeSize, ref intPosIn);
                    //the character Value is same as code for beginning
                    strChar = (char)intOldCode;
                    //write out the first character
                    bytTempData[intPosOut] = (byte)intOldCode;
                    intPosOut++;
                    //now get next code
                    intNewCode = InputCode(ref bytOriginalData, intCodeSize, ref intPosIn);
                }
                else {
                    // This code checks for the special STRING+character+STRING+character+STRING
                    // case which generates an undefined code.  It handles it by decoding
                    // the last code, and adding a single Character to the end of the decode string.
                    // (new_code will ONLY return a next_code Value if the condition exists;
                    // it should otherwise return a known code, or a ascii Value)
                    if ((intNewCode >= intNextCode)) {
                        //decode the string using old code
                        strDat = DecodeString(intOldCode);
                        //append the character code
                        strDat += strChar;
                    }
                    else {
                        //decode the string using new code
                        strDat = DecodeString(intNewCode);
                    }
                    //retreive the character Value
                    strChar = strDat[0];
                    //now send out decoded data (it's backwards in the string, so
                    //start at end and work back to beginning)
                    for (i = 0; i < strDat.Length; i++) {
                        bytTempData[intPosOut] = (byte)strDat[i];
                        intPosOut++;
                    }
                    //if no more room in the current bit-code table,
                    if ((intNextCode > lngMaxCode)) {
                        //get new code size (in number of bits per code)
                        intCodeSize = NewCodeSize(intCodeSize + 1);
                    }
                    //store code in prefix table
                    intPrefix[intNextCode - 257] = intOldCode;
                    //store append character in table
                    bytAppend[intNextCode - 257] = (byte)strChar;
                    //increment next code pointer
                    intNextCode++;
                    intOldCode = intNewCode;
                    //get the next code
                    intNewCode = InputCode(ref bytOriginalData, intCodeSize, ref intPosIn);
                }
            }

            ////copy array
            //Array.Resize(ref bytOriginalData, lngExpandedSize);
            //bytOriginalData = bytTempData;
            //free lzw arrays
            intPrefix = [];
            bytAppend = [];
            return bytTempData;
        }
        internal static byte[] CompressedCel(Cel Cel, bool blnMirror) {
            //this method compresses cel data
            //into run-length-encoded data that
            //can be written to an AGI View resource
            //blnMirror is used to ensure mirrored cels include enough room
            //for the flipped cel
            byte[] bytTempRLE;
            byte mHeight, mWidth, bytChunkLen;
            byte[,] mCelData;
            AGIColorIndex mTransColor;
            int lngByteCount = 0;
            byte bytChunkColor, bytNextColor;
            byte bytOut;
            bool blnFirstChunk;
            int lngMirrorCount = 0;
            int i, j;

            //copy cel data locally
            mHeight = Cel.Height;
            mWidth = Cel.Width;
            mTransColor = Cel.TransColor;
            mCelData = Cel.AllCelData;
            //assume one byte per pixel to start with
            //(include one byte for ending zero)
            bytTempRLE = new byte[mHeight * mWidth + 1];
            //step through each row
            for (j = 0; j < mHeight; j++) {
                //get first pixel color
                bytChunkColor = (byte)mCelData[0, j];
                bytChunkLen = 1;
                blnFirstChunk = true;
                //step through rest of pixels in this row
                for (i = 1; i < mWidth; i++) {
                    //get next pixel color
                    bytNextColor = (byte)mCelData[i, j];
                    //if different from current chunk
                    if ((bytNextColor != bytChunkColor) || (bytChunkLen == 0xF)) {
                        //write chunk
                        bytTempRLE[lngByteCount] = (byte)((int)bytChunkColor * 0x10 + bytChunkLen);
                        //increment Count
                        lngByteCount++;

                        //if this is NOT first chunk or NOT transparent)
                        if (!blnFirstChunk || (bytChunkColor != (byte)mTransColor)) {
                            //increment lngMirorCount for any chunks
                            //after the first, and also for the first
                            //if it is NOT transparent color
                            lngMirrorCount++;
                        }
                        blnFirstChunk = false;
                        //set chunk to new color
                        bytChunkColor = bytNextColor;
                        //reset length
                        bytChunkLen = 1;
                    }
                    else {
                        //increment chunk length
                        bytChunkLen++;
                    }
                }
                //if last chunk is NOT transparent
                if (bytChunkColor != (byte)mTransColor) {
                    //add last chunk
                    bytTempRLE[lngByteCount] = (byte)(bytChunkColor * 0x10 + bytChunkLen);
                    //increment Count
                    lngByteCount++;
                }
                //always Count last chunk for mirror
                lngMirrorCount++;
                //add zero to indicate end of row
                bytTempRLE[lngByteCount] = 0;
                lngByteCount++;
                lngMirrorCount++;
                //if mirroring
            }
            if (blnMirror) {
                //add zeros to make room
                while (lngByteCount < lngMirrorCount) {    //add a zero
                    bytTempRLE[lngByteCount] = 0;
                    lngByteCount++;
                }
            }

            //reset size of array
            Array.Resize(ref bytTempRLE, lngByteCount);

            //return the compressed data
            return bytTempRLE;
        }
        internal static string DecodeString(uint intCode) {
            //this function converts a code Value into its original string Value

            string retval = "";

            while (intCode > 255) {
                //if code Value exceeds table size,
                if (intCode > TABLE_SIZE) {
                    //Debug.Print "FATAL ERROR as  Invalid code (" + CStr(intCode) + ") in DecodeString."
                    return retval;
                }
                else {
                    //build string
                    retval = (char)bytAppend[intCode - 257] + retval;
                    intCode = intPrefix[intCode - 257];
                }
            }
            retval = (char)intCode + retval;
            return retval;
        }
        internal static uint InputCode(ref byte[] bytData, int intCodeSize, ref int intPosIn) {
            uint lngWord, lngRet;
            //this routine extracts the next code Value off the input stream
            //since the number of bits per code can vary between 9 and 12,
            //can't read in directly from the stream

            //unlike normal LZW, though, the bytes are actually written so the code boundaries
            //work from right to left, NOT left to right. for (example,an input stream that needs
            //to be split on a 9 bit boundary will use eight bits of first byte, plus LOWEST
            //bit of byte 2. The second code is then the upper seven bits of byte 2 and the lower
            //2 bits of byte 3 etc:
            //                          byte boundaries (8 bits per byte)
            //          byte4           byte3           byte2           byte1           byte0
            // ...|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
            // ... x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x
            // ... 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
            //                   code3             code2             code1             code0
            //                          code boundaries (9 bits per code)
            //
            //the data stream is read into a bit buffer 8 bits at a time (i.e. a single byte)
            //once the buffer is full of data, the input code is pulled out, and the buffer
            //is shifted.
            //the input data from the stream must be shifted to ensure it lines up with data
            //currently in the buffer.

            //read until the buffer is greater than 24-
            //this ensures that the eight bits read from the input stream
            //will fit in the buffer (which is a long integer==4 bytes==32 bits)
            //also stop reading data if end of data stream is reached)
            while ((lngBitsInBuffer <= 24) && (intPosIn < lngOriginalSize)) {
                //get next byte
                lngWord = bytData[intPosIn];
                intPosIn++;

                //shift the data to the left by enough bits so the byte being added will not
                //overwrite the bits currently in the buffer, and add the bits to the buffer
                lngBitBuffer |= (lngWord << lngBitsInBuffer);

                //increment Count of how many bits are currently in the buffer
                lngBitsInBuffer += 8;
            }// Loop

            //the input code starts at the lowest bit in the buffer
            //since the buffer has 32 bits total, need to clear out all bits above the desired
            //number of bits to define the code (i.e. if 9 bits, AND with 0x1FF; 10 bits,
            //AND with 0x3FF, etc.)
            lngRet = (uint)(lngBitBuffer & ((1 << intCodeSize) - 1));

            //now need to shift the buffer to the RIGHT by the number of bits per code
            lngBitBuffer >>= intCodeSize;

            //adjust number of bits currently loaded in buffer
            lngBitsInBuffer -= intCodeSize;

            //and return code Value
            return lngRet;
        }
        internal static int NewCodeSize(int intVal) {
            //this function supports the expansion of compressed resources
            //it sets the size of codes which the LZW routine uses. The size
            //of the code first starts at 9 bits, then increases as all code
            //tables are filled. the max code size is 12; if an attempt is
            //made to set a code size above that, the function does nothing

            //the function also recalculates the maximum number of codes
            //available to the expand subroutine. This number is used to
            //determine when to call this function again.

            //max code size is 12 bits
            const int MAXBITS = 12;
            int retval;

            if (intVal == MAXBITS) {
                retval = 11;
                //this makes no sense!!!!
                //as written, it means max code size is really 11, not
                //12; an attempt to set it to 12 keeps it at 11???

                // I think it should be
                //if (intVal > MAXBITS) {
                //retval = MAXBITS;

            }
            else {
                retval = intVal;
                lngMaxCode = (1 << retval) - 2;
            }
            return retval;
        }

        internal static byte[] BuildMIDI(Sound SoundIn) {
            int lngWriteTrack = 0;
            int i, j;
            byte bytNote;
            int intFreq;
            byte bytVol;
            int lngTrackCount = 0, lngTickCount;
            int lngStart, lngEnd;

            //calculate track Count
            if (!SoundIn[0].Muted && SoundIn[0].Notes.Count > 0) {
                lngTrackCount = 1;
            }
            if (!SoundIn[1].Muted && SoundIn[1].Notes.Count > 0) {
                lngTrackCount++;
            }
            if (!SoundIn[2].Muted && SoundIn[2].Notes.Count > 0) {
                lngTrackCount++;
            }
            if (!SoundIn[3].Muted && SoundIn[3].Notes.Count > 0) {
                //add two tracks if noise is not muted
                //because the white noise and periodic noise
                //are written as two separate tracks
                lngTrackCount += 2;
            }

            //set intial size of midi data array
            SndPlayer.mMIDIData = new byte[70 + 256];

            // write header
            SndPlayer.mMIDIData[0] = 77; //"M"
            SndPlayer.mMIDIData[1] = 84; //"T"
            SndPlayer.mMIDIData[2] = 104; //"h"
            SndPlayer.mMIDIData[3] = 100; //"d"
            lngPos = 4;


            WriteSndLong(6); //remaining length of header (3 integers = 6 bytes)
                             //write mode, trackcount and ppqn as bigendian integers
            WriteSndWord(1);   //mode 0 = single track
                               //mode 1 = multiple tracks, all start at zero
                               //mode 2 = multiple tracks, independent start times
                               //if no tracks,
            if (lngTrackCount == 0) {
                //need to build a //null// set of data!!!

                Array.Resize(array: ref SndPlayer.mMIDIData, 38);

                //one track, with one note of smallest length, and no sound
                WriteSndWord(1);  //one track
                WriteSndWord(30); //pulses per quarter note
                                  //add the track info
                WriteSndByte(77); //"M"
                WriteSndByte(84); //"T"
                WriteSndByte(114); //"r"
                WriteSndByte(107); //"k"
                WriteSndLong(15); //track length
                                  //write the track number
                WriteSndDelta(0);
                //write the set instrument status byte
                WriteSndByte(0xC0);
                //write the instrument number
                WriteSndByte(0);
                //write a slight delay note with no volume to end
                WriteSndDelta(0);
                WriteSndByte(0x90);
                WriteSndByte(60);
                WriteSndByte(0);
                WriteSndDelta(16);
                WriteSndByte(0x80);
                WriteSndByte(60);
                WriteSndByte(0);
                //add end of track info
                WriteSndDelta(0);
                WriteSndByte(0xFF);
                WriteSndByte(0x2F);
                WriteSndByte(0x0);
                //return
                return SndPlayer.mMIDIData;
            }
            //add track count
            WriteSndWord(lngTrackCount);
            //write pulses per quarter note
            //(agi sound tick is 1/60 sec; each tick of an AGI note
            //is 1/60 of a second; by default, MIDI defines a whole
            //note as 2 seconds; therefore, a quarter note is 1/2
            //second, or 30 ticks
            WriteSndWord(30);

            //write the sound tracks
            for (i = 0; i < 3; i++) {
                //if adding this instrument,
                if (!SoundIn[i].Muted && SoundIn[i].Notes.Count > 0) {
                    WriteSndByte(77); //"M"
                    WriteSndByte(84); //"T"
                    WriteSndByte(114); //"r"
                    WriteSndByte(107); //"k"
                                       //store starting position for this track//s data
                    lngStart = lngPos;
                    //place holder for data size
                    WriteSndLong(0);
                    //write the track number
                    //*********** i think this should be zero for all tracks
                    //it's the delta sound value for the instrument setting
                    WriteSndDelta(0); //CLng(lngWriteTrack)
                                      //write the set instrument status byte
                    WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    //write the instrument number
                    WriteSndByte(SoundIn[i].Instrument);

                    //write a slight delay note with no volume to start
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4); //16
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);

                    //step through notes in this track (5 bytes at a time)
                    for (j = 0; j <= SoundIn[i].Notes.Count - 1; j++) {
                        //calculate note to play
                        if (SoundIn[i].Notes[j].FreqDivisor > 0) {
                            //middle C is 261.6 HZ; from midi specs,
                            //middle C is a note with a Value of 60
                            //this requires a shift in freq of approx. 36.376
                            //however, this offset results in crappy sounding music;
                            //empirically, 36.5 seems to work best
                            //**** TODO: OK, in c#, changing from dbl to byte truncates the value, which 
                            // makes things sound wrong when useing 36.5; but if we go back
                            // to 36.376, then it sounds OK; seems like difference is whether 
                            // or not you truncate the value (then use 36.376) or round the
                            // value (then use 36.5); this means the calculations used in the
                            // sound editor will probably need to be modified too
                            //?????? well, here we are, months later; now the 36.376 sounds crappy! WTH?
                            // by using 'round' instead of 'floor' (which is what I think (byte) conversion
                            // does, 36.376 sounds right again; smh
                            //bytNote = (byte)((Math.Log10(111860 / (double)(SoundIn[i].Notes[j].FreqDivisor)) / LOG10_1_12) - 36.5);
                            bytNote = (byte)Math.Round((Math.Log10(111860 / (double)SoundIn[i].Notes[j].FreqDivisor) / LOG10_1_12) - 36.376);
                            //
                            //f = 111860 / (((Byte2 + 0x3F) << 4) + (Byte3 + 0x0F))
                            //
                            //(bytNote can never be <44 or >164)
                            //in case note is too high,
                            if (bytNote > 127) {
                                bytNote = 127;
                            }
                            bytVol = (byte)(127 * (15 - SoundIn[i].Notes[j].Attenuation) / 15);
                        }
                        else {
                            bytNote = 0;
                            bytVol = 0;
                        }

                        //write NOTE ON data
                        WriteSndDelta(0);
                        WriteSndByte((byte)(0x90 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(bytVol);
                        //write NOTE OFF data
                        WriteSndDelta(SoundIn[i].Notes[j].Duration);
                        WriteSndByte((byte)(0x80 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(0);
                    } //nxt j

                    //write a slight delay note with no volume to end
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4); //16
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);

                    //add end of track info
                    WriteSndDelta(0);
                    WriteSndByte(0xFF);
                    WriteSndByte(0x2F);
                    WriteSndByte(0x0);
                    //save track end position
                    lngEnd = lngPos;
                    //set cursor to start of track
                    lngPos = lngStart;
                    //write the track length
                    WriteSndLong((lngEnd - lngStart) - 4);
                    lngPos = lngEnd;
                }
                //increment track counter
                lngWriteTrack++;
            } //nxt i

            //seashore does a good job of imitating the white noise, with frequency adjusted empirically
            //harpsichord does a good job of imitating the tone noise, with frequency adjusted empirically

            //if adding noise track,
            if (!SoundIn[3].Muted && SoundIn[3].Notes.Count > 0) {
                //because there are two types of noise, must use two channels
                //one uses seashore (white noise)
                //other uses harpsichord (tone noise)

                for (i = 0; i < 2; i++) {
                    //0 means add tone
                    //1 means add white noise

                    WriteSndByte(77); //"M"
                    WriteSndByte(84); //"T"
                    WriteSndByte(114); //"r"
                    WriteSndByte(107); //"k"

                    //store starting position for this track//s data
                    lngStart = lngPos;
                    WriteSndLong(0);      //place holder for chunklength

                    //write track number
                    WriteSndDelta(lngWriteTrack);
                    //write the set instrument byte
                    WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    //write the instrument number
                    switch (i) {
                    case 0:  //tone
                        WriteSndByte(6); //harpsichord seems to be good simulation for tone
                        break;
                    case 1:  //white noise
                        WriteSndByte(122); //seashore seems to be good simulation for white noise
                                           //crank up the volume
                        WriteSndByte(0);
                        WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        WriteSndByte(7);
                        WriteSndByte(127);
                        //set legato
                        WriteSndByte(0);
                        WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        WriteSndByte(68);
                        WriteSndByte(127);
                        break;
                    }

                    //write a slight delay note with no volume to start
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4); //16
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);

                    //reset tick counter (used in case of need to borrow track 3 freq)
                    lngTickCount = 0;

                    //0 - add periodic
                    //1 - add white noise
                    for (j = 0; j < SoundIn[3].Notes.Count; j++) {
                        //add duration to tickcount
                        lngTickCount += SoundIn[3].Notes[j].Duration;
                        //Fourth byte: noise freq and type
                        //    In the case of the noise voice,
                        //    7  6  5  4  3  2  1  0
                        //
                        //    1  .  .  .  .  .  .  .      Always 1.
                        //    .  1  1  0  .  .  .  .      Register number in T1 chip (6)
                        //    .  .  .  .  X  .  .  .      Unused, ignored; can be set to 0 or 1
                        //    .  .  .  .  .  FB .  .      1 for white noise, 0 for periodic
                        //    .  .  .  .  .  . NF0 NF1    2 noise frequency control bits
                        //
                        //    NF0  NF1       Noise Frequency
                        //
                        //     0    0         1,193,180 / 512 = 2330
                        //     0    1         1,193,180 / 1024 = 1165
                        //     1    0         1,193,180 / 2048 = 583
                        //     1    1         Borrow freq from channel 3
                        //
                        //AGINote contains bits 2-1-0 only
                        //
                        //if this note matches desired type
                        if ((SoundIn[3].Notes[j].FreqDivisor & 4) == 4 * i) {
                            //if using borrow function:
                            if ((SoundIn[3].Notes[j].FreqDivisor & 3) == 3) {
                                //get frequency from channel 3
                                intFreq = GetTrack3Freq(SoundIn[2], lngTickCount);
                            }
                            else {
                                //get frequency from bits 0 and 1
                                intFreq = (int)(2330.4296875 / (1 << (SoundIn[3].Notes[j].FreqDivisor & 3)));
                            }

                            //convert to midi note
                            if ((SoundIn[3].Notes[j].FreqDivisor & 4) == 4) {
                                //for white noise, 96 is my best guess to imitate noise
                                //BUT... 96 causes some notes to come out negative;
                                //80 is max Value that ensures all AGI freq values convert
                                //to positive MIDI note values
                                bytNote = (byte)((Math.Log10(intFreq) / LOG10_1_12) - 80);
                            }
                            else {
                                //for periodic noise, 64 is my best guess to imitate noise
                                bytNote = (byte)((Math.Log10(intFreq) / LOG10_1_12) - 64);
                            }
                            //get volume
                            bytVol = (byte)(127 * (15 - SoundIn[3].Notes[j].Attenuation) / 15);
                        }
                        else {
                            //write a blank
                            bytNote = 0;
                            bytVol = 0;
                        }

                        //write NOTE ON data
                        //no delta time
                        WriteSndDelta(0);
                        //note on this track
                        WriteSndByte((byte)(0x90 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(bytVol);

                        //write NOTE OFF data
                        WriteSndDelta(SoundIn[3].Notes[j].Duration);
                        WriteSndByte((byte)(0x80 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(0);
                    } //nxt j

                    //write a slight delay note with no volume to end
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4); //16
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);

                    //write end of track data
                    WriteSndDelta(0);
                    WriteSndByte(0xFF);
                    WriteSndByte(0x2F);
                    WriteSndByte(0x0);
                    //save ending position
                    lngEnd = lngPos;
                    //go back to start of track, and write track length
                    lngPos = lngStart;
                    WriteSndLong((lngEnd - lngStart) - 4);
                    lngPos = lngEnd;
                    //increment track counter
                    lngWriteTrack++;
                } //nxt i
            }

            //remove any extra padding from the data array (-1?)
            Array.Resize(ref SndPlayer.mMIDIData, lngPos);
            return SndPlayer.mMIDIData;
        }

        internal static byte[] BuildIIgsMIDI(Sound SoundIn, ref double Length) {
            int lngInPos, lngOutPos;
            byte[] midiIn, midiOut;
            int lngTicks = 0, lngTime;
            byte bytIn, bytCmd = 0, bytChannel = 0;
            //it also counts max ticks, returning that so sound length
            //can be calculated
            //length value gets calculated by counting total number of ticks
            //assumption is 60 ticks per second; nothing to indicate that is
            //not correct; all sounds //sound// right, so we go with it
            //builds a midi chunk based on apple IIgs sound format -
            //the raw midi data embedded in the sound seems to play OK
            //when using //high-end// players (WinAmp as an example) but not
            //Windows Media Player, or the midi API functions; even in
            //WinAmp, the total sound length (ticks and seconds) doesn't
            //calculate correctly, even though it plays
            //this seems to be due to the presence of the 0xFC commands
            //it looks like every sound resource has them; sometimes
            //one 0xFC ends the file; other times there are a series of
            //them that are followed by a set of 0xDx and 0xBx commands
            //that appear to reset all 16 channels
            //eliminating the 0xFC command and everything that follows
            //plays the sound correctly (I think)
            //a common 'null' file is one with just four 0xFC codes, and
            //nothing else

            //local copy of data -easier to manipulate
            midiIn = SoundIn.Data.AllData;
            //start with size of input data, assuming it all gets used,
            //plus space for headers and track end command
            //   need 22 bytes for header
            //   size of sound data, minus the two byte header
            //   need to also add 'end of track' event, which takes up 4 bytes
            midiOut = new byte[26 + midiIn.Length];
            // write header
            midiOut[0] = 77; //"M"
            midiOut[1] = 84; //"T"
            midiOut[2] = 104; //"h"
            midiOut[3] = 100; //"d"
                              //size of header, as a long
            midiOut[4] = 0;
            midiOut[5] = 0;
            midiOut[6] = 0;
            midiOut[7] = 6;
            //mode, as an integer
            midiOut[8] = 0;
            midiOut[9] = 1;
            //track count as integer
            midiOut[10] = 0;
            midiOut[11] = 1;
            //write pulses per quarter note as integer
            midiOut[12] = 0;
            midiOut[13] = 30;
            //track header
            midiOut[14] = 77;  //"M"
            midiOut[15] = 84;  //"T"
            midiOut[16] = 114;  //"r"
            midiOut[17] = 107;  //"k"
                                //size of track data (placeholder)
            midiOut[18] = 0;
            midiOut[19] = 0;
            midiOut[20] = 0;
            midiOut[21] = 0;
            //null sounds will start with 0xFC in first four bytes
            // (and nothing else), so ANY file starting with 0xFC
            // is considered empty
            if (midiIn[2] == 0xFC) {
                //assume no sound
                midiOut = new byte[26];
                midiOut[21] = 4;
                //add end of track data
                midiOut[22] = 0;
                midiOut[23] = 0xFF;
                midiOut[24] = 0x2F;
                midiOut[25] = 0;
                Array.Resize(ref midiOut, 26);
                Length = 0;
                return midiOut;
            }
            //starting output pos
            lngOutPos = 22;
            //move the data over, one byte at a time
            lngInPos = 2;
            do {
                //get next byte of input data
                bytIn = midiIn[lngInPos];
                //add it to output
                midiOut[lngOutPos] = bytIn;
                lngOutPos++;
                //time is always first input; it is supposed to be a delta value
                //but it appears that agi used it as an absolute value; so if time
                //is greater than 0x7F, it will cause a hiccup in modern midi
                //players
                lngTime = bytIn;
                if ((bytIn & 0x80) == 0x80) {
                    //treat 0xFC as an end mark, even if found in time position
                    //0xF8 also appears to cause an end
                    if ((bytIn == 0xFC) || (bytIn == 0xF8)) {
                        //backup one to cancel this byte
                        lngOutPos--;
                        break;
                    }
                    //convert into two-byte time value (means expanding
                    // array size by one byte)
                    Array.Resize(ref midiOut, midiOut.Length + 1);
                    midiOut[lngOutPos - 1] = 129;
                    midiOut[lngOutPos] = (byte)(bytIn & 0x7F);
                    lngOutPos++;
                    //ignore 'normal' delta time calculations
                    //////      lngTime = lngTime & 0x7F;
                    //////      do
                    //////      {
                    //////        lngTime = lngTime * 128;
                    //////        lngInPos++;
                    //////        //err check
                    //////        if (lngInPos >= midiIn.Length)
                    //////        {
                    //////          break;
                    //////        }
                    //////        bytIn = midiIn[lngInPos];
                    //////        //add it to output
                    //////        midiOut[lngOutPos] = bytIn;
                    //////        lngOutPos++;
                    //////        lngTime = lngTime + (bytIn & 0x7F);
                    //////      }
                    //////      while ((bytIn & 0x80) = 0x80);
                }
                lngInPos++;
                //err check
                if (lngInPos >= midiIn.Length) {
                    break;
                }
                bytIn = midiIn[lngInPos];
                //next byte is a controller (>=0x80) OR a running status (<0x80)
                if (bytIn >= 0x80) {
                    //it's a command
                    bytCmd = (byte)(bytIn / 16);
                    bytChannel = (byte)(bytIn & 0xF);
                    //commands:
                    //    0x8 = note off
                    //    0x9 = note on
                    //    0xA = polyphonic key pressure
                    //    0xB = control change (volume, pan, etc)
                    //    0xC = set patch (instrument)
                    //    0xD = channel pressure
                    //    0xE = pitch wheel change
                    //    0xF = system command
                    // all agi sounds appear to start with 0xC commands, then
                    // optionally 0xB commands, followed by 0x8/0x9s; VERY
                    // rarely 0xD command will show up
                    // 0xFC command seems to be the terminating code for agi
                    // sounds; so if encountered, immediately stop processing
                    // sometimes extra 0xD and 0xB commands follow a 0xFC,
                    // but they cause hiccups in modern midi programs
                    if (bytIn == 0xFC) {
                        //back up so last time value gets overwritten
                        lngOutPos--;
                        break;
                    }
                    //assume any other 0xF command is OK;
                    //add it to output
                    midiOut[lngOutPos] = bytIn;
                    lngOutPos++;
                }
                else {
                    //it's a running status -
                    //back up one so next byte is event data
                    lngInPos--;
                }
                //increment tick count
                lngTicks += lngTime;
                //next comes event data; number of data points depends on command
                switch (bytCmd) {
                case 8:
                case 9:
                case 0xA:
                case 0xB:
                case 0xE:
                    //these all take two bytes of data
                    //get next byte
                    lngInPos++;
                    //err check
                    if (lngInPos >= midiIn.Length) {
                        break;
                    }
                    bytIn = midiIn[lngInPos];
                    //add it to output
                    midiOut[lngOutPos] = bytIn;
                    lngOutPos++;
                    //get next byte
                    lngInPos++;
                    //err check
                    if (lngInPos >= midiIn.Length) {
                        break;
                    }
                    bytIn = midiIn[lngInPos];
                    //add it to output
                    midiOut[lngOutPos] = bytIn;
                    lngOutPos++;
                    break;
                case 0xC:
                case 0xD:
                    //only one byte for program change, channel pressure
                    //get next byte
                    lngInPos++;
                    //err check
                    if (lngInPos >= midiIn.Length) {
                        break;
                    }
                    bytIn = midiIn[lngInPos];
                    //add it to output
                    midiOut[lngOutPos] = bytIn;
                    lngOutPos++;
                    break;
                case 0xF: //system messages
                          //depends on submsg (channel value) - only expected value is 0xC
                    switch (bytChannel) {
                    case 0:
                        //variable; go until 0xF7 found
                        do {
                            //get next byte
                            lngInPos++;
                            //err check
                            if (lngInPos >= midiIn.Length) {
                                break;
                            }
                            bytIn = midiIn[lngInPos];
                            //add it to output
                            midiOut[lngOutPos] = bytIn;
                            lngOutPos++;
                        }
                        while (bytIn != 0xF7);
                        break;
                    case 1:
                    case 4:
                    case 5:
                    case 9:
                    case 0xD:
                        //all undefined- indicates an error
                        //back up so last time value gets overwritten
                        lngOutPos--;
                        break;
                    case 2: //song position
                            //this uses two bytes
                            //get next byte
                        lngInPos++;
                        //err check
                        if (lngInPos >= midiIn.Length) {
                            break;
                        }
                        bytIn = midiIn[lngInPos];
                        //add it to output
                        midiOut[lngOutPos] = bytIn;
                        lngOutPos++;
                        //get next byte
                        lngInPos++;
                        //err check
                        if (lngInPos >= midiIn.Length) {
                            break;
                        }
                        bytIn = midiIn[lngInPos];
                        //add it to output
                        midiOut[lngOutPos] = bytIn;
                        lngOutPos++;
                        break;
                    case 3: //song select
                            //this uses one byte
                            //get next byte
                        lngInPos++;
                        //err check
                        if (lngInPos >= midiIn.Length) {
                            break;
                        }
                        bytIn = midiIn[lngInPos];
                        //add it to output
                        midiOut[lngOutPos] = bytIn;
                        lngOutPos++;
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 0xA:
                    case 0xB:
                    case 0xE:
                    case 0xF:
                        //these all have no bytes of data
                        // but only 0xFC is expected; it gets
                        // checked above, though, so it doesn't
                        // get checked here
                        break;
                    }
                    break;
                }
                //move to next byte (which should be another time value)
                lngInPos++;
            }
            while (lngInPos < midiIn.Length); //Loop Until lngInPos >= midiIn.Length

            //resize output array to remove any extra potential bytes
            if (lngOutPos + 4 < midiOut.Length) {
                Array.Resize(ref midiOut, lngOutPos + 4);
            }
            //add end of track data
            midiOut[lngOutPos] = 0;
            midiOut[lngOutPos + 1] = 0xFF;
            midiOut[lngOutPos + 2] = 0x2F;
            midiOut[lngOutPos + 3] = 0;
            lngOutPos += 4;
            //update size of track data (total length - 22)
            midiOut[18] = (byte)(((lngOutPos - 22) >> 24));
            midiOut[19] = (byte)(((lngOutPos - 22) >> 16) & 0xFF);
            midiOut[20] = (byte)(((lngOutPos - 22) >> 8) & 0xFF);
            midiOut[21] = (byte)((lngOutPos - 22) & 0xFF);
            //(convert ticks seconds)
            Length = (double)lngTicks / 60;
            return midiOut;
        }

        internal static byte[] BuildIIgsPCM(Sound SoundIn) {
            int i, lngSize;
            byte[] bData;
            //builds a wav file stream from an apple IIgs PCM sound resource
            //Positions Sample Value  Description
            //0 - 3 = "RIFF"  Marks the file as a riff file.
            //4 - 7 = var File size (integer) Size of the overall file
            //8 -11 = "WAVE"  File Type Header. for (our purposes, it always equals "WAVE".
            //12-15 = "fmt "  Format chunk marker. Includes trailing space
            //16-19 = 16  Length of format data as listed above
            //20-21 = 1 Type of format (1 is PCM) - 2 byte integer
            //22-23 = 1 Number of Channels - 2 byte integer
            //24-27 = 8000 Sample Rate - 32 byte integer.
            //28-31 = 8000  (Sample Rate * BitsPerSample * Channels) / 8.
            //32-33 = 1 (BitsPerSample * Channels) / 8. 1 - 8 bit mono; 2 - 8 bit stereo/16 bit mono; 4 - 16 bit stereo
            //34-35 = 8  Bits per sample
            //36-39 "data"  "data" chunk header. Marks the beginning of the data section.
            //40-43 = var  Size of the data section.
            //44+    data
            //local copy of data -easier to manipulate
            bData = SoundIn.Data.AllData;
            //header is 54 bytes for pcm sounds, but its purpose is still mostly
            //unknown; the total size is at pos 8-9; the rest of the
            //header appears identical across resources, with exception of
            //position 2- it seems to vary from low thirties to upper 60s,
            //(maybe it's a volume thing?)
            //all resources appear to end with a byte value of 0; not sure
            //if it's necessary for wav files, but we keep it anyway
            //size of sound data is total file size, minus header 
            lngSize = bData.Length - 54;
            //expand midi data array to hold the sound resource data plus
            //the WAV file header
            SndPlayer.mMIDIData = new byte[44 + lngSize];
            SndPlayer.mMIDIData[0] = 82;
            SndPlayer.mMIDIData[1] = 73;
            SndPlayer.mMIDIData[2] = 70;
            SndPlayer.mMIDIData[3] = 70;
            SndPlayer.mMIDIData[4] = (byte)((lngSize + 36) & 0xFF);
            SndPlayer.mMIDIData[5] = (byte)(((lngSize + 36) >> 8) & 0xFF);
            SndPlayer.mMIDIData[6] = (byte)(((lngSize + 36) >> 16) & 0xFF);
            SndPlayer.mMIDIData[7] = (byte)((lngSize + 36) >> 24);
            SndPlayer.mMIDIData[8] = 87;
            SndPlayer.mMIDIData[9] = 65;
            SndPlayer.mMIDIData[10] = 86;
            SndPlayer.mMIDIData[11] = 69;
            SndPlayer.mMIDIData[12] = 102;
            SndPlayer.mMIDIData[13] = 109;
            SndPlayer.mMIDIData[14] = 116;
            SndPlayer.mMIDIData[15] = 32;
            SndPlayer.mMIDIData[16] = 16;
            SndPlayer.mMIDIData[17] = 0;
            SndPlayer.mMIDIData[18] = 0;
            SndPlayer.mMIDIData[19] = 0;
            SndPlayer.mMIDIData[20] = 1;
            SndPlayer.mMIDIData[21] = 0;
            SndPlayer.mMIDIData[22] = 1;
            SndPlayer.mMIDIData[23] = 0;
            SndPlayer.mMIDIData[24] = 64;
            SndPlayer.mMIDIData[25] = 31;
            SndPlayer.mMIDIData[26] = 0;
            SndPlayer.mMIDIData[27] = 0;
            SndPlayer.mMIDIData[28] = 64;
            SndPlayer.mMIDIData[29] = 31;
            SndPlayer.mMIDIData[30] = 0;
            SndPlayer.mMIDIData[31] = 0;
            SndPlayer.mMIDIData[32] = 1;
            SndPlayer.mMIDIData[33] = 0;
            SndPlayer.mMIDIData[34] = 8;
            SndPlayer.mMIDIData[35] = 0;
            SndPlayer.mMIDIData[36] = 100;
            SndPlayer.mMIDIData[37] = 97;
            SndPlayer.mMIDIData[38] = 116;
            SndPlayer.mMIDIData[39] = 97;
            SndPlayer.mMIDIData[40] = (byte)((lngSize - 2) & 0xFF);
            SndPlayer.mMIDIData[41] = (byte)(((lngSize - 2) >> 8) & 0xFF);
            SndPlayer.mMIDIData[42] = (byte)(((lngSize - 2) >> 16) & 0xFF);
            SndPlayer.mMIDIData[43] = (byte)((lngSize - 2) >> 24);
            lngPos = 44;
            //copy data from sound resource, beginning at pos 2
            for (i = 54; i < bData.Length; i++) {
                //copy this one over
                SndPlayer.mMIDIData[lngPos] = bData[i];
                lngPos++;
            }
            return SndPlayer.mMIDIData;
        }
        internal static void WriteSndDelta(int LongIn) {
            //writes variable delta times
            int i;
            i = LongIn << 21; //LngSHR(LongIn, 21)
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn << 14; //LngSHR(LongIn, 14)
            if (i > 0) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn << 7; //LngSHR(LongIn, 7)
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            WriteSndByte((byte)(LongIn & 127));
        }
        internal static void WriteSndWord(int IntegerIn) {
            WriteSndByte((byte)(IntegerIn / 256));
            WriteSndByte((byte)(IntegerIn & 0xFF));
        }
        internal static void WriteSndLong(int LongIn) {
            WriteSndByte((byte)(LongIn >> 24));
            WriteSndByte((byte)((LongIn >> 16) & 0xFF));
            WriteSndByte((byte)((LongIn >> 8) & 0xFF));
            WriteSndByte((byte)(LongIn & 0xFF));
        }
        internal static int GetTrack3Freq(Track Track3, int lngTarget) {
            //if noise channel needs the frequency of track 3,
            //must step through track three until the same point in time is found
            //then use that frequency for noise channel
            int lngTickCount = 0;
            //step through notes in this track (5 bytes at a time)
            for (int i = 0; i < Track3.Notes.Count; i++) {
                //add duration
                lngTickCount += Track3.Notes[i].Duration;
                //if equal to or past current tick Count
                if (lngTickCount >= lngTarget) {
                    //this is the frequency we want
                    return Track3.Notes[i].FreqDivisor;
                }
            }
            //if nothing found, return 0
            return 0;
        }
        internal static void WriteSndByte(byte ByteIn) {
            SndPlayer.mMIDIData[lngPos] = ByteIn;
            lngPos++; ;
            //if at end
            if (lngPos >= SndPlayer.mMIDIData.Length) {
                //jack it up
                Array.Resize(ref SndPlayer.mMIDIData, lngPos + 256);
            }
        }
    }
}
