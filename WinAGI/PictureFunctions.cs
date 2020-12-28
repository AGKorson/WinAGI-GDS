using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public enum CornerDirection
  {
    cdX,
    cdY,
  }
  public static partial class WinAGI
  {
    //picture resource global variables
    private static byte[] VisBlankData = new byte[26880];
    private static byte[] PriBlankData = new byte[26880];
    private static byte[] VisBuildData, PriBuildData;
    private static byte[] agPicData;
    //private static int lngPos;
    private static int lngEndPos;
    private static short bytIn;
    private static PenStatus CurrentPen, SavePen;
    private static int[] Queue = new int[26880];
    private static byte[] CircleData = new byte[64];
    private static bool InitPlotData;

  internal static PenStatus GetToolStatus()
    {
      //NOTE: this method should ONLY be called
      //by the form that just completed a buildbmp call
      //otherwise, the results are meaningless
      return SavePen;
    }
    internal static int BuildBMPs(byte[] VisData, byte[] PriData, byte[] bytPicData, int EndPos, int StatusPos) 
    {
      return -1;
      /*
Public Function BuildBMPs(VisData() As Byte, PriData() As Byte, bytPicData() As Byte, ByVal EndPos As Long, ByVal StatusPos As Long) As Long
  //converts the data extracted from the picture resource into
  //a 256 bit color DIBitmap
  
  //return 0 if successful, no errors/warnings
  // non-zero for error/warning:
//////  //  -1 = error- can//t build the bitmap
  //  1 = no EOP marker
  //  2 = bad vis color data
  //  4 = invalid command byte
  //  8 = other error
  
  Dim tmpX As Integer, tmpY As Integer
  Dim i As Long, j As Long
  
  On Error GoTo ErrHandler
  
  //assume ok
  BuildBMPs = 0
  
  //if plot data not set yet
  if (!InitPlotData) {
    //initialize them
    InitializePlotData
  }
  
  //if blank data not set yet,
  if (VisBlankData(0) != AGIColors.agWhite) {
    //build blank bitmaps
    For j = 0 To 167
      For i = 0 To 159
        PriBlankData(j * 160 + i) = AGIColors.agRed
        VisBlankData(j * 160 + i) = AGIColors.agWhite
      Next i
    Next j
  }
  
  //copy picture data locally to increase speed
  //of picture drawing functions
  agPicData = bytPicData
  
  //load data arrays with blank info
  VisBuildData = VisBlankData
  PriBuildData = PriBlankData
  
  //if no end passed,
  if (EndPos = -1) {
    //get size of data
    EndPos = UBound(agPicData)
  }
  
  //if no status pos passed
  if (StatusPos = -1) {
    //status pos is end pos
    StatusPos = EndPos
  }
  
  //save endpos locally
  lngEndPos = EndPos
  
  //set default picture values
  With CurrentPen
    .VisColor = AGIColors.agNone
    .PriColor = AGIColors.agNone
    .PlotSize = 0
    .PlotShape = psCircle
    .PlotStyle = psSolid
  End With
  
  lngPos = 0
  
  //read first command byte
  bytIn = agPicData(lngPos)
  lngPos = lngPos + 1
  Do
    switch (bytIn
    case 0xF6 //Absolute line (long lines).
      DrawAbsLine
      
    case 0xF7 //Relative line (short lines).
      DrawRelLine
      
    case 0xF8 //Fill.
      PicFloodFill
      
    case 0xF4 //Draw a Y corner.
      DrawCorner cdY
      
    case 0xF5 //Draw an X corner.
      DrawCorner cdX
      
    case 0xF9 //Change pen size and style.
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      
      CurrentPen.PlotStyle = (bytIn && 0x20) / 0x20
      CurrentPen.PlotShape = (bytIn && 0x10) / 0x10
      CurrentPen.PlotSize = bytIn && 0x7
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      
    case 0xFA //Plot with pen.
      BrushPlot
      
    case 0xF0 //Change picture color and enable picture draw.
      //get color (only lower nibble is used)
      CurrentPen.VisColor = (agPicData(lngPos) && 0xF)
      //AGI has a slight bug; if color is >15, the
      //upper nibble will overwrite the priority color
      if (agPicData(lngPos) > 15) {
        //pass upper nibble to priority
        CurrentPen.PriColor = (CurrentPen.PriColor || agPicData(lngPos) \ 16)
        //set warning flag
        BuildBMPs = BuildBMPs || 2
      }
      
      lngPos = lngPos + 1
      //get next command byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1

    case 0xF1 //Disable picture draw.
      //disable visual drawing
      CurrentPen.VisColor = AGIColors.agNone
      //get next command byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      
    case 0xF2 //Change priority color and enable priority draw.
      //get color
      //AGI uses ONLY priority color; if the passed value is
      //greater than 15, the upper nibble gets ignored
      CurrentPen.PriColor = (agPicData(lngPos) && 0xF)
      lngPos = lngPos + 1
      //get next command byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1

    case 0xF3 //Disable priority draw.
      //disable priority
      CurrentPen.PriColor = AGIColors.agNone
      //get next command byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      
    case 0xFF //end of drawing
      //if pen status position is end of drawing
      if (StatusPos = EndPos) {
        //save tool status
        SavePen = CurrentPen
// these lines not needed because function is done
//////        //reset status pos to aviod changing save status
//////        StatusPos = 0x7FFFFFFF
      }
      Exit Do
      
    default:
      //if expecting a command, and byte is <240 but >250 (not 255)
      //just ignore it
      //get next command byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      //set warning flag
      BuildBMPs = BuildBMPs || 4
    End Select
    
    //if at Pen Status position,
    if (lngPos > StatusPos) {
      //save tool status
      SavePen = CurrentPen
      //reset statuspos to avoid changing save status
      StatusPos = 0x7FFFFFFF
    }
  Loop Until lngPos > EndPos
  
  //if at end of resource, was last command end-of-resource flag?
  if (lngPos > UBound(agPicData)) {
    if (agPicData(UBound(agPicData)) != 0xFF) {
      //set warning flag
      BuildBMPs = BuildBMPs || 1
    }
  }
  //copy resulting data back to calling function
  VisData = VisBuildData
  PriData = PriBuildData
Exit Function

ErrHandler:
  //capture error information
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  //pass back whatever is drawn, up to the error
  VisData = VisBuildData
  PriData = PriBuildData
  
  //depending on error, set warning level
  switch (lngError
  case 9  //subscript error- caused when a draw function expects
          //another byte of data, but end of data is reached
    //confirm it
    if (lngPos > EndPos) {
      //set warning flag
      BuildBMPs = BuildBMPs || 1
    Else
      //something else
      BuildBMPs = BuildBMPs || 8
    }
    
  default:
    //any other error- just pass it along
    BuildBMPs = BuildBMPs || 8
  End Select
End Function
      */

    }
    static void temp()
    {

      /*
  Option Explicit

  private Sub DrawPixel(ByVal xPos As Long, ByVal yPos As Long)

    On Error Resume Next

    Dim lngIndex As Long

    lngIndex = xPos + yPos * 160

    if (lngIndex <= 26879) {
      if (CurrentPen.VisColor < AGIColors.agNone) {
        VisBuildData(lngIndex) = CurrentPen.VisColor
      }
      if (CurrentPen.PriColor < AGIColors.agNone) {
        PriBuildData(lngIndex) = CurrentPen.PriColor
      }
    }
  End Sub

  Public Function GetColVal(lngEGAIn As Long) As Long
    //this one can't really be explained
    //basically it attempts to convert a long color Value
    //into the corresponding AGI color index with the least
    //amount of calculations
    //it is an empirically derived algorithm

    //NOTE: if (this method is called with a color Value
    //other than the defined EGA color values for AGI, then
    //I have absolutely no idea what the return Value may
    //look like.

    Dim cR As Long, cG As Long, cB As Long
    Dim vR As Long, vG As Long, vB As Long

    //split the color up

    cR = lngEGAIn % 256
    cG = (lngEGAIn / 256) % 256
    cB = (lngEGAIn / 65536) % 256


    //convert to component numbers
    if (cR = 0xFF) {
      vR = 32
    } else if (cR = 0) {
      vR = 0
    Else
      vR = 16
    }

    if (cG = 0xFF) {
      vG = 32
    } else if (cG = 0) {
      vG = 0
    Else
      vG = 16
    }

    if (cB = 0xFF) {
      vB = 32
    } else if (cB = 0) {
      vB = 0
    Else
      vB = 16
    }

    //build composite
    GetColVal = (vB + vG * 2 + vR * 4) / 16

    //if <5
    if (GetColVal < 5) {
      Exit Function
    }

    //if >9
    if (GetColVal > 9) {
      GetColVal = GetColVal + 1
      Exit Function
    }

    //if red is >80(0x50)
    if (cR > 0x50&) {
      Exit Function
    }

    //three cases left:
    //7,8,5 corresponding to 8,9,10
    if (GetColVal > 6) {
      GetColVal = GetColVal + 1
      Exit Function
    }

    //only one left is light green
    GetColVal = 10
  End Function

  private Sub InitializePlotData()
    //circle data used by the
    //brush drawing functions to paint
    //pictures
    CircleData(0) = 0x80
    CircleData(1) = 0xC0
    CircleData(2) = 0xC0
    CircleData(3) = 0xC0
    CircleData(4) = 0x40
    CircleData(5) = 0xE0
    CircleData(6) = 0xE0
    CircleData(7) = 0xE0
    CircleData(8) = 0x40
    CircleData(9) = 0x60
    CircleData(10) = 0x60
    CircleData(11) = 0xF0
    CircleData(12) = 0xF0
    CircleData(13) = 0xF0
    CircleData(14) = 0x60
    CircleData(15) = 0x60
    CircleData(16) = 0x20
    CircleData(17) = 0x70
    CircleData(18) = 0xF8
    CircleData(19) = 0xF8
    CircleData(20) = 0xF8
    CircleData(21) = 0xF8
    CircleData(22) = 0xF8
    CircleData(23) = 0x70
    CircleData(24) = 0x20
    CircleData(25) = 0x30
    CircleData(26) = 0x78
    CircleData(27) = 0x78
    CircleData(28) = 0x78
    CircleData(29) = 0xFC
    CircleData(30) = 0xFC
    CircleData(31) = 0xFC
    CircleData(32) = 0x78
    CircleData(33) = 0x78
    CircleData(34) = 0x78
    CircleData(35) = 0x30
    CircleData(36) = 0x38
    CircleData(37) = 0x7C
    CircleData(38) = 0x7C
    CircleData(39) = 0x7C
    CircleData(40) = 0xFE
    CircleData(41) = 0xFE
    CircleData(42) = 0xFE
    CircleData(43) = 0xFE
    CircleData(44) = 0xFE
    CircleData(45) = 0x7C
    CircleData(46) = 0x7C
    CircleData(47) = 0x7C
    CircleData(48) = 0x38
    CircleData(49) = 0x18
    CircleData(50) = 0x3C
    CircleData(51) = 0x7E
    CircleData(52) = 0x7E
    CircleData(53) = 0x7E
    CircleData(54) = 0xFF
    CircleData(55) = 0xFF
    CircleData(56) = 0xFF
    CircleData(57) = 0xFF
    CircleData(58) = 0xFF
    CircleData(59) = 0x7E
    CircleData(60) = 0x7E
    CircleData(61) = 0x7E
    CircleData(62) = 0x3C
    CircleData(63) = 0x18

    //set flag
    InitPlotData = True
  End Sub

  private Sub DrawLine(ByVal X1 As Long, ByVal Y1 As Long, ByVal X2 As Long, ByVal Y2 As Long)

    Dim xPos As Long, yPos As Long
    Dim DY As Long, DX As Long
    Dim vDir As Long, hDir As Long
    Dim XC As Long, YC As Long, MaxDelta As Long
    Dim i As Long

    //Sierra sucked at checking for overflows; if a bad value for coords was used that
    //overflowed the designated picture buffer, AGI didn//t care; it would just draw the pixel
    //in invalid memory and plod on; so we//re just going to ignore invalid pixels and
    //plod on too. ugh...

    //(would be nice if there was a way to warn the user; not sure I can do that though)

    On Error GoTo ErrHandler

    //determine delta x/delta y and direction
    DY = Y2 - Y1
    vDir = Sgn(DY)
    DX = X2 - X1
    hDir = Sgn(DX)

    //if a point
    if (DY = 0 && DX = 0) {
      //set the point
      DrawPixel X1, Y1

    //if horizontal
    } else if (DY = 0) {
      For i = X1 To X2 Step hDir
        //set point
        DrawPixel i, Y1
      Next i

    //if vertical
    } else if (DX = 0) {
      For i = Y1 To Y2 Step vDir
        //set point
        DrawPixel X1, i
      Next i

    Else
      //this line drawing function EXACTLY matches the Sierra
      //drawing function

      //set the starting point
      DrawPixel X1, Y1

      xPos = X1
      yPos = Y1

      //invert DX and DY if they are negative
      if (DY < 0) {
        DY = DY * -1
      }
      if ((DX < 0)) {
        DX = DX * -1
      }

      //set up the loop, depending on which direction is largest
      if (DX >= DY) {
        MaxDelta = DX
        YC = DX \ 2
        XC = 0
      Else
        MaxDelta = DY
        XC = DY \ 2
        YC = 0
      }

      For i = 1 To MaxDelta
        YC = YC + DY
        if (YC >= MaxDelta) {
          YC = YC - MaxDelta
          yPos = yPos + vDir
        }

        XC = XC + DX
        if (XC >= MaxDelta) {
          XC = XC - MaxDelta
          xPos = xPos + hDir
        }

        DrawPixel xPos, yPos
      Next i
    }
  Exit Sub

  ErrHandler:
    strError = Err.Description
    strErrSrc = Err.Source
    lngError = Err.Number

    On Error GoTo 0: Err.Raise vbObjectError + 662, strErrSrc, Replace(LoadResString(662), ARG1, CStr(lngError) & ":" & strError)
  End Sub

  private Sub DrawCorner(CurAxis As CornerDirection)
    Dim X1 As Byte, Y1 As Byte
    Dim X2 As Byte, Y2 As Byte

    //read in start coordinates
    X1 = agPicData(lngPos)
    lngPos = lngPos + 1
    Y1 = agPicData(lngPos)
    lngPos = lngPos + 1

    //draw first pixel
    DrawLine X1, Y1, X1, Y1

    //get next byte
    bytIn = agPicData(lngPos)
    lngPos = lngPos + 1

    Do Until bytIn >= 0xF0 || lngPos > lngEndPos

      if (CurAxis = cdX) {
        X2 = bytIn
        Y2 = Y1
        DrawLine X1, Y1, X2, Y2
        CurAxis = cdY
        X1 = X2
      Else
        Y2 = bytIn
        X2 = X1
        DrawLine X1, Y1, X2, Y2
        CurAxis = cdX
        Y1 = Y2
      }

      //get next byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
    Loop
  End Sub


  private Sub DrawAbsLine()
    Dim X1 As Byte, Y1 As Byte
    Dim X2 As Byte, Y2 As Byte

    //read in start position
    X1 = agPicData(lngPos)
    lngPos = lngPos + 1
    Y1 = agPicData(lngPos)
    lngPos = lngPos + 1

    //draw first pixel
    DrawLine X1, Y1, X1, Y1

    //get next potential coordinate
    bytIn = agPicData(lngPos)
    lngPos = lngPos + 1

    Do Until bytIn >= 0xF0 || lngPos > lngEndPos

      X2 = bytIn
      Y2 = agPicData(lngPos)
      lngPos = lngPos + 1
      DrawLine X1, Y1, X2, Y2
      X1 = X2
      Y1 = Y2
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
    Loop
  End Sub


  private Sub DrawRelLine()
    Dim xdisp As Integer, ydisp As Integer
    Dim X1 As Byte, Y1 As Byte
    Dim rtn As Long

    //read in starting position
    X1 = agPicData(lngPos)
    lngPos = lngPos + 1
    Y1 = agPicData(lngPos)
    lngPos = lngPos + 1
    //set pixel of starting position
    DrawPixel X1, Y1

    //get next potential command
    bytIn = agPicData(lngPos)
    lngPos = lngPos + 1

    Do Until bytIn >= 0xF0 || lngPos > lngEndPos
      //if horizontal high bit set
      if ((bytIn && 0x80)) {
        //displacement is negative
        xdisp = 0 - ((bytIn && 0x70) / 0x10)
      Else
        xdisp = ((bytIn && 0x70) / 0x10)
      }
      //if vertical high bit set
      if ((bytIn && 0x8)) {
        //displacement is negative
        ydisp = 0 - (bytIn && 0x7)
      Else
        ydisp = (bytIn && 0x7)
      }
      DrawLine X1, Y1, X1 + xdisp, Y1 + ydisp
      X1 = X1 + xdisp
      Y1 = Y1 + ydisp

      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
    Loop
  End Sub


  private Sub BrushPlot()

    Dim PlotX As Long, PlotY As Long
    Dim PatternNum As Byte
    Dim rtn As Long

    Dim X As Long, Y As Long

    On Error GoTo ErrHandler

    //get next value (Xpos or splatter code)
    bytIn = agPicData(lngPos)
    lngPos = lngPos + 1

    Do Until bytIn >= 0xF0 || lngPos > lngEndPos
      //if spatter mode is active,  current data point is the splatter value
      if (CurrentPen.PlotStyle = psSplatter) {
        PatternNum = bytIn || 1
        //next byte will be the Xpos
        bytIn = agPicData(lngPos)
        lngPos = lngPos + 1
        if (bytIn >= 0xF0) {
          //exit if a draw command is found
          Exit Do
        }
      }

      //store x value
      PlotX = bytIn
      //get y value
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
      if (bytIn >= 0xF0) {
        //exit if a draw command is found
        Exit Do
      }
      //store y value
      PlotY = bytIn

      //convert to correct upper/left values to start the plotting
      PlotX = PlotX - (CurrentPen.PlotSize + 1) \ 2
      if (PlotX < 0) {
        PlotX = 0
  //////    } else if (PlotX > 159 - CurrentPen.PlotSize) {
      } else if (PlotX > 160 - CurrentPen.PlotSize) {
        //there is a bug in AGI that uses 160 instead of 159
        //well, actually, it doubles the X value for the check
        // and uses a value of 320, but it//s the same effect)
        //
        //WinAGI needs to mimic that bug so pictures look
        //exactly the same
        PlotX = 160 - CurrentPen.PlotSize
      }
      PlotY = PlotY - CurrentPen.PlotSize
      if (PlotY < 0) {
        PlotY = 0
      } else if (PlotY > 167 - CurrentPen.PlotSize) {
        PlotY = 167 - CurrentPen.PlotSize
      }

      //if brush is a circle
      if (CurrentPen.PlotShape = psCircle) {
        For Y = 0 To CurrentPen.PlotSize * 2
          For X = 0 To CurrentPen.PlotSize
            //if pixel is within circle shape,
            if (CircleData(CurrentPen.PlotSize ^ 2 + Y) && 2 ^ (7 - X)) {
              //if style is splatter
              if (CurrentPen.PlotStyle = psSplatter) {
                //adjust pattern bit using Sierra//s algorithm
                if ((PatternNum && 1)) {
                  PatternNum = PatternNum \ 2 Xor 0xB8
                Else
                  PatternNum = PatternNum \ 2
                }

                //only draw if pattern bit is set
                if ((PatternNum && 3) == 2) {
                  DrawPixel X + PlotX, Y + PlotY
                }
              Else  //solid
                //set all pixels
                DrawPixel X + PlotX, Y + PlotY
              }
            }
          Next X
        Next Y
      Else //square
        For Y = 0 To CurrentPen.PlotSize * 2
          For X = 0 To CurrentPen.PlotSize
            //if style is splatter
            if (CurrentPen.PlotStyle = psSplatter) {
              //only draw if pattern bit is set
              if ((PatternNum && 1)) {
                PatternNum = PatternNum \ 2 Xor 0xB8
              Else
                PatternNum = PatternNum \ 2
              }
              if ((PatternNum && 3) == 2) {
                DrawPixel X + PlotX, Y + PlotY
              }
            Else  //solid
              //set all pixels
              DrawPixel X + PlotX, Y + PlotY
            }
          Next X
        Next Y
      }

      //get next byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
    Loop

  Exit Sub

  ErrHandler:
      //failure! pass error along
      //////Debug.Assert False
      Err.Raise Err.Number, Err.Source, Err.Description
  End Sub
  private Sub PicFloodFill()

    Dim QueueStart As Long, QueueEnd As Long
    Dim lngOffset As Long
    Dim X As Byte, Y As Byte

    On Error GoTo ErrHandler

    //get next byte
    bytIn = agPicData(lngPos)
    lngPos = lngPos + 1

    Do Until bytIn >= 0xF0 || lngPos > lngEndPos
      //save x, get Y
      X = bytIn
      Y = agPicData(lngPos)
      lngPos = lngPos + 1

      //if visual OR priority but not both
      if ((CurrentPen.VisColor < AGIColors.agNone) Xor (CurrentPen.PriColor < AGIColors.agNone)) {
        //if drawing visual
        if (CurrentPen.VisColor < AGIColors.agNone) {
          //if color is not white, and current pixel IS white,
          if (CurrentPen.VisColor != AGIColors.agWhite && VisBuildData(X + 160 * Y) = AGIColors.agWhite) {
            //store the starting point in first queue position
            QueueStart = 0
            QueueEnd = 1
            lngOffset = Y * 160 + X
            Queue(QueueStart) = lngOffset
            //set first point
            VisBuildData(lngOffset) = CurrentPen.VisColor

            Do
              lngOffset = Queue(QueueStart)
              X = lngOffset % 160
              Y = lngOffset \ 160
              QueueStart = QueueStart + 1

              //if pixel above is white,
              if (Y > 0) {
                lngOffset = (Y - 1) * 160 + X
                if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                  //set it
                  VisBuildData(lngOffset) = CurrentPen.VisColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel to left is white,
              if (X > 0) {
                lngOffset = Y * 160 + X - 1
                if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                  //set it
                  VisBuildData(lngOffset) = CurrentPen.VisColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel to right is white,
              if (X < 159) {
                lngOffset = Y * 160 + X + 1
                if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                  //set it
                  VisBuildData(lngOffset) = CurrentPen.VisColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel below is white
              if (Y < 167) {
                lngOffset = (Y + 1) * 160 + X
                if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                  //set it
                  VisBuildData(lngOffset) = CurrentPen.VisColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
            Loop Until QueueStart = QueueEnd
          }
        Else
          //if color is not red, and current pixel IS red,
          if (CurrentPen.PriColor != AGIColors.agRed && PriBuildData(X + 160 * Y) = AGIColors.agRed) {
            //store the starting point in first queue position
            QueueStart = 0
            QueueEnd = 1
            lngOffset = Y * 160 + X
            Queue(QueueStart) = lngOffset
            //set first point
            PriBuildData(lngOffset) = CurrentPen.PriColor

            Do
              lngOffset = Queue(QueueStart)
              X = lngOffset % 160
              Y = lngOffset \ 160
              QueueStart = QueueStart + 1

              //if pixel above is red,
              if (Y > 0) {
                lngOffset = (Y - 1) * 160 + X
                if (PriBuildData(lngOffset) = AGIColors.agRed) {
                  //set it
                  PriBuildData(lngOffset) = CurrentPen.PriColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel to left is red,
              if (X > 0) {
                lngOffset = Y * 160 + X - 1
                if (PriBuildData(lngOffset) = AGIColors.agRed) {
                  //set it
                  PriBuildData(lngOffset) = CurrentPen.PriColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel to right is red,
              if (X < 159) {
                lngOffset = Y * 160 + X + 1
                if (PriBuildData(lngOffset) = AGIColors.agRed) {
                  //set it
                  PriBuildData(lngOffset) = CurrentPen.PriColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
              //if pixel below is red
              if (Y < 167) {
                lngOffset = (Y + 1) * 160 + X
                if (PriBuildData(lngOffset) = AGIColors.agRed) {
                  //set it
                  PriBuildData(lngOffset) = CurrentPen.PriColor
                  //add to queue
                  Queue(QueueEnd) = lngOffset
                  QueueEnd = QueueEnd + 1
                }
              }
            Loop Until QueueStart = QueueEnd
          }
        }
      //if drawing both
      } else if ((CurrentPen.VisColor < AGIColors.agNone) && (CurrentPen.VisColor < AGIColors.agNone)) {
        //if picture draw color is NOT white, and current pixel is white
        if (CurrentPen.VisColor != AGIColors.agWhite && VisBuildData(X + 160 * Y) = AGIColors.agWhite) {
          //store the starting point in first queue position
          QueueStart = 0
          QueueEnd = 1
          lngOffset = Y * 160 + X
          Queue(QueueStart) = lngOffset
          //set first point
          VisBuildData(lngOffset) = CurrentPen.VisColor
          PriBuildData(lngOffset) = CurrentPen.PriColor

          Do
            lngOffset = Queue(QueueStart)
            X = lngOffset % 160
            Y = lngOffset \ 160
            QueueStart = QueueStart + 1

            //if pixel above is white,
            if (Y > 0) {
              lngOffset = (Y - 1) * 160 + X
              if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                //set it
                VisBuildData(lngOffset) = CurrentPen.VisColor
                PriBuildData(lngOffset) = CurrentPen.PriColor
                //add to queue
                Queue(QueueEnd) = lngOffset
                QueueEnd = QueueEnd + 1
              }
            }
            //if pixel to left is white,
            if (X > 0) {
              lngOffset = Y * 160 + X - 1
              if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                //set it
                VisBuildData(lngOffset) = CurrentPen.VisColor
                PriBuildData(lngOffset) = CurrentPen.PriColor
                //add to queue
                Queue(QueueEnd) = lngOffset
                QueueEnd = QueueEnd + 1
              }
            }
            //if pixel to right is white,
            if (X < 159) {
              lngOffset = Y * 160 + X + 1
              if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                //set it
                VisBuildData(lngOffset) = CurrentPen.VisColor
                PriBuildData(lngOffset) = CurrentPen.PriColor
                //add to queue
                Queue(QueueEnd) = lngOffset
                QueueEnd = QueueEnd + 1
              }
            }
            //if pixel below is white
            if (Y < 167) {
              lngOffset = (Y + 1) * 160 + X
              if (VisBuildData(lngOffset) = AGIColors.agWhite) {
                //set it
                VisBuildData(lngOffset) = CurrentPen.VisColor
                PriBuildData(lngOffset) = CurrentPen.PriColor
                //add to queue
                Queue(QueueEnd) = lngOffset
                QueueEnd = QueueEnd + 1
              }
            }
          Loop Until QueueStart = QueueEnd
        }
      }

      //get next byte
      bytIn = agPicData(lngPos)
      lngPos = lngPos + 1
    Loop
  Exit Sub

  ErrHandler:
    //////Debug.Assert False
    //no way to handle if a bad pixel is encountered; just give up
    strError = Err.Description
    strErrSrc = Err.Source
    lngError = Err.Number

    On Error GoTo 0: Err.Raise vbObjectError + 662, strErrSrc, Replace(LoadResString(662), ARG1, CStr(lngError) & ":" & strError)
  End Sub

      */
    }
  }
}
