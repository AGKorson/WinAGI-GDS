using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using WinAGI.Common;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public class WinAGIFCTB : FastColoredTextBox {
        public WinAGIFCTB() {
        }

        #region Properties
        public bool NoMouse {
            get; set;
        }
        #endregion

        #region Methods
        protected override void OnMouseMove(MouseEventArgs e) {
            if (!NoMouse) {
                base.OnMouseMove(e);
            }
        }

        /// <summary>
        /// Gets the token at the current selection position.
        /// </summary>
        /// <returns></returns>
        public AGIToken TokenFromPos() {
            if (Selection.Start <= Selection.End) {
                return TokenFromPos(Selection.Start);
            }
            else {
                return TokenFromPos(Selection.End);
            }
        }

        /// <summary>
        /// Identifies the token type at the specified location in a textbox
        /// range.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="start"></param>
        /// <returns>Token information as AGIToken object.</returns>
        public AGIToken TokenFromPos(Place start) {
            string checkline = Lines[start.iLine];
            AGIToken retval = TokenFromPos(checkline, start.iChar);
            retval.Line = start.iLine;
            return retval;
        }

        /// <summary>
        /// Gets the token immediately following the passed token. If multiline is
        /// true it will search past end of line to next line.
        /// </summary>
        /// <param name="fctb"></param>
        /// <param name="token"></param>
        /// <param name="multiline"></param>
        /// <returns></returns>
        public AGIToken NextToken(AGIToken token, bool multiline = false) {
            AGIToken nexttoken = new() {
                StartPos = token.EndPos,
                Line = token.Line
            };
            nexttoken.StartPos--;
            nexttoken.EndPos = nexttoken.StartPos;
            string linetext = Lines[token.Line];
            do {
                nexttoken.StartPos++;
                if (nexttoken.StartPos >= linetext.Length) {
                    if (multiline) {
                        nexttoken.Line++;
                        nexttoken.StartPos = 0;
                        nexttoken.EndPos = nexttoken.StartPos;
                        while (nexttoken.Line < LinesCount) {
                            linetext = Lines[nexttoken.Line];
                            if (linetext.Length == 0) {
                                nexttoken.Line++;
                            }
                            else {
                                break;
                            }
                        }
                        if (nexttoken.Line >= LinesCount) {
                            // end of text reached; no token found
                            return nexttoken;
                        }
                    }
                    else {
                        // end of line reached; no token found
                        nexttoken.StartPos = linetext.Length;
                        nexttoken.EndPos = nexttoken.StartPos;
                        return nexttoken;
                    }
                }
            } while (linetext[nexttoken.StartPos] <= (char)33);

            switch (linetext[nexttoken.StartPos]) {
            case '[':
                nexttoken.Type = AGITokenType.Comment;
                break;
            case '/':
                if (nexttoken.StartPos + 1 < linetext.Length && linetext[nexttoken.StartPos + 1] == '/') {
                    nexttoken.Type = AGITokenType.Comment;
                }
                else {
                    nexttoken.Type = AGITokenType.Symbol;
                }
                break;
            case '"':
                nexttoken.Type = AGITokenType.String;
                break;
            case '!' or '&' or '\'' or '(' or ')' or '*' or
                   '+' or ',' or '-' or '.' or ':' or ';' or
                   '>' or '=' or '<' or '?' or '@' or '\\' or
                   ']' or '^' or '`' or '{' or '|' or '}' or '~':
                nexttoken.Type = AGITokenType.Symbol;
                break;
            //case >= '0' and <= '9':
            //    nexttoken.Type = AGITokenType.Number;
            //    break;
            default:
                // numbers are considered identifiers until verified
                // after token is built
                nexttoken.Type = AGITokenType.Identifier;
                break;
            }
            return GetTokenEnd(nexttoken, linetext);
        }

        /// <summary>
        /// Gets the token immediately preceding the passed token. If multiline is
        /// true it will search past start of line to previous line.
        /// </summary>
        /// <param name="fctb"></param>
        /// <param name="token"></param>
        /// <param name="multiline"></param>
        /// <returns></returns>
        public AGIToken PreviousToken(AGIToken token, bool multiline = false) {
            AGIToken prevtoken = new() {
                Line = token.Line,
                StartPos = token.StartPos,
                EndPos = token.EndPos,
            };
            string linetext = Lines[token.Line];
            do {
                prevtoken.StartPos--;
                if (prevtoken.StartPos < 0) {
                    if (multiline) {
                        prevtoken.Line--;
                        while (prevtoken.Line >= 0) {
                            linetext = Lines[prevtoken.Line];
                            prevtoken.StartPos = linetext.Length - 1;
                            prevtoken.EndPos = prevtoken.StartPos;
                            if (linetext.Length == 0) {
                                prevtoken.Line--;
                            }
                            else {
                                break;
                            }
                        }
                        if (prevtoken.Line < 0) {
                            // end of text reached; no token found
                            return prevtoken;
                        }
                    }
                    else {
                        // beginning of line reached; no token found
                        prevtoken.StartPos = -1;
                        prevtoken.EndPos = -1;
                        return prevtoken;
                    }
                }
            } while (linetext[prevtoken.StartPos] <= (char)33);
            Place prevplace = new() {
                iLine = prevtoken.Line,
                iChar = prevtoken.StartPos
            };
            return TokenFromPos(prevplace);
        }

        /// <summary>
        /// Gets the starting position of the specified token text in this fctb.
        /// </summary>
        /// <param name="sourcetext"></param>
        /// <param name="tokentext"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        public int FindTokenPos(string tokentext, int startpos = 0) {
            do {
                startpos = Text.IndexOf(tokentext, startpos);
                if (startpos != -1) {
                    // if found, confirm it's a token, and not part of a larger token
                    AGIToken token = TokenFromPos(PositionToPlace(startpos));
                    if (token.Text == tokentext) {
                        // found
                        break;
                    }
                    // try again
                    startpos++;
                }
            } while (startpos != -1);
            return startpos;
        }

        internal void RemoveLine(int line) {
            Place start = Selection.Start;
            if (start.iLine > line) {
                start.iLine--;
            }
            else if (start.iLine == line) {
                start.iChar = 0;
            }

            Place end = Selection.End;
            if (end.iLine > line) {
                end.iLine--;
            }
            else if (end.iLine == line) {
                end.iChar = 0;
            }
            List<int> lines = [line];
            RemoveLines(lines);
            Selection.Start = start;
            Selection.End = end;
        }

        internal void InsertLine(int line, string text) {
            Place start = Selection.Start;
            if (start.iLine > line) {
                start.iLine++;
            }
            Place end = Selection.End;
            if (end.iLine > line) {
                end.iLine++;
            }
            Selection.Start = Selection.End = new Place(0, line);
            InsertText(text + "\n", false);
            Selection.Start = start;
            Selection.End = end;
        }

        internal void SelectLine(int iLine) {
            try {
                Selection.Start = new(0, iLine);
                Selection.End = new(GetLineLength(iLine), iLine);
            }
            catch {
                // ignore errors
            }
        }

        public void UpdateText(string newtext) {
            Place start = Selection.Start;
            Place end = Selection.End;
            Text = newtext;
            Selection.Start = start;
            Selection.End = end;
            DoRangeVisible(VisibleRange);
        }

        public void ReplaceToken(AGIToken token, string newtext) {
            Place start = Selection.Start;
            Place end = Selection.End;
            Selection.Start = token.Start;
            Selection.End = token.End;
            SelectedText = newtext;
            Selection.Start = start;
            Selection.End = end;
            DoRangeVisible(VisibleRange);
        }

        public void ReplaceText(int startpos, int length, string newtext) {
            Place start = Selection.Start;
            Place end = Selection.End;
            Selection.Start = PositionToPlace(startpos);
            Selection.End = PositionToPlace(startpos + length);
            SelectedText = newtext;
            Selection.Start = start;
            Selection.End = end;
            DoRangeVisible(VisibleRange);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Parses a line of text to determine the token type at the 
        /// specified position. The start argument is updated to the start
        /// position of the detected token.
        /// </summary>
        /// <param name="linetext"></param>
        /// <returns>Token type.
        /// </returns>
        private static AGITokenType ParseLine(string linetext, ref int start) {
            bool inQuote = false;
            bool slashcode = false;
            int tokenstart = 0;
            AGITokenType retval = AGITokenType.None;

            if (start >= linetext.Length) {
                return retval;
            }
            int i;
            // find line start
            for (i = start; i > 0; i--) {
                if (linetext[i - 1] == '\r' || linetext[i - 1] == '\n') {
                    break;
                }
            }
            for (; i < linetext.Length; i++) {
                // check for line end
                if (linetext[i] == '\r' || linetext[i] == '\n') {
                    break;
                }
                if (inQuote && slashcode) {
                    // ignore char after a slashcode that's inside a string
                    slashcode = false;
                }
                else {
                    switch (linetext[i]) {
                    case '"':
                        inQuote = !inQuote;
                        if (inQuote) {
                            retval = AGITokenType.String;
                            tokenstart = i;
                        }
                        break;
                    case '\\':
                        if (inQuote) {
                            // start a slashcode
                            slashcode = true;
                        }
                        else {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                        }
                        break;
                    case '/':
                        if (!inQuote) {
                            if (i + 1 < linetext.Length && linetext[i + 1] == '/') {
                                start = i;
                                return AGITokenType.Comment;
                            }
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '[':
                        if (!inQuote) {
                            start = i;
                            return AGITokenType.Comment;
                        }
                        break;
                    case '!':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '&':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '&') {
                                i++;
                            }
                        }
                        break;
                    case '*':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '+':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && (linetext[i + 1] == '+' || linetext[i + 1] == '=')) {
                                i++;
                            }
                        }
                        break;
                    case '-':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && (linetext[i + 1] == '-' || linetext[i + 1] == '=')) {
                                i++;
                            }
                        }
                        break;
                    case '>':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && (linetext[i + 1] == '=' || linetext[i + 1] == '<')) {
                                i++;
                            }
                        }
                        break;
                    case '=':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && (linetext[i + 1] == '=' || linetext[i + 1] == '<' || linetext[i + 1] == '>' || linetext[i + 1] == '@')) {
                                i++;
                            }
                        }
                        break;
                    case '<':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && (linetext[i + 1] == '=' || linetext[i + 1] == '>')) {
                                i++;
                            }
                        }
                        break;
                    case '@':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '|':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i + 1 < linetext.Length && linetext[i + 1] == '|') {
                                i++;
                            }
                        }
                        break;
                    case '\'' or '(' or ')' or ',' or ':' or ';' or '?' or ']' or
                         '^' or '`' or '{' or '}' or '~':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                        }
                        break;
                    case <= (char)33:
                        if (!inQuote) {
                            retval = AGITokenType.None;
                            tokenstart = i;
                        }
                        break;
                    case '.':
                        if (!inQuote) {
                            if (retval != AGITokenType.Identifier) {
                                retval = AGITokenType.Symbol;
                                tokenstart = i;
                            }
                        }
                        break;
                    default:
                        // 35, 36, 37, 48-57, 65-90, 95, 97-122, >127
                        // #   $   %   0-9    A-Z    _    a-z    extended
                        // numbers are consdered identifiers until verified
                        // after token is built
                        if (!inQuote) {
                            if (retval != AGITokenType.Identifier) {
                                retval = AGITokenType.Identifier;
                                tokenstart = i;
                            }
                        }
                        break;
                    }
                }
                if (i >= start) {
                    break;
                }
            }
            start = tokenstart;
            return retval;
        }

        private static AGIToken GetTokenEnd(AGIToken retval, string checkline) {
            int endpos = retval.StartPos + 1;
            switch (retval.Type) {
            case AGITokenType.Comment:
                while (endpos < checkline.Length) {
                    // check for line end
                    if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                        break;
                    }
                    endpos++;
                }
                break;
            case AGITokenType.String:
                bool slashcode = false;
                while (endpos < checkline.Length) {
                    // check for line end
                    if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                        break;
                    }
                    if (slashcode) {
                        // ignore char after slashcode
                        slashcode = false;
                    }
                    else {
                        if (checkline[endpos] == '\\') {
                            slashcode = true;
                        }
                        if (checkline[endpos] == '"') {
                            endpos++;
                            break;
                        }
                    }
                    endpos++;
                }
                break;
            case AGITokenType.Symbol:
                if (endpos < checkline.Length) {
                    // check for line end
                    if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                        break;
                    }
                    switch (checkline[retval.StartPos]) {
                    case '!':
                        // ! or !=
                        if (checkline[endpos] == '=') {
                            endpos++;
                        }
                        break;
                    case '&':
                        // & or &&
                        if (checkline[endpos] == '&') {
                            endpos++;
                        }
                        break;
                    case '*':
                        // * or *=
                        if (checkline[endpos] == '=') {
                            endpos++;
                        }
                        break;
                    case '+':
                        // + or ++ or +=
                        if ((checkline[endpos] == '+' || checkline[endpos] == '=')) {
                            endpos++;
                        }
                        break;
                    case '-':
                        // -## is a number
                        if (checkline[endpos] >= '0' && checkline[endpos] <= '9') {
                            retval.Type = AGITokenType.Number;
                            while (endpos < checkline.Length) {
                                // check for line end
                                if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                                    break;
                                }
                                char c = checkline[endpos];
                                if (c is not >= ((char)48) or not <= ((char)57)) {
                                    break;
                                }
                                endpos++;
                            }
                            break;
                        }
                        // - or -- or -=
                        if (checkline[endpos] == '-' || checkline[endpos] == '=') {
                            endpos++;
                        }
                        break;
                    case '/':
                        // / or /=
                        if (checkline[endpos] == '=') {
                            endpos++;
                        }
                        break;
                    case '>':
                        // > or >= or ><
                        if ((checkline[endpos] == '=' || checkline[endpos] == '<')) {
                            endpos++;
                        }
                        break;
                    case '=':
                        // = or == or =< or => or =@
                        switch (checkline[endpos]) {
                        case '=' or '<' or '>' or '@':
                            endpos++;
                            break;
                        }
                        break;
                    case '<':
                        // < or <= or <>
                        if ((checkline[endpos] == '=' || checkline[endpos] == '>')) {
                            endpos++;
                        }
                        break;
                    case '|':
                        // | or ||
                        if (checkline[endpos] == '|') {
                            endpos++;
                        }
                        break;
                    case '@':
                        // @ or @=
                        if (checkline[endpos] == '=') {
                            endpos++;
                        }
                        break;
                    }
                }
                // these are always single character tokens
                // 39, 40, 41, 44, 46, 58, 59, 63, 92, 93, 94, 96, 123, 125, 126
                // '  (  )  ,  .  :  ;  ?  \  ]  ^  `  {  }  ~
                break;
            case AGITokenType.Number:
                Debug.Assert(false);
                break;
            case AGITokenType.Identifier:
                bool done = false;
                while (endpos < checkline.Length) {
                    // check for line end
                    if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                        break;
                    }
                    char c = checkline[endpos];
                    switch (c) {
                    case >= 'A' and <= 'Z':
                    case >= 'a' and <= 'z':
                    case >= '0' and <= '9':
                    case '_' or '$' or '#' or '%' or '.':
                    case > (char)127:
                        break;
                    default:
                        done = true;
                        break;
                    }
                    if (done) {
                        break;
                    }
                    endpos++;
                }
                break;
            case AGITokenType.None:
                while (endpos < checkline.Length) {
                    // check for line end
                    if (checkline[endpos] == '\r' || checkline[endpos] == '\n') {
                        break;
                    }
                    if (checkline[endpos] > 32) {
                        break;
                    }
                    endpos++;
                }
                break;
            }
            retval.EndPos = endpos;
            retval.Text = checkline[retval.StartPos..endpos];
            if (retval.Type == AGITokenType.Identifier) {
                // check for numbers
                if (retval.Text.IsInt()) {
                    retval.Type = AGITokenType.Number;
                }
            }
            return retval;
        }

        public static AGIToken TokenFromPos(string checkline, int start) {
            // strategy:
            // - check if in a comment ([ or //)
            // - if not, check if in a string (characters inside quotes)
            // - check if in white space (pos a space or tab, or at end of line)
            // - next check for two-char symbols: != && *= ++ += -- -= /= >= >< =< => ||
            // - then check for single char symbols: ! & ' ( ) * + , - / : ; > = < ? \ ] ^ ` { | } ~
            // - must be an identifier; search backward to white space to find start 
            //   (adjusting for non-allowed start characters) and foward to find end 
            AGIToken retval = new AGIToken {
                Line = 0,
                StartPos = start,
                EndPos = start
            };
            int startpos = start;
            if (startpos >= checkline.Length) {
                return retval;
            }
            // check for comment
            retval.Type = ParseLine(checkline, ref startpos);
            retval.StartPos = startpos;
            return GetTokenEnd(retval, checkline);
        }

        public static AGIToken NextToken(string checkline, int startPos, bool includelinebreaks = false) {
            // gets the next token in the specified text, allowing for line breaks
            AGIToken token = new() {
                StartPos = startPos,
                EndPos = startPos,
                Line = 0
            };
            return NextToken(checkline, token, includelinebreaks);
        }

        public static AGIToken NextToken(string checkline, AGIToken token, bool includelinebreaks = false) {
            AGIToken nexttoken = new() {
                StartPos = token.EndPos,
                Line = token.Line
            };
            nexttoken.StartPos--;
            nexttoken.EndPos = nexttoken.StartPos;
            do {
                nexttoken.StartPos++;
                if (nexttoken.StartPos >= checkline.Length) {
                    // end of line reached; no token found
                    nexttoken.StartPos = checkline.Length;
                    nexttoken.EndPos = nexttoken.StartPos;
                    nexttoken.Type = AGITokenType.None;
                    return nexttoken;
                }
                // when includelinebreaks is true, line breaks are returned as tokens
                if (includelinebreaks) {
                    // return '\r' or '\n' or "\r\n" as a token
                    if (checkline[nexttoken.StartPos] == '\r') {
                        nexttoken.Type = AGITokenType.LineBreak;
                        nexttoken.EndPos = nexttoken.StartPos + 1;
                        if (nexttoken.EndPos < checkline.Length && checkline[nexttoken.EndPos] == '\n') {
                            nexttoken.EndPos++;
                        }
                        return nexttoken;
                    }
                    else if (checkline[nexttoken.StartPos] == '\n') {
                        nexttoken.Type = AGITokenType.LineBreak;
                        nexttoken.EndPos = nexttoken.StartPos + 1;
                        return nexttoken;
                    }
                }
            } while (checkline[nexttoken.StartPos] <= (char)33);

            switch (checkline[nexttoken.StartPos]) {
            case '[':
                nexttoken.Type = AGITokenType.Comment;
                break;
            case '/':
                if (nexttoken.StartPos + 1 < checkline.Length && checkline[nexttoken.StartPos + 1] == '/') {
                    nexttoken.Type = AGITokenType.Comment;
                }
                else {
                    nexttoken.Type = AGITokenType.Symbol;
                }
                break;
            case '"':
                nexttoken.Type = AGITokenType.String;
                break;
            case '!' or '&' or '\'' or '(' or ')' or '*' or
                   '+' or ',' or '-' or '.' or ':' or ';' or
                   '>' or '=' or '<' or '?' or '@' or '\\' or
                   ']' or '^' or '`' or '{' or '|' or '}' or '~':
                nexttoken.Type = AGITokenType.Symbol;
                break;
            //case >= '0' and <= '9':
            //    nexttoken.Type = AGITokenType.Number;
            //    break;
            default:
                // numbers are considered identifiers until verified
                // after token is built
                nexttoken.Type = AGITokenType.Identifier;
                break;
            }
            return GetTokenEnd(nexttoken, checkline);
        }

        public static AGIToken PreviousToken(string checkline, AGIToken token, bool includelinebreaks = false) {
            AGIToken prevtoken = new AGIToken {
                Line = token.Line,
                StartPos = token.StartPos,
                EndPos = token.StartPos
            };
            do {
                prevtoken.StartPos--;
                if (prevtoken.StartPos < 0) {
                    // beginning of line reached; no token found
                    prevtoken.StartPos = -1;
                    prevtoken.EndPos = -1;
                    prevtoken.Type = AGITokenType.None;
                    return prevtoken;
                }
                // when includelinebreaks is true, line breaks are returned as tokens
                if (includelinebreaks) {
                    // return '\r' or '\n' or "\r\n" as a token
                    if (checkline[prevtoken.StartPos] == '\n') {
                        prevtoken.Type = AGITokenType.LineBreak;
                        prevtoken.EndPos = prevtoken.StartPos + 1;
                        if (prevtoken.StartPos > 0 && checkline[prevtoken.StartPos - 1] == '\r') {
                            prevtoken.StartPos--;
                        }
                        return prevtoken;
                    }
                    else if (checkline[prevtoken.StartPos] == '\r') {
                        prevtoken.Type = AGITokenType.LineBreak;
                        prevtoken.EndPos = prevtoken.StartPos + 1;
                        return prevtoken;
                    }
                }
            } while (checkline[prevtoken.StartPos] <= (char)33);
            return TokenFromPos(checkline, prevtoken.StartPos);
        }

        /// <summary>
        /// Gets the starting position of the specified token text.
        /// </summary>
        /// <param name="sourcetext"></param>
        /// <param name="tokentext"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        public static int FindTokenPos(string sourcetext, string tokentext, int startpos = 0) {
            do {
                startpos = sourcetext.IndexOf(tokentext, startpos);
                if (startpos != -1) {
                    // if found, confirm it's a token, and not part of a larger token
                    AGIToken token = TokenFromPos(sourcetext, startpos);
                    if (token.Text == tokentext) {
                        // found
                        break;
                    }
                    // try again
                    startpos++;
                }
            } while (startpos != -1);
            return startpos;
        }

        /// <summary>
        /// Gets the starting position of the specified token text, searching
        /// backwards from the specified start position.
        /// </summary>
        /// <param name="sourcetext"></param>
        /// <param name="tokentext"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        public static int FindTokenPosRev(string sourcetext, string tokentext, int startpos = -1) {
            do {
                if (startpos == -1) {
                    startpos = sourcetext.Length - 1;
                }
                startpos = sourcetext.LastIndexOf(tokentext, startpos);
                if (startpos != -1) {
                    // if found, confirm it's a token, and not part of a larger token
                    AGIToken token = TokenFromPos(sourcetext, startpos);
                    if (token.Text == tokentext) {
                        // found
                        break;
                    }
                    // try again
                    startpos--;
                }
            } while (startpos != -1);
            return startpos;
        }
        #endregion
    }
}
