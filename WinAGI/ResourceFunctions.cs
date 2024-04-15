using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.MIDIPlayer;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using System.Diagnostics;

namespace WinAGI.Engine {
    public static partial class Base {
        // constants used in extracting compressed resources
        const int TABLE_SIZE = 18041;
        const int START_BITS = 9;

        // variables used in extracting compressed resources
        private static int lngMaxCode;
        private static uint[] intPrefix;
        private static byte[] bytAppend;
        private static int lngBitsInBuffer;
        private static uint lngBitBuffer;
        private static int lngOriginalSize;


        /// <summary>
        /// Gets the resources from VOL files, and adds them to the game.
        /// </summary>
        /// <param name="game"></param>
        /// <returns> false if resources loaded with no warnings or errors<br />
        /// true if one or more recoverable errors occur during load<br />
        /// throws an exception if critical error enountered that prevents game from loading</returns>
        internal static bool ExtractResources(AGIGame game) {
            byte bytResNum;
            AGIResType bytResType;
            string strDirFile = "";
            byte[] bytBuffer = [];
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

            if (game.agIsVersion3) {
                strDirFile = game.agGameDir + game.agGameID + "DIR";
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
                if (bytBuffer.Length < 11) {
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
                    // no offset for version 2
                    lngDirOffset = 0;
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
                // max size of useable directory is 768 (256*3)
                if (lngDirSize > 768) {
                    // warning- file might be invalid
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
                    // assume the max for now
                    intResCount = 256;
                }
                else {
                    intResCount = lngDirSize / 3;
                }
                if (intResCount > 0) {
                    // check for bad resources
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
                        // get location data for this resource
                        byte1 = bytBuffer[lngDirOffset + bytResNum * 3];
                        byte2 = bytBuffer[lngDirOffset + bytResNum * 3 + 1];
                        byte3 = bytBuffer[lngDirOffset + bytResNum * 3 + 2];
                        // ignore any 0xFFFFFF sequences,
                        if (byte1 != 0xff) {
                            bytVol = (sbyte)(byte1 >> 4);
                            lngLoc = ((byte1 & 0xF) << 16) + (byte2 << 8) + byte3;
                            switch (bytResType) {
                            case AGIResType.rtLogic:
                                game.agLogs.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agLogs[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(AGIResType.rtLogic, bytResNum, game.agLogs[bytResNum].ErrLevel, game.agLogs[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agLogs.Exists(bytResNum)) {
                                    game.agLogs[bytResNum].PropDirty = false;
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
                                    game.agPics[bytResNum].PropDirty = false;
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
                                    game.agSnds[bytResNum].PropDirty = false;
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
                                // make sure it was added before finishing
                                if (game.agViews.Exists(bytResNum)) {
                                    game.agViews[bytResNum].PropDirty = false;
                                    game.agViews[bytResNum].IsDirty = false;
                                    game.agViews[bytResNum].Unload();
                                }
                                break;
                            }
                        }
                    }
                }
            }
            // return any warning codes
            return loadWarnings;
        }

        /// <summary>
        /// Sends warnings encountered during game load to the main app window as events. 
        /// </summary>
        /// <param name="resType"></param>
        /// <param name="resNum"></param>
        /// <param name="errlevel"></param>
        /// <param name="errdata"></param>
        internal static void AddLoadWarning(AGIResType resType, byte resNum, int errlevel, string[] errdata) {
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

        /// <summary>
        /// Expands a version 3 picture resource using Sierra's custom run length encoding.
        /// </summary>
        /// <param name="bytOriginalData"></param>
        /// <param name="fullsize"></param>
        /// <returns></returns>
        internal static byte[] DecompressPicture(byte[] bytOriginalData, int fullsize) {
            short intPosIn = 0;
            byte bytCurComp, bytBuffer = 0, bytCurUncomp;
            bool blnOffset = false;
            int lngTempCurPos = 0;
            byte[] bytExpandedData = new byte[fullsize];

            // decompress the picture
            do {
                bytCurComp = bytOriginalData[intPosIn++];
                if (blnOffset) {
                    bytBuffer += (byte)(bytCurComp >> 4);
                    //extract uncompressed byte
                    bytCurUncomp = bytBuffer;
                    //shift buffer back
                    bytBuffer = (byte)(bytCurComp << 4);
                }
                else {
                    // byte is not compressed
                    bytCurUncomp = bytCurComp;
                }
                bytExpandedData[lngTempCurPos++] = bytCurUncomp;
                // check if byte sets or restores offset
                if (((bytCurUncomp == 0xF0) || (bytCurUncomp == 0xF2)) && (intPosIn < bytOriginalData.Length)) {
                    if (blnOffset) {
                        //write rest of buffer byte
                        bytExpandedData[lngTempCurPos++] = (byte)(bytBuffer >> 4);
                        blnOffset = false;
                    }
                    else {
                        bytCurComp = bytOriginalData[intPosIn++];
                        bytExpandedData[lngTempCurPos++] = (byte)(bytCurComp >> 4);
                        //f ill buffer
                        bytBuffer = (byte)(bytCurComp << 4);
                        blnOffset = true;
                    }
                }
            }
            // continue until all original data has been read
            while (intPosIn < bytOriginalData.Length);
            return bytExpandedData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytOriginalData"></param>
        /// <param name="fullsize"></param>
        /// <returns></returns>
        internal static byte[] ExpandV3ResData(byte[] bytOriginalData, int fullsize) {
            int intPosIn, intPosOut, intNextCode, i;
            uint intOldCode, intNewCode;
            string strDat;
            char strChar;
            int intCodeSize;
            byte[] bytTempData;
            intPrefix = new uint[TABLE_SIZE];
            bytAppend = new byte[TABLE_SIZE];
            // set temporary data field
            bytTempData = new byte[fullsize];
            // original size is determined by array bounds
            lngOriginalSize = bytOriginalData.Length;
            intPosIn = 0;
            intPosOut = 0;
            lngBitBuffer = 0;
            lngBitsInBuffer = 0;

            // initialize
            intCodeSize = NewCodeSize(START_BITS);
            intNextCode = 257;
            strChar = (char)0;

            // read in the first code.
            intOldCode = ReadCode(ref bytOriginalData, intCodeSize, ref intPosIn);
            // first code for SIERRA resouces should always be 256
            if (intOldCode != 256) {
                intPrefix = [];
                bytAppend = [];
                WinAGIException wex = new(LoadResString(559).Replace(ARG1, "(invalid compression data)")) {
                    HResult = WINAGI_ERR + 559,
                };
                throw wex;
            }
            // now begin decompressing actual data
            intNewCode = ReadCode(ref bytOriginalData, intCodeSize, ref intPosIn);
            // continue extracting data, until all bytes are read (or end code is reached)
            while ((intPosIn <= lngOriginalSize) && (intNewCode != 0x101)) {
                // check for reset
                if (intNewCode == 0x100) {
                    // Restart LZW process
                    intNextCode = 258;
                    intCodeSize = NewCodeSize(START_BITS);
                    // read in the first code
                    intOldCode = ReadCode(ref bytOriginalData, intCodeSize, ref intPosIn);
                    // the character Value is same as code for beginning
                    strChar = (char)intOldCode;
                    //write out the first character
                    bytTempData[intPosOut++] = (byte)intOldCode;
                    //now get next code
                    intNewCode = ReadCode(ref bytOriginalData, intCodeSize, ref intPosIn);
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
                        // append the character code
                        strDat += strChar;
                    }
                    else {
                        // decode the string using new code
                        strDat = DecodeString(intNewCode);
                    }
                    // retreive the character Value
                    strChar = strDat[0];
                    // now send out decoded data (it's backwards in the string, so
                    // start at end and work back to beginning)
                    for (i = 0; i < strDat.Length; i++) {
                        bytTempData[intPosOut++] = (byte)strDat[i];
                    }
                    // if no more room in the current bit-code table,
                    if (intNextCode > lngMaxCode) {
                        // get new code size (in number of bits per code)
                        intCodeSize = NewCodeSize(intCodeSize + 1);
                    }
                    // store code in prefix table
                    intPrefix[intNextCode - 257] = intOldCode;
                    // store append character in table
                    bytAppend[intNextCode - 257] = (byte)strChar;
                    // increment next code pointer
                    intNextCode++;
                    intOldCode = intNewCode;
                    // get the next code
                    intNewCode = ReadCode(ref bytOriginalData, intCodeSize, ref intPosIn);
                }
            }
            // free lzw arrays
            intPrefix = [];
            bytAppend = [];
            return bytTempData;
        }

        /// <summary>
        /// Checks all resource IDs looking for duplicates.
        /// </summary>
        /// <param name="agRes"></param>
        /// <returns>true if another resource in this game has same ID as agRes</returns>
        internal static bool NotUniqueID(AGIResource agRes) {
            string checkID = agRes.ID;
            foreach (Logic tmpRes in agRes.parent.agLogs) {
                if (agRes != tmpRes && tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (Picture tmpRes in agRes.parent.agPics) {
                if (agRes != tmpRes && tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (Sound tmpRes in agRes.parent.agSnds) {
                if (agRes != tmpRes && tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (View tmpRes in agRes.parent.agViews) {
                if (agRes != tmpRes && tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            // not a duplicate
            return false;
        }

        /// <summary>
        /// Checks all resource IDs to see if ceckID is already in use. 
        /// </summary>
        /// <param name="checkID"></param>
        /// <returns>true if any resource in the game equals checkID<br />
        /// false if checkID is unique</returns>
        internal static bool NotUniqueID(string checkID, AGIGame game) {
            foreach (Logic tmpRes in game.agLogs) {
                if (tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (Picture tmpRes in game.agPics) {
                if (tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (Sound tmpRes in game.agSnds) {
                if (tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            foreach (View tmpRes in game.agViews) {
                if (tmpRes.ID == checkID) {
                    // duplicate
                    return true;
                }
            }
            // not a duplicate
            return false;
        }

        /// <summary>
        /// This method compresses cel data into run-length-encoded data that
        /// can be written to an AGI View resource.
        /// blnMirror is used to ensure mirrored cels include enough room
        /// for the flipped cel.
        /// </summary>
        /// <param name="Cel"></param>
        /// <param name="blnMirror"></param>
        /// <returns></returns>
        internal static byte[] CompressedCel(Cel Cel, bool blnMirror) {
            byte[] bytTempRLE;
            byte mHeight, mWidth, bytChunkLen;
            byte[,] mCelData;
            AGIColorIndex mTransColor;
            int lngByteCount = 0;
            byte bytChunkColor, bytNextColor;
            bool blnFirstChunk;
            int lngMirrorCount = 0;
            int i, j;

            mHeight = Cel.Height;
            mWidth = Cel.Width;
            mTransColor = Cel.TransColor;
            mCelData = Cel.AllCelData;
            // assume one byte per pixel to start with
            // (include one byte for ending zero)
            bytTempRLE = new byte[mHeight * mWidth + 1];
            for (j = 0; j < mHeight; j++) {
                // first pixel
                bytChunkColor = (byte)mCelData[0, j];
                bytChunkLen = 1;
                blnFirstChunk = true;
                //step through rest of pixels in this row
                for (i = 1; i < mWidth; i++) {
                    bytNextColor = (byte)mCelData[i, j];
                    if ((bytNextColor != bytChunkColor) || (bytChunkLen == 0xF)) {
                        bytTempRLE[lngByteCount++] = (byte)((int)bytChunkColor * 0x10 + bytChunkLen);
                        // if NOT first chunk or NOT transparent
                        if (!blnFirstChunk || (bytChunkColor != (byte)mTransColor)) {
                            // increment lngMirorCount for any chunks
                            // after the first, and also for the first
                            // if it is NOT transparent color
                            lngMirrorCount++;
                        }
                        blnFirstChunk = false;
                        bytChunkColor = bytNextColor;
                        bytChunkLen = 1;
                    }
                    else {
                        bytChunkLen++;
                    }
                }
                // if last chunk is NOT transparent
                if (bytChunkColor != (byte)mTransColor) {
                    bytTempRLE[lngByteCount++] = (byte)(bytChunkColor * 0x10 + bytChunkLen);
                }
                // always Count last chunk for mirror
                lngMirrorCount++;
                //add zero to indicate end of row
                bytTempRLE[lngByteCount++] = 0;
                lngMirrorCount++;
            }
            if (blnMirror) {
                //add zeros to make room for mirror loop
                while (lngByteCount < lngMirrorCount) {
                    bytTempRLE[lngByteCount] = 0;
                    lngByteCount++;
                }
            }
            Array.Resize(ref bytTempRLE, lngByteCount);
            return bytTempRLE;
        }

        /// <summary>
        /// LZW Decompression: A method that converts a code value into its original string value.
        /// </summary>
        /// <param name="intCode"></param>
        /// <returns></returns>
        internal static string DecodeString(uint intCode) {
            string retval = "";

            while (intCode > 255) {
                if (intCode > TABLE_SIZE) {
                    return retval;
                }
                else {
                    retval = (char)bytAppend[intCode - 257] + retval;
                    intCode = intPrefix[intCode - 257];
                }
            }
            retval = (char)intCode + retval;
            return retval;
        }

        /// <summary>
        /// LZW Decompression: This method extracts the next code Value off the input stream.
        /// Since the number of bits per code can vary between 9 and 12, we can't read in
        /// directly from the stream.
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="intCodeSize"></param>
        /// <param name="intPosIn"></param>
        /// <returns></returns>
        internal static uint ReadCode(ref byte[] bytData, int intCodeSize, ref int intPosIn) {
            uint lngWord, lngRet;
            // Unlike normal LZW, the bytes are actually written so the code boundaries work
            // from right to left, NOT left to right. For example, if the input stream needs
            // to be split on a 9 bit boundary it will use eight bits of first byte, plus LOWEST
            // bit of byte 2. The second code is then the upper seven bits of byte 2 and the
            // lower 2 bits of byte 3 etc:
            //                           byte boundaries (8 bits per byte)
            //           byte4           byte3           byte2           byte1           byte0
            //  ...|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
            //  ... x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x
            //  ... 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
            //                    code3             code2             code1             code0
            //                           code boundaries (9 bits per code)
            //
            // The data stream is read into a bit buffer 8 bits at a time (i.e. a single byte).
            // Once the buffer is full of data, the input code is pulled out, and the buffer
            // is shifted.
            // The input data from the stream must be shifted to ensure it lines up with data
            // currently in the buffer.

            // Read bytes into the buffer until has 24 or mor bits. This ensures that the
            // eight bits read from the input stream will fit in the buffer (which is a long
            // integer [4 bytes==32 bits]). Also stop reading data if end of data stream is
            // reached.
            while ((lngBitsInBuffer <= 24) && (intPosIn < lngOriginalSize)) {
                lngWord = bytData[intPosIn++];
                // shift the data to the left by enough bits so the byte being added will not
                //overwrite the bits currently in the buffer, and add the bits to the buffer
                lngBitBuffer |= (lngWord << lngBitsInBuffer);
                //increment count of how many bits are currently in the buffer
                lngBitsInBuffer += 8;
            }
            // The input code starts at the lowest bit in the buffer. Since the buffer has
            // 32 bits total, we need to clear out all bits above the desired number of bits
            // to define the code (i.e. if 9 bits, AND with 0x1FF; 10 bits,AND with 0x3FF,
            // etc.)
            lngRet = (uint)(lngBitBuffer & ((1 << intCodeSize) - 1));
            // then shift the buffer to the right by the number of bits per code
            lngBitBuffer >>= intCodeSize;
            lngBitsInBuffer -= intCodeSize;
            // return code Value
            return lngRet;
        }

        /// <summary>
        /// LZW Decompression: This method supports the expansion of compressed resources.
        /// It sets the size of codes which the LZW routine uses. The code size starts at
        /// 9 bits, then increases as all code tables are filled. The max code size is 11;
        /// if an attempt is made to set a code size above that, the function does nothing.
        /// <br />
        /// The function also recalculates the maximum number of codes available to the
        /// expand subroutine. This number is used to determine when to call this function
        /// again.
        /// </summary>
        /// <param name="intVal"></param>
        /// <returns></returns>
        internal static int NewCodeSize(int intVal) {
            const int MAXBITS = 11;
            int retval;

            if (intVal >= MAXBITS) {
                retval = 11;
            }
            else {
                retval = intVal;
                lngMaxCode = (1 << retval) - 2;
            }
            return retval;
        }

        /// <summary>
        /// This method converts a standard AGI sound resource into a MIDI stream.
        /// </summary>
        /// <param name="SoundIn"></param>
        /// <returns></returns>
        internal static byte[] BuildMIDI(Sound SoundIn) {
            int lngWriteTrack = 0;
            int i, j;
            byte bytNote;
            int intFreq;
            byte bytVol;
            int lngTrackCount = 0, lngTickCount;
            int lngStart, lngEnd;

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
                // add two tracks if noise is not muted
                // because the white noise and periodic noise
                // are written as two separate tracks
                lngTrackCount += 2;
            }
            // set intial size of midi data array
            midiPlayer.mMIDIData = new byte[70 + 256];

            // header
            midiPlayer.mMIDIData[0] = 77;  // "M"
            midiPlayer.mMIDIData[1] = 84;  // "T"
            midiPlayer.mMIDIData[2] = 104; // "h"
            midiPlayer.mMIDIData[3] = 100; // "d"
            lngPos = 4;
            // remaining length of header
            WriteSndLong(6);
            // track mode
            //   mode 0 = single track
            //   mode 1 = multiple tracks, all start at zero
            //   mode 2 = multiple tracks, independent start times
            WriteSndWord(1);
            if (lngTrackCount == 0) {
                // create an empty MIDI stream
                Array.Resize(array: ref midiPlayer.mMIDIData, 38);
                // one track, with one note of smallest length, and no sound
                WriteSndWord(1);
                // pulses per quarter note
                WriteSndWord(30);
                WriteSndByte(77);  // "M"
                WriteSndByte(84);  // "T"
                WriteSndByte(114); // "r"
                WriteSndByte(107); // "k"
                // track length
                WriteSndLong(15);
                // track number
                WriteSndDelta(0);
                //  set instrument status byte
                WriteSndByte(0xC0);
                // instrument number
                WriteSndByte(0);
                // add a slight delay note with no volume to end
                WriteSndDelta(0);
                WriteSndByte(0x90);
                WriteSndByte(60);
                WriteSndByte(0);
                WriteSndDelta(16);
                WriteSndByte(0x80);
                WriteSndByte(60);
                WriteSndByte(0);
                // add end of track info
                WriteSndDelta(0);
                WriteSndByte(0xFF);
                WriteSndByte(0x2F);
                WriteSndByte(0x0);
                return midiPlayer.mMIDIData;
            }
            // track count
            WriteSndWord(lngTrackCount);
            // pulses per quarter note
            // (agi sound tick is 1/60 sec; each tick of an AGI note
            // is 1/60 of a second; by default, MIDI defines a whole
            // note as 2 seconds; therefore, a quarter note is 1/2
            // second, or 30 ticks
            WriteSndWord(30);
            // sound tracks
            for (i = 0; i < 3; i++) {
                if (!SoundIn[i].Muted && SoundIn[i].Notes.Count > 0) {
                    WriteSndByte(77);  // "M"
                    WriteSndByte(84);  // "T"
                    WriteSndByte(114); //"r"
                    WriteSndByte(107); //"k"
                    // starting position for this track's data
                    lngStart = lngPos;
                    // place holder for data size
                    WriteSndLong(0);
                    // track number
                    //     *********** i think this should be zero for all tracks
                    //     it's the delta sound value for the instrument setting
                    WriteSndDelta(0);
                    // instrument status byte
                    WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    // instrument number
                    WriteSndByte(SoundIn[i].Instrument);
                    // add a slight delay note with no volume to start
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4);
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    // add notes from this track
                    for (j = 0; j <= SoundIn[i].Notes.Count - 1; j++) {
                        // calculate note to play - convert FreqDivisor to frequency
                        //     f = 111860 / FreqDiv
                        // then convert that into appropriate MIDI note; MIDI notes
                        // go from 1 to 127, with 60 being middle C (261.6 Hz); every
                        // twelve notes, the frequency doubles, this gives the following
                        // forumula to convert frequency to midinote:
                        //     midinote = log10(f)/log10(2^(1/12)) - X
                        // sinc emiddle C is 261.6 Hz, this means X = 36.375
                        // however, this offset results in crappy sounding music, likely due
                        // to the way the sounds were originally written (as tones on the PCjr
                        // sound chip which could produce much higher resolution than twelve
                        // notes per doubling of frequency) combined with the way that VB math
                        // functions handle rounding/truncation when converting between
                        // long/byte/double - empirically, 36.5 seems to work best (which is
                        // what has been used by most AGI sound tools since the format was
                        // originally decyphered by fans)
                        if (SoundIn[i].Notes[j].FreqDivisor > 0) {
                            bytNote = (byte)((Math.Log10(111860 / (double)(SoundIn[i].Notes[j].FreqDivisor)) / LOG10_1_12) - 36.5);
                            // in case note is too high,
                            if (bytNote > 127) {
                                bytNote = 127;
                            }
                            bytVol = (byte)(127 * (15 - SoundIn[i].Notes[j].Attenuation) / 15);
                        }
                        else {
                            bytNote = 0;
                            bytVol = 0;
                        }
                        // NOTE ON
                        WriteSndDelta(0);
                        WriteSndByte((byte)(0x90 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(bytVol);
                        // NOTE OFF
                        WriteSndDelta(SoundIn[i].Notes[j].Duration);
                        WriteSndByte((byte)(0x80 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end of track
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4);
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    // add end of track info
                    WriteSndDelta(0);
                    WriteSndByte(0xFF);
                    WriteSndByte(0x2F);
                    WriteSndByte(0x0);
                    //save track end position
                    lngEnd = lngPos;
                    // set cursor to start of track
                    lngPos = lngStart;
                    // add the track length
                    WriteSndLong((lngEnd - lngStart) - 4);
                    lngPos = lngEnd;
                }
                lngWriteTrack++;
            }
            // seashore does a good job of imitating the white noise, with frequency
            // adjusted empirically
            // harpsichord does a good job of imitating the tone noise, with frequency
            // adjusted empirically
            if (!SoundIn[3].Muted && SoundIn[3].Notes.Count > 0) {
                // because there are two types of noise, use a different channel for each type
                // 0 means add tone, 1 means add white noise
                for (i = 0; i < 2; i++) {
                    // add track header
                    WriteSndByte(77);  // "M"
                    WriteSndByte(84);  // "T"
                    WriteSndByte(114); // "r"
                    WriteSndByte(107); // "k"
                    // store starting position for this track's data
                    lngStart = lngPos;
                    // place holder for chunklength
                    WriteSndLong(0);
                    // track number
                    WriteSndDelta(lngWriteTrack);
                    // instrument
                    WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    // instrument number
                    switch (i) {
                    case 0:
                        // tone - use harpsichord
                        WriteSndByte(6);
                        break;
                    case 1:
                        // white noise - use seashore
                        WriteSndByte(122);
                        //crank up the volume
                        WriteSndByte(0);
                        WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        WriteSndByte(7);
                        WriteSndByte(127);
                        // set legato
                        WriteSndByte(0);
                        WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        WriteSndByte(68);
                        WriteSndByte(127);
                        break;
                    }
                    // add a slight delay note with no volume to start the track
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4);
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    // reset tick counter (used in case of need to borrow track 3 freq)
                    lngTickCount = 0;
                    for (j = 0; j < SoundIn[3].Notes.Count; j++) {
                        // add duration to tickcount
                        lngTickCount += SoundIn[3].Notes[j].Duration;
                        // Fourth byte: noise freq and type
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
                        // AGINote contains bits 2-1-0 only
                        //
                        // if this note matches desired type (tone or white noise)
                        if ((SoundIn[3].Notes[j].FreqDivisor & 4) == 4 * i) {
                            if ((SoundIn[3].Notes[j].FreqDivisor & 3) == 3) {
                                // get frequency from channel 3
                                intFreq = GetTrack3Freq(SoundIn[2], lngTickCount);
                            }
                            else {
                                // get frequency from bits 0 and 1
                                intFreq = (int)(2330.4296875 / (1 << (SoundIn[3].Notes[j].FreqDivisor & 3)));
                            }
                            if ((SoundIn[3].Notes[j].FreqDivisor & 4) == 4) {
                                // for white noise, 96 is my best guess to imitate noise
                                // BUT... 96 causes some notes to come out negative;
                                // 80 is max Value that ensures all AGI freq values convert
                                // to positive MIDI note values
                                bytNote = (byte)((Math.Log10(intFreq) / LOG10_1_12) - 80);
                            }
                            else {
                                // for periodic noise, 64 is my best guess to imitate noise
                                bytNote = (byte)((Math.Log10(intFreq) / LOG10_1_12) - 64);
                            }
                            bytVol = (byte)(127 * (15 - SoundIn[3].Notes[j].Attenuation) / 15);
                        }
                        else {
                            // write a blank note as a placeholder
                            bytNote = 0;
                            bytVol = 0;
                        }

                        // NOTE ON
                        WriteSndDelta(0);
                        WriteSndByte((byte)(0x90 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(bytVol);
                        // NOTE OFF
                        WriteSndDelta(SoundIn[3].Notes[j].Duration);
                        WriteSndByte((byte)(0x80 + lngWriteTrack));
                        WriteSndByte(bytNote);
                        WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end
                    WriteSndDelta(0);
                    WriteSndByte((byte)(0x90 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    WriteSndDelta(4);
                    WriteSndByte((byte)(0x80 + lngWriteTrack));
                    WriteSndByte(60);
                    WriteSndByte(0);
                    // add end of track data
                    WriteSndDelta(0);
                    WriteSndByte(0xFF);
                    WriteSndByte(0x2F);
                    WriteSndByte(0x0);
                    // save ending position
                    lngEnd = lngPos;
                    // go back to start of track, and add track length
                    lngPos = lngStart;
                    WriteSndLong((lngEnd - lngStart) - 4);
                    lngPos = lngEnd;
                    lngWriteTrack++;
                }
            }
            // remove any extra padding from the data array
            Array.Resize(ref midiPlayer.mMIDIData, lngPos);
            return midiPlayer.mMIDIData;
        }

        /// <summary>
        /// This method formats a native Apple IIgs MIDI sound for playback on
        /// modern MIDI devices. It also calculates the sound length, returning it in the
        /// Length parameter.
        /// </summary>
        /// <param name="SoundIn"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        internal static byte[] BuildIIgsMIDI(Sound SoundIn, ref double Length) {
            // Length value gets calculated by counting total number of ticks.
            // Assumption is 60 ticks per second; nothing to indicate that is
            // not correct. All sounds 'sound' right, so we go with it.

            // The raw midi data embedded in the sound seems to play OK when
            // using 'high-end' players (WinAmp as an example) but not in
            // Windows Media Player, or the Windows midi API functions. ; even
            // in WinAmp, the total sound length (ticks and seconds) doesn't
            // calculate correctly, even though it plays. This seems to be due
            // to the presence of the 0xFC commands. It looks like every IIgs
            // sound resource has them; sometimes one 0xFC ends the file, other
            // times there are a series of them that are followed by a set of
            // 0xDx and 0xBx commands that appear to reset all 16 channels.
            // Eliminating the 0xFC command and everything that follows plays
            // the sound correctly (I think). A common 'null' file in IIgs games
            // is one with just four 0xFC codes, and nothing else.
            int lngInPos, lngOutPos;
            byte[] midiIn, midiOut;
            int lngTicks = 0, lngTime;
            byte bytIn, bytCmd = 0, bytChannel = 0;
            //local copy of data -easier to manipulate
            midiIn = SoundIn.Data.AllData;

            // start with size of input data, assuming it all gets used, plus
            // space for headers and track end command; need 22 bytes for header;
            //  need to also add 'end of track' event, which takes up 4 bytes:
            midiOut = new byte[26 + midiIn.Length];
            // add header
            midiOut[0] = 77;  // "M"
            midiOut[1] = 84;  // "T"
            midiOut[2] = 104; // "h"
            midiOut[3] = 100; // "d"
            // size of header (4 bytes = long)
            midiOut[4] = 0;
            midiOut[5] = 0;
            midiOut[6] = 0;
            midiOut[7] = 6;
            // mode, as an integer
            midiOut[8] = 0;
            midiOut[9] = 1;
            // track count as integer
            midiOut[10] = 0;
            midiOut[11] = 1;
            // pulses per quarter note as integer
            midiOut[12] = 0;
            midiOut[13] = 30;
            // track header
            midiOut[14] = 77;   // "M"
            midiOut[15] = 84;   // "T"
            midiOut[16] = 114;  // "r"
            midiOut[17] = 107;  // "k"
            // size of track data (placeholder)
            midiOut[18] = 0;
            midiOut[19] = 0;
            midiOut[20] = 0;
            midiOut[21] = 0;
            // null sounds will start with 0xFC in first four bytes
            // (and nothing else), so ANY file starting with 0xFC
            // is considered empty
            if (midiIn[2] == 0xFC) {
                // assume no sound
                Array.Resize(ref midiIn, 26);
                midiOut[21] = 4;
                // add end of track data
                midiOut[22] = 0;
                midiOut[23] = 0xFF;
                midiOut[24] = 0x2F;
                midiOut[25] = 0;
                Length = 0;
                return midiOut;
            }
            lngOutPos = 22;
            lngInPos = 2;
            do {
                // get next byte of input data
                bytIn = midiIn[lngInPos++];
                if (lngInPos >= midiIn.Length) {
                    break;
                }
                //add it to output
                midiOut[lngOutPos++] = bytIn;
                // time is always first input; it is supposed to be a delta value
                // but it appears that agi used it as an absolute value; so if time
                // is greater than 0x7F, it will cause a hiccup in modern midi
                // players
                lngTime = bytIn;
                if ((bytIn & 0x80) == 0x80) {
                    // treat 0xFC as an end mark, even if found in time position
                    // 0xF8 also appears to cause an end
                    if ((bytIn == 0xFC) || (bytIn == 0xF8)) {
                        // backup one to cancel this byte
                        lngOutPos--;
                        break;
                    }
                    // convert into two-byte time value (means expanding
                    // array size by one byte)
                    Array.Resize(ref midiOut, midiOut.Length + 1);
                    midiOut[lngOutPos - 1] = 129;
                    midiOut[lngOutPos++] = (byte)(bytIn & 0x7F);
                }
                // next byte is a command (>=0x80) OR a running status (<0x80)
                bytIn = midiIn[lngInPos++];
                if (lngInPos >= midiIn.Length) {
                    break;
                }
                if (bytIn >= 0x80) {
                    bytCmd = (byte)(bytIn / 16);
                    bytChannel = (byte)(bytIn & 0xF);
                    // commands:
                    //     0x8 = note off
                    //     0x9 = note on
                    //     0xA = polyphonic key pressure
                    //     0xB = control change (volume, pan, etc)
                    //     0xC = set patch (instrument)
                    //     0xD = channel pressure
                    //     0xE = pitch wheel change
                    //     0xF = system command
                    // all IIgs AGI sounds appear to start with 0xC commands, then
                    // optionally 0xB commands, followed by 0x8/0x9s; VERY
                    // rarely 0xD command will show up
                    // 0xFC command seems to be the terminating code for IIgs AGI
                    // sounds; so if encountered, immediately stop processing
                    // sometimes extra 0xD and 0xB commands follow a 0xFC,
                    // but they cause hiccups in modern midi programs
                    if (bytIn == 0xFC) {
                        // back up so last time value gets overwritten
                        lngOutPos--;
                        break;
                    }
                    // assume any other 0xF command is OK;
                    // add it to output
                    midiOut[lngOutPos++] = bytIn;
                }
                // increment tick count
                lngTicks += lngTime;
                // next comes event data; number of data points depends on command
                switch (bytCmd) {
                case 8:
                case 9:
                case 0xA:
                case 0xB:
                case 0xE:
                    // these all take two bytes of data
                    midiOut[lngOutPos++] = midiIn[lngInPos++];
                    if (lngInPos >= midiIn.Length) {
                        continue;
                    }
                    midiOut[lngOutPos++] = midiIn[lngInPos++];
                    if (lngInPos >= midiIn.Length) {
                        continue;
                    }
                    break;
                case 0xC:
                case 0xD:
                    // only one byte for program change, channel pressure
                    midiOut[lngOutPos++] = midiIn[lngInPos++];
                    if (lngInPos >= midiIn.Length) {
                        continue;
                    }
                    break;
                case 0xF:
                    // system messages
                    // depends on submsg (channel value) - only expected value is 0xC
                    switch (bytChannel) {
                    case 0:
                        // variable; go until 0xF7 found
                        do {
                            midiOut[lngOutPos++] = midiIn[lngInPos++];
                            if (lngInPos >= midiIn.Length) {
                                break;
                            }
                        }
                        while (bytIn != 0xF7);
                        if (lngInPos >= midiIn.Length) {
                            continue;
                        }
                        break;
                    case 1:
                    case 4:
                    case 5:
                    case 9:
                    case 0xD:
                        // all undefined- indicates an error
                        // back up so last time value gets overwritten
                        lngOutPos--;
                        continue;
                    case 2:
                        // song position
                        // this uses two bytes
                        midiOut[lngOutPos++] = midiIn[lngInPos++];
                        if (lngInPos >= midiIn.Length) {
                            continue;
                        }
                        midiOut[lngOutPos++] = midiIn[lngInPos++];
                        if (lngInPos >= midiIn.Length) {
                            continue;
                        }
                        break;
                    case 3:
                        // song select
                        // this uses one byte
                        midiOut[lngOutPos++] = midiIn[lngInPos++];
                        if (lngInPos >= midiIn.Length) {
                            continue;
                        }
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 0xA:
                    case 0xB:
                    case 0xE:
                    case 0xF:
                        // these all have no bytes of data
                        // but only 0xFC is expected; it gets
                        // checked above, though, so it doesn't
                        // get checked here
                        break;
                    }
                    break;
                }
            }
            while (lngInPos < midiIn.Length);

            // resize output array to remove any extra bytes
            // allowing room for end of track data
            if (lngOutPos + 4 < midiOut.Length) {
                Array.Resize(ref midiOut, lngOutPos + 4);
            }
            // add end of track data
            midiOut[lngOutPos] = 0;
            midiOut[lngOutPos + 1] = 0xFF;
            midiOut[lngOutPos + 2] = 0x2F;
            midiOut[lngOutPos + 3] = 0;
            lngOutPos += 4;
            // update size of track data (total length - 22)
            midiOut[18] = (byte)(((lngOutPos - 22) >> 24));
            midiOut[19] = (byte)(((lngOutPos - 22) >> 16) & 0xFF);
            midiOut[20] = (byte)(((lngOutPos - 22) >> 8) & 0xFF);
            midiOut[21] = (byte)((lngOutPos - 22) & 0xFF);
            // convert ticks to seconds
            Length = (double)lngTicks / 60;
            return midiOut;
        }

        /// <summary>
        /// This method formats a native Apple IIgs PCM sound for playback on
        /// modern WAV devices.
        /// </summary>
        /// <param name="SoundIn"></param>
        /// <returns></returns>
        internal static byte[] BuildIIgsPCM(Sound SoundIn) {
            int i, lngSize;
            byte[] bData;
            // required format for WAV data file:
            //Positions     Value           Description
            // 0 - 3        "RIFF"          Marks the file as a riff (WAV) file.
            // 4 - 7        <varies>        Size of the overall file
            // 8 -11        "WAVE"          File Type Header. (should always equals "WAVE")
            // 12-15        "fmt "          Format chunk marker. Includes trailing space
            // 16-19        16              Length of format header data as listed above
            // 20-21        1               Type of format (1 is PCM)
            // 22-23        1               Number of Channels
            // 24-27        8000            Sample Rate
            // 28-31        8000            (Sample Rate * BitsPerSample * Channels) / 8
            // 32-33        1               (BitsPerSample * Channels) / 8 (1 - 8 bit mono)
            // 34-35        8               Bits per sample
            // 36-39        "data"          "data" chunk header. Marks the beginning of the data section.
            // 40-43        <varies>        Size of the data section.
            // 44+          data

            // IIgs header for pcm sounds is 54 bytes, but its purpose is still
            // mostly unknown; the total size is at pos 8-9; the rest of the
            // header appears identical across resources, with exception of
            // position 2- it seems to vary from low thirties to upper 60s,
            // (maybe it's a volume thing?)
            // All resources appear to end with a byte value of 0; not sure if
            // it's necessary for wav files, but we keep it anyway.

            // local copy of data -easier to manipulate
            bData = SoundIn.Data.AllData;
            // size of sound data is total file size, minus the PCM header 
            lngSize = bData.Length - 54;
            // expand midi data array to hold the sound resource data plus
            // the WAV file header
            midiPlayer.mMIDIData = new byte[44 + lngSize];
            // add header
            midiPlayer.mMIDIData[0] = 82;
            midiPlayer.mMIDIData[1] = 73;
            midiPlayer.mMIDIData[2] = 70;
            midiPlayer.mMIDIData[3] = 70;
            midiPlayer.mMIDIData[4] = (byte)((lngSize + 36) & 0xFF);
            midiPlayer.mMIDIData[5] = (byte)(((lngSize + 36) >> 8) & 0xFF);
            midiPlayer.mMIDIData[6] = (byte)(((lngSize + 36) >> 16) & 0xFF);
            midiPlayer.mMIDIData[7] = (byte)((lngSize + 36) >> 24);
            midiPlayer.mMIDIData[8] = 87;
            midiPlayer.mMIDIData[9] = 65;
            midiPlayer.mMIDIData[10] = 86;
            midiPlayer.mMIDIData[11] = 69;
            midiPlayer.mMIDIData[12] = 102;
            midiPlayer.mMIDIData[13] = 109;
            midiPlayer.mMIDIData[14] = 116;
            midiPlayer.mMIDIData[15] = 32;
            midiPlayer.mMIDIData[16] = 16;
            midiPlayer.mMIDIData[17] = 0;
            midiPlayer.mMIDIData[18] = 0;
            midiPlayer.mMIDIData[19] = 0;
            midiPlayer.mMIDIData[20] = 1;
            midiPlayer.mMIDIData[21] = 0;
            midiPlayer.mMIDIData[22] = 1;
            midiPlayer.mMIDIData[23] = 0;
            midiPlayer.mMIDIData[24] = 64;
            midiPlayer.mMIDIData[25] = 31;
            midiPlayer.mMIDIData[26] = 0;
            midiPlayer.mMIDIData[27] = 0;
            midiPlayer.mMIDIData[28] = 64;
            midiPlayer.mMIDIData[29] = 31;
            midiPlayer.mMIDIData[30] = 0;
            midiPlayer.mMIDIData[31] = 0;
            midiPlayer.mMIDIData[32] = 1;
            midiPlayer.mMIDIData[33] = 0;
            midiPlayer.mMIDIData[34] = 8;
            midiPlayer.mMIDIData[35] = 0;
            midiPlayer.mMIDIData[36] = 100;
            midiPlayer.mMIDIData[37] = 97;
            midiPlayer.mMIDIData[38] = 116;
            midiPlayer.mMIDIData[39] = 97;
            midiPlayer.mMIDIData[40] = (byte)((lngSize - 2) & 0xFF);
            midiPlayer.mMIDIData[41] = (byte)(((lngSize - 2) >> 8) & 0xFF);
            midiPlayer.mMIDIData[42] = (byte)(((lngSize - 2) >> 16) & 0xFF);
            midiPlayer.mMIDIData[43] = (byte)((lngSize - 2) >> 24);
            // copy data from sound resource
            lngPos = 44;
            for (i = 54; i < bData.Length; i++) {
                midiPlayer.mMIDIData[lngPos++] = bData[i];
            }
            return midiPlayer.mMIDIData;
        }

        /// <summary>
        /// Writes variable delta times to midi data array.
        /// </summary>
        /// <param name="LongIn"></param>
        internal static void WriteSndDelta(int LongIn) {
            int i = LongIn >> 21;
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn >> 14;
            if (i > 0) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn >> 7;
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            WriteSndByte((byte)(LongIn & 127));
        }

        /// <summary>
        /// Writes a two byte integer value to midi array data.
        /// </summary>
        /// <param name="IntegerIn"></param>
        internal static void WriteSndWord(int IntegerIn) {
            WriteSndByte((byte)(IntegerIn / 256));
            WriteSndByte((byte)(IntegerIn & 0xFF));
        }

        /// <summary>
        /// Writes a four byte long value to midi array data.
        /// </summary>
        /// <param name="LongIn"></param>
        internal static void WriteSndLong(int LongIn) {
            WriteSndByte((byte)(LongIn >> 24));
            WriteSndByte((byte)((LongIn >> 16) & 0xFF));
            WriteSndByte((byte)((LongIn >> 8) & 0xFF));
            WriteSndByte((byte)(LongIn & 0xFF));
        }

        /// <summary>
        /// Returns the track3 frequency divisor value at desired position. Used 
        /// by noise channel when building MIDI data.
        /// </summary>
        /// <param name="Track3"></param>
        /// <param name="lngTarget"></param>
        /// <returns></returns>
        internal static int GetTrack3Freq(Track Track3, int lngTarget) {
            // Have to step through track three until the same point in time is found
            int lngTickCount = 0;

            for (int i = 0; i < Track3.Notes.Count; i++) {
                lngTickCount += Track3.Notes[i].Duration;
                if (lngTickCount >= lngTarget) {
                    // this is the frequency we want
                    return Track3.Notes[i].FreqDivisor;
                }
            }
            // if nothing found, return 0
            return 0;
        }

        /// <summary>
        /// Writes a four byte long value to midi array data.
        /// </summary>
        /// <param name="ByteIn"></param>
        internal static void WriteSndByte(byte ByteIn) {
            midiPlayer.mMIDIData[lngPos++] = ByteIn;
            if (lngPos >= midiPlayer.mMIDIData.Length) {
                // bump up size to hold more data
                Array.Resize(ref midiPlayer.mMIDIData, lngPos + 256);
            }
        }
    }
}
