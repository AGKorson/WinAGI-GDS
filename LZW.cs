using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Common {
    public static class LZW {
        static ushort intNextCode, intCurCodeSize;
        static string[] strCodeValues;
        static byte[] bytCompData;
        static int lngBitBuffer;
        static ushort intBitCount;
        static int lngInPos, lngOutPos;
        static ushort intClearCode, intEndCode;

        public static byte[] GifLZW(ref byte[] bytCelData) {
            //used by picture and view export functions for creating GIFs
            int lngLen;
            string wSTRING, wCHAR;
            short curCode, oldCode;
            bool blnCleared = false;
            ushort i, intCodeSize;
            // for sixteen colors, initial code size should be 5
            intCodeSize = 5;
            //make output byte array same size; after compression, shrink it down to min size
            bytCompData = new byte[bytCelData.Length];
            //reset everything
            lngInPos = 0;
            lngOutPos = 0;
            intCurCodeSize = intCodeSize;
            intClearCode = (ushort)(1 << (intCodeSize - 1)); // 2 ^ (intCodeSize - 1)

            //reserve 'end' code (clear code +1)
            intEndCode = (ushort)(intClearCode + 1);
            lngBitBuffer = 0;
            intBitCount = 0;
            //send 'clear' code to output
            WriteCode(intClearCode);
            //set pointer to next available code to end code +1
            intNextCode = (ushort)(intEndCode + 1);
            //reset code table
            strCodeValues = new string[(1 << intCurCodeSize) - 1]; //2 ^ intCurCodeSize - 1

            //prepopulate with regular characters
            for (i = 0; i < intClearCode; i++) {
                strCodeValues[i] = ((char)i).ToString();
            }
            //NOW BEGIN THE COMPRESSION
            //get first character
            wSTRING = ((char)bytCelData[0]).ToString();
            lngInPos = 1;
            lngLen = bytCelData.Length;
            //first code is just the character value of first byte
            oldCode = bytCelData[0];

            while (lngInPos < lngLen) {
                //get next character
                wCHAR = ((char)bytCelData[lngInPos]).ToString();
                lngInPos++;
                //if the combination of wSTRING+wCHAR is already in the table, then
                //combine them to new wSTRING Value, and get the next character
                //if not, add the code for current sSTRING to output, add new entry to table for wSTRING+wCHAR
                //and reset current wSTRING to single wCHAR
                curCode = FindCodeInTable(wSTRING + wCHAR);
                if (curCode != -1) {
                    wSTRING += wCHAR;
                    oldCode = curCode;
                }
                else {
                    //output code for wSTRING
                    WriteCode((ushort)oldCode);
                    //add new entry for wSTRING+wCHAR:
                    //need to check if table is full, first
                    if (intNextCode == 1 << intCurCodeSize) { // 2 ^ intCurCodeSize
                                                              //increase size IF room (Max codesize is 12 bits)
                        if (intCurCodeSize < 12) {
                            intCurCodeSize++;
                            Array.Resize(ref strCodeValues, 1 << intCurCodeSize);// 2 ^ intCurCodeSize
                        }
                        else {
                            //too big- clear everything; reset to initial code width
                            blnCleared = true;
                            //output clear code
                            WriteCode(intClearCode);
                            //reset code size and code table
                            intCurCodeSize = intCodeSize;
                            strCodeValues = new string[1 << intCurCodeSize]; //(2 ^ intCurCodeSize)
                            for (i = 0; i < intClearCode; i++) {
                                strCodeValues[i] = ((char)i).ToString();
                            }
                            intNextCode = (ushort)(intEndCode + 1);
                        }
                    }
                    //if not resetting,
                    if (!blnCleared) {
                        //now add new entry for wSTRING+wCHAR
                        strCodeValues[intNextCode] = wSTRING + wCHAR;
                        intNextCode++;
                    }
                    else {
                        //if cleared, write out next char
                        //WriteCode(curCode);
                        //reset
                        blnCleared = false;
                    }
                    //reset wSTRING
                    wSTRING = wCHAR;
                    oldCode = (byte)wCHAR[0];
                }
            }
            //output last string code (wSTRING)
            WriteCode((ushort)oldCode);
            //output an 'end' code
            WriteCode(intEndCode);
            //if any bits left in buffer, need to add them
            if (intBitCount > 0) {
                //write out last byte
                bytCompData[lngOutPos] = (byte)lngBitBuffer;
            }
            //resize array to final size
            Array.Resize(ref bytCompData, lngOutPos + 1);
            //if no errors, return compressed array
            return bytCompData;
        }
        
        static short FindCodeInTable(string strCodeVal) {
            //used by GifLZW
            //searches the code table, and returns the code number if strCodeVal is found
            //otherwise returns -1
            short i, intMax;
            intMax = (short)(intNextCode - 1);
            for (i = 0; i <= intMax; i++) {
                if (strCodeValues[i] == strCodeVal) {
                    return i;
                }
            }
            //not found
            return -1;
        }
        
        static void WriteCode(ushort CodeValue) {
            //adds this codevalue to the output buffer,
            //and writes bytes to the buffer when enough bits are in the buffer

            //if necessary, this subroutine will also expand the output data array
            //(I think we can mathematically prove it's not possible for it to get bigger,
            //at least if we start with 9bit codes; but just in case, leave option
            //to expand size of output array if necessary)

            //as a reminder, we are using LSB packing:
            //       byte4           byte3           byte2           byte2           byte1           byte0
            // ... 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
            //     ... 8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
            //             code4             code3             code2             code1             code0

            int lngCodeIn, lngByteOut;
            //convert code to long
            lngCodeIn = CodeValue;

            //if there are already bits in the buffer, shift the code before adding it to the buffer
            if (intBitCount != 0) {
                //lngCodeIn = SHL(lngCodeIn, intBitCount)
                lngCodeIn <<= intBitCount;
            }
            //add code to the buffer
            lngBitBuffer |= lngCodeIn;
            //increment bit Count
            intBitCount += intCurCodeSize;
            //if there are at least eight bits in the buffer, we need to pull off the byte, and add it to output
            while (intBitCount >= 8) {
                //get lower 8 bits
                lngByteOut = lngBitBuffer & 0xFF;
                //convert to a byte, and store in output buffer
                if (lngOutPos >= bytCompData.Length) {
                    Array.Resize(ref bytCompData, bytCompData.Length + 1024);
                }
                bytCompData[lngOutPos] = (byte)lngByteOut;
                //increment output pointer
                lngOutPos++;
                //shift buffer right by 8 positions
                lngBitBuffer >>= 8; // SHR(lngBitBuffer, 8)
                                    //decrement bit Count
                intBitCount -= 8;
            }
        }
    }
}
