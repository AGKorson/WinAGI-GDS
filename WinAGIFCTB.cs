using System.Collections.Generic;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using FastColoredTextBoxNS;

namespace WinAGI.Editor {
    public class WinAGIFCTB : FastColoredTextBox {
        public WinAGIFCTB() {
        }

        public bool NoMouse {
            get; set;
        }

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
            AGIToken nexttoken = new AGIToken();
            nexttoken.StartPos = token.EndPos;
            nexttoken.Line = token.Line;
            nexttoken.StartPos--;
            nexttoken.EndPos = nexttoken.StartPos;
            string strLine = Lines[token.Line];
            do {
                nexttoken.StartPos++;
                if (nexttoken.StartPos >= strLine.Length) {
                    if (multiline) {
                        nexttoken.Line++;
                        nexttoken.StartPos = 0;
                        nexttoken.EndPos = nexttoken.StartPos;
                        while (nexttoken.Line < LinesCount) {
                            strLine = Lines[nexttoken.Line];
                            if (strLine.Length == 0) {
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
                        nexttoken.StartPos = strLine.Length;
                        nexttoken.EndPos = nexttoken.StartPos;
                        return nexttoken;
                    }
                }
            } while (strLine[nexttoken.StartPos] <= (char)33);

            switch (strLine[nexttoken.StartPos]) {
            case '[':
                nexttoken.Type = AGITokenType.Comment;
                break;
            case '/':
                if (nexttoken.StartPos + 1 < strLine.Length && strLine[nexttoken.StartPos + 1] == '/') {
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
            case >= '0' and <= '9':
                nexttoken.Type = AGITokenType.Number;
                break;
            default:
                nexttoken.Type = AGITokenType.Identifier;
                break;
            }
            return GetTokenEnd(nexttoken, strLine);
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
            AGIToken prevtoken = new AGIToken();
            prevtoken.Line = token.Line;
            prevtoken.StartPos = token.StartPos;
            prevtoken.EndPos = prevtoken.StartPos;
            string strLine = Lines[token.Line];
            do {
                prevtoken.StartPos--;
                if (prevtoken.StartPos < 0) {
                    if (multiline) {
                        prevtoken.Line--;
                        while (prevtoken.Line >= 0) {
                            strLine = Lines[prevtoken.Line];
                            prevtoken.StartPos = strLine.Length - 1;
                            prevtoken.EndPos = prevtoken.StartPos;
                            if (strLine.Length == 0) {
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
            } while (strLine[prevtoken.StartPos] <= (char)33);
            Place prevplace = new();
            prevplace.iLine = prevtoken.Line;
            prevplace.iChar = prevtoken.StartPos;
            return TokenFromPos(prevplace);
        }

        /// <summary>
        /// Parses a line of text to determine the token type at the 
        /// specified position. The start argument is updated to the start
        /// position of the detected token.
        /// </summary>
        /// <param name="strLine"></param>
        /// <returns>Token type.
        ///</returns>
        private static AGITokenType ParseLine(string strLine, ref int start) {
            bool inQuote = false;
            bool slashcode = false;
            int tokenstart = 0;
            AGITokenType retval = AGITokenType.None;

            if (start >= strLine.Length) {
                return retval;
            }
            int i;
            // find line start
            for (i = start; i > 0; i--) {
                if (strLine[i - 1] == '\r' || strLine[i - 1] == '\n') {
                    break;
                }
            }
            for (; i < strLine.Length; i++) {
                // check for line end
                if (strLine[i] == '\r' || strLine[i] == '\n') {
                    break;
                }
                if (inQuote && slashcode) {
                    // ignore char after a slashcode that's inside a string
                    slashcode = false;
                }
                else {
                    switch (strLine[i]) {
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
                            if (i < strLine.Length && strLine[i + 1] == '/') {
                                start = i;
                                return AGITokenType.Comment;
                            }
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && strLine[i + 1] == '=') {
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
                            if (i < strLine.Length && strLine[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '&':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && strLine[i + 1] == '&') {
                                i++;
                            }
                        }
                        break;
                    case '*':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && strLine[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '+':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && (strLine[i + 1] == '+' || strLine[i + 1] == '=')) {
                                i++;
                            }
                        }
                        break;
                    case '-':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && (strLine[i + 1] == '-' || strLine[i + 1] == '=')) {
                                i++;
                            }
                        }
                        break;
                    case '>':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && (strLine[i + 1] == '=' || strLine[i + 1] == '<')) {
                                i++;
                            }
                        }
                        break;
                    case '=':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && (strLine[i + 1] == '=' || strLine[i + 1] == '<' || strLine[i + 1] == '>' || strLine[i + 1] == '@')) {
                                i++;
                            }
                        }
                        break;
                    case '<':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && (strLine[i + 1] == '=' || strLine[i + 1] == '>')) {
                                i++;
                            }
                        }
                        break;
                    case '@':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && strLine[i + 1] == '=') {
                                i++;
                            }
                        }
                        break;
                    case '|':
                        if (!inQuote) {
                            retval = AGITokenType.Symbol;
                            tokenstart = i;
                            if (i < strLine.Length && strLine[i + 1] == '|') {
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
                    case >= (char)48 and <= (char)57:
                        if (!inQuote) {
                            if (retval != AGITokenType.Number && retval != AGITokenType.Identifier) {
                                retval = AGITokenType.Number;
                                tokenstart = i;
                            }
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
                        // 35, 36, 37, 65-90, 95, 97-122, >127
                        // #   $   %   A-Z    _    a-z    extended
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
                    //39, 40, 41, 44, 46, 58, 59, 63, 92, 93, 94, 96, 123, 125, 126
                    switch (checkline[retval.StartPos]) {
                    //case '\'' or '(' or ')' or ',' or '.' or ':' or ';' or '?' or
                    //     '\\' or ']' or '^' or '`' or '{' or '}' or '~':
                    //    // always a single code
                    //    break;
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
                break;
            case AGITokenType.Number:
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

        public static AGIToken NextToken(string checkline, AGIToken token, bool includelinebreaks = false) {
            AGIToken nexttoken = new AGIToken();
            nexttoken.StartPos = token.EndPos;
            nexttoken.Line = token.Line;
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
            case >= '0' and <= '9':
                nexttoken.Type = AGITokenType.Number;
                break;
            default:
                nexttoken.Type = AGITokenType.Identifier;
                break;
            }
            return GetTokenEnd(nexttoken, checkline);
        }

        public static AGIToken NextToken(string checkline, int startPos, bool includelinebreaks = false) {
            // gets the next token in the specified text, allowing for line breaks
            AGIToken token = new() {
                StartPos = startPos,
                Line = 0
            };
            return NextToken(checkline, token, includelinebreaks);
        }
        
        public static AGIToken PreviousToken(string checkline, AGIToken token, bool includelinebreaks = false) {
            AGIToken prevtoken = new AGIToken();
            prevtoken.Line = token.Line;
            prevtoken.StartPos = token.StartPos;
            prevtoken.EndPos = prevtoken.StartPos;
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
            List<int> lines = new List<int>();
            lines.Add(line);
            base.RemoveLines(lines);
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
            FastColoredTextBoxNS.Range vr = VisibleRange;
            Text = newtext;
            Selection.Start = start;
            Selection.End = end;
            DoRangeVisible(vr);
        }

        public void ReplaceToken(AGIToken token, string newtext) {
            Place start = Selection.Start;
            Place end = Selection.End;
            FastColoredTextBoxNS.Range vr = VisibleRange;
            Selection.Start = token.Start;
            Selection.End = token.End;
            this.SelectedText = newtext;
            Selection.Start = start;
            Selection.End = end;
            DoRangeVisible(vr);
        }
    }
}
