using System;
using System.Collections.Generic;
using System.IO;

namespace WinAGI.Common {
    public static class LZW {

        public static byte[] GifLZW(byte[] imagedata) {
            // used by picture and view export functions for creating GIFs
            // these values based on 4 bit colorvalues (0-15) and
            // standard GIF LZW compression format
            const int MaxCode = 4095;
            const int ClearCode = 16;
            const int EndCode = 17;
            const int FirstFreeCode = 18;
            const int InitialCodeSize = 5;

            // use a memory stream with a custom bitwriter to build the output
            using MemoryStream ms = new(Math.Max(16, imagedata.Length));
            BitWriter bw = new(ms);

            int codeSize = InitialCodeSize;
            int nextCode = FirstFreeCode;
            Dictionary<int, int> dictionary = new(4096);

            // send 'clear' code to output
            bw.WriteBits(ClearCode, codeSize);

            int currentCode = imagedata[0];

            // NOW BEGIN THE COMPRESSION
            for (int i = 1; i < imagedata.Length; i++) {
                // get next byte
                int nextByte = imagedata[i];
                // add to current string to check against table
                int key = currentCode << 8 | nextByte;
                if (dictionary.TryGetValue(key, out int existingCode)) {
                    // key is already in the table; 
                    // update current code and get next char
                    currentCode = existingCode;
                }
                else {
                    // key does not exist, so add current code to output
                    bw.WriteBits(currentCode, codeSize);

                    if (nextCode > MaxCode) {
                        // too big- clear everything; reset to initial codesize

                        // output clear code (MUST USE CURRENT CODESIZE!)
                        bw.WriteBits(ClearCode, codeSize);
                        // reset code size and code table
                        codeSize = InitialCodeSize;
                        dictionary.Clear();
                        nextCode = FirstFreeCode;
                    }
                    else {
                        if (nextCode == 1 << codeSize) {
                            codeSize++;
                        }
                        // add code for checkstring
                        dictionary[key] = nextCode++;
                    }
                    // reset current string
                    currentCode = nextByte;
                }
            }
            // output last code
            bw.WriteBits(currentCode, codeSize);
            // output an 'end' code
            bw.WriteBits(EndCode, codeSize);
            // flush the bitwriter
            bw.Flush();

            // convert memory stream, and return
            return ms.ToArray();
        }

        private class BitWriter {
            private readonly Stream stream;
            private int bitCount;
            private uint buffer;

            public BitWriter(Stream stream) {
                this.stream = stream;
                buffer = 0;
                bitCount = 0;
            }

            public void WriteBits(int codevalue, int codesize) {
                // adds this codevalue to the output stream,

                // as a reminder, we are using LSB packing:
                //       byte4      |    byte3      |    byte2      |    byte2      |    byte1      |    byte0
                // ... 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
                //     ... 8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
                //        |    code4        |    code3        |    code2        |    code1        |    code0

                // add code to the buffer, shifting as needed
                buffer |= (uint)codevalue << bitCount;
                // increment bit Count
                bitCount += codesize;
                // if there are at least eight bits in the buffer, we need to pull off
                // the byte, and add it to output
                while (bitCount >= 8) {
                    // output get lower 8 bits to stream
                    stream.WriteByte((byte)(buffer & 0xFF));
                    // shift buffer right by 8 bits
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
                buffer = 0;
                bitCount = 0;
            }
        }
    }
}
