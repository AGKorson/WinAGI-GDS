using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Common {
    public static class LZW {

        public static byte[] GifLZW(byte[] bytCelData) {
            // used by picture and view export functions for creating GIFs

            string currentString = "";
            // for sixteen colors, initial code size should be 5
            int initCodeSize = 5;
            int codeSize = initCodeSize;
            int clearCode = 16;
            int endCode = 17; // reserve 'end' code (clear code +1)
            int nextCode = 18; // set pointer to next available code to end code +1
            Dictionary<string, int> dictionary = [];
            // prepopulate with regular characters
            for (int i = 0; i < 16; i++) {
                dictionary.Add(((char)i).ToString(), i);
            }

            using MemoryStream ms = new();
            BitWriter bw = new(ms);

            //send 'clear' code to output
            bw.WriteCode(clearCode, codeSize);
            
            // NOW BEGIN THE COMPRESSION
            for (int i = 0; i < bytCelData.Length; i++) {
                //get next character
                char currentChar = (char)bytCelData[i];
                // add to current string to check against table
                string checkstring = currentString + currentChar;
                if (dictionary.ContainsKey(checkstring)) {
                    // checkString is already in the table; 
                    // update current string and get next char
                    currentString = checkstring;
                }
                else {
                    // checkstring does not exist, so add code
                    // for current string to output
                    bw.WriteCode(dictionary[currentString], codeSize);

                    if (nextCode == 4096) {
                        // too big- clear everything; reset to initial codesize

                        // output clear code (MUST USE CURRENT CODESIZE!)
                        bw.WriteCode(clearCode, codeSize);
                        // reset code size and code table
                        codeSize = initCodeSize;
                        dictionary = [];
                        // prepopulate with regular characters
                        for (int j = 0; j < 16; j++) {
                            dictionary.Add(((char)j).ToString(), j);
                        }
                        nextCode = (ushort)(endCode + 1);
                    }
                    else {
                        if (nextCode == 1 << codeSize) {
                            codeSize++;
                        }
                        // add code for checkstring
                        dictionary.Add(checkstring, nextCode++);
                    }
                    // reset current string
                    currentString = currentChar.ToString();
                }
            }
            //output last string code
            bw.WriteCode(dictionary[currentString], codeSize);
            //output an 'end' code
            bw.WriteCode(endCode, codeSize);
            // flush the bitwriter
            bw.Flush();

            // convert memory stream, and return
            return ms.ToArray();
        }

        private class BitWriter {
            private readonly Stream stream;
            private int bitCount;
            private int buffer;

            public BitWriter(Stream stream) {
                this.stream = stream;
                buffer = 0;
            }

            public void WriteCode(int codevalue, int codesize) {
                // adds this codevalue to the output stream,

                // as a reminder, we are using LSB packing:
                //       byte4      |    byte3      |    byte2      |    byte2      |    byte1      |    byte0
                // ... 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
                //     ... 8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
                //        |    code4        |    code3        |    code2        |    code1        |    code0

                // if there are already bits in the buffer, shift the code before adding it to the buffer
                if (bitCount != 0) {
                    //lngCodeIn = SHL(lngCodeIn, intBitCount)
                    codevalue <<= bitCount;
                }
                // add code to the buffer
                buffer |= codevalue;
                // increment bit Count
                bitCount += codesize;
                // if there are at least eight bits in the buffer, we need to pull off
                // the byte, and add it to output
                while (bitCount >= 8) {
                    // output get lower 8 bits to stream
                    stream.WriteByte((byte)(buffer & 0xFF));
                    //shift buffer right by 8 bits
                    buffer >>= 8;
                    // decrement bit Count
                    bitCount -= 8;
                }
            }

            public void Flush() {
                // if anything left in buffer, add it to the stream
                if (bitCount > 0) {
                    stream.WriteByte((byte)buffer);
                }
                else {
                    stream.WriteByte((byte)buffer);
                }
            }
        }
    }

public static class LZWai {
        public static byte[] GifLZW(byte[] input) {
            int maxTableSize = 4096;
            int codeSize = 5;
            int clearCode = 16;
            int endCode = 17;
            int nextCode = 18;

            Dictionary<string, int> dictionary = [];
            for (int i = 0; i < 16; i++) {
                dictionary.Add(((char)i).ToString(), i);
            }

            string currentString = string.Empty;
            List<byte> output = [];

            byte[] tmp1, tmp2;
            using (MemoryStream ms = new MemoryStream()) {
                BitWriter bitWriter = new BitWriter(ms);
                bitWriter.WriteBits(clearCode, codeSize);

                foreach (byte b in input) {
                    char currentChar = (char)b;
                    string combinedString = currentString + currentChar;

                    if (dictionary.ContainsKey(combinedString)) {
                        currentString = combinedString;
                    }
                    else {
                        bitWriter.WriteBits(dictionary[currentString], codeSize);
                        if (nextCode < maxTableSize) {
                            if (nextCode == (1 << codeSize)) {
                                codeSize++;
                            }
                            dictionary.Add(combinedString, nextCode++);
                        }

                        currentString = currentChar.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(currentString)) {
                    bitWriter.WriteBits(dictionary[currentString], codeSize);
                }

                bitWriter.WriteBits(endCode, codeSize);
                bitWriter.Flush();

                output.AddRange(ms.ToArray());
                tmp1 = ms.ToArray();
            }
            tmp2 = output.ToArray();
            return output.ToArray();
        }

        private class BitWriter {
            private readonly Stream _stream;
            private int _currentByte;
            private int _bitPosition;

            private int _currentByte1;
            private int _bitPosition1;
            private int buffer;
            public BitWriter(Stream stream) {
                _stream = stream;
                _currentByte = 0;
                _bitPosition = 0;

                _currentByte1 = 0;
                _bitPosition1 = 0;
                buffer = 0;
            }

            public void WriteBits(int value, int bitLength) {
                int val1 = value;
                if (_bitPosition1 != 0) {
                    val1 <<= _bitPosition1;
                }
                buffer |= val1;
                _bitPosition1 += bitLength;
                while (_bitPosition1 >= 8) {
                    _currentByte1 = buffer & 0xFF;
                    //_stream.WriteByte((byte)_currentByte1);
                    buffer >>= 8;
                    _bitPosition1 -= 8;
                }

                for (int i = 0; i < bitLength; i++) {
                    if ((_bitPosition & 7) == 0 && _bitPosition > 0) {
                        _stream.WriteByte((byte)_currentByte);
                        _currentByte = 0;
                    }

                    _currentByte |= ((value >> i) & 1) << (_bitPosition & 7);
                    _bitPosition++;
                }
            }

            public void Flush() {
                if (_bitPosition > 0) {
                    _stream.WriteByte((byte)_currentByte);
                }
            }
        }
    }

}
