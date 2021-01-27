using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using tom;
using stdole;
//using olelib;

using System.Windows.Forms;
using static RichEdAGI.RichEdAGI;
using Microsoft.VisualStudio.OLE.Interop;
namespace RichEdAGI
{

  public class RichEdAGI : RichTextBox, IDisposable
  {
    [DllImport("user32.dll")]
    static extern int SendMessage(IntPtr hWnd, Int32 wMsg, IntPtr wParam, IntPtr lParam);
    private const int WM_USER = 0x400;
    private const int EM_GETOLEINTERFACE = WM_USER + 60;
    IntPtr m_hWndRE;
    ITextDocument m_oDocument;
    protected IntPtr IRichEditOlePtr = IntPtr.Zero;
    
    protected IRichEditOle IRichEditOleValue = null;
    internal bool noSelChange = false;
    internal int SelStartLine, SelEndLine;

    public struct SyntaxHighlightStyle
    {
      public bool Bold;
      public bool Italic;
      public Color ForeColor;
    }
    SyntaxHighlight mSyntaxHighlight;
    public RichEdAGI()
    {
      // create syntax highlight style object
      mSyntaxHighlight = new SyntaxHighlight(base.ForeColor);
      IRichEditOle oREO = null;

      m_hWndRE = this.Handle;
      // _ = SendMessage(this.Handle, EM_GETOLEINTERFACE, IntPtr.Zero, oREO);
      //m_oDocument = (ITextDocument)oREO;


      // Alloc a pointer to hold the return value for EM_GETOLEINTERFACE
      IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));
      // Clear the pointer
      Marshal.WriteIntPtr(ptr, IntPtr.Zero);
      try {
        if (0 != SendMessage(this.Handle,
                                 EM_GETOLEINTERFACE, IntPtr.Zero, ptr)) {
          // Read the returned pointer.  It is what we're looking for!
          IntPtr pRichEdit = Marshal.ReadIntPtr(ptr);
          try {
            Guid guid = new Guid("00020D00-0000-0000-c000-000000000046");
            Marshal.QueryInterface(pRichEdit, ref guid, out this.IRichEditOlePtr);
            // Wrap it in the C# interface for IRichEditOle.
            this.IRichEditOleValue = (IRichEditOle)Marshal.GetTypedObjectForIUnknown(this.IRichEditOlePtr, typeof(IRichEditOle));
            if (this.IRichEditOleValue == null) {
              throw new Exception("Failed to get the object wrapper for the interface.");
            }
            oREO = (IRichEditOle)IRichEditOleValue;
            m_oDocument = (ITextDocument)oREO;
          }
          finally {
            Marshal.Release(pRichEdit);
          }
        }
        else {
          throw new Exception("EM_GETOLEINTERFACE failed.");
        }
      }
      finally {
        // Free the ptr memory.
        Marshal.FreeCoTaskMem(ptr);
      }

    }
    public bool AGIKeywords
    { get; set; }
    public bool AutoIndent
    { get; set; }
    public int DefaultTabStop
    { get; set; }
    public bool Dirty
    { get; set; }
    public bool DisableNoScroll
    { get; set; }
    public string FileName
    { get; set; }
    public SyntaxHighlight Highlight
    { get => mSyntaxHighlight; }
    public bool Highlighting
    { get; set; }
    public void BeginEditCollection()
    {

    }
    public void EndEditCollection()
    {

    }
    public void Freeze()
    {

    }
    public TRange Range(int StartPos = -1, int EndPos = -1)
    {
      ITextRange oTR;
      // if no parameters passed, return entire document
      if (StartPos < 0 && EndPos < 0) {
        oTR = m_oDocument.Range(0, 0);
        oTR.Expand((int)tomConstants.tomStory);
      }
      else if (StartPos < 0) {
        oTR = m_oDocument.Range(0, EndPos);
      }
      else if (EndPos < 0) {
        oTR = m_oDocument.Range(StartPos, 0x7fffffff);
      }
      else {
        oTR = m_oDocument.Range(StartPos, EndPos);
      }
      TRange retval = new TRange(this.Handle, m_oDocument, this);
      retval.Range = oTR;
      return retval;

    }  
    public TRange RangeFromPoint(int X, int Y)
    {
      Point tPoint = new Point(X, Y);
      //*// TODO... 
      //client to screen...???
      TRange rangeFromPt = new TRange(this.Handle, m_oDocument, this);
      rangeFromPt.Range = m_oDocument.RangeFromPoint(tPoint.X, tPoint.Y);
      return rangeFromPt;
    }
    public void RefreshHighlight(int StartLine = -1, int EndLine = -1)
    {

    }
    public Selection Selection
    { get => new Selection(this.Handle, m_oDocument.Selection, m_oDocument, this);
    }
    public void UnFreeze()
    {

    }
    public void LoadFile(string FileName, int Flags = 0, int CodePage = 0)
    {
      IRichEditOle oREO;
      int lngLen;

      try {
        m_oDocument.Saved = 1;
        //set host name
        oREO = (IRichEditOle)m_oDocument;
        oREO.SetHostNames("RichEdAGI Control", FileName);
        oREO = null;
        m_oDocument.Open(FileName, Flags, CodePage);
      }
      catch (Exception) {
        System.Diagnostics.Debug.Print("buggers, file didn't load...");
      }

    }
    internal void EditLine(int LineNo)
    {
      //*// TODO...

    }
    internal void FormatLines(int StartLine, int EndLine)
    {
      //*// TODO...
    }
    internal void Hilightem()
    {

    }
  }
  public class SyntaxHighlight
  {
    public SyntaxHighlight(Color defaultcolor)
    {
      // initialize all styles to no bold, no italic, with default colors
      Comment.Bold = false;
      Comment.Italic = false;
      Comment.ForeColor = defaultcolor;
      Key.Bold = false;
      Key.Italic = false;
      Key.ForeColor = defaultcolor;
      Identifier.Bold = false;
      Identifier.Italic = false;
      Identifier.ForeColor = defaultcolor;
      String.Bold = false;
      String.Italic = false;
      String.ForeColor = defaultcolor;
      Normal.Bold = false;
      Normal.Italic = false;
      Normal.ForeColor = defaultcolor;
    }
    public SyntaxHighlightStyle Comment;
    public SyntaxHighlightStyle Key;
    public SyntaxHighlightStyle Identifier;
    public SyntaxHighlightStyle String;
    public SyntaxHighlightStyle Normal;
  }
  public class TRange : ITextRange
  {
    ITextRange m_Range;
    IntPtr m_hWnd;
    RichEdAGI mParent;
    ITextDocument m_Doc;

    public ITextRange Range
    {
      get { return m_Range; }
      internal set { m_Range = value; }
    }

    internal TRange(IntPtr handle, ITextDocument oDoc, RichEdAGI parent)
    {
      m_hWnd = handle;
      m_Doc = oDoc;
      mParent = parent;
    }
    public void ChangeCase(int NewCase)
    {
      m_Range.ChangeCase(NewCase);
    }

    public void Collapse(int bStart)
    {
      m_Range.Collapse(bStart);
    }
    public void Comment()
    {
      int lngPos, lngStart;
      int lngLine = 0, lngCount;
      int lngStartLine, lngEndLine;

      //if no handle
      if (m_hWnd == IntPtr.Zero) {
        return;
      }
      //if no document
      if (m_Doc == null) {
        return;
      }
      //freeze the doc
      m_Doc.Freeze();
      //expand to select complete Lines
      m_Range.StartOf((int)tom.tomConstants.tomLine, 1);
      m_Range.EndOf((int)tomConstants.tomLine, 1);
      //save start value, and lines
      lngStart = m_Range.Start;
      //count lines (CRs)
      lngPos = m_Range.Text.IndexOf('\r');
      lngCount = -1;
      while (lngPos != 0) {
        lngCount++;
        lngPos = m_Range.Text.IndexOf('\r', lngPos + 1);
      }
      //add comment marker at beginning, and after each carriage return
      m_Doc.Selection.Start = lngStart;
      m_Doc.Selection.End = lngStart;
      //add the marker
      m_Doc.Selection.TypeText("[");
      //move to start of next line, until all lines are commented
      lngLine++;
      while (lngLine <= lngCount) {
        m_Doc.Selection.MoveUntil("\r", (int)tomConstants.tomForward);
        m_Doc.Selection.MoveRight((int)tomConstants.tomCharacter, 1, 0);
        m_Doc.Selection.TypeText("[");
        lngLine++;
      }
      //set selection to entire range
      m_Doc.Selection.Start = lngStart;
      m_Doc.Selection.MoveEndUntil("\r", (int)tomConstants.tomForward);
      m_Doc.Selection.MoveEnd((int)tomConstants.tomCharacter, 1);

      //now format the lines to make them look like comments (if highlighting)
      if (mParent.Highlighting) {
        m_Doc.Selection.Font.ForeColor = (mParent.Highlight.Comment.ForeColor).ToArgb();
        m_Doc.Selection.Font.Bold = Convert.ToInt32(mParent.Highlight.Comment.Bold);
        m_Doc.Selection.Font.Italic = Convert.ToInt32(mParent.Highlight.Comment.Italic);
      }
      //all done, unfreeze
      m_Doc.Unfreeze();
    }
    public int Expand(int Unit)
    {
      return m_Range.Expand(Unit);
    }
    public int GetIndex(int Unit)
    {
      return m_Range.GetIndex(Unit);
    }

    public void SetIndex(int Unit, int Index, int Extend)
    {
      m_Range.SetIndex(Unit, Index, Extend);
    }

    public void SetRange(int cpActive, int cpOther)
    {
      m_Range.SetRange(cpActive, cpOther);
    }

    public int InRange(ITextRange pRange)
    {
      return m_Range.InRange(pRange);
    }

    public int InStory(ITextRange pRange)
    {
      return m_Range.InStory(pRange);
    }

    public int IsEqual(ITextRange pRange)
    {
      return m_Range.IsEqual(pRange);
    }

    public void Select()
    {
      m_Range.Select();
    }

    public int StartOf(int Unit, int Extend)
    {
      return m_Range.StartOf(Unit, Extend);
    }

    public int EndOf(int Unit = (int)tomConstants.tomWord, int Extend = 0)
    {
      return m_Range.EndOf(Unit, Extend);
    }

    public int Move(int Unit, int Count)
    {
      return m_Range.Move(Unit, Count);
    }

    public int MoveStart(int Unit, int Count)
    {
      return m_Range.MoveStart(Unit, Count);
    }

    public int MoveEnd(int Unit, int Count)
    {
      return m_Range.MoveEnd(Unit, Count);
    }

    public int MoveWhile(ref object Cset, int Count)
    {
      return m_Range.MoveWhile(ref Cset, Count);
    }

    public int MoveStartWhile(ref object Cset, int Count)
    {
      return m_Range.MoveStartWhile(ref Cset, Count);
    }

    public int MoveEndWhile(ref object Cset, int Count)
    {
      return m_Range.MoveEndWhile(ref Cset, Count);
    }

    public int MoveUntil(ref object Cset, int Count)
    {
      return m_Range.MoveUntil(ref Cset, Count);
    }

    public int MoveStartUntil(ref object Cset, int Count)
    {
      return m_Range.MoveStartUntil(ref Cset, Count);
    }

    public int MoveEndUntil(ref object Cset, int Count)
    {
      return m_Range.MoveEndUntil(ref Cset, Count);
    }

    public int FindText(string Text, int cch, int Flags)
    {
      return m_Range.FindText(Text, cch, Flags);
    }

    public int FindTextStart(string bstr, int cch, int Flags)
    {
      return m_Range.FindTextStart(bstr, cch, Flags);
    }

    public int FindTextEnd(string bstr, int cch, int Flags)
    {
      return m_Range.FindTextEnd(bstr, cch, Flags);
    }

    public int Delete(int Unit = (int)tomConstants.tomSection, int Count = 1)
    {
      return m_Range.Delete(Unit, Count);
    }

    public void Cut(out object pVar)
    {
      //*// TODO:
      // for syntax highlighting, the control needs to know if an
      // entire line has been selected
      //blnSWL = (SendMessage(m_hWnd, EM_LINEINDEX, SendMessage(m_hWnd, EM_EXLINEFROMCHAR, 0, ByVal m_Range.Start), ByVal 0 &) = m_Range.Start) And _
      //                (SendMessage(m_hWnd, EM_LINEINDEX, SendMessage(m_hWnd, EM_EXLINEFROMCHAR, 0, ByVal m_Range.end), ByVal 0 &) = m_Range.end)
      //mParent.SetSWL(blnSWL);

      //for reasons I don't understand, the built -in cut / copy / paste
      //functions of ITextRange don't handle ranges that end in
      //carriage returns correctly (including a strange glitch when
      //the range includes the last character of the text)

      ////solution is to use VB's clipboard object-no errors, no
      ////glitches; lines cut/copy/paste properly without having to
      ////do special checks/fixes
      ////have to make sure clipboard is cleared first
      //Clipboard.Clear();
      ////then set clipboard text
      //Clipboard.SetText(m_Range.Text, TextDataFormat.Text);
      ////then clear the range
      //m_Range.Text = "";

      //built -in cut function sucks
      m_Range.Cut(out pVar);
    }

    public void Copy(out object pVar)
    {
      //for reasons I don't understand, the built -in cut / copy / paste
      //functions of ITextRange don't handle ranges that end in
      //carriage returns correctly (including a strange glitch when
      //the range includes the last character of the text)

      //solution is to use VB's clipboard object-no errors, no
      //glitches; lines cut/copy/paste properly without having to
      //do special checks/fixes

      ////have to make sure clipboard is cleared first
      //Clipboard.Clear();
      ////then set clipboard text
      //Clipboard.SetText(m_Range.Text, TextDataFormat.Text);

      //built-in copy function sucks
      m_Range.Copy(out pVar);
    }

    public void Paste(ref object pVar, int Format)
    {
      int lngStartLine, lngEndLine = 0;
      string strText;
      bool blnAtEnd = false, blnFixFmt = false;
      int lngSLPos;


      //if locked (readonly), ignore the paste operation
      if (mParent.ReadOnly) {
        return;
      }
      ////for reasons I don't understand, the built -in cut / copy / paste
      ////functions of ITextRange don't handle ranges that end in
      ////carriage returns correctly (including a strange glitch when
      ////the range includes the last character of the text)

      ////solution is to use VB's clipboard object-no errors, only a
      ////few glitches; lines cut/copy/paste properly without having to
      ////do complicated special checks/fixes

      //// get clipboard text
      //strText = Clipboard.GetText(TextDataFormat.Text);
      ////if there is text on the clipboard
      //if (strText.Length > 0) {

    if (m_Range.CanPaste(ref pVar, Format) != 0) {
        // get starting line before paste starts
        lngStartLine = mParent.GetLineFromCharIndex(m_Range.Start);
        //position of line start
        lngSLPos = mParent.GetFirstCharIndexFromLine(lngStartLine);
        //if startpos is at start of line
        if (m_Range.Start == lngSLPos) {
          // set the flag
          blnAtEnd = true;
          // is startpos is at end of line
        }
        else if (m_Range.Start == lngSLPos + mParent.Lines[lngStartLine].Length) {
          blnAtEnd = true;
        }

        ////replace range text
        //m_Range.Text = strText;

        //built -in paste function sucks
        strText = (string)pVar;
        m_Range.Paste(0, Format);

        //if highlighting syntax

        if (mParent.Highlighting) {
          //need to format the newly pasted lines
          //(just the lines affected by paste)
          lngEndLine = mParent.GetLineFromCharIndex(m_Range.End);
          //if more than one line affected
          if (lngStartLine <= lngEndLine - 1) {
            blnFixFmt = true;
          }
          //if paste textdoesn't ends with a cr
          if (strText[strText.Length - 1] != '\r' && strText[strText.Length - 1] != '\n') {
            //edit the line with the end cursor
            //Debug.Print "edit12: "; lngEndLine
            mParent.EditLine(lngEndLine);
          }
          else {
            //if it does end with a CR -
            //if not pasted at beginning or end of line
            if (!blnAtEnd) {
              //format the last line, don't edit it
              //Debug.Print "fmt 11: "; lngEndLine
              //            .EditLine lngEndLine
              mParent.FormatLines(lngEndLine, lngEndLine);
            }
          }
        }
        //collapse selection to end (but ignore the selection change)
        mParent.noSelChange = true;
        m_Range.Start = m_Range.End;
        mParent.noSelChange = false;
        //force selstartline to this line
        mParent.SelStartLine = lngEndLine;
        mParent.SelEndLine = lngEndLine;

        //wth??? for some reason, if pasted text is comprised
        // of incomplete lines, and paste location is start of
        // a line, the formatlines function doesn't work;
        //so we have to do it here, after finishing
        if (mParent.Highlighting && blnFixFmt) {
          //Debug.Print "fmt 10: "; lngStartLine, lngEndLine - 1
          mParent.FormatLines(lngStartLine, lngEndLine - 1);
        }
      }
    }

    public int CanPaste(ref object pVar, int Format)
    {
      return m_Range.CanPaste(ref pVar, Format);
    }

    public int CanEdit()
    {
      return m_Range.CanEdit();
    }

    public void GetPoint(int Type, out int px, out int py)
    {
      m_Range.GetPoint(Type, out px, out py);
    }

    public void SetPoint(int x, int y, int Type, int Extend)
    {
      m_Range.SetPoint(x, y, Type, Extend);
    }

    public void ScrollIntoView(int Value)
    {
       m_Range.ScrollIntoView(Value);
    }

    public dynamic GetEmbeddedObject()
    {
      // not used
      return null;
    }

    public string Text { get => m_Range.Text; set => m_Range.Text = value; }
    public int Char { get => m_Range.Char; set => m_Range.Char = value; }

    public ITextRange Duplicate
    {
      get
      {
        return new TRange(m_hWnd, m_Doc, mParent);
      }
    }

    public ITextRange FormattedText { get => m_Range.FormattedText; set => m_Range.FormattedText = value; }
    public int Start { get => m_Range.Start; set => m_Range.Start = value; }
    public int End { get => m_Range.End; set => m_Range.End = value; }
    public ITextFont Font
    {
      get
      {
        //*// TODO:
        //// Create the Font object if it wasn't created yet
        //if (m_Font == null) {
        //  m_Font = new RichFont();
        //  m_Font.frFont = m_Range.Font;
        //}
        return m_Range.Font;
      }
      set
      {
        //? duplicate
        m_Range.Font = value.Duplicate;
      }
    }
    public ITextPara Para { get => m_Range.Para; set => m_Range.Para = value; }

    public int StoryLength => m_Range.StoryLength;

    public int StoryType => m_Range.StoryType;
  }
  public class Selection : ITextSelection
  {
    ITextSelection m_Sel;
    ITextDocument m_Doc;
    IntPtr m_hWnd;
    TRange m_Range;
    RichEdAGI mParent;

    public Selection(IntPtr hWnd, ITextSelection oSel, ITextDocument oDoc, RichEdAGI reParent)
    {
      m_hWnd = hWnd;
      m_Sel = oSel;
      m_Doc = oDoc;
      mParent = reParent;
    }

    public void Collapse(int bStart)
    {
      m_Sel.Collapse(bStart);
    }

    public int Expand(int Unit)
    {
      return m_Sel.Expand(Unit);
    }

    public int GetIndex(int Unit)
    {
      return m_Sel.GetIndex(Unit);
    }

    public void SetIndex(int Unit, int Index, int Extend)
    {
      m_Sel.SetIndex(Unit, Index, Extend);
    }

    public void SetRange(int cpActive, int cpOther)
    {
      m_Sel.SetRange(cpActive, cpOther);
    }

    public int InRange(ITextRange pRange)
    {
      return m_Sel.InRange(pRange);
    }

    public int InStory(ITextRange pRange)
    {
      return m_Sel.InStory(pRange);
    }

    public int IsEqual(ITextRange pRange)
    {
      return m_Sel.IsEqual(pRange);
    }

    public void Select()
    {
      // select the selection? seems odd, but OK...
      m_Sel.Select();
    }

    public int StartOf(int Unit, int Extend)
    {
      return m_Sel.StartOf(Unit, Extend);
    }

    public int EndOf(int Unit, int Extend)
    {
      return m_Sel.EndOf(Unit, Extend);
    }

    public int Move(int Unit, int Count)
    {
      return m_Sel.Move(Unit, Count);
    }

    public int MoveStart(int Unit, int Count)
    {
      return m_Sel.MoveStart(Unit, Count);
    }

    public int MoveEnd(int Unit, int Count)
    {
      return m_Sel.MoveEnd(Unit, Count);
    }

    public int MoveWhile(ref object Cset, int Count)
    {
      return m_Sel.MoveWhile(ref Cset, Count);
    }

    public int MoveStartWhile(ref object Cset, int Count)
    {
      return m_Sel.MoveStartWhile(ref Cset, Count);
    }

    public int MoveEndWhile(ref object Cset, int Count)
    {
      return m_Sel.MoveEndWhile(ref Cset, Count);
    }

    public int MoveUntil(ref object Cset, int Count)
    {
      return m_Sel.MoveUntil(ref Cset, Count);
    }

    public int MoveStartUntil(ref object Cset, int Count)
    {
      return m_Sel.MoveStartUntil(ref Cset, Count);
    }

    public int MoveEndUntil(ref object Cset, int Count)
    {
      return m_Sel.MoveEndUntil(ref Cset, Count);
    }

    public int FindText(string bstr, int cch, int Flags)
    {
      return m_Sel.FindText(bstr, cch, Flags);
    }

    public int FindTextStart(string bstr, int cch, int Flags)
    {
      return m_Sel.FindTextStart(bstr, cch, Flags);
    }

    public int FindTextEnd(string bstr, int cch, int Flags)
    {
      return FindTextEnd(bstr, cch, Flags);
    }

    public int Delete(int Unit, int Count)
    {
      return m_Sel.Delete(Unit, Count);
    }

    public void Cut(out object pVar)
    {
      m_Sel.Cut(out pVar);
    }

    public void Copy(out object pVar)
    {
      m_Sel.Copy(out pVar);
    }

    public void Paste(ref object pVar, int Format)
    {
      // ? use custom range paste? to make sure formatting occurs?
      m_Range.Paste(ref pVar, Format);
    }

    public int CanPaste(ref object pVar, int Format)
    {
      return m_Sel.CanPaste(ref pVar, Format);
    }

    public int CanEdit()
    {
      return m_Sel.CanEdit();
    }

    public void ChangeCase(int Type)
    {
      m_Sel.ChangeCase(Type);
    }

    public void GetPoint(int Type, out int px, out int py)
    {
      m_Sel.GetPoint(Type, out px, out py);
    }

    public void SetPoint(int x, int y, int Type, int Extend)
    {
      m_Sel.SetPoint(x, y, Type, Extend);
    }

    public void ScrollIntoView(int Value)
    {
      m_Sel.ScrollIntoView(Value);
    }

    public dynamic GetEmbeddedObject()
    {
      // not implemented
      return null;
    }

    public int MoveLeft(int Unit, int Count, int Extend)
    {
      return m_Sel.MoveLeft(Unit, Count, Extend);
    }

    public int MoveRight(int Unit, int Count, int Extend)
    {
      return m_Sel.MoveRight(Unit, Count, Extend);
    }

    public int MoveUp(int Unit, int Count, int Extend)
    {
      return m_Sel.MoveUp(Unit, Count, Extend);
    }

    public int MoveDown(int Unit, int Count, int Extend)
    {
      return m_Sel.MoveDown(Unit, Count, Extend);
    }

    public int HomeKey(int Unit, int Extend)
    {
      return m_Sel.HomeKey(Unit, Extend);
    }

    public int EndKey(int Unit, int Extend)
    {
      return m_Sel.EndKey(Unit, Extend);
    }

    public void TypeText(string bstr)
    {
      m_Sel.TypeText(bstr);
    }

    public string Text { get => m_Sel.Text; set => m_Sel.Text = value; }
    public int Char { get => m_Sel.Char; set => m_Sel.Char = value; }

    public ITextRange Duplicate => m_Sel.Duplicate;

    public ITextRange FormattedText { get => m_Sel.FormattedText; set => m_Sel.FormattedText = value; }
    public int Start { get => m_Sel.Start; set => m_Sel.Start = value; }
    public int End { get => m_Sel.End; set => m_Sel.End = value; }
    public ITextFont Font { get => m_Sel.Font; set => m_Sel.Font = value.Duplicate; }
    public ITextPara Para { get => m_Sel.Para; set => m_Sel.Para = value; }

    public int StoryLength => m_Sel.StoryLength;

    public int StoryType => m_Sel.StoryType;

    public int Flags { get => m_Sel.Flags; set => m_Sel.Flags = value; }

    public int Type => m_Sel.Type;
  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("00020D00-0000-0000-c000-000000000046")]
  public interface IRichEditOle
  {
    int GetClientSite(IntPtr lplpolesite);
    int GetObjectCount();
    int GetLinkCount();
    int GetObject(int iob, REOBJECT lpreobject, [MarshalAs(UnmanagedType.U4)] GetObjectOptions flags);
    int InsertObject(REOBJECT lpreobject);
    int ConvertObject(int iob, CLSID rclsidNew, string lpstrUserTypeNew);
    int ActivateAs(CLSID rclsid, CLSID rclsidAs);
    int SetHostNames(string lpstrContainerApp, string lpstrContainerObj);
    int SetLinkAvailable(int iob, int fAvailable);
    int SetDvaspect(int iob, uint dvaspect);
    int HandsOffStorage(int iob);
    int SaveCompleted(int iob, IntPtr lpstg);
    int InPlaceDeactivate();
    int ContextSensitiveHelp(int fEnterMode);
    //int GetClipboardData(CHARRANGE FAR * lpchrg, uint reco, IntPtr lplpdataobj);
    //int ImportDataObject(IntPtr lpdataobj, CLIPFORMAT cf, HGLOBAL hMetaPict);
  }

  public enum GetObjectOptions
  {
    REO_GETOBJ_NO_INTERFACES = 0x00000000,
    REO_GETOBJ_POLEOBJ = 0x00000001,
    REO_GETOBJ_PSTG = 0x00000002,
    REO_GETOBJ_POLESITE = 0x00000004,
    REO_GETOBJ_ALL_INTERFACES = 0x00000007,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct CLSID
  {
    public int a;
    public short b;
    public short c;
    public byte d;
    public byte e;
    public byte f;
    public byte g;
    public byte h;
    public byte i;
    public byte j;
    public byte k;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct SIZEL
  {
    public int x;
    public int y;
  }

  [StructLayout(LayoutKind.Sequential)]
  public class REOBJECT
  {
    public REOBJECT()
    {
    }

    public int cbStruct = Marshal.SizeOf(typeof(REOBJECT));   // Size of structure
    public int cp = 0;                        // Character position of object
    public CLSID clsid = new CLSID();               // Class ID of object
    public IntPtr poleobj = IntPtr.Zero;                // OLE object interface
    public IntPtr pstg = IntPtr.Zero;                 // Associated storage interface
    public IntPtr polesite = IntPtr.Zero;               // Associated client site interface
    public SIZEL sizel = new SIZEL();               // Size of object (may be 0,0)
    public uint dvaspect = 0;                   // Display aspect to use
    public uint dwFlags = 0;                    // Object status flags
    public uint dwUser = 0;                     // Dword for user's use
  }
} 

