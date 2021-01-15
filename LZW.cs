using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI_GDS
{
  public static class LZW
  {
    static ushort intNextCode, intCurCodeSize;
    static string[] strCodeValues;
    static byte[] bytCompData;
    static int lngBitBuffer;
    static ushort intBitCount;
    static byte[] bytCelData;
    static int lngInPos, lngOutPos;
    static ushort[] intAppendChar, intCodePrefix;
    static ushort intClearCode, intEndCode;
    static bool blnMaxedOut;

    public static void ExpandFile(string strFileIn, ushort intCodeSize = 5)
    {
      //currently not used anywhere

      //  short intFreeFile;
      //  string strFileOut;
      //
      //  On Error GoTo ErrHandler
      //
      ////////        strFileIn = "C:\Users\d3m294\Documents\output.dat"
      //
      //  //open input file
      //  using FileStream fsIn = new FileStream(strFileIn);
      //  //copy data to input array
      //  bytCelData = new byte[fsIn.Length];
      //  bytCelData = fsIn.Read();
      //  //close input file
      //  fsIn.Dispose();
      //
      //  lngInPos = 0;
      //  lngOutPos = 0;
      //
      //  if (ExpandLZW(intCodeSize)) {
      //    //resize output buffer to match actual amount of data
      //    Array.Resize(ref bytCompData, lngOutPos);
      //    //store the data in a temporary output file
      //    strFileOut = Path.GetTempFileName();
      //    try
      //    {
      //      if (File.Exists(strFileOut)) {
      //      File.Delete(strFileOut);
      //      intFreeFile = FreeFile()
      //      Open strFileOut For Binary;
      //      Put #intFreeFile, 1, bytCompData()
      //      Close #intFreeFile
      //    }
      //    catch (Exception)
      //    {
      //      throw;
      //    }
      //  } else {
      //    throw new Exception("error in compress function :-(");
      //  }
    }
    public static byte[] GifLZW(ref byte[] bytCelData)
    {
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
      strCodeValues = new string[1 << intCurCodeSize - 1]; //2 ^ intCurCodeSize - 1

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

      while (lngInPos < lngLen) // Until lngInPos > lngLen
                                //get next character
      {
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
        } else {
          //output code for wSTRING
          WriteCode((ushort)oldCode);
          //add new entry for wSTRING+wCHAR:
          //need to check if table is full, first
          if (intNextCode == 1 << intCurCodeSize) { // 2 ^ intCurCodeSize
                                                    //increase size IF room (Max codesize is 12 bits)
            if (intCurCodeSize < 12) {
              intCurCodeSize++;
              Array.Resize(ref strCodeValues, 1 << intCurCodeSize);// 2 ^ intCurCodeSize
            } else {
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
          } else {
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
    static short FindCodeInTable(string strCodeVal)
    {
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
    static void WriteCode(ushort CodeValue)
    {
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
      lngBitBuffer = lngBitBuffer | lngCodeIn;
      //increment bit Count
      intBitCount = (ushort)(intBitCount + intCurCodeSize);
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
    public static bool ExpandLZW(short intCodeSize)
    {
      //only used by ExpandFile
      short intCode, intAppendCode;
      //reset everything
      intCurCodeSize = (ushort)intCodeSize;
      intClearCode = (ushort)(1 << (intCodeSize - 1)); //2 ^ (intCodeSize - 1)
                                                       //reserve 'end' code (clear code +1)
      intEndCode = (ushort)(intClearCode + 1);
      //set pointer for next available code
      intNextCode = (ushort)(intEndCode + 1);
      lngBitBuffer = 0;
      intBitCount = 0;
      //initialize code tables (values up to clear code are not needed in the tables)
      intAppendChar = new ushort[4096 - 1 << (intCodeSize - 1) + 1]; //4095 - 2 ^ (intCodeSize - 1) + 1)
      intCodePrefix = new ushort[4096 - 1 << (intCodeSize - 1) + 1]; //(4095 - 2 ^ (intCodeSize - 1) + 1)
                                                                     //intialize output buffer
      bytCompData = new byte[256];
      //get first code
      intCode = GetNextCode();
      //verify it's a clear code
      if (intCode != intClearCode) {
        return false;
      }
      //get next code (first code of actual data)
      intCode = GetNextCode();
      //write it to output
      WriteByte((byte)intCode);
      //store this byte as prefix for first code after the 'end' code
      intCodePrefix[intNextCode - intEndCode - 1] = (ushort)intCode;
      //increment nextcode pointer
      intNextCode++;
      //now read off codes, one at a time
      do {
        intCode = GetNextCode();
        //if it's an end code, exit
        if (intCode == intEndCode) {
          return true;
        }

        //if it's a clear code
        if (intCode == intClearCode) {
          //need to reset
          blnMaxedOut = false;
          intCurCodeSize = (ushort)intCodeSize;
          intAppendChar = new ushort[4096 - 1 << (intCodeSize - 1) + 1];
          intCodePrefix = new ushort[4096 - 1 << (intCodeSize - 1) + 1];
          intNextCode = (ushort)(intEndCode + 1);
          //start over
          intCode = GetNextCode();
          //write it to output
          WriteByte((byte)intCode);
          //store this byte as prefix for first code after the 'end' code
          intCodePrefix[intNextCode - intEndCode - 1] = (ushort)intCode;
          //increment nextcode pointer
          intNextCode++;
        } else {
          //if it is a base char,
          if (intCode < intEndCode) {
            //just output it to output stream
            WriteByte((byte)intCode);
            intAppendCode = intCode;
          } else {
            //convert code to string of bytes and write them to output
            //return Value is first character of string, which is needed
            //to build the table entry
            intAppendCode = (short)(ushort)OutputCode(intCode);
          }
          //store this byte as prefix for next code
          intCodePrefix[intNextCode - intEndCode - 1] = (ushort)intCode;
          //add this code as append char to previous code
          intAppendChar[intNextCode - intEndCode - 2] = (ushort)intAppendCode;
          //increment nextcode pointer
          intNextCode++;
          if (intNextCode == (1 << intCurCodeSize) + 1) {
            //need to assess potential reset here?
            //if already at 12 bits
            if (intCurCodeSize == 12) {
              //tell compiler no more codes until a reset comes
              blnMaxedOut = true;
              //for now, we don't do anything, as we expect the
              //compressor to properly reset before we have this problem
            } else {
              intCurCodeSize++;
            }
          }
        }
      } while (lngInPos < bytCelData.Length); // Until lngInPos > UBound(bytCelData)
      return true;
    }
    static short OutputCode(short intCodeIn)
    {
      //only used by ExpandLZW
      byte[] bytOutput;
      short intPos;
      short intCode, i;
      //intCode should never be greater than next code Value;
      //need to recurse through code table until we build the completed string of output characters
      bytOutput = new byte[1];
      intCode = intCodeIn;
      do {
        //expand output buffer by one byte
        Array.Resize(ref bytOutput, bytOutput.Length + 1);
        //move everything over one space
        for (i = (short)(bytOutput.Length - 1); i >= 1; i--) {
          bytOutput[i] = bytOutput[i - 1];
        }
        //get prefix code
        intCode = (short)intCodePrefix[intCode - intEndCode - 1];
        if (intCode < intEndCode) {
          //add this code
          bytOutput[0] = (byte)intCode;
        } else {
          //add the prefix
          bytOutput[0] = (byte)intAppendChar[intCode - intEndCode - 1];
        }
      }
      while (intCode >= intEndCode); // Until intCode < intEndCode
      if (intCodeIn == intNextCode - 1) {
        //end char is same as start char
        bytOutput[bytOutput.Length - 1] = bytOutput[0];
      } else {
        //add append char for input code
        bytOutput[bytOutput.Length - 1] = (byte)intAppendChar[intCodeIn - intEndCode - 1];
      }
      //now output the bytes
      for (i = 0; i < bytOutput.Length; i++) {
        WriteByte(bytOutput[i]);
      }
      //return first char so it can be used in adding table entries
      return bytOutput[0];
    }
    static void WriteByte(byte ByteVal)
    {
      //used by ExpandLZW and OutputCode
      //make sure output buffer has room; if not, expand it by 256 bytes
      if (lngOutPos >= bytCompData.Length) {
        Array.Resize(ref bytCompData, bytCompData.Length + 256);
      }
      //add the byte
      bytCompData[lngOutPos] = ByteVal;
      //increment pointer
      lngOutPos++;
    }
    public static short GetNextCode()
    {
      short intCodeMask;
      int lngNextByte;
      //only used by ExpandLZW
      //add bits to the buffer until enough are added to read out the next code
      while (intBitCount < intCurCodeSize) { // Until intBitCount >= intCurCodeSize
                                             //get next byte from input and shift it to end of current bitbuffer
        lngNextByte = bytCelData[lngInPos] << intBitCount; //SHL(CLng(bytCelData(lngInPos)), intBitCount)
                                                           //increment pointer
        lngInPos++;
        //add byte to buffer
        lngBitBuffer = lngBitBuffer | lngNextByte;
        //increment bit counter
        intBitCount += 8;
      }
      //now extract off the code
      intCodeMask = (short)(1 << intCurCodeSize - 1);
      short retval = (short)(lngBitBuffer & intCodeMask);
      //shift buffer to right
      lngBitBuffer = lngBitBuffer >> intCurCodeSize; //SHR(lngBitBuffer, intCurCodeSize)
                                                     //decrement bit Count
      intBitCount = (ushort)(intBitCount - intCurCodeSize);
      return retval;
    }
  }
}
