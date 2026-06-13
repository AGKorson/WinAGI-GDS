using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;

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
        internal static bool ExtractResources(AGIGame game, OpenGameMode mode) {
            byte resNum;
            AGIResType resType;
            string dirFile = "";
            string volFile;
            byte[] buffer = [];
            byte byte1, byte2, byte3;
            int dirOffset = 0;  // offset of this resource's directory in Dir file (for v3)
            int dirSize = 0, intResCount, i;
            sbyte volNum;
            int loc;
            bool loadWarnings = false;
            WinAGIEventInfo warnInfo = new() {
                InfoType = InfoType.Resources,
            };
            LoadEventStatus(mode, warnInfo);
            // set up for warnings
            warnInfo.Type = EventType.ResourceWarning;

            if (game.agIntVersion.IsV3) {
                // check for DIR file
                dirFile = Path.Combine(game.agGameDir, game.agGameID + "DIR");
                if (!File.Exists(dirFile)) {
                    throw new FileNotFoundException(dirFile);
                }
                // check DIR file for readonly
                if ((File.GetAttributes(dirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, dirFile)) {
                        HResult = WINAGI_ERR + 539,
                    };
                    wex.Data["badfile"] = dirFile;
                    throw wex;
                }
                try {
                    // open the file, load it into buffer, and close it
                    using FileStream fsDIR = new(dirFile, FileMode.Open, FileAccess.Read);
                    buffer = new byte[fsDIR.Length];
                    fsDIR.Read(buffer);
                }
                catch (Exception e) {
                    WinAGIException wex = new(EngineResourceByNum(541)) {
                        HResult = WINAGI_ERR + 541
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = Path.GetFileName(dirFile);
                    throw wex;
                }
                if (buffer.Length < 11) {
                    // not enough bytes to hold at least the 4 dir pointers + 1 resource
                    WinAGIException wex = new(EngineResourceByNum(505).Replace(ARG1, dirFile)) {
                        HResult = WINAGI_ERR + 505
                    };
                    wex.Data["baddir"] = Path.GetFileName(dirFile);
                    throw wex;
                }
                volFile = Path.Combine(game.agGameDir, game.agGameID + "VOL.0");
            }
            else {
                // v2 DIR files checked later- now just check for vol.0
                volFile = Path.Combine(game.agGameDir, "VOL.0");
            }
            // confirm vol.0 exists and is accessible
            if (!File.Exists(volFile)) {
                throw new FileNotFoundException(volFile);
            }
            // check for readonly
            if ((File.GetAttributes(volFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, volFile)) {
                    HResult = WINAGI_ERR + 539,
                };
                wex.Data["badfile"] = volFile;
                throw wex;
            }

            // step through all four resource types
            for (resType = 0; resType <= AGIResType.View; resType++) {
                if (game.agIntVersion.IsV3) {
                    // calculate offset and size of dir component
                    switch (resType) {
                    case AGIResType.Logic:
                        dirOffset = buffer[0] + (buffer[1] << 8);
                        dirSize = buffer[2] + (buffer[3] << 8) - dirOffset;
                        break;
                    case AGIResType.Picture:
                        dirOffset = buffer[2] + (buffer[3] << 8);
                        dirSize = buffer[4] + (buffer[5] << 8) - dirOffset;
                        break;
                    case AGIResType.View:
                        dirOffset = buffer[4] + (buffer[5] << 8);
                        dirSize = buffer[6] + (buffer[7] << 8) - dirOffset;
                        break;
                    case AGIResType.Sound:
                        dirOffset = buffer[6] + (buffer[7] << 8);
                        dirSize = buffer.Length - dirOffset;
                        break;
                    default:
                        break;
                    }
                }
                else {
                    // no offset for version 2
                    dirOffset = 0;
                    dirFile = Path.Combine(game.agGameDir, ResTypeAbbrv[(int)resType] + "DIR");
                    // verify DIR file exists
                    if (!File.Exists(dirFile)) {
                        throw new FileNotFoundException(dirFile);
                    }
                    // readonly not allowed
                    if ((File.GetAttributes(dirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                        WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, dirFile)) {
                            HResult = WINAGI_ERR + 539,
                        };
                        wex.Data["badfile"] = dirFile;
                        throw wex;
                    }
                    try {
                        // open the file, load it into buffer, and close it
                        using FileStream fsDIR = new(dirFile, FileMode.Open, FileAccess.Read);
                        buffer = new byte[fsDIR.Length];
                        fsDIR.Read(buffer);
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(EngineResourceByNum(541)) {
                            HResult = WINAGI_ERR + 541
                        };
                        wex.Data["exception"] = e;
                        wex.Data["dirfile"] = dirFile;
                        throw wex;
                    }
                    dirSize = buffer.Length;
                }
                // check for invalid dir information
                if ((dirOffset < 0) || (dirSize < 0)) {
                    WinAGIException wex = new(EngineResourceByNum(505).Replace(ARG1, dirFile)) {
                        HResult = WINAGI_ERR + 505
                    };
                    wex.Data["baddir"] = Path.GetFileName(dirFile);
                    throw wex;
                }
                if (dirSize >= 3) {
                    // invalid v3 DIR block
                    if (dirOffset + dirSize > buffer.Length) {
                        WinAGIException wex = new(EngineResourceByNum(505).Replace(ARG1, dirFile)) {
                            HResult = WINAGI_ERR + 505
                        };
                        wex.Data["baddir"] = Path.GetFileName(dirFile);
                        throw wex;
                    }
                }
                // max size of useable directory is 768 (256*3)
                if (dirSize > 768) {
                    // warning- file might be invalid
                    loadWarnings = true;
                    if (game.agIntVersion.IsV3) {
                        warnInfo.ResType = AGIResType.Game;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "RW01";
                        warnInfo.Text = EngineResources.RW01.Replace(
                            ARG1, ResTypeAbbrv[(int)resType]);
                        warnInfo.Line = -1;
                        warnInfo.Module = "--";
                        LoadEventStatus(mode, warnInfo);
                    }
                    else {
                        warnInfo.ResType = AGIResType.Game;
                        warnInfo.ResNum = 0;
                        warnInfo.ID = "RW02";
                        warnInfo.Text = EngineResources.RW02.Replace(
                            ARG1, ResTypeAbbrv[(int)resType]);
                        warnInfo.Line = -1;
                        warnInfo.Module = "--";
                        LoadEventStatus(mode, warnInfo);
                    }
                    // assume the max for now
                    intResCount = 256;
                }
                else {
                    intResCount = dirSize / 3;
                }
                if (intResCount > 0) {
                    // check for bad resources
                    for (i = 0; i < intResCount; i++) {
                        resNum = (byte)i;
                        warnInfo.Type = EventType.Info;
                        warnInfo.InfoType = InfoType.Resources;
                        warnInfo.ResType = resType;
                        warnInfo.ResNum = resNum;
                        warnInfo.ID = "";
                        warnInfo.Text = "";
                        warnInfo.Line = -1;
                        warnInfo.Module = "--";
                        LoadEventStatus(mode, warnInfo);
                        warnInfo.Type = EventType.ResourceWarning;
                        // get location data for this resource
                        byte1 = buffer[dirOffset + resNum * 3];
                        byte2 = buffer[dirOffset + resNum * 3 + 1];
                        byte3 = buffer[dirOffset + resNum * 3 + 2];
                        // ignore invalid entries
                        bool isvalid;
                        if (game.agIntVersion.IsV3) {
                            // invalid only if all three bytes are 0xFF
                            isvalid = (byte1 != 0xFF) || (byte2 != 0xFF) || (byte3 != 0xFF);
                        }
                        else {
                            // invalid if upper nibble of byte1 is 0xF
                            isvalid = (byte1 & 0xF0) != 0xF0;
                        }
                        if (isvalid) {
                            volNum = (sbyte)(byte1 >> 4);
                            loc = ((byte1 & 0xF) << 16) + (byte2 << 8) + byte3;
                            switch (resType) {
                            case AGIResType.Logic:
                                game.agLogs.InitLoad(resNum, volNum, loc);
                                if (game.agLogs[resNum].Error != ResourceErrorType.NoError) {
                                    AddLoadError(mode, game, AGIResType.Logic, resNum, game.agLogs[resNum].Error, game.agLogs[resNum].ErrData);
                                    loadWarnings = true;
                                }
                                // source errors checkedlater, and logic resources have no warnings
                                // make sure it was added before finishing
                                if (game.agLogs.Contains(resNum)) {
                                    Debug.Assert(!game.agLogs[resNum].PropsChanged);
                                    Debug.Assert(!game.agLogs[resNum].IsChanged);
                                    // logic source checks come after all resources
                                    // loaded so leave it loaded
                                }
                                break;
                            case AGIResType.Picture:
                                game.agPics.InitLoad(resNum, volNum, loc);
                                if (game.agPics[resNum].Error != ResourceErrorType.NoError) {
                                    AddLoadError(mode, game, AGIResType.Picture, resNum, game.agPics[resNum].Error, game.agPics[resNum].ErrData);
                                    loadWarnings = true;
                                }
                                if (game.agPics[resNum].Warnings != 0) {
                                    AddLoadWarning(mode, game, AGIResType.Picture, resNum, game.agPics[resNum].Warnings, game.agPics[resNum].WarnData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agPics.Contains(resNum)) {
                                    Debug.Assert(!game.agPics[resNum].PropsChanged);
                                    Debug.Assert(!game.agPics[resNum].IsChanged);
                                    game.agPics[resNum].Unload();
                                }
                                break;
                            case AGIResType.Sound:
                                game.agSnds.InitLoad(resNum, volNum, loc);
                                if (game.agSnds[resNum].Error != ResourceErrorType.NoError) {
                                    AddLoadError(mode, game, AGIResType.Sound, resNum, game.agSnds[resNum].Error, game.agSnds[resNum].ErrData);
                                    loadWarnings = true;
                                }
                                if (game.agSnds[resNum].Warnings != 0) {
                                    AddLoadWarning(mode, game, AGIResType.Sound, resNum, game.agSnds[resNum].Warnings, game.agSnds[resNum].WarnData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agSnds.Contains(resNum)) {
                                    game.agSnds[resNum].PropsChanged = false;
                                    game.agSnds[resNum].Unload();
                                }
                                break;
                            case AGIResType.View:
                                game.agViews.InitLoad(resNum, volNum, loc);
                                if (game.agViews[resNum].Error != ResourceErrorType.NoError) {
                                    AddLoadError(mode, game, AGIResType.View, resNum, game.agViews[resNum].Error, game.agViews[resNum].ErrData);
                                    loadWarnings = true;
                                }
                                if (game.agViews[resNum].Warnings != 0) {
                                    AddLoadWarning(mode, game, AGIResType.View, resNum, game.agViews[resNum].Warnings, game.agViews[resNum].WarnData);
                                    loadWarnings = true;
                                }
                                // make sure it was added before finishing
                                if (game.agViews.Contains(resNum)) {
                                    Debug.Assert(!game.agViews[resNum].PropsChanged);
                                    game.agViews[resNum].Unload();
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
        /// Sends errors encountered during game load to the main app window as events. 
        /// </summary>
        internal static void AddLoadError(OpenGameMode mode, AGIGame game, AGIResType resType, byte resNum, ResourceErrorType errlevel, string[] errdata) {
            WinAGIEventInfo error = ErrorByNum(resType, resNum, errlevel, errdata);
            LoadEventStatus(mode, error);
        }

        /// <summary>
        /// Sends warnings encountered during game load to the main app window as events. 
        /// </summary>
        internal static void AddLoadWarning(OpenGameMode mode, AGIGame game, AGIResType resType, byte resNum, int warnings, string[] warndata) {
            List<WinAGIEventInfo> errCol = WarningsFromField(resType, resNum, warnings, warndata);
            foreach (WinAGIEventInfo warning in errCol) {
                LoadEventStatus(mode, warning);
            }
        }

        /// <summary>
        /// Expands a v3 AGI resource that is compressed using Sierra's custom LZW
        /// implementation.
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="fullsize"></param>
        /// <returns></returns>
        internal static byte[] ExpandV3ResData(byte[] originalData, int fullsize) {
            const int TABLE_SIZE = 18041;
            const int START_BITS = 9;
            const int MAX_BITS = 11;
            int MaxCode;
            uint[] Prefix = [];
            byte[] Append = [];
            int BitsInBuffer;
            uint BitBuffer;
            int OriginalSize;

            int inPos = 0;
            int outPos = 0;
            int nextCode;
            int codeSize;
            uint oldCode, newCode;
            var decodebuffer = new byte[4096];

            Prefix = new uint[TABLE_SIZE];
            Append = new byte[TABLE_SIZE];
            byte[] output = new byte[fullsize];
            // original size is determined by array bounds
            OriginalSize = originalData.Length;
            BitBuffer = 0;
            BitsInBuffer = 0;

            // initialize
            codeSize = NewCodeSize(START_BITS);
            nextCode = 257;

            // read in the first code.
            oldCode = ReadCode(originalData, codeSize, ref inPos);
            // first code for SIERRA resouces should always be 256
            if (oldCode != 256) {
                WinAGIException wex = new(EngineResourceByNum(509).Replace(ARG1, "(invalid compression data)")) {
                    HResult = WINAGI_ERR + 509,
                };
                throw wex;
            }
            // now begin decompressing actual data
            newCode = ReadCode(originalData, codeSize, ref inPos);
            // continue extracting data, until all bytes are read (or end code is reached)
            while ((inPos <= OriginalSize) && (newCode != 0x101)) {
                byte databyte;
                // check for reset
                if (newCode == 0x100) {
                    // Restart LZW process
                    nextCode = 258;
                    codeSize = NewCodeSize(START_BITS);
                    // read in the first code
                    oldCode = ReadCode(originalData, codeSize, ref inPos);
                    // the character Value is same as code for beginning
                    databyte = (byte)oldCode;
                    // write out the first character
                    output[outPos++] = databyte;
                    // now get next code
                    newCode = ReadCode(originalData, codeSize, ref inPos);
                    continue;
                }
                // This code checks for the special STRING+character+STRING+character+STRING
                // case which generates an undefined code.  It handles it by decoding
                // the last code, and adding a single Character to the end of the decode string.
                // (new_code will ONLY return a next_code Value if the condition exists;
                // it should otherwise return a known code, or a ascii Value)
                int start, length;
                if (newCode >= nextCode) {
                    // decode the string using old code
                    length = DecodeBytes(oldCode, decodebuffer, out start);
                    if (length == 0) {
                        break;
                    }
                    databyte = decodebuffer[start];
                    Buffer.BlockCopy(decodebuffer, start, output, outPos, length);
                    outPos += length;
                    // append the character code
                    output[outPos++] = databyte;
                }
                else {
                    // decode the string using new code
                    length = DecodeBytes(newCode, decodebuffer, out start);
                    if (length == 0) {
                        break;
                    }
                    databyte = decodebuffer[start];
                    Buffer.BlockCopy(decodebuffer, start, output, outPos, length);
                    outPos += length;
                }
                // if no more room in the current bit-code table,
                if (nextCode > MaxCode) {
                    // get new code size (in number of bits per code)
                    codeSize = NewCodeSize(codeSize + 1);
                }
                // store code in prefix table
                Prefix[nextCode - 257] = oldCode;
                // store append character in table
                Append[nextCode - 257] = databyte;
                // increment next code pointer
                nextCode++;
                oldCode = newCode;
                // get the next code
                newCode = ReadCode(originalData, codeSize, ref inPos);
            }
            return output;

            int DecodeBytes(uint code, byte[] scratch, out int start) {
                // Decodes a code into bytes and writes them into the scratch buffer.
                // The result is written at the end of the buffer so the byte order is correct.
                // Returns the number of bytes written, and the start index in the scratch buffer.
                int pos = scratch.Length;
                while (code > 255) {
                    if (code > TABLE_SIZE) {
                        start = scratch.Length;
                        return 0;
                    }
                    else {
                        scratch[--pos] = Append[code - 257];
                        code = Prefix[code - 257];
                    }
                }
                scratch[--pos] = (byte)code;
                start = pos;
                return scratch.Length - pos;
            }

            uint ReadCode(byte[] data, int codeSize, ref int inPos) {
                // This method extracts the next code Value off the input stream.
                // Since the number of bits per code can vary between 9 and 12, we can't read in
                // directly from the stream.

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
                while ((BitsInBuffer <= 24) && (inPos < OriginalSize)) {
                    uint worddata = data[inPos++];
                    // shift the data to the left by enough bits so the byte being added will not
                    // overwrite the bits currently in the buffer, and add the bits to the buffer
                    BitBuffer |= worddata << BitsInBuffer;
                    // increment count of how many bits are currently in the buffer
                    BitsInBuffer += 8;
                }
                // The input code starts at the lowest bit in the buffer. Since the buffer has
                // 32 bits total, we need to clear out all bits above the desired number of bits
                // to define the code (i.e. if 9 bits, AND with 0x1FF; 10 bits,AND with 0x3FF,
                // etc.)
                uint retval = BitBuffer & (uint)((1 << codeSize) - 1);
                // then shift the buffer to the right by the number of bits per code
                BitBuffer >>= codeSize;
                BitsInBuffer -= codeSize;
                // return code Value
                return retval;
            }

            int NewCodeSize(int value) {
                // This method supports the expansion of compressed resources.
                // It sets the size of codes which the LZW routine uses. The code size starts at
                // 9 bits, then increases as all code tables are filled. The max code size is 11;
                // if an attempt is made to set a code size above that, the function does nothing.
                // 
                // The function also recalculates the maximum number of codes available to the
                // expand subroutine. This number is used to determine when to call this function
                // again.
                int retval;

                if (value >= MAX_BITS) {
                    retval = MAX_BITS;
                }
                else {
                    retval = value;
                }
                MaxCode = (1 << retval) - 2;
                return retval;
            }
        }

        internal static void AddCompileError(AGIResType resType, byte resNum, ResourceErrorType errlevel, string[] errdata) {
            WinAGIEventInfo error = ErrorByNum(resType, resNum, errlevel, errdata);
            GameCompileStatus errstat = GameCompileStatus.ResError;
            OnCompileGameStatus(errstat, error);
        }

        internal static bool AddCompileWarnings(AGIResType resType, byte resNum, int warnings, string[] warndata) {
            bool retval = false;
            List<WinAGIEventInfo> warnCol = WarningsFromField(resType, resNum, warnings, warndata);
            GameCompileStatus warnstat = GameCompileStatus.Warning;
            foreach (WinAGIEventInfo warning in warnCol) {
                retval |= CheckForCancel(warnstat, warning);
            }
            return retval;
        }

        private static WinAGIEventInfo ErrorByNum(AGIResType resType, byte resNum, ResourceErrorType error, string[] errdata) {
            WinAGIEventInfo warnInfo = new() {
                ResType = resType,
                ResNum = resNum,
                Type = EventType.ResourceError,
                Line = -1,
            };
            switch (error) {
            case ResourceErrorType.FileNotFound:
                warnInfo.ID = "RE01";
                warnInfo.Text = EngineResources.RE01.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, Path.GetFileName(errdata[0]));
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.FileIsReadonly:
                warnInfo.ID = "RE02";
                warnInfo.Text = EngineResources.RE02.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, Path.GetFileName(errdata[0]));
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.FileAccessError:
                warnInfo.ID = "RE03";
                warnInfo.Text = EngineResources.RE03.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, errdata[0]);
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.InvalidLocation:
                warnInfo.ID = "RE04";
                warnInfo.Text = EngineResources.RE04.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, errdata[0]).Replace(
                    "%4", errdata[2]);
                warnInfo.Module = errdata[3];
                break;
            case ResourceErrorType.InvalidHeader:
                warnInfo.ID = "RE05";
                warnInfo.Text = EngineResources.RE05.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, errdata[0]).Replace(
                    "%4", errdata[2]);
                warnInfo.Module = errdata[3];
                break;
            case ResourceErrorType.DecompressionError:
                warnInfo.ID = "RE06";
                warnInfo.Text = EngineResources.RE06.Replace(
                    ARG1, "Logic").Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG2, errdata[0]).Replace(
                    "%4", errdata[1]);
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.LogicSourceIsReadonly:
                warnInfo.ID = "RE07";
                warnInfo.Text = EngineResources.RE07.Replace(
                    ARG1, resNum.ToString()).Replace(
                    ARG2, errdata[0]);
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.LogicSourceAccessError:
                warnInfo.ID = "RE08";
                warnInfo.Text = EngineResources.RE08.Replace(
                    ARG1, resNum.ToString()).Replace(
                    ARG2, errdata[0]);
                warnInfo.Module = errdata[1];
                break;
            case ResourceErrorType.SoundNoData:
                warnInfo.ID = "RE09";
                warnInfo.Text = EngineResources.RE09.Replace(
                    ARG1, resNum.ToString());
                warnInfo.Module = errdata[0];
                break;
            case ResourceErrorType.SoundBadTracks:
                warnInfo.ID = "RE10";
                warnInfo.Text = EngineResources.RE10.Replace(
                    ARG1, resNum.ToString()).Replace(
                    ARG2, errdata[1]);
                warnInfo.Module = errdata[0];
                break;
            case ResourceErrorType.ViewNoData:
                warnInfo.ID = "RE11";
                warnInfo.Text = EngineResources.RE11.Replace(
                    ARG1, resNum.ToString());
                warnInfo.Module = errdata[0];
                break;
            case ResourceErrorType.ViewNoLoops:
                warnInfo.ID = "RE12";
                warnInfo.Text = EngineResources.RE12.Replace(
                    ARG1, resNum.ToString()).Replace(
                    ARG2, errdata[1]);
                warnInfo.Module = errdata[0];
                break;
            case ResourceErrorType.ObjectNoFile:
                warnInfo.ID = "RE13";
                warnInfo.Text = EngineResources.RE13;
                break;
            case ResourceErrorType.ObjectIsReadOnly:
                warnInfo.ID = "RE14";
                warnInfo.Text = EngineResources.RE14;
                break;
            case ResourceErrorType.ObjectAccessError:
                warnInfo.ID = "RE15";
                warnInfo.Text = EngineResources.RE15.Replace(
                    ARG1, errdata[0]);
                break;
            case ResourceErrorType.ObjectNoData:
                warnInfo.ID = "RE16";
                warnInfo.Text = EngineResources.RE16;
                break;
            case ResourceErrorType.ObjectDecryptError:
                warnInfo.ID = "RE17";
                warnInfo.Text = EngineResources.RE17;
                break;
            case ResourceErrorType.ObjectBadHeader:
                warnInfo.ID = "RE18";
                warnInfo.Text = EngineResources.RE18;
                break;
            case ResourceErrorType.WordsTokNoFile:
                warnInfo.ID = "RE19";
                warnInfo.Text = EngineResources.RE19;
                break;
            case ResourceErrorType.WordsTokIsReadOnly:
                warnInfo.ID = "RE20";
                warnInfo.Text = EngineResources.RE20;
                break;
            case ResourceErrorType.WordsTokAccessError:
                warnInfo.ID = "RE21";
                warnInfo.Text = EngineResources.RE21.Replace(
                    ARG1, errdata[0]);
                break;
            case ResourceErrorType.WordsTokNoData:
                warnInfo.ID = "RE22";
                warnInfo.Text = EngineResources.RE22;
                break;
            case ResourceErrorType.WordsTokBadIndex:
                warnInfo.ID = "RE23";
                warnInfo.Text = EngineResources.RE23;
                break;
            case ResourceErrorType.DefinesNoFile:
                warnInfo.ID = "RE24";
                warnInfo.Text = EngineResources.RE24;
                break;
            case ResourceErrorType.DefinesReadOnly:
                warnInfo.ID = "RE25";
                warnInfo.Text = EngineResources.RE25;
                break;
            case ResourceErrorType.DefinesAccessError:
                warnInfo.ID = "RE26";
                warnInfo.Text = EngineResources.RE26.Replace(
                    ARG1, errdata[0]);
                break;
            case ResourceErrorType.SierraResourceError:
                warnInfo.ID = "RE27";
                warnInfo.Text = EngineResources.RE27.Replace(
                    ARG1, resType.ToString()).Replace(
                    ARG2, resNum.ToString()).Replace(
                    ARG3, errdata[0]);
                break;
            }
            return warnInfo;
        }

        private static List<WinAGIEventInfo> WarningsFromField(AGIResType resType, byte resNum, int warnings, string[] warndata) {
            List<WinAGIEventInfo> retval = [];
            WinAGIEventInfo warnInfo = new() {
                ResType = resType,
                ResNum = resNum,
                Line = -1,
                Type = EventType.ResourceWarning
            };
            switch (resType) {
            case AGIResType.Logic:
                // none
                break;
            case AGIResType.Picture:
                if ((warnings & 1) == 1) {
                    // missing EOP marker
                    warnInfo.ID = "RW05";
                    warnInfo.Text = EngineResources.RW05.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 2) == 2) {
                    // bad color
                    warnInfo.ID = "RW06";
                    warnInfo.Text = EngineResources.RW06.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 4) == 4) {
                    // bad cmd
                    warnInfo.ID = "RW07";
                    warnInfo.Text = EngineResources.RW07.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 8) == 8) {
                    // extra data
                    warnInfo.ID = "RW08";
                    warnInfo.Text = EngineResources.RW08.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 16) == 16) {
                    // extra data
                    warnInfo.ID = "RW09";
                    warnInfo.Text = EngineResources.RW09.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 32) == 32) {
                    // extra data
                    warnInfo.ID = "RW10";
                    warnInfo.Text = EngineResources.RW10.Replace(ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                break;
            case AGIResType.Sound:
                if ((warnings & 1) == 1) {
                    warnInfo.ID = "RW11";
                    warnInfo.Text = EngineResources.RW11.Replace(
                        ARG1, resNum.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                if ((warnings & 2) == 2) {
                    warnInfo.ID = "RW12";
                    warnInfo.Text = EngineResources.RW12.Replace(
                        ARG1, resNum.ToString()).Replace(
                        ARG2, warnings.ToString());
                    warnInfo.Module = warndata[0];
                    retval.Add(warnInfo);
                }
                break;
            case AGIResType.View:
                if ((warnings & 1) == 1) {
                    warnInfo.ID = "RW13";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[1].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW13.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 2) == 2) {
                    warnInfo.ID = "RW14";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[2].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW14.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 4) == 4) {
                    warnInfo.ID = "RW15";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[3].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW15.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 8) == 8) {
                    warnInfo.ID = "RW16";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[4].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW16.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 16) == 16) {
                    warnInfo.ID = "RW17";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[5].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW17.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]).Replace(
                            ARG3, data[++i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 32) == 32) {
                    warnInfo.ID = "RW18";
                    warnInfo.Module = warndata[0];
                    string[] data = warndata[6].Split('|');
                    for (int i = 1; i < data.Length; i++) {
                        warnInfo.Text = EngineResources.RW18.Replace(
                            ARG1, resNum.ToString()).Replace(
                            ARG2, data[i]).Replace(
                            ARG3, data[++i]);
                        retval.Add(warnInfo);
                    }
                }
                if ((warnings & 64) == 64) {
                    warnInfo.ID = "RW19";
                    warnInfo.Module = warndata[0];
                    warnInfo.Text = EngineResources.RW19.Replace(
                        ARG1, resNum.ToString());
                    retval.Add(warnInfo);
                }
                break;
            case AGIResType.Objects:
                warnInfo.Module = "OBJECT";
                if ((warnings & 1) == 1) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW20";
                    warnInfo.Text = EngineResources.RW20;
                    retval.Add(warnInfo);
                }
                if ((warnings & 2) == 2) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW21";
                    warnInfo.Text = EngineResources.RW21;
                    retval.Add(warnInfo);
                }
                break;
            case AGIResType.Words:
                warnInfo.Module = "WORDS.TOK";
                if ((warnings & 1) == 1) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW22";
                    warnInfo.Text = EngineResources.RW22;
                    retval.Add(warnInfo);
                }
                if ((warnings & 2) == 2) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW23";
                    warnInfo.Text = EngineResources.RW23;
                    retval.Add(warnInfo);
                }
                if ((warnings & 4) == 4) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW24";
                    warnInfo.Text = EngineResources.RW24;
                    retval.Add(warnInfo);
                }
                if ((warnings & 8) == 8) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW25";
                    warnInfo.Text = EngineResources.RW25;
                    retval.Add(warnInfo);
                }
                if ((warnings & 16) == 16) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW26";
                    warnInfo.Text = EngineResources.RW26;
                    retval.Add(warnInfo);
                }
                if ((warnings & 32) == 32) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW27";
                    warnInfo.Text = EngineResources.RW27;
                    retval.Add(warnInfo);
                }
                if ((warnings & 64) == 64) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW28";
                    warnInfo.Text = EngineResources.RW28;
                    retval.Add(warnInfo);
                }
                if ((warnings & 128) == 128) {
                    warnInfo.Type = EventType.ResourceWarning;
                    warnInfo.ID = "RW29";
                    warnInfo.Text = EngineResources.RW29.Replace(
                        ARG1, warndata[0]);
                    retval.Add(warnInfo);
                }
                break;
            case AGIResType.Globals:
                warnInfo.Type = EventType.ResourceWarning;
                switch (warnings) {
                case 1:
                    // invalid entries found
                    warnInfo.ID = "RW30";
                    warnInfo.Text = EngineResources.RW30;
                    break;
                }
                retval.Add(warnInfo);
                break;
            }
            return retval;
        }

        /// <summary>
        /// Expands a version 3 picture resource using Sierra's custom run length encoding.
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="fullsize"></param>
        /// <returns></returns>
        internal static byte[] DecompressPicture(byte[] originalData, int fullsize) {
            short posIn = 0;
            byte curComp, buffer = 0, curUncomp;
            bool offset = false;
            int tempCurPos = 0;
            byte[] expandedData = new byte[fullsize];

            // decompress the picture
            do {
                curComp = originalData[posIn++];
                if (offset) {
                    buffer += (byte)(curComp >> 4);
                    // extract uncompressed byte
                    curUncomp = buffer;
                    // shift buffer back
                    buffer = (byte)(curComp << 4);
                }
                else {
                    // byte is not compressed
                    curUncomp = curComp;
                }
                expandedData[tempCurPos++] = curUncomp;
                // check if byte sets or restores offset
                if (((curUncomp == 0xF0) || (curUncomp == 0xF2)) && (posIn < originalData.Length)) {
                    if (offset) {
                        // write rest of buffer byte
                        expandedData[tempCurPos++] = (byte)(buffer >> 4);
                        offset = false;
                    }
                    else {
                        curComp = originalData[posIn++];
                        expandedData[tempCurPos++] = (byte)(curComp >> 4);
                        // fill buffer
                        buffer = (byte)(curComp << 4);
                        offset = true;
                    }
                }
            }
            // continue until all original data has been read
            while (posIn < originalData.Length);
            return expandedData;
        }

        /// <summary>
        /// Checks all resource IDs looking for duplicates.
        /// </summary>
        /// <param name="agRes"></param>
        /// <returns>true if another resource in this game has same ID as agRes</returns>
        internal static bool NotUniqueID(AGIResource agRes) {
            string checkID = agRes.ID;

            // non-game resources always have unique IDs
            if (agRes.parent is null) {
                return false;
            }
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
        /// mirror is used to ensure mirrored cels include enough room
        /// for the flipped cel.
        /// </summary>
        /// <param name="cel"></param>
        /// <param name="mirror"></param>
        /// <returns></returns>
        internal static byte[] CompressedCel(Cel cel, bool mirror) {
            List<byte> tempRLE;
            byte height, width, chunkLength;
            byte[,] celData;
            AGIColorIndex transColor;
            byte chunkColor, nextColor;
            bool firstChunk;
            int mirrorCount = 0;
            int i, j;

            height = cel.Height;
            width = cel.Width;
            transColor = cel.TransColor;
            celData = cel.AllCelData;
            // assume one byte per pixel to start with
            // (include one byte for ending zero)
            tempRLE = [];
            for (j = 0; j < height; j++) {
                // first pixel
                chunkColor = celData[0, j];
                chunkLength = 1;
                firstChunk = true;
                // step through rest of pixels in this row
                for (i = 1; i < width; i++) {
                    nextColor = celData[i, j];
                    if ((nextColor != chunkColor) || (chunkLength == 0xF)) {
                        tempRLE.Add((byte)(chunkColor * 0x10 + chunkLength));
                        // if NOT first chunk or NOT transparent
                        if (!firstChunk || (chunkColor != (byte)transColor)) {
                            // increment mirorCount for any chunks
                            // after the first, and also for the first
                            // if it is NOT transparent color
                            mirrorCount++;
                        }
                        firstChunk = false;
                        chunkColor = nextColor;
                        chunkLength = 1;
                    }
                    else {
                        chunkLength++;
                    }
                }
                if (chunkColor != (byte)transColor) {
                    // last chunk is NOT transparent
                    tempRLE.Add((byte)(chunkColor * 0x10 + chunkLength));
                }
                // always Count last chunk for mirror
                mirrorCount++;
                // add zero to indicate end of row
                tempRLE.Add(0);
                mirrorCount++;
            }
            if (mirror) {
                // add zeros to make room for mirror loop
                while (tempRLE.Count < mirrorCount) {
                    tempRLE.Add(0);
                }
            }
            return tempRLE.ToArray();
        }

        /// <summary>
        /// This method creates a MIDI audio data stream from a PCjr formatted sound resource
        /// for playback.
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        internal static byte[] BuildMIDI(Sound sound) {
            int writeTrack = 0;
            int i, j;
            byte midinote;
            int freqdivisor;
            byte volume;
            int trackCount = 0, tickCount;
            long startPos, endPos;
            SoundData sndOut = new();

            if (!sound[0].Muted && sound[0].Notes.Count > 0) {
                trackCount = 1;
            }
            if (!sound[1].Muted && sound[1].Notes.Count > 0) {
                trackCount++;
            }
            if (!sound[2].Muted && sound[2].Notes.Count > 0) {
                trackCount++;
            }
            if (!sound[3].Muted && sound[3].Notes.Count > 0) {
                // add two tracks if noise is not muted
                // because the white noise and periodic noise
                // are written as two separate tracks
                trackCount += 2;
            }

            // header
            sndOut.WriteSndByte(77);  // "M"
            sndOut.WriteSndByte(84);  // "T"
            sndOut.WriteSndByte(104); // "h"
            sndOut.WriteSndByte(100); // "d"

            // remaining length of header
            sndOut.WriteSndLong(6);
            // track mode
            //   mode 0 = single track
            //   mode 1 = multiple tracks, all start at zero
            //   mode 2 = multiple tracks, independent start times
            sndOut.WriteSndWord(1);
            if (trackCount == 0) {
                // create an empty MIDI stream

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
                sndOut.WriteSndByte(0);
                //  set instrument status byte
                sndOut.WriteSndByte(0xC0);
                // instrument number
                sndOut.WriteSndByte(0);
                // add a slight delay note with no volume to end
                sndOut.WriteSndByte(0);
                sndOut.WriteSndByte(0x90);
                sndOut.WriteSndByte(60);
                sndOut.WriteSndByte(0);
                sndOut.WriteSndByte(16);
                sndOut.WriteSndByte(0x80);
                sndOut.WriteSndByte(60);
                sndOut.WriteSndByte(0);
                // add end of track info
                sndOut.WriteSndByte(0);
                sndOut.WriteSndByte(0xFF);
                sndOut.WriteSndByte(0x2F);
                sndOut.WriteSndByte(0x0);
                return sndOut.ToArray();
            }
            // track count
            sndOut.WriteSndWord(trackCount);
            // pulses per quarter note
            // (agi sound tick is 1/60 sec; each tick of an AGI note
            // is 1/60 of a second; by default, MIDI defines a whole
            // note as 2 seconds; therefore, a quarter note is 1/2
            // second, or 30 ticks
            sndOut.WriteSndWord(30);
            // sound tracks
            for (i = 0; i < 3; i++) {
                if (!sound[i].Muted && sound[i].Notes.Count > 0) {
                    sndOut.WriteSndByte(77);  // "M"
                    sndOut.WriteSndByte(84);  // "T"
                    sndOut.WriteSndByte(114); // "r"
                    sndOut.WriteSndByte(107); // "k"
                    // starting position for this track's data
                    startPos = sndOut.Position;
                    // place holder for data size
                    sndOut.WriteSndLong(0);
                    // track number
                    //     *********** i think this should be zero for all tracks
                    //     it's the value for the instrument setting
                    sndOut.WriteSndByte(0);
                    // instrument status byte
                    sndOut.WriteSndByte((byte)(0xC0 + writeTrack));
                    // instrument number
                    sndOut.WriteSndByte(sound[i].Instrument);
                    // add a slight delay note with no volume to start
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(4);
                    sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add notes from this track
                    for (j = 0; j <= sound[i].Notes.Count - 1; j++) {
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
                        if (sound[i].Notes[j].FreqDivisor > 0) {
                            midinote = (byte)(Math.Round((Math.Log10(111860 / (double)(sound[i].Notes[j].FreqDivisor)) / LOG10_1_12) - 36.5));
                            // in case note is too high,
                            if (midinote > 127) {
                                midinote = 127;
                            }
                            volume = (byte)(127 * (15 - sound[i].Notes[j].Attenuation) / 15);
                        }
                        else {
                            midinote = 0;
                            volume = 0;
                        }
                        // NOTE ON
                        sndOut.WriteSndByte(0);
                        sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                        sndOut.WriteSndByte(midinote);
                        sndOut.WriteSndByte(volume);
                        // NOTE OFF
                        sndOut.WriteSndDelta(sound[i].Notes[j].Duration);
                        sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                        sndOut.WriteSndByte(midinote);
                        sndOut.WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end of track
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(4);
                    sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add end of track info
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(0xFF);
                    sndOut.WriteSndByte(0x2F);
                    sndOut.WriteSndByte(0x0);
                    // save track end position
                    endPos = sndOut.Position;
                    // set cursor to start of track
                    sndOut.Position = startPos;
                    // add the track length
                    sndOut.WriteSndLong((int)(endPos - startPos) - 4);
                    sndOut.Position = endPos;
                }
                writeTrack++;
            }
            // seashore does a good job of imitating the white noise, with frequency
            // adjusted empirically
            // harpsichord does a good job of imitating the tone noise, with frequency
            // adjusted empirically
            if (!sound[3].Muted && sound[3].Notes.Count > 0) {
                // because there are two types of noise, use a different channel for each type
                // 0 means add tone, 1 means add white noise
                for (i = 0; i < 2; i++) {
                    // add track header
                    sndOut.WriteSndByte(77);  // "M"
                    sndOut.WriteSndByte(84);  // "T"
                    sndOut.WriteSndByte(114); // "r"
                    sndOut.WriteSndByte(107); // "k"
                    // store starting position for this track's data
                    startPos = sndOut.Position;
                    // place holder for chunklength
                    sndOut.WriteSndLong(0);
                    // track number
                    sndOut.WriteSndByte((byte)writeTrack);
                    // instrument
                    sndOut.WriteSndByte((byte)(0xC0 + writeTrack));
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
                        sndOut.WriteSndByte((byte)(0xB0 + writeTrack));
                        sndOut.WriteSndByte(7);
                        sndOut.WriteSndByte(127);
                        // set legato
                        sndOut.WriteSndByte(0);
                        sndOut.WriteSndByte((byte)(0xB0 + writeTrack));
                        sndOut.WriteSndByte(68);
                        sndOut.WriteSndByte(127);
                        break;
                    }
                    // add a slight delay note with no volume to start the track
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(4);
                    sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // reset tick counter (used in case of need to borrow track 3 freq)
                    tickCount = 0;
                    for (j = 0; j < sound[3].Notes.Count; j++) {
                        // add duration to tickcount
                        tickCount += sound[3].Notes[j].Duration;
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
                        if ((sound[3].Notes[j].FreqDivisor & 4) == 4 * i) {
                            if ((sound[3].Notes[j].FreqDivisor & 3) == 3) {
                                // get frequency from channel 3
                                freqdivisor = GetTrack3Freq(sound[2], tickCount);
                            }
                            else {
                                // get frequency from bits 0 and 1
                                freqdivisor = (int)(2330.4296875 / (1 << (sound[3].Notes[j].FreqDivisor & 3)));
                            }
                            if ((sound[3].Notes[j].FreqDivisor & 4) == 4) {
                                // for white noise, 96 is my best guess to imitate noise
                                // BUT... 96 causes some notes to come out negative;
                                // 80 is max Value that ensures all AGI freq values convert
                                // to positive MIDI note values
                                midinote = (byte)((Math.Log10(freqdivisor) / LOG10_1_12) - 80);
                            }
                            else {
                                // for periodic noise, 64 is my best guess to imitate noise
                                midinote = (byte)((Math.Log10(freqdivisor) / LOG10_1_12) - 64);
                            }
                            volume = (byte)(127 * (15 - sound[3].Notes[j].Attenuation) / 15);
                        }
                        else {
                            // write a blank note as a placeholder
                            midinote = 0;
                            volume = 0;
                        }

                        // NOTE ON
                        sndOut.WriteSndByte(0);
                        sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                        sndOut.WriteSndByte(midinote);
                        sndOut.WriteSndByte(volume);
                        // NOTE OFF
                        sndOut.WriteSndDelta(sound[3].Notes[j].Duration);
                        sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                        sndOut.WriteSndByte(midinote);
                        sndOut.WriteSndByte(0);
                    }
                    // add a slight delay note with no volume to end
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte((byte)(0x90 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(4);
                    sndOut.WriteSndByte((byte)(0x80 + writeTrack));
                    sndOut.WriteSndByte(60);
                    sndOut.WriteSndByte(0);
                    // add end of track data
                    sndOut.WriteSndByte(0);
                    sndOut.WriteSndByte(0xFF);
                    sndOut.WriteSndByte(0x2F);
                    sndOut.WriteSndByte(0x0);
                    // save ending position
                    endPos = sndOut.Position;
                    // go back to start of track, and add track length
                    sndOut.Position = startPos;
                    sndOut.WriteSndLong((int)(endPos - startPos - 4));
                    sndOut.Position = endPos;
                    writeTrack++;
                }
            }
            return sndOut.ToArray();
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
            int inPos, outPos;
            byte[] midiIn, midiOut;
            int tickcount = 0, time;
            byte bytedata, cmd = 0, channel = 0;
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
            outPos = 22;
            inPos = 2;
            do {
                // get next byte of input data
                bytedata = midiIn[inPos++];
                if (inPos >= midiIn.Length) {
                    break;
                }
                // add it to output
                midiOut[outPos++] = bytedata;
                // time is always first input; it is supposed to be a delta value
                // but it appears that agi used it as an absolute value; so if time
                // is greater than 0x7F, it will cause a hiccup in modern midi
                // players
                time = bytedata;
                if ((bytedata & 0x80) == 0x80) {
                    // treat 0xFC as an end mark, even if found in time position
                    // 0xF8 also appears to cause an end
                    if ((bytedata == 0xFC) || (bytedata == 0xF8)) {
                        // backup one to cancel this byte
                        outPos--;
                        break;
                    }
                    // convert into two-byte time value (means expanding
                    // array size by one byte)
                    Array.Resize(ref midiOut, midiOut.Length + 1);
                    midiOut[outPos - 1] = 129;
                    midiOut[outPos++] = (byte)(bytedata & 0x7F);
                }
                // next byte is a command (>=0x80) OR a running status (<0x80)
                bytedata = midiIn[inPos++];
                if (inPos >= midiIn.Length) {
                    break;
                }
                if (bytedata >= 0x80) {
                    cmd = (byte)(bytedata / 16);
                    channel = (byte)(bytedata & 0xF);
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
                    if (bytedata == 0xFC) {
                        // back up so last time value gets overwritten
                        outPos--;
                        break;
                    }
                    // assume any other 0xF command is OK;
                    // add it to output
                    midiOut[outPos++] = bytedata;
                }
                else {
                    // it's a running status - back up so next byte is event data
                    inPos--;
                }
                // increment tick count
                tickcount += time;
                // next comes event data; number of data points depends on command
                switch (cmd) {
                case 8:
                case 9:
                case 0xA:
                case 0xB:
                case 0xE:
                    // these all take two bytes of data
                    midiOut[outPos++] = midiIn[inPos++];
                    if (inPos >= midiIn.Length) {
                        continue;
                    }
                    midiOut[outPos++] = midiIn[inPos++];
                    if (inPos >= midiIn.Length) {
                        continue;
                    }
                    break;
                case 0xC:
                case 0xD:
                    // only one byte for program change, channel pressure
                    midiOut[outPos++] = midiIn[inPos++];
                    if (inPos >= midiIn.Length) {
                        continue;
                    }
                    break;
                case 0xF:
                    // system messages
                    // depends on submsg (channel value) - only expected value is 0xC
                    switch (channel) {
                    case 0:
                        // variable; go until 0xF7 found
                        do {
                            midiOut[outPos++] = midiIn[inPos++];
                            if (inPos >= midiIn.Length) {
                                break;
                            }
                        }
                        while (bytedata != 0xF7);
                        if (inPos >= midiIn.Length) {
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
                        outPos--;
                        continue;
                    case 2:
                        // song position
                        // this uses two bytes
                        midiOut[outPos++] = midiIn[inPos++];
                        if (inPos >= midiIn.Length) {
                            continue;
                        }
                        midiOut[outPos++] = midiIn[inPos++];
                        if (inPos >= midiIn.Length) {
                            continue;
                        }
                        break;
                    case 3:
                        // song select
                        // this uses one byte
                        midiOut[outPos++] = midiIn[inPos++];
                        if (inPos >= midiIn.Length) {
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
            while (inPos < midiIn.Length);

            // resize output array to remove any extra bytes
            // allowing room for end of track data
            if (outPos + 4 < midiOut.Length) {
                Array.Resize(ref midiOut, outPos + 4);
            }
            // add end of track data
            midiOut[outPos] = 0;
            midiOut[outPos + 1] = 0xFF;
            midiOut[outPos + 2] = 0x2F;
            midiOut[outPos + 3] = 0;
            outPos += 4;
            // update size of track data (total length - 22)
            midiOut[18] = (byte)(((outPos - 22) >> 24));
            midiOut[19] = (byte)(((outPos - 22) >> 16) & 0xFF);
            midiOut[20] = (byte)(((outPos - 22) >> 8) & 0xFF);
            midiOut[21] = (byte)((outPos - 22) & 0xFF);
            // convert ticks to seconds
            Length = (double)tickcount / 60;
            return midiOut;
        }

        /// <summary>
        /// This method creates a data stream from a native Apple IIgs PCM sound for playback on
        /// modern WAV devices.
        /// </summary>
        /// <param name="SoundIn"></param>
        /// <returns></returns>
        internal static byte[] BuildIIgsPCM(Sound SoundIn) {
            // This method creates just the data stream, not the header. The header is
            // only needed when creating a WAV file.
            // IIgs WAV sounds are 8000 sample rate, 8bit, one channel.
            int datasize;
            byte[] sounddata, output;

            sounddata = SoundIn.Data;
            // size of sound data is total resource size, minus the PCM header that
            // AGI includes in the sound resource (54 bytes) plus four bytes for an
            // end of stream marker (for a net of -50 bytes).
            datasize = sounddata.Length - 50;
            output = new byte[datasize];
            // copy data from sound resource
            int pos = 0;
            for (int i = 54; i < sounddata.Length; i++) {
                output[pos++] = sounddata[i];
            }
            // add four bytes of silence as an end marker
            output[^1] = 127;
            output[^2] = 127;
            output[^3] = 127;
            output[^4] = 127;
            return output;
        }

        /// <summary>
        /// Returns the track3 frequency divisor value at desired position. Used 
        /// by noise channel when building MIDI data.
        /// </summary>
        /// <param name="track3"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static int GetTrack3Freq(Track track3, int target) {
            // Have to step through track three until the same point in time is found
            int tickcount = 0;

            for (int i = 0; i < track3.Notes.Count; i++) {
                tickcount += track3.Notes[i].Duration;
                if (tickcount >= target) {
                    // this is the frequency we want
                    return track3.Notes[i].FreqDivisor;
                }
            }
            // if nothing found, return 0
            return 0;
        }
        #endregion
    }
}
