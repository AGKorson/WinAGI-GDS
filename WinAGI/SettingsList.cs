using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {

    /// <summary>
    /// A class to manage a settings file. It handles opening, adding and deleting sections
    /// and key/value pairs. Sections can be assigned to groups to keep the file organized.
    /// </summary>
    public class SettingsList {
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
        //  if string is multline, use '\n' control code (and use multiline option)

        #region Members
        public StringList Lines = [];
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new settings list. Allowable options for mode are FileMode.Create,
        /// FileMode.OpenOrCreate, or FileMode.Open. If filename is not valid, or if file
        /// is readonly, an excpetion is thrown.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mode"></param>
        public SettingsList(string filename, FileMode mode) {
            FileStream fsConfig;
            StreamWriter swConfig;
            StreamReader srConfig;

            if (filename is null || filename.Length == 0) {
                WinAGIException wex = new(LoadResString(604)) {
                    HResult = WINAGI_ERR + 604,
                };
                throw wex;
            }
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
                        try {
                            File.SetAttributes(filename, FileAttributes.Normal);
                        }
                        catch {
                            WinAGIException wex = new(LoadResString(700).Replace(ARG1, Filename)) {
                                HResult = WINAGI_ERR + 700,
                            };
                            wex.Data["badfile"] = Filename;
                            throw wex;
                        }
                    }
                }
                break;
            case FileMode.Open:
                // open (file must already exist)
                if (!File.Exists(filename)) {
                    WinAGIException wex = new(LoadResString(524).Replace(ARG1, filename)) {
                        HResult = WINAGI_ERR + 524
                    };
                    wex.Data["missingfile"] = filename;
                    throw wex;
                }
                // file can't be readonly
                if ((File.GetAttributes(filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    WinAGIException wex = new(LoadResString(700).Replace(ARG1, Filename)) {
                        HResult = WINAGI_ERR + 700,
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
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, ex.HResult.ToString()).Replace(ARG2, Filename)) {
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
        public SettingsList() {
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
                        if (strLine.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (Mid(strLine, Key.Length, 1) == " " || Mid(strLine, Key.Length, 1) == "=")) {
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

            //if this was last section, AND section is NOT in its group
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
        /// Saves the list to file. If filename is missing, invalid, or restricted access
        /// the appropriate exception is thrown.
        /// </summary>
        public void Save() {
            // verify filename exists
            if (Filename.Length == 0) {
                WinAGIException wex = new("settngs list not loaded") {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
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
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, ex.HResult.ToString()).Replace(ARG2, Filename)) {
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
            string retval = value;
            if (retval.Length == 0) {
                return "\"\"";
            }
            // if value contains spaces or '#', it must be enclosed in quotes
            if (retval.Contains(' ') || retval.Contains('#')) {
                if (retval[0] != '"') {
                    retval = "\"" + retval;
                }
                if ((retval[^1] != '"')) {
                    retval += "\"";
                }
            }
            // if value contains any carriage returns, replace them with control characters
            if (retval.Contains("\r\n", StringComparison.OrdinalIgnoreCase)) {
                retval = retval.Replace("\r\n", "\\n");
            }
            if (retval.Contains('\r', StringComparison.OrdinalIgnoreCase)) {
                retval = retval.Replace("\r", "\\n");
            }
            if (retval.Contains('\n', StringComparison.OrdinalIgnoreCase)) {
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
                    //if another section is found, stop here
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
                                // now strip off anything past value (keeping quote delimiters)
                                strLine = strLine[..endPos].Trim();
                                if (strLine.Length > 0) {
                                    if (strLine[0] == '"') {
                                        strLine = strLine[1..];
                                        if (strLine.Length > 0) {
                                            if (strLine[^1] == '"') {
                                                strLine = strLine[..^1];
                                            } 
                                            else {
                                                strLine = strLine.TrimEnd();
                                            }
                                        }
                                    }
                                }
                                if (strLine.IndexOf("\\n", 0) >= 0) {
                                    //replace any newline control characters
                                    strLine = strLine.Replace("\\n", "\r\n");
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
            //back up until a nonblank line is found
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

        public int GetSetting(string Section, string Key, int Default) {
            return GetSetting(Section, Key, Default, false, false, null);
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
            else if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToInt32(strValue, 16);
                }
                catch (Exception) {
                    return Default;
                }
            }
            else if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    int retval = Convert.ToInt32(Right(strValue, strValue.Length - 2), 16);
                    //write the value in correct format
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
            else if (enumType != null) {
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
            else if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToUInt32(strValue, 16);
                }
                catch (Exception) {
                    return Default;
                }
            }
            else if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    UInt32 retval = Convert.ToUInt32(Right(strValue, strValue.Length - 2), 16);
                    //write the value in correct format
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
                if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase) || Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
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
                        if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
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
        /// Retrieves the next value in the specified section from the list, beginning
        /// at line Start. If found, Start is updated to the next line. 
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        public string GetNextSetting(ref string Section, ref string Key, ref int Start) {
            // this function looks for keys in a wild card section; meaning the next
            // section that begins with the Section value; it allows for multiple sections
            // to share a name, or use counting values, such as those used in earlier
            // WinAGI snippet files
            // no option for default is provided in this case; if no match found, empty
            // string is returned
            // Start value is the line where the search begins; if a section is found,
            // it is updated to the line matching section value
            // Group argument is not used
            int lngSection = 0, lngPos;
            int i;
            string strLine;
            string strCheck;

            if (Section.Length == 0) {
                return "";
            }
            if (Key.Length == 0) {
                return "";
            }
            // find the section we are looking for (beginning at start)
            for (i = Start; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        // look for a bracket
                        if (strLine[0] == '[') {
                            // find end bracket
                            lngPos = strLine.IndexOf(']');
                            if (lngPos >= 0) {
                                strCheck = strLine[1..(lngPos - 1)];
                            }
                            else {
                                strCheck = strLine[1..];
                            }
                            // look for partial matches
                            if (strCheck.StartsWith(Section, StringComparison.OrdinalIgnoreCase)) {
                                // found it
                                lngSection = i;
                                Start = i;
                                Section = strCheck;
                                break;
                            }
                        }
                    }
                }
            }
            // if not found,
            if (lngSection == 0) {
                return "";
            }
            // step through all lines beginning at next ine; find matching key
            for (i = Start + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        // not a comment
                        // if another section is found, stop here
                        if (strLine[0] == '[') {
                            break;
                        }
                        // look for 'key'
                        // validate that this is an exact match, and not a key that starts with
                        // the same letters by verifying next char is either a space, or an equal sign
                        if (strLine.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (strLine.IndexOf(' ') == Key.Length || strLine.IndexOf('=') == Key.Length)) {
                            //found it- extract value (if there is a comment on the end, drop it)
                            strLine = strLine[strLine.Length..].Trim();
                            // check for nullstring, incase line has ONLY the key and nothing else
                            if (strLine.Length > 0) {
                                // expect an equal sign
                                if (strLine[0] == '=') {
                                    // remove it
                                    strLine = strLine[1..];
                                }
                                if (strLine[0] == '"') {
                                    // string delimiter; find ending delimiter
                                    lngPos = strLine.IndexOf('"');
                                }
                                else {
                                    // look for comment marker
                                    lngPos = strLine.IndexOf('#');
                                }
                                if (lngPos == 0) {
                                    // no delimiter found; assume entire line
                                    lngPos = strLine.Length + 1;
                                }
                                // now strip off anything past value (including delimiter)
                                strLine = strLine[..(lngPos - 1)].Trim();
                                if (strLine.Length > 0) {
                                    //if in quotes, remove them
                                    // should never have an end quote; it will be caught as the ending delimiter
                                    if (strLine[0] == '"') {
                                        strLine = strLine[1..^1];
                                    }
                                }
                                if (strLine.Length > 0) {
                                    if (strLine[^1] == '"') {
                                        Debug.Assert(false);
                                        strLine = strLine[..^1];
                                    }
                                }
                                if (strLine.Contains("\\n")) {
                                    // replace any newline control characters
                                    strLine = strLine.Replace("\\n", "\r\n");
                                }
                            }
                            return strLine;
                        }
                    }
                }
            }
            // not found - return null
            return "";
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

            //find the section we are looking for
            lngSection = FindSettingSection(Section);
            //if not found,
            if (lngSection <= 0) {
                //nothing to delete
                return;
            }
            //step through all lines in this section; find matching key
            lenKey = Key.Length;
            for (i = lngSection + 1; i < Lines.Count; i++) {
                //skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') { //not a comment
                                             //if another section is found, stop here
                        if (strLine[0] == '[') {
                            //nothing to delete
                            return;
                        }
                        //look for key
                        if (Left(strLine, lenKey) == Key) {
                            //found it- delete this line
                            Lines.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
            //not found - nothing to delete
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
                //skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        //look for a bracket
                        if (strLine[0] == '[') {
                            //find end bracket
                            lngPos = strLine.IndexOf(']', 1);
                            string strCheck;
                            if (lngPos > 0) {
                                strCheck = Mid(strLine, 1, lngPos - 1);
                            }
                            else {
                                strCheck = Right(strLine, strLine.Length - 1);
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
}
