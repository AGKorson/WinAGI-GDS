using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using WinAGI.Editor;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor
{
  public partial class frmPalette : Form
  {
    Color SelColor;
    string[] strColName = new string[16];
    Color[] lngTempCol = new Color[16];
    int FormMode; // 0 = palette mode; 1 = prevwin bkgd color mode

    public frmPalette(int mode)
    {
      InitializeComponent();
      FormMode = mode;
      switch (FormMode) {
      case 0:
        // adjust caption depending on whether game is loaded or not
        if (EditGame.GameLoaded) {
          Text = "Modify Color Palette for this Game";
        } else {
          Text = "Modify Default Color Palette";
      }
        break;
      case 1:
        // set form for choosing view editor background
        Text = "New Bkgd Color:";
        /*
        this.Width = 2265 / 15;
        this.Height = 3675 / 15;
        cmdOK.Top = 2680 / 15;
        cmdOK.Left = 150 / 15;
        cmdOK.Width = 870 / 15;
        cmdOK.Default = true
        cmdCancel.Top = 2680 / 15;
        cmdCancel.Left = 1170 / 15;
        cmdCancel.Width = 870 / 15;
        cmdCancel.Cancel = true;
        cmdColorDlg.Text = "Reset to Default";
        cmdColorDlg.FontBold = false;
        cmdColorDlg.Left = 150 / 15;
        cmdColorDlg.Width = 1890 / 15;
        cmdColorDlg.Top = 2245 / 15;
        Label33.Visible = false;
        picColChange.Visible = false;
        */
        break;
      }
    }
    void tmpForm()
    {
      /*
  private void LoadPalette(string LoadFile)
      {
    //opens the load file, and finds the palette section
    //the file should be a NAGI.ini file

    Dim intFile As Integer, Index As Long
    Dim i As Long, strLine As String
    Dim blnFound As Boolean, strValues() As String

    On Error Resume Next
    intFile = FreeFile()
    Open LoadFile For Input As intFile
    if (Err.Number <> 0) {
      MsgBox "Unable to open this file. Try another one.", vbCritical + vbOKOnly, "Load Palette Error"
      return;
    }

    On Error GoTo ErrHandler

    //load lines until palette found
    Do
      Line Input #intFile, strLine

      if (LCase$(strLine) = "[palette]") {
        //found it!
        blnFound = true
        Exit Do
      }
    Loop Until EOF(intFile)

    if (Not blnFound) {
      //no palette
      MsgBoxEx "This file does not contain a NAGI palette entry. Try another file.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Load Palette Error", WinAGIHelp, "htm\winagi\Palette.htm#load"
      return;
    }

    //load colors, one at a time
    Do
      //get next line
      Line Input #intFile, strLine
      strLine = Trim$(strLine)

      //avoid error with blank line, by using single space
      if (strLine = vbNullString) { strLine = ";"

      //action depends on ascii Value of line
      Select Case Asc(strLine)
      Case 91  //[ == starting another section
        Exit Do
      Case 59 //; == comment; ignore line

      Case 99  //c == probably  a color line
        //if this is a color line
        if (Left$(strLine, 6) = "color") {
          //strip off color
          strLine = Right$(strLine, Len(strLine) - 6)
          //get color index val
          Index = CLng(Val(strLine))

          //strip off only stuff past equal sign
          strValues = Split(strLine, "=")

          //should be two elements
          if (UBound(strValues) = 1) {
            //replace //0x// with //&H//
            strValues(1) = Replace(strValues(1), "0x", "&H")
            //split into three values
            strValues = Split(Trim$(strValues(1)), ",")

            //now, should be three elements
            if (UBound(strValues) = 2) {
              //each element should be in hex (0x00) format
              if (Left$(strValues(0), 2) = "&H" And _
                 Left$(strValues(1), 2) = "&H" And _
                 Left$(strValues(2), 2) = "&H") {
                //build color Value by using hex values
                lngTempCol(Index) = Val(strValues(2)) * &H10000 + _
                                    Val(strValues(1)) * &H100& + _
                                    Val(strValues(0))
              } //values start //&H//
            }  //three values
          }  //correct assignment to color line
        }  //valid color entry

      default: // what the heck is going on; just ignore
      End Select

    Loop Until EOF(intFile)

    //close the file
    Close intFile

    //force update of color grid
    picPalette_Paint
  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }

  private void SavePalette(ByVal SaveFile As String)

    //saves color palette to nagi.ini file

    //opens the load file, and finds the palette section
    //the file should be a NAGI.ini file

    Dim intFile1 As Integer, intFile2 As Integer
    Dim blnFound As Boolean
    Dim i As Long, strLine As String
    Dim strTempFile As String

    //open the save file
    On Error Resume Next
    intFile1 = FreeFile()
    Open SaveFile For Input As intFile1
    if (Err.Number <> 0) {
      MsgBox "Unable to open this file. Try another one.", vbCritical + vbOKOnly, "Save Palette Error"
      Close intFile1
      return;
    }

    //open temp file
    strTempFile = TempFileName()
    intFile2 = FreeFile()
    Open strTempFile For Output As intFile2

    if (Err.Number <> 0) {
      MsgBox "Unable to save to this file. Try another one.", vbCritical + vbOKOnly, "Save Palette Error"
      Close intFile1, intFile2
      return;
    }

    On Error GoTo ErrHandler
    //transfer lines to new file, skipping palette section
    Do
      Line Input #intFile1, strLine

      if (blnFound) {
        //check for start of another section
        if (Trim$(strLine) = vbNullString) {
        } else if ( Asc(Trim$(strLine)) = 91) {
          //start of another section
          blnFound = false
        }
      } else {
        if (LCase$(strLine) = "[palette]") {
          //found it!
          blnFound = true
        }
      }

      //now add line IF not in palette section
      if (Not blnFound) {
        //add it to new file
        Print #intFile2, strLine
      }

    Loop Until EOF(intFile1)

    //done with original file
    Close intFile1

    //now build new palette section
    Print #intFile2, "[palette]"
    Print #intFile2, ";palette option allows you to change the colors used by NAGI"
    Print #intFile2, ";default colors are used if a color is not defined"
    Print #intFile2, ";define colors as three hexadecimal values, representing"
    Print #intFile2, ";red, green, blue components"

    //add color lines
    For i = 0 To 15
      strLine = "color" & CStr(i) & "=" & "0x" & Hex2(lngTempCol(i) And &HFF&) & "," & _
                                     "0x" & Hex2((lngTempCol(i) And &HFF00&) \ &H100&) & "," & _
                                     "0x" & Hex2(lngTempCol(i) \ &H10000)

      Print #intFile2, strLine
    Next i
    Print #intFile2, , vbNullString
    //done with the temp file
    Close #intFile2

    //now copy temp file to desired file
    On Error Resume Next
    Kill SaveFile
    FileCopy strTempFile, SaveFile
  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }


  public void SetForm(int NewMode)
{
  }

  private void cmdCancel_Click()

    //just unload the settings form
    Unload Me

  }

  private void cmdColorDlg_Click()

    //show color dialog

    On Error GoTo ErrHandler

    Select Case FormMode
    Case 0 //palette change
      With cdColors
        .DialogTitle = "Choose New AGI Palette Color"
        .Color = lngTempCol(SelColor)

        .Flags = cdlCCRGBInit Or cdlCCFullOpen
        .ShowColor

        lngTempCol(SelColor) = .Color

        //update picture
        picPalette_Paint
        picColChange_Paint
      End With
      picPalette.SetFocus

    Case 1 //change prevwin bkgd color
      PrevWinBColor = &H8000000F
      //hide the form by canceling
      cmdCancel_Click
    End Select
  return;

  ErrHandler:
    //if canceling,
    if (Err.Number = cdlCancel) {
      return;
    }

    //Debug.Assert false
    Resume Next
  }


  private void cmdDefColors_Click()

    Dim i As Long

    For i = 0 To 15
      lngTempCol(i) = DefEGAColor(i)
    Next i

    picPalette_Paint
    picColChange_Paint
  }

  private void cmdLoad_Click()

    //get a file name

    On Error GoTo ErrHandler

    //show dialog
    With MainDialog
      .Flags = cdlOFNHideReadOnly + cdlOFNFileMustExist
      .DialogTitle = "Open NAGI INI File"
      .DefaultExt = "ini"
      .Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*"
      .FilterIndex = 1
      .FileName = vbNullString
      .InitDir = DefaultResDir

      .ShowOpen

      //load this file
      LoadPalette .FileName

      DefaultResDir = JustPath(.FileName)
    End With
  return;

  ErrHandler:
    //if user canceled the dialogbox,
    if (Err.Number = cdlCancel) {
      return;
    }

    //Debug.Assert false
    Resume Next
  }

  private void cmdOK_Click()

    Dim i As Long

    On Error GoTo ErrHandler

    //hide form and set colors
    this.Hide

    //action depends on mode
    Select Case FormMode
    Case 0 //palette change mode
      //set new colors
      For i = 0 To 15
        EGAColor(i) = lngTempCol(i)
        lngEGACol(i) = lngTempCol(i)

        //if no game is loaded
        if (Not GameLoaded) {
          //save default values
          DefEGAColor(i) = lngTempCol(i)
          WriteSetting GameSettings, sDEFCOLORS, "DefEGAColor" & CStr(i), "&H" & PadHex(lngTempCol(i), 8)
        }
      Next i


    Case 1  //change prev window background color
      PrevWinBColor = EGAColor(SelColor)

    End Select

    //unload the palette form
    Unload Me

  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }

  private void cmdSave_Click()

    On Error GoTo ErrHandler

    With MainSaveDlg
      .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
      .DialogTitle = "Save to NAGI INI File"
      .DefaultExt = "ini"
      .Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*"
      .FilterIndex = 1
      .FullName = vbNullString
      .InitDir = DefaultResDir

      .ShowSaveAs

      //save this palette
      SavePalette .FileName

      DefaultResDir = JustPath(.FileName)
    End With

  return;

  ErrHandler:
    //if NOT canceled,
    if (Err.Number <> cdlCancel) {
      //Debug.Assert false
      Resume Next
    }
  }

  private void Form_KeyDown(KeyCode As Integer, Shift As Integer)

    On Error GoTo ErrHandler

    //detect and respond to keyboard shortcuts

    //check for help
    if (Shift = 0 And KeyCode = vbKeyF1) {
      //help
      HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Palette.htm"
      KeyCode = 0
    }

  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }

  private void Form_Load()

    Dim i As Long

    //get temp copy of current colors
    For i = 0 To 15
      lngTempCol(i) = EGAColor(i)
    Next i

  }

  private void picColChange_Paint()

    //draw the current color and the default color in the change box
    With picColChange
      picColChange.Line (0, 0)-Step(.Width / 2 - 30, .Height), DefEGAColor(SelColor), BF
      picColChange.Line (.Width / 2 - 30, 0)-(.Width, .Height), lngTempCol(SelColor), BF
      picColChange.Line (.Width / 2 - 30, 0)-Step(0, .Height), vbBlack
    End With

  }


  private void picPalette_DoubleClick()

    Select Case FormMode
    Case 0
      //if in palette mode, then dblclick automatically edit this color...
      cmdColorDlg_Click

    Case 1
      //if in change prevwin bkgd color mode, dblclick selects and closes
      cmdOK_Click
    End Select

  }

  private void picPalette_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

    //select a color, based on cursor pos
    Dim sngRow As Single, sngCol As Single
    Dim intRow As Integer, intCol As Integer

    On Error GoTo ErrHandler

    //check if inside left/top margin
    if (X < 5 Or Y < 5) {
      return;
    }

    //convert x,y to row/col
    sngRow = (Y - 5) / 30
    sngCol = (X - 5) / 30
    intRow = Int(sngRow)
    intCol = Int(sngCol)

    //if either element exceeds more than .8333
    //then cursor must be inbetween boxes
    if ((sngRow - intRow) > 0.83333333333333 Or _
       (sngCol - intCol) > 0.83333333333333) {
      return;
    }

    //verify values
    if (intRow > 3 Or intCol > 3) {
      //ignore it
      return;
    }

    //select the color, and update the palette
    SelColor = intCol * 4 + intRow
    picPalette_Paint
    picColChange_Paint
  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }
  private void picPalette_Paint()

    //draw the current color palette

    Dim i As Long, j As Long
    Dim lngHiLite As Long

    On Error GoTo ErrHandler

    picPalette.Cls

    For i = 0 To 3
      For j = 0 To 3
        //draw the palette color block first
        picPalette.Line (i * 30 + 5, j * 30 + 5)-Step(25, 25), lngTempCol(4 * i + j), BF

        //if this is the selected color, then highlight it
        if (SelColor = 4 * i + j) {

          //draw hilite over selected color
          With picPalette
            .DrawStyle = vbDot
            .DrawWidth = 1
            .FillStyle = vbFSTransparent
            if (SelColor <= 9) {
              lngHiLite = vbWhite
            } else {
              lngHiLite = vbBlack
            }
            picPalette.Line (i * 30 + 5, j * 30 + 5)-Step(25, 25), lngHiLite, B

            .DrawStyle = vbSolid
          End With
        }
      Next j
    Next i

    //show color name on label
    lblCurCol.Text = CStr(SelColor) & ": " & LoadResString(COLORNAME + SelColor)
  return;

  ErrHandler:
    //Debug.Assert false
    Resume Next
  }
      */
    }
  }
}
