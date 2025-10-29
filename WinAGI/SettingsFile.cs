using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.LogicDecoder;

namespace WinAGI.Common {

    /// <summary>
    /// A class to manage a settings file. It handles opening, adding and deleting sections
    /// and key/value pairs. Sections can be assigned to groups to keep the file organized.
    /// </summary>
    public class SettingsFile {
        // elements of a settings list file:
        //
        //  #comments begin with hashtag; all characters on line after hashtag are ignored
        //  comments can be added to end of valid section or key / value line;
        //  NO QUOTES allowed in comments
        //  blank lines are ignored
        //  [::BEGIN group::] marker to indicate a group of sections
        //  [::END group::]   marker to indicate end of a group
        //  [section]         sections indicated by square brackets; anything else on the
        //  key=value         line gets ignored key/value pairs separated by an equal sign;
        //  key="a value"     no quotes around values means only single word; use quotes
        //                    for multiword strings, or for values that include '#'
        //  in strings control codes are '\n' for new line, '\\' for backslash 

        #region Members
        public List<string> Lines = [];
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new settings list. Allowable options for mode are FileMode.Create,
        /// FileMode.OpenOrCreate, or FileMode.Open. If filename is not valid, or if file
        /// is readonly, an excpetion is thrown.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mode"></param>
        public SettingsFile(string filename, FileMode mode) {
            FileStream fsConfig;
            StreamWriter swConfig;
            StreamReader srConfig;

            ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));
            // save filename
            Filename = filename;

