using System;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    public static partial class Base {

        #region Methods
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
            game.OnLoadGameStatus(warnInfo);
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
                    // open the file, load it into buffer, and close it
                    using FileStream fsDIR = new(strDirFile, FileMode.Open);
                    bytBuffer = new byte[fsDIR.Length];
                    fsDIR.Read(bytBuffer);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = Path.GetFileName(strDirFile);
                    throw wex;
                }
                if (bytBuffer.Length < 11) {
                    // not enough bytes to hold at least the 4 dir pointers + 1 resource
                    WinAGIException wex = new(LoadResString(542).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 542
                    };
                    wex.Data["baddir"] = Path.GetFileName(strDirFile);
                    throw wex;
                }
            }
            // step through all four resource types
            for (bytResType = 0; bytResType <= AGIResType.View; bytResType++) {
                if (game.agIsVersion3) {
                    // calculate offset and size of dir component
                    switch (bytResType) {
                    case AGIResType.Logic:
                        lngDirOffset = bytBuffer[0] + (bytBuffer[1] << 8);
                        lngDirSize = bytBuffer[2] + (bytBuffer[3] << 8) - lngDirOffset;
                        break;
                    case AGIResType.Picture:
                        lngDirOffset = bytBuffer[2] + (bytBuffer[3] << 8);
                        lngDirSize = bytBuffer[4] + (bytBuffer[5] << 8) - lngDirOffset;
                        break;
                    case AGIResType.View:
                        lngDirOffset = bytBuffer[4] + (bytBuffer[5] << 8);
                        lngDirSize = bytBuffer[6] + (bytBuffer[7] << 8) - lngDirOffset;
                        break;
                    case AGIResType.Sound:
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
                    //verify DIR file exists
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
                        // open the file, load it into buffer, and close it
                        using FileStream fsDIR = new FileStream(strDirFile, FileMode.Open);
                        bytBuffer = new byte[fsDIR.Length];
                        fsDIR.Read(bytBuffer);
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
                // check for invalid dir information
                if ((lngDirOffset < 0) || (lngDirSize < 0)) {
                    WinAGIException wex = new(LoadResString(542).Replace(ARG1, strDirFile)) {
                        HResult = WINAGI_ERR + 542
                    };
                    wex.Data["baddir"] = Path.GetFileName(strDirFile);
                    throw wex;
                }
                if (lngDirSize >= 3) {
                    // invalid v3 DIR block
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
                        warnInfo.ResType = AGIResType.Game;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "DW01";
                        warnInfo.Text = ResTypeAbbrv[(int)bytResType] + " portion of DIR file is larger than expected; it may be corrupted";
                        warnInfo.Line = "--";
                        warnInfo.Module = "--";
                        game.OnLoadGameStatus(warnInfo);
                    }
                    else {
                        warnInfo.ResType = AGIResType.Game;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "DW02";
                        warnInfo.Text = ResTypeAbbrv[(int)bytResType] + "DIR file is larger than expected; it may be corrupted";
                        warnInfo.Line = "--";
                        warnInfo.Module = "--";
                        game.OnLoadGameStatus(warnInfo);
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
                        game.OnLoadGameStatus(warnInfo);
                        warnInfo.Type = EventType.etWarning;
                        // get location data for this resource
                        byte1 = bytBuffer[lngDirOffset + bytResNum * 3];
                        byte2 = bytBuffer[lngDirOffset + bytResNum * 3 + 1];
                        byte3 = bytBuffer[lngDirOffset + bytResNum * 3 + 2];
                        // ignore invalid entries
                        bool isvalid;
                        if (game.agIsVersion3) {
                            // invalid only if all three bytes are 0xFF
                            isvalid = (byte1 != 0xFF) || (byte2 != 0xFF) || (byte3 != 0xFF);
                        }
                        else {
                            // invalid if upper nibble of byte1 is 0xF
                            isvalid = (byte1 & 0xF0) != 0xF0;
                        }
                        if (isvalid) {
                            bytVol = (sbyte)(byte1 >> 4);
                            lngLoc = ((byte1 & 0xF) << 16) + (byte2 << 8) + byte3;
                            switch (bytResType) {
                            case AGIResType.Logic:
                                game.agLogs.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agLogs[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(game, AGIResType.Logic, bytResNum, game.agLogs[bytResNum].ErrLevel, game.agLogs[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agLogs.Contains(bytResNum)) {
                                    game.agLogs[bytResNum].PropsDirty = false;
                                    game.agLogs[bytResNum].IsDirty = false;
                                    // logic source checks come after all resources loaded so leave it loaded
                                }
                                break;
                            case AGIResType.Picture:
                                game.agPics.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agPics[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(game, AGIResType.Picture, bytResNum, game.agPics[bytResNum].ErrLevel, game.agPics[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agPics.Contains(bytResNum)) {
                                    game.agPics[bytResNum].PropsDirty = false;
                                    game.agPics[bytResNum].IsDirty = false;
                                    game.agPics[bytResNum].Unload();
                                }
                                break;
                            case AGIResType.Sound:
                                game.agSnds.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agSnds[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(game, AGIResType.Sound, bytResNum, game.agSnds[bytResNum].ErrLevel, game.agSnds[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agSnds.Contains(bytResNum)) {
                                    game.agSnds[bytResNum].PropsDirty = false;
                                    game.agSnds[bytResNum].IsDirty = false;
                                    game.agSnds[bytResNum].Unload();
                                }
                                break;
                            case AGIResType.View:
                                game.agViews.InitLoad(bytResNum, bytVol, lngLoc);
                                if (game.agViews[bytResNum].ErrLevel != 0) {
                                    AddLoadWarning(game, AGIResType.View, bytResNum, game.agViews[bytResNum].ErrLevel, game.agViews[bytResNum].ErrData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agViews.Contains(bytResNum)) {
                                    game.agViews[bytResNum].PropsDirty = false;
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
        internal static void AddLoadWarning(AGIGame game, AGIResType resType, byte resNum, int errlevel, string[] errdata) {
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.etWarning,
                ResType = resType,
                ResNum = resNum,
                ID = "",
                Module = "--",
                Text = "",
                Line = "--"
            };


            // check for negative error values
            if (errlevel < 0) {
                switch (errlevel) {
                case -1:
                    // error 606: Can't load resource: file not found (%1)
                    warnInfo.ID = "VW01";
                    warnInfo.Text = $"{resType} {resNum} is in a VOL file ({Path.GetFileName(errdata[0])}) that does not exist";
                    warnInfo.Module = errdata[1];
                    break;
                case -2:
                    // error 700: file (%1) is readonly
                    warnInfo.ID = "VW02";
                    warnInfo.Text = $"{resType} {resNum} is in a VOL file ({Path.GetFileName(errdata[0])}) marked readonly";
                    warnInfo.Module = errdata[1];
                    break;
                case -3:
                    // error 502: Error %1 occurred while trying to access %2.
                    warnInfo.ID = "VW03";
                    warnInfo.Text = $"{resType} {resNum} is invalid due to file access error ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -4:
                    // error 505: Invalid resource location (%1) in %2.
                    warnInfo.ID = "VW04";
                    warnInfo.Text = $"{resType} {resNum} has an invalid location ({errdata[0]}) in volume file {errdata[2]}";
                    warnInfo.Module = errdata[3];
                    break;
                case -5:
                    // 506: invalid header
                    warnInfo.ID = "RW01";
                    warnInfo.Text = $"{resType} {resNum} has an invalid resource header at location {errdata[0]} in {errdata[2]}";
                    warnInfo.Module = errdata[3];
                    break;
                case -6:
                    // 704: sourcefile missing
                    warnInfo.ID = "RW02";
                    warnInfo.Text = $"Logic {resNum} source file is missing ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -7:
                    // 700: sourcefile is readonly
                    warnInfo.ID = "RW03";
                    warnInfo.Text = $"Logic {resNum} source file is marked readonly ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -8:
                    // 502: Error %1 occurred while trying to access logic source file(%2).
                    warnInfo.ID = "RW04";
                    warnInfo.Text = $"Logic {resNum} is invalid due to file access error ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -9:
                    // error 688: Error %1 occurred while decompiling message section.
                    warnInfo.ID = "RW05";
                    warnInfo.Text = $"Logic {resNum} is invalid due to error in message section ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -10:
                    // error 688: Error %1 occurred while decompiling labels.
                    warnInfo.ID = "RW06";
                    warnInfo.Text = $"Logic {resNum} is invalid due to error in label search ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -11:
                    // error 688: Error %1 occurred while decompiling if block.
                    warnInfo.ID = "RW07";
                    warnInfo.Text = $"Logic {resNum} is invalid due to error in if block section ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -12:
                    // error 688: Error %1 occurred while decompiling - invalid message.
                    warnInfo.ID = "RW08";
                    warnInfo.Text = $"Logic {resNum} is invalid due to invalid message value ({errdata[0]})";
                    warnInfo.Module = errdata[1];
                    break;
                case -13:
                    // error 598: invalid sound data
                    warnInfo.ID = "RW09";
                    warnInfo.Text = $"Invalid sound data format, unable to load tracks";
                    warnInfo.Module = errdata[0];
                    break;
                case -14:
                    // error 565: sound invalid data error
                    warnInfo.ID = "RW10";
                    warnInfo.Text = $"Error encountered in LoadTracks ({errdata[1]})";
                    warnInfo.Module = errdata[0];
                    break;
                case -15:
                    // error 595: invalid view data
                    warnInfo.ID = "RW11";
                    warnInfo.Text = $"Invalid view data, unable to load view";
                    warnInfo.Module = errdata[0];
                    break;
                case -16:
                    // error 548: invalid loop pointer
                    warnInfo.ID = "RW12";
                    warnInfo.Text = $"Invalid loop data pointer detected (loop {errdata[1]})";
                    warnInfo.Module = errdata[0];
                    break;
                case -17:
                    // error 539: invalid source loop for mirror/invalid mirror loop number
                    warnInfo.ID = "RW13";
                    warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[2]} and/or loop {errdata[3]})";
                    warnInfo.Module = errdata[0];
                    break;
                case -18:
                    // error 550: invalid mirror data, target loop already mirrored
                    warnInfo.ID = "RW14";
                    warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[3]} already mirrored)";
                    warnInfo.Module = errdata[0];
                    break;
                case -19:
                    // 551: invalid mirror data, source already a mirror
                    warnInfo.ID = "RW15";
                    warnInfo.Text = $"Invalid Mirror loop value detected (loop {errdata[2]} already mirrored)";
                    warnInfo.Module = errdata[0];
                    break;
                case -20:
                    // 553: invalid cel data pointer
                    warnInfo.ID = "RW16";
                    warnInfo.Text = $"Invalid cel pointer detected (cel {errdata[2]} of loop {errdata[1]})";
                    warnInfo.Module = errdata[0];
                    break;
                }
                game.OnLoadGameStatus(warnInfo);
                return;
            }
            switch (resType) {
            case AGIResType.Logic:
                // none
                break;
            case AGIResType.Picture:
                if (errlevel > 0) {
                    if ((errlevel & 1) == 1) {
                        // missing EOP marker
                        warnInfo.ID = "RW17";
                        warnInfo.Text = $"Picture {resNum} is missing its 'end-of-resource' marker and may be corrupt";
                        warnInfo.Module = errdata[0];
                        game.OnLoadGameStatus(warnInfo);
                    }
                    if ((errlevel & 2) == 2) {
                        // bad color
                        warnInfo.ID = "RW18";
                        warnInfo.Text = $"Picture {resNum} has at least one invalid color assignment - picture may be corrupt";
                        warnInfo.Module = errdata[0];
                        game.OnLoadGameStatus(warnInfo);
                    }
                    if ((errlevel & 4) == 4) {
                        // bad cmd
                        warnInfo.ID = "RW19";
                        warnInfo.Text = $"Picture {resNum} has at least one invalid command byte - picture may be corrupt";
                        warnInfo.Module = errdata[0];
                        game.OnLoadGameStatus(warnInfo);
                    }
                    if ((errlevel & 8) == 8) {
                        // extra data
                        warnInfo.ID = "RW20";
                        warnInfo.Text = $"{resType} {resNum} has extra data past the end of resource";
                        warnInfo.Module = errdata[0];
                        game.OnLoadGameStatus(warnInfo);
                    }
                    if ((errlevel & 16) == 16) {
                        // unhandled error
                        warnInfo.ID = "RW21";
                        warnInfo.Text = $"Unhandled error in Picture {resNum} data- picture may not display correctly ({errlevel})";
                        warnInfo.Module = errdata[0];
                        game.OnLoadGameStatus(warnInfo);
                    }

                }
                break;
            case AGIResType.Sound:
                warnInfo.ID = "RW22";
                warnInfo.Text = $"Sound {resNum} has an invalid track pointer ({errlevel})";
                warnInfo.Module = errdata[0];
                game.OnLoadGameStatus(warnInfo);
                break;
            case AGIResType.View:
                warnInfo.ID = "RW23";
                warnInfo.Text = $"View {resNum} has an invalid view description pointer";
                warnInfo.Module = errdata[0];
                game.OnLoadGameStatus(warnInfo);
                break;
            case AGIResType.Objects:
                warnInfo.Module = "OBJECT";
                if ((errlevel & 1) == 1) {
                    warnInfo.ID = "RW24";
                    warnInfo.Text = "OBJECT file has no items";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 2) == 2) {
                    warnInfo.ID = "RW25";
                    warnInfo.Text = "Unable to decrypt OBJECT file";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 4) == 4) {
                    warnInfo.ID = "RW26";
                    warnInfo.Text = "Invalid OBJECT file header, unable to read item data";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 8) == 8) {
                    warnInfo.ID = "RW27";
                    warnInfo.Text = "Invalid text pointer encountered in OBJECT file";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 16) == 16) {
                    warnInfo.ID = "RW28";
                    warnInfo.Text = "First item is not the null '?' item";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 32) == 32) {
                    warnInfo.ID = "RW29";
                    warnInfo.Text = "File access error, unable to read OBJECT file";
                    game.OnLoadGameStatus(warnInfo);
                }
                break;
            case AGIResType.Words:
                warnInfo.Module = "WORDS.TOK";
                if ((errlevel & 1) == 1) {
                    warnInfo.ID = "RW30";
                    warnInfo.Text = "Abnormal index table";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 2) == 2) {
                    warnInfo.ID = "RW31";
                    warnInfo.Text = "Unexpected end of file";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 4) == 4) {
                    warnInfo.ID = "RW32";
                    warnInfo.Text = "Upper case characters detected";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 8) == 8) {
                    warnInfo.ID = "RW33";
                    warnInfo.Text = "Empty WORDS.TOK file";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 16) == 16) {
                    warnInfo.ID = "RW34";
                    warnInfo.Text = "Invalid index table";
                    game.OnLoadGameStatus(warnInfo);
                }
                if ((errlevel & 32) == 32) {
                    warnInfo.ID = "RW35";
                    warnInfo.Text = "File access error, unable to read OBJECT file";
                    game.OnLoadGameStatus(warnInfo);
                }
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
                    // extract uncompressed byte
                    bytCurUncomp = bytBuffer;
                    // shift buffer back
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
                        // write rest of buffer byte
                        bytExpandedData[lngTempCurPos++] = (byte)(bytBuffer >> 4);
                        blnOffset = false;
                    }
                    else {
                        bytCurComp = bytOriginalData[intPosIn++];
                        bytExpandedData[lngTempCurPos++] = (byte)(bytCurComp >> 4);
                        // fill buffer
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
                // step through rest of pixels in this row
                for (i = 1; i < mWidth; i++) {
                    bytNextColor = mCelData[i, j];
                    if ((bytNextColor != bytChunkColor) || (bytChunkLen == 0xF)) {
                        bytTempRLE[lngByteCount++] = (byte)(bytChunkColor * 0x10 + bytChunkLen);
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
                if (bytChunkColor != (byte)mTransColor) {
                    // last chunk is NOT transparent
                    bytTempRLE[lngByteCount++] = (byte)(bytChunkColor * 0x10 + bytChunkLen);
                }
                // always Count last chunk for mirror
                lngMirrorCount++;
                // add zero to indicate end of row
                bytTempRLE[lngByteCount++] = 0;
                lngMirrorCount++;
            }
            if (blnMirror) {
                // add zeros to make room for mirror loop
                while (lngByteCount < lngMirrorCount) {
                    bytTempRLE[lngByteCount] = 0;
                    lngByteCount++;
                }
            }
            Array.Resize(ref bytTempRLE, lngByteCount);
            return bytTempRLE;
        }

        /// <summary>
        /// This method creates a MIDI audio data stream from a PCjr formatted sound resource
        /// for playback.
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
            SoundData sndOut = new();

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
            sndOut.Data = new byte[70 + 256];

            // header
            sndOut.Data[0] = 77;  // "M"
            sndOut.Data[1] = 84;  // "T"
            sndOut.Data[2] = 104; // "h"
            sndOut.Data[3] = 100; // "d"
            sndOut.Pos = 4;
            // remaining length of header
            sndOut.WriteSndLong(6);
            // track mode
            //   mode 0 = single track
            //   mode 1 = multiple tracks, all start at zero
            //   mode 2 = multiple tracks, independent start times
            sndOut.WriteSndWord(1);
            if (lngTrackCount == 0) {
                // create an empty MIDI stream
                sndOut.Data = new byte[38];
                // one track, with one note of smallest length, and no sound
                sndOut.WriteSndWord(1);
                // pulses per quarter note
                sndOut.WriteSndWord(30);
                sndOut.WriteSndByte(77);  // "M"
                sndOut.WriteSndByte(84);  // "T"
                sndOut.WriteSndByte(114); // "r"
                sndOut.WriteSndByte(107); // "k"
                // track length
                sndOut.WriteSndLong(15);
                // track number
                sndOut.WriteSndDelta(0);
                //  set instrument status byte
                sndOut.WriteSndByte(0xC0);
                // instrument number
                sndOut.WriteSndByte(0);
                // add a slight delay note with no volume to end
                sndOut.WriteSndDelta(0);
                sndOut.WriteSndByte(0x90);
                sndOut.WriteSndByte(60);
                sndOut.WriteSndByte(0);
                sndOut.WriteSndDelta(16);
                sndOut.WriteSndByte(0x80);
                sndOut.WriteSndByte(60);
                sndOut.WriteSndByte(0);
                // add end of track info
                sndOut.WriteSndDelta(0);
                sndOut.WriteSndByte(0xFF);
                sndOut.WriteSndByte(0x2F);
                sndOut.WriteSndByte(0x0);
                return sndOut.Data;
            }
            // track count
            sndOut.WriteSndWord(lngTrackCount);
            // pulses per quarter note
            // (agi sound tick is 1/60 sec; each tick of an AGI note
            // is 1/60 of a second; by default, MIDI defines a whole
            // note as 2 seconds; therefore, a quarter note is 1/2
            // second, or 30 ticks
            sndOut.WriteSndWord(30);
            // sound tracks
            for (i = 0; i < 3; i++) {
                if (!SoundIn[i].Muted && SoundIn[i].Notes.Count > 0) {
                    sndOut.WriteSndByte(77);  // "M"
                    sndOut.WriteSndByte(84);  // "T"
                    sndOut.WriteSndByte(114); //"r"
                    sndOut.WriteSndByte(107); //"k"
                    // starting position for this track's data
                    lngStart = sndOut.Pos;
                    // place holder for data size
                    sndOut.WriteSndLong(0);
                    // track number
                    //     *********** i think this should be zero for all tracks
                    //     it's the delta sound value for the instrument setting
                    sndOut.WriteSndDelta(0);
                    // instrument status byte
                    sndOut.WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    // instrument number
                    sndOut.WriteSndByte(SoundIn[i].Instrument);
                    // add a slight delay note with no volume to start
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndDelta(4);
                    sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add notes from this track
                    for (j = 0; j <= SoundIn[i].Notes.Count - 1; j++) {
                        // calculate note to play - convert FreqDivisor to frequency
                        //     f = 111860 / FreqDiv
                        // then convert that into appropriate MIDI note; MIDI notes
                        // go from 1 to 127, with 60 being middle C (261.6 Hz); every
                        // twelve notes, the frequency doubles, this gives the following
                        // forumula to convert frequency to midinote:
                        //     midinote = log10(f)/log10(2^(1/12)) - X
                        // since middle C is 261.6 Hz, this means X = 36.375
                        // however, this offset results in crappy sounding music, likely due
                        // to the way the sounds were originally written (as tones on the PCjr
                        // sound chip which could produce much higher resolution than twelve
                        // notes per doubling of frequency) combined with the way that VS math
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
                        sndOut.WriteSndDelta(0);
                        sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                        sndOut.WriteSndByte(bytNote);
                        sndOut.WriteSndByte(bytVol);
                        // NOTE OFF
                        sndOut.WriteSndDelta(SoundIn[i].Notes[j].Duration);
                        sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                        sndOut.WriteSndByte(bytNote);
                        sndOut.WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end of track
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndDelta(4);
                    sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add end of track info
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte(0xFF);
                    sndOut.WriteSndByte(0x2F);
                    sndOut.WriteSndByte(0x0);
                    // save track end position
                    lngEnd = sndOut.Pos;
                    // set cursor to start of track
                    sndOut.Pos = lngStart;
                    // add the track length
                    sndOut.WriteSndLong((lngEnd - lngStart) - 4);
                    sndOut.Pos = lngEnd;
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
                    sndOut.WriteSndByte(77);  // "M"
                    sndOut.WriteSndByte(84);  // "T"
                    sndOut.WriteSndByte(114); // "r"
                    sndOut.WriteSndByte(107); // "k"
                    // store starting position for this track's data
                    lngStart = sndOut.Pos;
                    // place holder for chunklength
                    sndOut.WriteSndLong(0);
                    // track number
                    sndOut.WriteSndDelta(lngWriteTrack);
                    // instrument
                    sndOut.WriteSndByte((byte)(0xC0 + lngWriteTrack));
                    // instrument number
                    switch (i) {
                    case 0:
                        // tone - use harpsichord
                        sndOut.WriteSndByte(6);
                        break;
                    case 1:
                        // white noise - use seashore
                        sndOut.WriteSndByte(122);
                        // crank up the volume
                        sndOut.WriteSndByte(0);
                        sndOut.WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        sndOut.WriteSndByte(7);
                        sndOut.WriteSndByte(127);
                        // set legato
                        sndOut.WriteSndByte(0);
                        sndOut.WriteSndByte((byte)(0xB0 + lngWriteTrack));
                        sndOut.WriteSndByte(68);
                        sndOut.WriteSndByte(127);
                        break;
                    }
                    // add a slight delay note with no volume to start the track
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndDelta(4);
                    sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
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
                        //    .  1  1  .  .  .  .  .      Register number in T1 chip (3)
                        //    .  .  .  0  .  .  .  .      Data type (0 = frequency)
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
                        sndOut.WriteSndDelta(0);
                        sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                        sndOut.WriteSndByte(bytNote);
                        sndOut.WriteSndByte(bytVol);
                        // NOTE OFF
                        sndOut.WriteSndDelta(SoundIn[3].Notes[j].Duration);
                        sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                        sndOut.WriteSndByte(bytNote);
                        sndOut.WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte((byte)(0x90 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndDelta(4);
                    sndOut.WriteSndByte((byte)(0x80 + lngWriteTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add end of track data
                    sndOut.WriteSndDelta(0);
                    sndOut.WriteSndByte(0xFF);
                    sndOut.WriteSndByte(0x2F);
                    sndOut.WriteSndByte(0x0);
                    // save ending position
                    lngEnd = sndOut.Pos;
                    // go back to start of track, and add track length
                    sndOut.Pos = lngStart;
                    sndOut.WriteSndLong(lngEnd - lngStart - 4);
                    sndOut.Pos = lngEnd;
                    lngWriteTrack++;
                }
            }
            // remove any extra padding from the data array
            Array.Resize(ref sndOut.Data, sndOut.Pos);
            return sndOut.Data;
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
            //
            // The raw midi data embedded in the sound seems to play OK when
            // using 'high-end' players (WinAmp as an example) but not in
            // Windows Media Player, or the Windows midi API functions. ; even
            // in WinAmp, the total sound length (ticks and seconds) doesn't
            // calculate correctly, even though it plays. This seems to be due
            // to the presence of the 0xFC commands. It looks like every IIgs
            // sound resource has them; sometimes one 0xFC ends the file, other
            // times there are a series of them that are followed by a set of
            // 0xD# and 0xB# commands that appear to reset all 16 channels.
            // Eliminating the 0xFC command and everything that follows plays
            // the sound correctly (I think). A common 'null' file in IIgs games
            // is one with just four 0xFC codes, and nothing else.
            int lngInPos, lngOutPos;
            byte[] midiIn, midiOut;
            int lngTicks = 0, lngTime;
            byte bytIn, bytCmd = 0, bytChannel = 0;
            // local copy of data -easier to manipulate
            midiIn = SoundIn.Data;

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
                else {
                    // it's a running status - back up so next byte is event data
                    lngInPos--;
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
        /// This method creates a data stream from a native Apple IIgs PCM sound for playback on
        /// modern WAV devices.
        /// </summary>
        /// <param name="SoundIn"></param>
        /// <returns></returns>
        internal static byte[] BuildIIgsPCM(Sound SoundIn) {
            int lngSize;
            byte[] bData, bOutput;
            // This method creates just the data stream, not the header. The header is
            // only needed when creating a WAV file.

            bData = SoundIn.Data;
            // size of sound data is total resource size, minus the PCM header that
            // AGI includes in the sound resource (54 bytes) plus four bytes for an
            // end of stream marker (for a net of -50 bytes).
            lngSize = bData.Length - 50;
            bOutput = new byte[lngSize];
            // copy data from sound resource
            int pos = 0;
            for (int i = 54; i < bData.Length; i++) {
                bOutput[pos++] = bData[i];
            }
            // add four bytes of silence as an end marker
            bOutput[^1] = 127;
            bOutput[^2] = 127;
            bOutput[^3] = 127;
            bOutput[^4] = 127;
            return bOutput;
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
        #endregion
    }

    /// <summary>
    /// A class to provide LZW decompression support for AGI v3 resources.
    /// </summary>
    internal static class AGILZW {
        #region Members
        const int TABLE_SIZE = 18041;
        const int START_BITS = 9;
        private static int lngMaxCode;
        private static uint[] intPrefix;
        private static byte[] bytAppend;
        private static int lngBitsInBuffer;
        private static uint lngBitBuffer;
        private static int lngOriginalSize;
        #endregion

        #region Methods
        /// <summary>
        /// Expands a v3 AGI resource that is compressed using Sierra's custom LZW
        /// implementation.
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
            intPrefix = new uint[TABLE_SIZE];
            bytAppend = new byte[TABLE_SIZE];
            // set temporary data field
            byte[] bytTempData = new byte[fullsize];
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
                    HResult = Base.WINAGI_ERR + 559,
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
        /// A method that converts a code value into its original string value.
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
        /// This method extracts the next code Value off the input stream.
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
                // overwrite the bits currently in the buffer, and add the bits to the buffer
                lngBitBuffer |= (lngWord << lngBitsInBuffer);
                // increment count of how many bits are currently in the buffer
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
        /// This method supports the expansion of compressed resources.
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
        #endregion
    }
}