            // check mode to determine how to open the file
            switch (mode) {
            case FileMode.Create:
                // create newfile, overwiting if it already exists
                break;
            case FileMode.OpenOrCreate:
                // open if it already exists, otherwise create
                if (File.Exists(filename)) {
                    // existing file can't be write-protected
                    if ((File.GetAttributes(filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                        WinAGIException wex = new(LoadResString(539).Replace(ARG1, Filename)) {
                            HResult = WINAGI_ERR + 539,
                        };
                        wex.Data["badfile"] = Filename;
                        throw wex;
                    }
                }
                break;
            case FileMode.Open:
                // open (file must already exist)
                
                if (!File.Exists(filename)) {
                    throw new FileNotFoundException("settins file not found", nameof(filename));
                }
                // file can't be readonly
                if ((File.GetAttributes(filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    WinAGIException wex = new(LoadResString(539).Replace(ARG1, Filename)) {
                        HResult = WINAGI_ERR + 539,
                    };
                    wex.Data["badfile"] = Filename;
                    throw wex;
                }
                break;
            default:
                // not valid
                throw new ArgumentException("invalid file mode");
            }
            try {
                // open the config file in desired mode
                fsConfig = new FileStream(Filename, mode);
            }
            catch (Exception ex) {
                WinAGIException wex = new(LoadResString(502).Replace(
                    ARG1, ex.HResult.ToString()).Replace(
                    ARG2, Filename)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = ex;
                wex.Data["badfile"] = Filename;
                throw wex;
            }
            // if this is an empty file (either previously empty or created by this call)
            if (fsConfig.Length == 0) {
                swConfig = new StreamWriter(fsConfig);
                // add a single comment to the list
                Lines.Add("#");
                // and write it to the file
                swConfig.WriteLine("#");
                swConfig.Dispose();
            }
            else {
                // grab the file data
                srConfig = new StreamReader(fsConfig);
                while (!srConfig.EndOfStream) {
                    Lines.Add(srConfig.ReadLine());
                }
                srConfig.Dispose();
            }
            fsConfig.Dispose();
        }

        /// <summary>
        /// Creates a new settings list without an attached file. Before saving, a filename must
        /// be assigned.
        /// </summary>
        public SettingsFile() {
            //
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the filename for the settings file.
        /// </summary>
        public string Filename {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds or updates a key/value pair of type string. 
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteSetting(string Section, string Key, string Value, string Group = "") {
            int i;
            string strLine;
            int lngSectionEnd = 0;
            bool blnFound = false;
            int lngInsertLine;

            // format the string before writing it
            string strCheck = Value is null ? "\"\"" : FormatValue(Value.ToString());
            // need at least one line...
            if (Lines.Count == 0) {
                // just add the line and exit
                Lines.Add("#");
                Lines.Add("");
                if (Group.Length > 0) {
                    Lines.Add("[::BEGIN " + Group + "::]");
                }
                Lines.Add("[" + Section + "]");
                Lines.Add("   " + Key + " = " + strCheck);
                if (Group.Length > 0) {
                    Lines.Add("[::END " + Group + "::]");
                }
                Lines.Add("");
                //  done
                return;
            }
            // if a group is provided, we add new items inside the group;
            // existing items will be left where they are
            int lngGrpStart = -1;
            int lngGrpEnd = -1;
            int lngPos = -1;
            if (Group.Length > 0) {
                for (i = 1; i <= Lines.Count - 1; i++) {
                    // skip blanks, and lines starting with a comment
                    strLine = Lines[i].Replace("\t", " ").Trim();
                    if (strLine.Length > 0) {
                        if (strLine[0] != '#') {
                            // if not found yet, look for the starting marker
                            if (!blnFound) {
                                if (strLine.Equals("[::BEGIN " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                    lngGrpStart = i;
                                    blnFound = true;
                                    // if start found after end we are done
                                    if (lngGrpEnd >= 0) {
                                        break;
                                    }
                                }
                            }
                            else {
                                // start has been found; check for another group start
                                if (strLine.StartsWith("[::BEGIN ", StringComparison.OrdinalIgnoreCase)) {
                                    // mark position of first new start, so we can move the end marker here
                                    if (lngPos < 0) {
                                        lngPos = i;
                                    }
                                }
                            }
                            // check for end marker even if start not found yet
                            // just in case they are backwards
                            if (strLine.Equals("[::END " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                lngGrpEnd = i;
                                // if we also have a start, we can exit the loop
                                if (blnFound) {
                                    break;
                                }
                            }
                        }
                    }
                }
                // possible outcomes:
                //  - start and end both found; start before end
                //    this is what we want
                //
                //  - start and end both found, but end before start
                //    this is backwards; we fix by moving end to
                //    the line after start
                //
                //  - start found, but no end; we add end
                //    to just before next group start, or
                //    to end of file if no other group start
                //
                //  - end found, but no start; we fix by putting
                //    start right in front of end
                //
                //  - neither found; add the group to the end

                // if both found
                if (lngGrpStart >= 0 && lngGrpEnd >= 0) {
                    // if backwards, move end to line after start
                    if (lngGrpEnd < lngGrpStart) {
                        Lines.Insert(lngGrpStart + 1, Lines[lngGrpEnd]);
                        Lines.RemoveAt(lngGrpEnd);
                        lngGrpStart -= 1;
                        lngGrpEnd = lngGrpStart + 1;
                    }
                }
                // if only start found
                else if (lngGrpStart >= 0) {
                    // if there was another start found, insert end there
                    if (lngPos > 0) {
                        Lines.Insert(lngPos, "[::END " + Group + "::]");
                        Lines.Insert(lngPos + 1, "");
                        lngGrpEnd = lngPos;
                    }
                    else {
                        // otherwise insert group end at end of file
                        lngGrpEnd = Lines.Count;
                        Lines.Add("[::END " + Group + "::]");
                    }
                }
                // if end found but no start
                else if (lngGrpEnd >= 0) {
                    // insert start in front of end
                    lngGrpStart = lngGrpEnd;
                    Lines.Insert(lngGrpStart, "[::START " + Group + "::]");
                    lngGrpEnd = lngGrpStart + 1;
                }
                else {
                    // means neither found
                    // make sure at least one blank line
                    if (Lines[^1].Trim().Length > 0) {
                        Lines.Add("");
                    }
                    lngGrpStart = Lines.Count;
                    Lines.Add("[::BEGIN " + Group + "::]");
                    Lines.Add("[::END " + Group + "::]");
                    lngGrpEnd = lngGrpStart + 1;
                }
            }
            // find the section we are looking for
            int lngSectionStart = FindSettingSection(Section);
            // if not found, create it at end of group (if group is provided)
            // otherwise at end of list
            if (lngSectionStart < 0) {
                if (lngGrpStart >= 0) {
                    lngInsertLine = lngGrpEnd;
                }
                else {
                    lngInsertLine = Lines.Count;
                }
                // make sure there is at least one blank line (unless this is first line in list)
                if (lngInsertLine > 0) {
                    if (Lines[lngInsertLine - 1].Trim().Length != 0) {
                        Lines.Insert(lngInsertLine, "");
                        lngInsertLine++;
                    }
                }
                Lines.Insert(lngInsertLine, "[" + Section + "]");
                Lines.Insert(lngInsertLine + 1, "   " + Key + " = " + strCheck);
                //  done
                return;
            }
            // search section; find matching key
            blnFound = false;
            for (i = lngSectionStart + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0 && strLine[0] != '#') {
                    // if another section is found, stop here
                    if (strLine[0] == '[') {
                        // if part of a group; last line of the section
                        // is line prior to the new section
                        if (lngGrpStart >= 0) {
                            lngSectionEnd = i - 1;
                        }
                        // if not already added, add it now
                        if (!blnFound) {
                            // back up until a nonblank line is found
                            for (lngPos = i - 1; lngPos >= lngSectionStart; lngPos--) {
                                if (Lines[lngPos].Trim().Length > 0) {
                                    break;
                                }
                            }
                            // add the key and value at this pos
                            Lines.Insert(lngPos + 1, "   " + Key + " = " + strCheck);
                            // this also bumps down the section end
                            lngSectionEnd++;
                            // it also may bump down group start/end
                            if (lngGrpStart >= lngPos + 1) {
                                lngGrpStart++;
                            }
                            if (lngGrpEnd >= lngPos + 1) {
                                lngGrpEnd++;
                            }
                        }
                        // we are done, but if part of a group we need to verify the
                        // section is in the group
                        if (lngGrpStart >= 0) {
                            blnFound = true;
                            break;
                        }
                        else {
                            return;
                        }
                    }
                    // if not already found, look for key
                    if (!blnFound) {
                        if (strLine.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (strLine.Mid(Key.Length, 1) == " " || strLine.Mid(Key.Length, 1) == "=")) {
                            // remove key
                            strLine = strLine[Key.Length..].Trim();
                            if (strLine.Length > 0) {
                                // expect an equal sign
                                if (strLine[0] == '=') {
                                    // remove it
                                    strLine = strLine[1..].Trim();
                                }
                            }
                            if (strLine.Length > 0) {
                                if (strLine[0] == '"') {
                                    // string delimiter; find ending delimiter
                                    lngPos = strLine.IndexOf('"', 1) + 1;
                                }
                                else {
                                    // look for comment marker
                                    lngPos = strLine.IndexOf('#', 1);
                                    // if none found, look for space delimiter
                                    if (lngPos == -1) {
                                        // look for a space as a delimiter
                                        lngPos = strLine.IndexOf(' ', 1);
                                    }
                                }
                                if (lngPos <= 0) {
                                    // no delimiter found; assume entire line is value
                                    lngPos = strLine.Length;
                                }
                                // now strip off the old value, leaving potential comment
                                strLine = strLine[lngPos..].Trim();
                            }
                            // if something left, maks sure it's a comment
                            if (strLine.Length > 0) {
                                if (strLine[0] != '#') {
                                    strLine = "#" + strLine;
                                }
                                // make sure it starts with white space
                                strLine = "   " + strLine;
                            }
                            // now update line with new value
                            strLine = "   " + Key + " = " + strCheck + strLine;
                            Lines[i] = strLine;
                            // we are done, but if part of a group
                            // we need to keep going to find end so
                            // we can validate section is in the group
                            if (lngGrpStart >= 0) {
                                blnFound = true;
                            }
                            else {
                                return;
                            }
                        }
                    }
                }
            }
            // if not found (will only happen if this the last section in the
            // list, probably NOT in a group, but still possible (if the
            // section is outside the defined group)
            if (!blnFound) {
                // back up until a nonblank line is found
                for (lngPos = i - 1; i >= lngSectionStart; i--) {
                    if (Lines[lngPos].Trim().Length > 0) {
                        break;
                    }
                }
                // add the key and value at this pos
                Lines.Insert(lngPos + 1, "   " + Key + " = " + strCheck);
                if (lngGrpStart < 0) {
                    // no group - all done
                    return;
                }
                // note that we don't need to bother adjusting group
                // start/end, because we only added a line to the
                // end of the file, and we know that the group
                // start/end markers MUST be before the start
                // of this section
            }
            // list has been updated, but a group was passed, so need to
            // verify the section is in the group, moving it if necessary

            // if this was last section, AND section is NOT in its group
            // then then section end won't be set yet
            if (lngSectionEnd <= 0) {
                lngSectionEnd = Lines.Count - 1;
            }
            // if the section is not in the group, then move it
            // (depends on whether section is BEFORE group start or AFTER group end)
            if (lngSectionStart < lngGrpStart) {
                // make sure at least one blank line above the group end
                if (Lines[lngGrpEnd - 1].Trim().Length > 0) {
                    Lines.Insert(lngGrpEnd++, "");
                }
                // add the section to end of group
                for (i = lngSectionStart; i <= lngSectionEnd; i++) {
                    Lines.Insert(lngGrpEnd++, Lines[i]);
                }
                // then delete the section from it's current location
                for (i = lngSectionStart; i <= lngSectionEnd; i++) {
                    Lines.RemoveAt(lngSectionStart);
                }
            }
            else if (lngSectionStart > lngGrpEnd) {
                // make sure at least one blank line above the group end
                if (Lines[lngGrpEnd - 1].Trim().Length > 0) {
                    Lines.Insert(lngGrpEnd++, "");
                    lngSectionStart++;
                    lngSectionEnd++;
                }
                // add the section to end of group
                for (i = lngSectionEnd; i >= lngSectionStart; i--) {
                    Lines.Insert(lngGrpEnd, Lines[lngSectionEnd]);
                    // delete the line in current location
                    Lines.RemoveAt(lngSectionEnd + 1);
                }
            }
        }

        /// <summary>
        /// Adds or updates a key/value pair of type bool.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteSetting(string Section, string Key, bool Value, string Group = "") {
            WriteSetting(Section, Key, Value.ToString(), Group);
        }

        /// <summary>
        /// Adds or updates a key/value pair of type int.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteSetting(string Section, string Key, int Value, string Group = "") {
            WriteSetting(Section, Key, Value.ToString(), Group);
        }

        /// <summary>
        /// Adds or updates a key/value pair of type Color.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteSetting(string Section, string Key, Color Value, string Group = "") {
            WriteSetting(Section, Key, EGAColors.ColorText(Value), Group);
        }

        /// <summary>
        /// Saves the list to file. If filename is missing, invalid, or restricted access
        /// the appropriate exception is thrown.
        /// </summary>
        public void Save() {
            // verify filename exists
            ArgumentException.ThrowIfNullOrWhiteSpace("filename is missing");
            try {
                using StreamWriter cfgSR = new(Filename);
                // output the results to the file
                foreach (string line in Lines) {
                    cfgSR.WriteLine(line);
                }
                cfgSR.Dispose();
            }
            catch (Exception ex) {
                // file access error
                WinAGIException wex = new(LoadResString(502).Replace(
                    ARG1, ex.HResult.ToString()).Replace(
                    ARG2, Filename)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = ex;
                wex.Data["badfile"] = Filename;
                throw wex;
            }
        }

        /// <summary>
        /// Formats a string value so it can be added to the settings list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string FormatValue(string value) {
            // single words don't need quotes; if value has spaces or '#'
            // then value must be embedded in quotes
            // multiline strings (with \n, \r, or \r\n characters) have the 
            // characters replaced with '\n'; this requires single slashes
            // to also be replaced with '\\'

            string retval = value;
            if (retval.Length == 0) {
                return "\"\"";
            }
            // if value contains spaces or '#', it must be enclosed in quotes
            if (retval.Contains(' ') || retval.Contains('#')) {
                Debug.Assert(retval[0] != '\"');
                retval = "\"" + retval + "\"";
            }
            // replace single '\' with double "\\"
            retval = retval.Replace("\\", "\\\\");
            // replace newline characters with "\n"
            if (retval.Contains("\r\n")) {
                retval = retval.Replace("\r\n", "\\n");
            }
            if (retval.Contains('\r')) {
                retval = retval.Replace("\r", "\\n");
            }
            if (retval.Contains('\n')) {
                retval = retval.Replace("\n", "\\n");
            }
            return retval;
        }

        /// <summary>
        /// Retrieves a value from the list of type string.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public string GetSetting(string Section, string Key, string Default = "", bool DontAdd = false) {
            string strLine;
            int i;
            int startPos, endPos;

            // find the section we are looking for
            int lngSection = FindSettingSection(Section);
            // if not found,
            if (lngSection < 0) {
                if (!DontAdd) {
                    // add the section and the value
                    WriteSetting(Section, Key, Default);
                }
                // and return the default value
                return Default;
            }
            // step through all lines in this section; find matching key
            for (i = lngSection + 1; i < Lines.Count; i++) {
                strLine = Lines[i].Replace("\t", " ").Trim();
                // skip blanks, and lines starting with a comment
                if (strLine.Length > 0 && strLine[0] != '#') {
                    // if another section is found, stop here
                    if (strLine[0] == '[') {
                        break;
                    }
                    // look for 'key'
                    // validate that this is an exact match, and not a key that starts with
                    // the same letters by verifying next char is either a space, or an equal sign
                    if (strLine.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (strLine.IndexOf(' ') == Key.Length || strLine.IndexOf('=') == Key.Length)) {
                        // found it- strip off key
                        strLine = strLine[Key.Length..].Trim();
                        // check for nullstring, in case line has ONLY the key and nothing else
                        if (strLine.Length > 0) {
                            // expect an equal sign
                            if (strLine[0] == '=') {
                                // remove it
                                strLine = strLine[1..].Trim();
                            }
                            if (strLine.Length > 0) {
                                if (strLine[0] == '"') {
                                    // string delimiter; find ending delimiter
                                    endPos = strLine.LastIndexOf('"', strLine.Length - 1);
                                    if (endPos < 0) {
                                        endPos = strLine.Length;
                                    }
                                    else {
                                        // if only one (end == start == 0)
                                        if (endPos == 0) {
                                            // set end to line length
                                            endPos = strLine.Length;
                                        }
                                    }
                                }
                                else {
                                    // look for comment marker
                                    endPos = strLine.IndexOf('#', 1);
                                    if (endPos < 0) {
                                        endPos = strLine.Length;
                                    }
                                }
                                // now strip off anything past value (including trailing quote delimiter)
                                strLine = strLine[..endPos].Trim();
                                if (strLine.Length > 0) {
                                    if (strLine[0] == '"') {
                                        strLine = strLine[1..];
                                    }
                                }
                                // check for control characters '\\' and '\n'
                                int pos = strLine.IndexOf('\\');
                                while (pos != -1) {
                                    if (strLine.IndexOf('\\', pos + 1) == pos + 1) {
                                        strLine = strLine.ReplaceFirst("\\\\", "\\", pos);
                                    }
                                    else if (strLine.IndexOf("n", pos + 1) == pos + 1) {
                                        strLine = strLine.ReplaceFirst("\\n", "\r\n", pos);
                                    }
                                    pos = strLine.IndexOf('\\', pos + 1);
                                }
                            }
                        }
                        return strLine;
                    }
                }
            }
            // not found
            if (DontAdd) {
                // return the default
                return Default;
            }
            // stash the default value unmodified by control codes
            string sReturn = Default;
            // back up until a nonblank line is found
            for (startPos = i - 1; startPos >= lngSection; startPos--) {
                if (Lines[startPos].Trim().Length > 0) {
                    break;
                }
            }
            // add the key and default value at this pos
            Default = FormatValue(Default);
            Lines.Insert(startPos + 1, "   " + Key + " = " + Default);
            return sReturn;
        }

        public int GetSetting(string Section, string Key, int Default = 0, bool DontAdd = false) {
            return GetSetting(Section, Key, Default, false, DontAdd, null);
        }

        public int GetSetting(string Section, string Key, int Default, Type enumType) {
            return GetSetting(Section, Key, Default, false, false, enumType);
        }

        /// <summary>
        /// Retrieves a value from the list of type int.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="hex"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public int GetSetting(string Section, string Key, int Default, bool hex, bool DontAdd, Type enumType) {
            // get the setting value; if it converts to int value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, hex ? "0x" + Default.ToString("x8") : Default.ToString(), DontAdd);
            if (strValue.Length == 0) {
                return Default;
            }
            else if (strValue.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToInt32(strValue, 16);
                }
                catch (Exception) {
                    return Default;
                }
            }
            else if (strValue.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    int retval = Convert.ToInt32(strValue.Right(strValue.Length - 2), 16);
                    // write the value in correct format
                    WriteSetting(Section, Key, "0x" + retval.ToString("x8"));
                    return retval;
                }
                catch (Exception) {
                    return Default;
                }
            }
            else if (int.TryParse(strValue, out int iResult)) {
                return iResult;
            }
            else if (enumType is not null) {
                try {
                    return (int)Enum.Parse(enumType, strValue, true);
                }
                catch {
                    return Default;
                }
            }
            else {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type uint.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public uint GetSetting(string Section, string Key, uint Default = 0, bool DontAdd = false) {
            // get the setting value; if it converts to uint value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, Default.ToString(), DontAdd);

            if (strValue.Length == 0) {
                return Default;
            }
            else if (strValue.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToUInt32(strValue, 16);
                }
                catch (Exception) {
                    return Default;
                }
            }
            else if (strValue.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    uint retval = Convert.ToUInt32(strValue.Right(strValue.Length - 2), 16);
                    // write the value in correct format
                    WriteSetting(Section, Key, "0x" + retval.ToString("x8"));
                    return retval;
                }
                catch (Exception) {
                    return Default;
                }
            }
            else {

                if (uint.TryParse(strValue, out uint iResult)) {
                    return iResult;
                }
                else {
                    return Default;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type byte.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public byte GetSetting(string Section, string Key, byte Default = 0, bool DontAdd = false) {
            // get the setting value; if it converts to byte value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, Default.ToString(), DontAdd);
            if (strValue.Length == 0) {
                return Default;
            }
            else {
                if (byte.TryParse(strValue, out byte bResult)) {
                    return bResult;
                }
                else {
                    return Default;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type double.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public double GetSetting(string Section, string Key, double Default = 0, bool DontAdd = false) {
            // get the setting value; if it converts to single value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, Default.ToString(), DontAdd);

            if (strValue.Length == 0) {
                return Default;
            }
            else {
                if (double.TryParse(strValue, out double sResult)) {
                    return sResult;
                }
                else {
                    return Default;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type float.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public float GetSetting(string Section, string Key, float Default = 0, bool DontAdd = false) {
            // get the setting value; if it converts to single value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, Default.ToString(), DontAdd);

            if (strValue.Length == 0) {
                return Default;
            }
            else {
                if (float.TryParse(strValue, out float sResult)) {
                    return sResult;
                }
                else {
                    return Default;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type bool.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public bool GetSetting(string Section, string Key, bool Default = false, bool DontAdd = false) {
            // get the setting value; if it converts to boolean value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, Default.ToString(), DontAdd);
            if (strValue.Length == 0) {
                return Default;
            }
            else {
                if (bool.TryParse(strValue, out bool bResult)) {
                    return bResult;
                }
                else {
                    return Default;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type Color. Preferred format is
        /// '0xrr, 0xgg, 0xbb'
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public Color GetSetting(string Section, string Key, Color Default, bool DontAdd = false) {
            // get the setting value; if it converts to color value, use it;
            // if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, "", DontAdd);
            if (strValue.Length == 0) {
                // for blank entries, replace with default
                if (!DontAdd) {
                    WriteSetting(Section, Key, EGAColors.ColorText(Default));
                }
                return Default;
            }
            // expected format is '0xrr, 0xgg, 0xbb'
            string[] comp = strValue.Split(",");
            if (comp.Length == 3) {
                try {
                    int r, g, b;
                    r = Convert.ToInt32(comp[0].Trim(), 16) % 0x100;
                    g = Convert.ToInt32(comp[1].Trim(), 16) % 0x100;
                    b = Convert.ToInt32(comp[2].Trim(), 16) % 0x100;
                    return Color.FromArgb(r, g, b);
                }
                catch (Exception) {
                    // for invalid entries, replace with default
                    if (!DontAdd) {
                        WriteSetting(Section, Key, EGAColors.ColorText(Default));
                    }
                    return Default;
                }
            }
            else {
                Color retColor = Default;
                if (strValue.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase) || strValue.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                    // convert hex integer color value; assume it is '0xaarrggbb' or '&Haabbggrr';
                    // **************************************************************
                    // * in VB colors are four byte:                                *
                    // *         xxbbggrr                                           *
                    // * colors in VB versions of WinAGI are stored as '&Haabbggrr' *
                    // **************************************************************

                    // the translator expects colors to be in '0xaabbggrr' format so
                    // we can't just translate because the '0x' version is reversed
                    // from the &H version...
                    // also, we ignore the alpha; all colors are returned with only
                    // rgb components set; the alpha channel is always left to default
                    try {
                        bool flipit = false;
                        if (strValue.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                            strValue = "0x" + strValue[2..];
                            flipit = true;
                        }
                        // assign to int number
                        int iColor = Convert.ToInt32(strValue, 16);
                        // parse it into rgb components
                        int r, g, b;
                        b = iColor % 0x100;
                        g = (iColor >> 8) % 0x100;
                        r = (iColor >> 16) % 0x100;
                        if (flipit) {
                            retColor = Color.FromArgb(b, g, r);
                        }
                        else {
                            retColor = Color.FromArgb(r, g, b);
                        }
                    }
                    catch (Exception) {
                        // keep default
                    }
                }
                else {
                    if (int.TryParse(strValue, out int iColor)) {
                        // it might be a non-hex color number
                        try {
                            // parse it into rgb components
                            int r, g, b;
                            b = (int)(unchecked((uint)iColor) % 0x100);
                            g = (int)(unchecked(((uint)iColor) >> 8) % 0x100);
                            r = (int)(unchecked(((uint)iColor) >> 16) % 0x100);
                            retColor = Color.FromArgb(r, g, b);
                        }
                        catch (Exception) {
                            // keep default;
                        }
                    }
                    else {
                        // not sure what it is; keep the default value
                    }
                }
                // for invalid entries, always replace with updated text
                if (!DontAdd) {
                    WriteSetting(Section, Key, EGAColors.ColorText(retColor));
                }
                return retColor;
            }
        }

        /// <summary>
        /// Retrieves all key/value pairs from the list in the next section that
        /// matches the specified section, beginning at line Start. If found, Start
        /// is updated to the next line. 
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        public KeyValuePair<string, string>[] GetNextSection(string Section, ref int Start) {
            // This function looks for key/value pairs in a wild card section,
            // meaning the next section that begins with the Section value.
            // Start value is the line where the search begins; if a matching section is
            // found, Start is updated to the line following the last key/value pair.
            // Group argument is not used.

            int sectionpos = -1;

            if (Section.Length == 0) {
                Start = Lines.Count;
                return [];
            }
            if (Start < 0 || Start >= Lines.Count) {
                Start = Lines.Count;
                return [];
            }
            // find the section we are looking for (beginning at start)
            for (int i = Start; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                string strLine = Lines[i].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        if (strLine[0] == '[') {
                            int lngPos = strLine.IndexOf(']');
                            if (lngPos >= 0) {
                                strLine = strLine[1..lngPos].Trim();
                            }
                            else {
                                continue;
                            }
                            if (strLine.Equals(Section, StringComparison.OrdinalIgnoreCase)) {
                                // found it 
                                sectionpos = i;
                                break;
                            }
                        }
                    }
                }
            }
            if (sectionpos == -1) {
                Start = Lines.Count;
                return [];
            }
            KeyValuePair<string, string>[] retval = [];
            // step through all lines beginning at next line;
            // find matching key/value pairs
            for (int i = sectionpos + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                string strLine = Lines[i].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        if (strLine[0] == '[') {
                            // another section is found, stop here
                            Start = i;
                            return retval;
                        }
                        // look for key/value pairs
                        int split = strLine.IndexOf('=');
                        if (split >= 0) {
                            string key = strLine[..split].Trim();
                            string value = strLine[(split + 1)..].Trim();
                            int lngPos;
                            if (value.Length != 0) {
                                if (value[0] == '"') {
                                    // string delimiter
                                    value = value[1..];
                                    // find ending delimiter
                                    lngPos = value.LastIndexOf('"');
                                    if (lngPos >= 0) {
                                        value = value[..lngPos];
                                    }
                                }
                                else {
                                    // TODO: when adding a value, if it contains '#' it must be in quotes
                                    lngPos = value.IndexOf('#');
                                    if (lngPos == -1) {
                                        // no delimiter or comment found; assume entire line
                                        lngPos = value.Length;
                                    }
                                    value = value[..lngPos].Trim();
                                }
                            }
                            if (key.Length > 0 && value.Length > 0) {
                                KeyValuePair<string, string> add = new(key, value);
                                Array.Resize(ref retval, retval.Length + 1);
                                retval[^1] = add;
                            }
                        }
                    }
                }
            }
            // end of file; return results
            Start = Lines.Count;
            return retval;
        }

        /// <summary>
        /// Deletes an entire section, including all key/value pairs, from the list.
        /// </summary>
        /// <param name="Section"></param>
        public void DeleteSection(string Section) {
            int lngSection;
            string strLine;

            // find the section we are looking for
            lngSection = FindSettingSection(Section);
            if (lngSection == -1) {
                // nothing to delete
                return;
            }
            // step through all lines in this section, deleting until another section
            // or end of list is found
            while (lngSection < Lines.Count) {
                Lines.RemoveAt(lngSection);
                if (lngSection >= Lines.Count) {
                    return;
                }
                strLine = Lines[lngSection].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] == '[') {
                        // nothing to delete
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a key/value pair from the list.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        public void DeleteKey(string Section, string Key) {
            int i;
            string strLine;
            int lngSection, lenKey;

            // find the section we are looking for
            lngSection = FindSettingSection(Section);
            // if not found,
            if (lngSection <= 0) {
                // nothing to delete
                return;
            }
            // step through all lines in this section; find matching key
            lenKey = Key.Length;
            for (i = lngSection + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        // not a comment
                        // if another section is found, stop here
                        if (strLine[0] == '[') {
                            // nothing to delete
                            return;
                        }
                        // look for key
                        if (strLine.Left(lenKey) == Key) {
                            // found it- delete this line
                            Lines.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
            // not found - nothing to delete
        }

        /// <summary>
        /// Returns the line corresponding to Section, or -1 if not found.
        /// </summary>
        /// <param name="Section"></param>
        /// <returns></returns>
        public int FindSettingSection(string Section) {
            int i, lngPos;
            string strLine;
            // find the section we are looking for
            for (i = 0; i <= Lines.Count - 1; i++) {
                // skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        // look for a bracket
                        if (strLine[0] == '[') {
                            // find end bracket
                            lngPos = strLine.IndexOf(']', 1);
                            string strCheck;
                            if (lngPos > 0) {
                                strCheck = strLine.Mid(1, lngPos - 1);
                            }
                            else {
                                strCheck = strLine.Right(strLine.Length - 1);
                            }
                            if (strCheck.Equals(Section, StringComparison.OrdinalIgnoreCase)) {
                                // found it
                                return i;
                            }
                        }
                    }
                }
            }
            // not found
            return -1;
        }
        #endregion
    }

    public class SettingItem {
        #region Members
        internal string itemName;
        internal string itemSection;
        internal string itemGroup;
        #endregion

        #region Constructors
        public SettingItem() {
            //
        }
        #endregion

        #region Properties
        public string Name {
            get => itemName;
        }

        public string Section {
            get => itemSection;
        }

        public string Group {
            get => itemGroup;
        }

        public Type Type {
            get;
            internal set;
        }
        #endregion

        #region Methods
        #endregion
    }

    public class SettingString : SettingItem {
        #region Members
        private string itemValue;
        private string defaultValue;
        #endregion

        #region Constructors
        public SettingString(string name, string itemvalue, string section, string group, string defaultvalue) {
            base.Type = typeof(string);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingString(string name, string itemvalue, string section, string defaultvalue) {
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingString(string name, string itemvalue, string section) {
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingString(SettingString clone) {
            base.Type = typeof(string);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public string Value {
            get => itemValue;
            set => itemValue = value;
        }

        public string DefaultValue {
            get => defaultValue;
            set => defaultValue = value;
        }

        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public string ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }

        public string ReadSetting(SettingsFile file, string defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }

        public string ReadSetting(SettingsFile file, string defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, Value, Group);
        }
        #endregion
    }

    public class SettingBool : SettingItem {
        #region Members
        private bool itemValue;
        private readonly bool defaultValue;
        #endregion

        #region Constructors
        public SettingBool(string name, bool itemvalue, string section, string group, bool defaultvalue) {
            base.Type = typeof(bool);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingBool(string name, bool itemvalue, string section, string group) {
            base.Type = typeof(bool);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingBool(string name, bool itemvalue, string section, bool defaultvalue) {
            base.Type = typeof(bool);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingBool(string name, bool itemvalue, string section) {
            base.Type = typeof(bool);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingBool(SettingBool clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public bool Value {
            get => itemValue;
            set => itemValue = value;

        }

        public bool DefaultValue {
            get => defaultValue;
        }

        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public bool ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public bool ReadSetting(SettingsFile file, bool defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public bool ReadSetting(SettingsFile file, bool defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }
        
        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, Value, Group);
        }
        #endregion
    }

    public class SettingInt : SettingItem {
        #region Members
        private int itemValue;
        private bool hex;
        private readonly int defaultValue;
        #endregion

        #region Constructors
        public SettingInt(string name, int itemvalue, string section, string group, int defaultvalue) {
            base.Type = typeof(int);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingInt(string name, int itemvalue, string section, string group) {
            base.Type = typeof(int);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingInt(string name, int itemvalue, string section, int defaultvalue) {
            base.Type = typeof(int);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingInt(string name, int itemvalue, string section) {
            base.Type = typeof(int);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingInt(SettingInt clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            hex = clone.hex;
        }
        #endregion

        #region Properties
        public int Value {
            get => itemValue;
            set => itemValue = value;
        }

        public int DefaultValue {
            get => defaultValue;
        }

        public bool SaveHex {
            get => hex;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public int ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public int ReadSetting(SettingsFile file, int defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public int ReadSetting(SettingsFile file, int defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false, dontadd, null);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveHex) {
                file.WriteSetting(Section, Name, "0x" + Value.ToString("x8"), Group);
            }
            else {
                file.WriteSetting(Section, Name, Value, Group);
            }
        }
        #endregion
    }

    public class SettingByte : SettingItem {
        #region Members
        private byte itemValue;
        private bool hex;
        private readonly byte defaultValue;
        #endregion

        #region Constructors
        public SettingByte(string name, byte itemvalue, string section, string group, byte defaultvalue) {
            base.Type = typeof(byte);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingByte(string name, byte itemvalue, string section, string group) {
            base.Type = typeof(byte);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingByte(string name, byte itemvalue, string section, byte defaultvalue) {
            base.Type = typeof(byte);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingByte(string name, byte itemvalue, string section) {
            base.Type = typeof(byte);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingByte(SettingByte clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            hex = clone.hex;
        }
        #endregion

        #region Properties
        public byte Value {
            get => itemValue;
            set => itemValue = value;
        }

        public byte DefaultValue {
            get => defaultValue;
        }

        public bool SaveHex {
            get => hex;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public byte ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public byte ReadSetting(SettingsFile file, byte defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public byte ReadSetting(SettingsFile file, byte defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveHex) {
                file.WriteSetting(Section, Name, "0x" + Value.ToString("x8"), Group);
            }
            else {
                file.WriteSetting(Section, Name, Value, Group);
            }
        }
        #endregion
    }

    public class SettingUint : SettingItem {
        #region Members
        private uint itemValue;
        private readonly uint defaultValue;
        private bool hex;
        #endregion

        #region Constructors
        public SettingUint(string name, uint itemvalue, string section, string group, uint defaultvalue) {
            base.Type = typeof(uint);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingUint(string name, uint itemvalue, string section, string group) {
            base.Type = typeof(uint);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingUint(string name, uint itemvalue, string section, uint defaultvalue) {
            base.Type = typeof(uint);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingUint(string name, uint itemvalue, string section) {
            base.Type = typeof(uint);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingUint(SettingUint clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            hex = clone.hex;
        }
        #endregion

        #region Properties
        public uint Value {
            get => itemValue;
            set => itemValue = value;
        }

        public uint DefaultValue {
            get => defaultValue;
        }

        public bool SaveHex {
            get => hex;
            set => hex = value;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public uint ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public uint ReadSetting(SettingsFile file, uint defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public uint ReadSetting(SettingsFile file, uint defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveHex) {
                file.WriteSetting(Section, Name, "0x" + Value.ToString("x8"), Group);
            }
            else {
                file.WriteSetting(Section, Name, Value.ToString(), Group);
            }
        }
        #endregion
    }

    public class SettingColor : SettingItem {
        // store internally as int so it can be easily cloned
        #region Members
        private int itemValue;
        private readonly int defaultValue;
        #endregion

        #region Constructors
        public SettingColor(string name, Color itemvalue, string section, string group, Color defaultvalue) {
            base.Type = typeof(Color);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue =  itemvalue.ToArgb();
            defaultValue = defaultvalue.ToArgb();
        }

        public SettingColor(string name, Color itemvalue, string section, string group) {
            base.Type = typeof(Color);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue.ToArgb();
            defaultValue = itemvalue.ToArgb();
        }

        public SettingColor(string name, Color itemvalue, string section, Color defaultvalue) {
            base.Type = typeof(Color);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue.ToArgb();
            defaultValue = defaultvalue.ToArgb();
        }

        public SettingColor(string name, Color itemvalue, string section) {
            base.Type = typeof(Color);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue.ToArgb();
            defaultValue = itemvalue.ToArgb();
        }

        public SettingColor(SettingColor clone) {
            base.Type = typeof(Color);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public Color Value {
            get {
                return Color.FromArgb(itemValue);
            }
            set => itemValue = value.ToArgb();

        }

        public Color DefaultValue {
            get => Color.FromArgb(defaultValue);
        }

        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public Color ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            Color retval = file.GetSetting(Section, Name, DefaultValue, false);
            itemValue = retval.ToArgb();
            return retval;
        }
        public Color ReadSetting(SettingsFile file, Color defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            Color retval = file.GetSetting(Section, Name, defaultvalue, false);
            itemValue = retval.ToArgb();
            return retval;
        }
        public Color ReadSetting(SettingsFile file, Color defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            Color retval = file.GetSetting(Section, Name, defaultvalue, dontadd);
            itemValue = retval.ToArgb();
            return retval;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, Value, Group);
        }
        #endregion
    }

    public class SettingFontStyle : SettingItem {
        #region Members
        private FontStyle itemValue;
        private readonly FontStyle defaultValue;
        #endregion

        #region Constructors
        public SettingFontStyle(string name, FontStyle itemvalue, string section, string group, FontStyle defaultvalue) {
            base.Type = typeof(FontStyle);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingFontStyle(string name, FontStyle itemvalue, string section, string group) {
            base.Type = typeof(FontStyle);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingFontStyle(string name, FontStyle itemvalue, string section, FontStyle defaultvalue) {
            base.Type = typeof(FontStyle);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingFontStyle(string name, FontStyle itemvalue, string section) {
            base.Type = typeof(FontStyle);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingFontStyle(SettingFontStyle clone) {
            base.Type = typeof(FontStyle);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public FontStyle Value {
            get => itemValue;
            set => itemValue = value;

        }

        public FontStyle DefaultValue {
            get => defaultValue;
        }

        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public FontStyle ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = (FontStyle)file.GetSetting(Section, Name, (int)defaultValue, false);
            return itemValue;
        }
        public FontStyle ReadSetting(SettingsFile file, FontStyle defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = (FontStyle)file.GetSetting(Section, Name, (int)defaultvalue, false);
            return itemValue;
        }
        public FontStyle ReadSetting(SettingsFile file, FontStyle defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = (FontStyle)file.GetSetting(Section, Name, (int)defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, (int)itemValue, Group);
        }
        #endregion
    }

    public class SettingDouble : SettingItem {
        #region Members
        private double itemValue;
        private readonly double defaultValue;
        #endregion

        #region Constructors
        public SettingDouble(string name, double itemvalue, string section, string group, double defaultvalue) {
            base.Type = typeof(double);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingDouble(string name, double itemvalue, string section, string group) {
            base.Type = typeof(double);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingDouble(string name, double itemvalue, string section, double defaultvalue) {
            base.Type = typeof(double);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingDouble(string name, double itemvalue, string section) {
            base.Type = typeof(double);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingDouble(SettingDouble clone) {
            base.Type = typeof(double);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public double Value {
            get => itemValue;
            set => itemValue = value;

        }

        public double DefaultValue {
            get => defaultValue;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public double ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public double ReadSetting(SettingsFile file, double defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public double ReadSetting(SettingsFile file, double defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, itemValue.ToString(), Group);
        }
        #endregion
    }

    public class SettingFloat : SettingItem {
        #region Members
        private float itemValue;
        private readonly float defaultValue;
        #endregion

        #region Constructors
        public SettingFloat(string name, float itemvalue, string section, string group, float defaultvalue) {
            base.Type = typeof(float);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingFloat(string name, float itemvalue, string section, string group) {
            base.Type = typeof(float);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingFloat(string name, float itemvalue, string section, float defaultvalue) {
            base.Type = typeof(float);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingFloat(string name, float itemvalue, string section) {
            base.Type = typeof(float);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingFloat(SettingFloat clone) {
            base.Type = typeof(float);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
        }
        #endregion

        #region Properties
        public float Value {
            get => itemValue;
            set => itemValue = value;

        }

        public float DefaultValue {
            get => defaultValue;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public double ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultValue, false);
            return itemValue;
        }
        public float ReadSetting(SettingsFile file, float defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, false);
            return itemValue;
        }
        public float ReadSetting(SettingsFile file, float defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            file.WriteSetting(Section, Name, itemValue.ToString(), Group);
        }
        #endregion
    }

    public class SettingEResListType : SettingItem {
        #region Members
        private ResListType itemValue;
        private readonly ResListType defaultValue;
        private bool savetext;
        #endregion

        #region Constructors
        public SettingEResListType(string name, ResListType itemvalue, string section, string group, ResListType defaultvalue) {
            base.Type = typeof(ResListType);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingEResListType(string name, ResListType itemvalue, string section, string group) {
            base.Type = typeof(ResListType);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingEResListType(string name, ResListType itemvalue, string section, ResListType defaultvalue) {
            base.Type = typeof(ResListType);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingEResListType(string name, ResListType itemvalue, string section) {
            base.Type = typeof(ResListType);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingEResListType(SettingEResListType clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            savetext = clone.savetext;
        }
        #endregion

        #region Properties
        public ResListType Value {
            get => itemValue;
            set => itemValue = value;
        }

        public ResListType DefaultValue {
            get => defaultValue;
        }

        public int IntValue {
            get => (int)itemValue;
        }

        public string TextValue {
            get => itemValue.ToString();
        }

        public bool SaveText {
            get => savetext;
            set => savetext = value;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public ResListType ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultValue, false);
        }
        public ResListType ReadSetting(SettingsFile file, ResListType defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultvalue, false);
        }
        public ResListType ReadSetting(SettingsFile file, ResListType defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            int retval = file.GetSetting(Section, Name, (int)defaultvalue, false, dontadd, typeof(ResListType));
            if (retval < 0) {
                retval = 0;
            }
            if (retval > 2) {
                retval = 2;
            }
            itemValue = (ResListType)retval;
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveText) {
                file.WriteSetting(Section, Name, TextValue, Group);
            }
            else {
                file.WriteSetting(Section, Name, IntValue, Group);
            }
        }

        #endregion
    }

    public class SettingAskOption : SettingItem {
        #region Members
        private AskOption itemValue;
        private readonly AskOption defaultValue;
        private bool savetext;
        #endregion

        #region Constructors
        public SettingAskOption(string name, AskOption itemvalue, string section, string group, AskOption defaultvalue) {
            base.Type = typeof(AskOption);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingAskOption(string name, AskOption itemvalue, string section, string group) {
            base.Type = typeof(AskOption);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingAskOption(string name, AskOption itemvalue, string section, AskOption defaultvalue) {
            base.Type = typeof(AskOption);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingAskOption(string name, AskOption itemvalue, string section) {
            base.Type = typeof(AskOption);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingAskOption(SettingAskOption clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            savetext = clone.savetext;
        }
        #endregion

        #region Properties
        public AskOption Value {
            get => itemValue;
            set => itemValue = value;
        }

        public AskOption DefaultValue {
            get => defaultValue;
        }

        public int IntValue {
            get => (int)itemValue;
        }

        public string TextValue {
            get => itemValue.ToString();
        }

        public bool SaveText {
            get => savetext;
            set => savetext = value;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public AskOption ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultValue, false);
        }
        public AskOption ReadSetting(SettingsFile file, AskOption defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultvalue, false);
        }
        public AskOption ReadSetting(SettingsFile file, AskOption defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            int retval = file.GetSetting(Section, Name, (int)defaultvalue, false, dontadd, typeof(AskOption));
            if (retval < 0) {
                retval = 0;
            }
            if (retval > 2) {
                retval = 2;
            }
            itemValue = (AskOption)retval;
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            if (SaveText) {
                ArgumentNullException.ThrowIfNull(file);
                file.WriteSetting(Section, Name, TextValue, Group);
            }
            else {
                file.WriteSetting(Section, Name, IntValue, Group);
            }
        }
        #endregion
    }

    public class SettingLogicErrorLevel : SettingItem {
        #region Members
        private LogicErrorLevel itemValue;
        private readonly LogicErrorLevel defaultValue;
        private bool savetext;
        #endregion

        #region Constructors
        public SettingLogicErrorLevel(string name, LogicErrorLevel itemvalue, string section, string group, LogicErrorLevel defaultvalue) {
            base.Type = typeof(LogicErrorLevel);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingLogicErrorLevel(string name, LogicErrorLevel itemvalue, string section, string group) {
            base.Type = typeof(LogicErrorLevel);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingLogicErrorLevel(string name, LogicErrorLevel itemvalue, string section, LogicErrorLevel defaultvalue) {
            base.Type = typeof(LogicErrorLevel);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingLogicErrorLevel(string name, LogicErrorLevel itemvalue, string section) {
            base.Type = typeof(LogicErrorLevel);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingLogicErrorLevel(SettingLogicErrorLevel clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            savetext = clone.savetext;
        }
        #endregion

        #region Properties
        public LogicErrorLevel Value {
            get => itemValue;
            set => itemValue = value;
        }

        public LogicErrorLevel DefaultValue {
            get => defaultValue;
        }

        public int IntValue {
            get => (int)itemValue;
        }

        public string TextValue {
            get => itemValue.ToString();
        }

        public bool SaveText {
            get => savetext;
            set => savetext = value;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public LogicErrorLevel ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultValue, false);
        }
        public LogicErrorLevel ReadSetting(SettingsFile file, LogicErrorLevel defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultvalue, false);
        }
        public LogicErrorLevel ReadSetting(SettingsFile file, LogicErrorLevel defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            int retval = file.GetSetting(Section, Name, (int)defaultvalue, false, dontadd, typeof(LogicErrorLevel));
            if (retval < 0) {
                retval = 0;
            }
            if (retval > 2) {
                retval = 2;
            }
            itemValue = (LogicErrorLevel)retval;
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveText) {
                file.WriteSetting(Section, Name, TextValue, Group);
            }
            else {
                file.WriteSetting(Section, Name, IntValue, Group);
            }
        }
        #endregion
    }

    public class SettingAGICodeStyle : SettingItem {
        #region Members
        private AGICodeStyle itemValue;
        private readonly AGICodeStyle defaultValue;
        private bool savetext;
        #endregion

        #region Constructors
        public SettingAGICodeStyle(string name, AGICodeStyle itemvalue, string section, string group, AGICodeStyle defaultvalue) {
            base.Type = typeof(AGICodeStyle);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingAGICodeStyle(string name, AGICodeStyle itemvalue, string section, string group) {
            base.Type = typeof(AGICodeStyle);
            itemName = name;
            itemSection = section;
            itemGroup = group;
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }

        public SettingAGICodeStyle(string name, AGICodeStyle itemvalue, string section, AGICodeStyle defaultvalue) {
            base.Type = typeof(AGICodeStyle);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = defaultvalue;
        }

        public SettingAGICodeStyle(string name, AGICodeStyle itemvalue, string section) {
            base.Type = typeof(AGICodeStyle);
            itemName = name;
            itemSection = section;
            itemGroup = "";
            itemValue = itemvalue;
            defaultValue = itemvalue;
        }
        public SettingAGICodeStyle(SettingAGICodeStyle clone) {
            base.Type = typeof(bool);
            itemName = clone.itemName;
            itemSection = clone.Section;
            itemGroup = clone.Group;
            itemValue = clone.itemValue;
            defaultValue = clone.defaultValue;
            savetext = clone.savetext;
        }
        #endregion

        #region Properties
        public AGICodeStyle Value {
            get => itemValue;
            set => itemValue = value;
        }

        public AGICodeStyle DefaultValue {
            get => defaultValue;
        }

        public int IntValue {
            get => (int)itemValue;
        }

        public string TextValue {
            get => itemValue.ToString();
        }

        public bool SaveText {
            get => savetext;
            set => savetext = value;
        }
        #endregion

        #region Methods
        public void Reset(SettingsFile savefile = null) {
            itemValue = defaultValue;
            if (savefile is not null) {
                WriteSetting(savefile);
            }
        }

        public AGICodeStyle ReadSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultValue, false);
        }
        public AGICodeStyle ReadSetting(SettingsFile file, AGICodeStyle defaultvalue) {
            ArgumentNullException.ThrowIfNull(file);
            return ReadSetting(file, defaultvalue, false);
        }
        public AGICodeStyle ReadSetting(SettingsFile file, AGICodeStyle defaultvalue, bool dontadd) {
            ArgumentNullException.ThrowIfNull(file);
            int retval = file.GetSetting(Section, Name, (int)defaultvalue, false, dontadd, typeof(AskOption));
            if (retval < 0) {
                retval = 0;
            }
            if (retval > 2) {
                retval = 2;
            }
            itemValue = (AGICodeStyle)retval;
            return itemValue;
        }

        public void WriteSetting(SettingsFile file) {
            ArgumentNullException.ThrowIfNull(file);
            if (SaveText) {
                file.WriteSetting(Section, Name, TextValue, Group);
            }
            else {
                file.WriteSetting(Section, Name, IntValue, Group);
            }
        }
        #endregion
    }

}
