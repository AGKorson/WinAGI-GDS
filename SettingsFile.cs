using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                        WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, Filename)) {
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
                    WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, Filename)) {
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
                WinAGIException wex = new(EngineResourceByNum(502).Replace(
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
                using StreamWriter swConfig = new(fsConfig);
                // add a single comment to the list
                Lines.Add("#");
                // and write it to the file
                swConfig.WriteLine("#");
                swConfig.Dispose();
            }
            else {
                // grab the file data
                using StreamReader srConfig = new(fsConfig);
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
            string linetext;
            int sectionEnd = 0;
            bool found = false;
            int insertLine;

            // format the string before writing it
            string checktext = Value is null ? "\"\"" : FormatValue(Value.ToString());
            // need at least one line...
            if (Lines.Count == 0) {
                // just add the line and exit
                Lines.Add("#");
                Lines.Add("");
                if (Group.Length > 0) {
                    Lines.Add("[::BEGIN " + Group + "::]");
                }
                Lines.Add("[" + Section + "]");
                Lines.Add("   " + Key + " = " + checktext);
                if (Group.Length > 0) {
                    Lines.Add("[::END " + Group + "::]");
                }
                Lines.Add("");
                //  done
                return;
            }
            // if a group is provided, we add new items inside the group;
            // existing items will be left where they are
            int groupStart = -1;
            int groupEnd = -1;
            int pos = -1;
            if (Group.Length > 0) {
                for (i = 1; i <= Lines.Count - 1; i++) {
                    // skip blanks, and lines starting with a comment
                    linetext = Lines[i].Replace("\t", " ").Trim();
                    if (linetext.Length > 0) {
                        if (linetext[0] != '#') {
                            // if not found yet, look for the starting marker
                            if (!found) {
                                if (linetext.Equals("[::BEGIN " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                    groupStart = i;
                                    found = true;
                                    // if start found after end we are done
                                    if (groupEnd >= 0) {
                                        break;
                                    }
                                }
                            }
                            else {
                                // start has been found; check for another group start
                                if (linetext.StartsWith("[::BEGIN ", StringComparison.OrdinalIgnoreCase)) {
                                    // mark position of first new start, so we can move the end marker here
                                    if (pos < 0) {
                                        pos = i;
                                    }
                                }
                            }
                            // check for end marker even if start not found yet
                            // just in case they are backwards
                            if (linetext.Equals("[::END " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                groupEnd = i;
                                // if we also have a start, we can exit the loop
                                if (found) {
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
                if (groupStart >= 0 && groupEnd >= 0) {
                    // if backwards, move end to line after start
                    if (groupEnd < groupStart) {
                        Lines.Insert(groupStart + 1, Lines[groupEnd]);
                        Lines.RemoveAt(groupEnd);
                        groupStart -= 1;
                        groupEnd = groupStart + 1;
                    }
                }
                // if only start found
                else if (groupStart >= 0) {
                    // if there was another start found, insert end there
                    if (pos > 0) {
                        Lines.Insert(pos, "[::END " + Group + "::]");
                        Lines.Insert(pos + 1, "");
                        groupEnd = pos;
                    }
                    else {
                        // otherwise insert group end at end of file
                        groupEnd = Lines.Count;
                        Lines.Add("[::END " + Group + "::]");
                    }
                }
                // if end found but no start
                else if (groupEnd >= 0) {
                    // insert start in front of end
                    groupStart = groupEnd;
                    Lines.Insert(groupStart, "[::START " + Group + "::]");
                    groupEnd = groupStart + 1;
                }
                else {
                    // means neither found
                    // make sure at least one blank line
                    if (Lines[^1].Trim().Length > 0) {
                        Lines.Add("");
                    }
                    groupStart = Lines.Count;
                    Lines.Add("[::BEGIN " + Group + "::]");
                    Lines.Add("[::END " + Group + "::]");
                    groupEnd = groupStart + 1;
                }
            }
            // find the section we are looking for
            int sectionStart = FindSettingSection(Section);
            // if not found, create it at end of group (if group is provided)
            // otherwise at end of list
            if (sectionStart < 0) {
                if (groupStart >= 0) {
                    insertLine = groupEnd;
                }
                else {
                    insertLine = Lines.Count;
                }
                // make sure there is at least one blank line (unless this is first line in list)
                if (insertLine > 0) {
                    if (Lines[insertLine - 1].Trim().Length != 0) {
                        Lines.Insert(insertLine, "");
                        insertLine++;
                    }
                }
                Lines.Insert(insertLine, "[" + Section + "]");
                Lines.Insert(insertLine + 1, "   " + Key + " = " + checktext);
                //  done
                return;
            }
            // search section; find matching key
            found = false;
            for (i = sectionStart + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                linetext = Lines[i].Replace("\t", " ").Trim();
                if (linetext.Length > 0 && linetext[0] != '#') {
                    // if another section is found, stop here
                    if (linetext[0] == '[') {
                        // if part of a group; last line of the section
                        // is line prior to the new section
                        if (groupStart >= 0) {
                            sectionEnd = i - 1;
                        }
                        // if not already added, add it now
                        if (!found) {
                            // back up until a nonblank line is found
                            for (pos = i - 1; pos >= sectionStart; pos--) {
                                if (Lines[pos].Trim().Length > 0) {
                                    break;
                                }
                            }
                            // add the key and value at this pos
                            Lines.Insert(pos + 1, "   " + Key + " = " + checktext);
                            // this also bumps down the section end
                            sectionEnd++;
                            // it also may bump down group start/end
                            if (groupStart >= pos + 1) {
                                groupStart++;
                            }
                            if (groupEnd >= pos + 1) {
                                groupEnd++;
                            }
                        }
                        // we are done, but if part of a group we need to verify the
                        // section is in the group
                        if (groupStart >= 0) {
                            found = true;
                            break;
                        }
                        else {
                            return;
                        }
                    }
                    // if not already found, look for key
                    if (!found) {
                        if (linetext.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (linetext.Mid(Key.Length, 1) == " " || linetext.Mid(Key.Length, 1) == "=")) {
                            // remove key
                            linetext = linetext[Key.Length..].Trim();
                            if (linetext.Length > 0) {
                                // expect an equal sign
                                if (linetext[0] == '=') {
                                    // remove it
                                    linetext = linetext[1..].Trim();
                                }
                            }
                            if (linetext.Length > 0) {
                                if (linetext[0] == '"') {
                                    // string delimiter; find ending delimiter
                                    pos = linetext.IndexOf('"', 1) + 1;
                                }
                                else {
                                    // look for comment marker
                                    pos = linetext.IndexOf('#', 1);
                                    // if none found, look for space delimiter
                                    if (pos == -1) {
                                        // look for a space as a delimiter
                                        pos = linetext.IndexOf(' ', 1);
                                    }
                                }
                                if (pos <= 0) {
                                    // no delimiter found; assume entire line is value
                                    pos = linetext.Length;
                                }
                                // now strip off the old value, leaving potential comment
                                linetext = linetext[pos..].Trim();
                            }
                            // if something left, maks sure it's a comment
                            if (linetext.Length > 0) {
                                if (linetext[0] != '#') {
                                    linetext = "#" + linetext;
                                }
                                // make sure it starts with white space
                                linetext = "   " + linetext;
                            }
                            // now update line with new value
                            linetext = "   " + Key + " = " + checktext + linetext;
                            Lines[i] = linetext;
                            // we are done, but if part of a group
                            // we need to keep going to find end so
                            // we can validate section is in the group
                            if (groupStart >= 0) {
                                found = true;
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
            if (!found) {
                // back up until a nonblank line is found
                for (pos = i - 1; i >= sectionStart; i--) {
                    if (Lines[pos].Trim().Length > 0) {
                        break;
                    }
                }
                // add the key and value at this pos
                Lines.Insert(pos + 1, "   " + Key + " = " + checktext);
                if (groupStart < 0) {
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
            if (sectionEnd <= 0) {
                sectionEnd = Lines.Count - 1;
            }
            // if the section is not in the group, then move it
            // (depends on whether section is BEFORE group start or AFTER group end)
            if (sectionStart < groupStart) {
                // make sure at least one blank line above the group end
                if (Lines[groupEnd - 1].Trim().Length > 0) {
                    Lines.Insert(groupEnd++, "");
                }
                // add the section to end of group
                for (i = sectionStart; i <= sectionEnd; i++) {
                    Lines.Insert(groupEnd++, Lines[i]);
                }
                // then delete the section from it's current location
                for (i = sectionStart; i <= sectionEnd; i++) {
                    Lines.RemoveAt(sectionStart);
                }
            }
            else if (sectionStart > groupEnd) {
                // make sure at least one blank line above the group end
                if (Lines[groupEnd - 1].Trim().Length > 0) {
                    Lines.Insert(groupEnd++, "");
                    sectionStart++;
                    sectionEnd++;
                }
                // add the section to end of group
                for (i = sectionEnd; i >= sectionStart; i--) {
                    Lines.Insert(groupEnd, Lines[sectionEnd]);
                    // delete the line in current location
                    Lines.RemoveAt(sectionEnd + 1);
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
            ArgumentException.ThrowIfNullOrWhiteSpace(Filename);
            StackTrace st = new(true);
            string sti = st.GetFrame(1).GetMethod().DeclaringType.Name + "." +
                st.GetFrame(1).GetMethod().Name;
            sti += ": " + Path.GetFileName(Filename);
            Debug.Print(sti);

            if (!CanOpen(Filename)) {
                Debug.Print("File access error!");
            }

            int attempts = 0;
            while (attempts < 4) {
                try {
                    using StreamWriter cfgSR = new(Filename);
                    // output the results to the file
                    foreach (string line in Lines) {
                        cfgSR.WriteLine(line);
                    }
                    cfgSR.Dispose();
                    break;
                }
                catch (Exception ex) {
                    // file access error
                    attempts++;
                    if (attempts > 3) {
                        Debug.Print("failed to write");
                        WinAGIException wex = new(EngineResourceByNum(502).Replace(
                            ARG1, ex.HResult.ToString()).Replace(
                            ARG2, Filename)) {
                            HResult = WINAGI_ERR + 502,
                        };
                        wex.Data["exception"] = ex;
                        wex.Data["badfile"] = Filename;
                        throw wex;
                    }
                    else {
                        Debug.Print("error: retry " + attempts);
                    }
                }
            }

            // Method to check if the file can be opened for exclusive access
            static bool CanOpen(string filePath) {
                try {
                    // Try to open the file with exclusive access
                    using FileStream fs = new(filePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None);
                    return true; // File is accessible
                }
                catch (IOException) {
                    return false; // File is in use or inaccessible
                }
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
            string linetext;
            int i;
            int startPos, endPos;

            // find the section we are looking for
            int section = FindSettingSection(Section);
            // if not found,
            if (section < 0) {
                if (!DontAdd) {
                    // add the section and the value
                    WriteSetting(Section, Key, Default);
                }
                // and return the default value
                return Default;
            }
            // step through all lines in this section; find matching key
            for (i = section + 1; i < Lines.Count; i++) {
                linetext = Lines[i].Replace("\t", " ").Trim();
                // skip blanks, and lines starting with a comment
                if (linetext.Length > 0 && linetext[0] != '#') {
                    // if another section is found, stop here
                    if (linetext[0] == '[') {
                        break;
                    }
                    // look for 'key'
                    // validate that this is an exact match, and not a key that starts with
                    // the same letters by verifying next char is either a space, or an equal sign
                    if (linetext.StartsWith(Key, StringComparison.OrdinalIgnoreCase) && (linetext.IndexOf(' ') == Key.Length || linetext.IndexOf('=') == Key.Length)) {
                        // found it- strip off key
                        linetext = linetext[Key.Length..].Trim();
                        // check for nullstring, in case line has ONLY the key and nothing else
                        if (linetext.Length > 0) {
                            // expect an equal sign
                            if (linetext[0] == '=') {
                                // remove it
                                linetext = linetext[1..].Trim();
                            }
                            if (linetext.Length > 0) {
                                if (linetext[0] == '"') {
                                    // string delimiter; find ending delimiter
                                    endPos = linetext.LastIndexOf('"', linetext.Length - 1);
                                    if (endPos < 0) {
                                        endPos = linetext.Length;
                                    }
                                    else {
                                        // if only one (end == start == 0)
                                        if (endPos == 0) {
                                            // set end to line length
                                            endPos = linetext.Length;
                                        }
                                    }
                                }
                                else {
                                    // look for comment marker
                                    endPos = linetext.IndexOf('#', 1);
                                    if (endPos < 0) {
                                        endPos = linetext.Length;
                                    }
                                }
                                // now strip off anything past value (including trailing quote delimiter)
                                linetext = linetext[..endPos].Trim();
                                if (linetext.Length > 0) {
                                    if (linetext[0] == '"') {
                                        linetext = linetext[1..];
                                    }
                                }
                                // check for control characters '\\' and '\n'
                                int pos = linetext.IndexOf('\\');
                                while (pos != -1) {
                                    if (linetext.IndexOf('\\', pos + 1) == pos + 1) {
                                        linetext = linetext.ReplaceFirst("\\\\", "\\", pos);
                                    }
                                    else if (linetext.IndexOf("n", pos + 1) == pos + 1) {
                                        linetext = linetext.ReplaceFirst("\\n", "\r\n", pos);
                                    }
                                    pos = linetext.IndexOf('\\', pos + 1);
                                }
                            }
                        }
                        return linetext;
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
            for (startPos = i - 1; startPos >= section; startPos--) {
                if (Lines[startPos].Trim().Length > 0) {
                    break;
                }
            }
            // add the key and default value at this pos
            Default = FormatValue(Default);
            Lines.Insert(startPos + 1, "   " + Key + " = " + Default);
            return sReturn;
        }

        /// <summary>
        /// Retrieves a value from the list of type int.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public int GetSetting(string Section, string Key, int Default = 0, bool DontAdd = false) {
            return GetSetting(Section, Key, Default, false, DontAdd);
        }

        /// <summary>
        /// Retrieves a value from the list of type PlatformType.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public PlatformType GetSetting(string Section, string Key, PlatformType Default = PlatformType.None, bool DontAdd = false) {
            // get a string value, parse it to PlatformType;
            // if it doesn't parse, return default
            string value = GetSetting(Section, Key, Default.ToString(), DontAdd);
            try {
                PlatformType retval = Enum.Parse<PlatformType>(value, true);
                if (Enum.IsDefined(retval)) {
                    return retval;
                }
                else {
                    return Default;
                }
            }
            catch {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type ResListType.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public ResListType GetSetting(string Section, string Key, ResListType Default = ResListType.None, bool DontAdd = false) {
            // get a string value, parse it to ResListType;
            // if it doesn't parse, return default
            string value = GetSetting(Section, Key, Default.ToString(), false);
            try {
                ResListType retval = Enum.Parse<ResListType>(value, true);
                if (Enum.IsDefined(retval)) {
                    return retval;
                }
                else {
                    return Default;
                }
            }
            catch {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type AskOption.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public AskOption GetSetting(string Section, string Key, AskOption Default = AskOption.Ask, bool DontAdd = false) {
            // get a string value, parse it to AskOption;
            // if it doesn't parse, return default
            string value = GetSetting(Section, Key, Default.ToString(), false);
            try {
                AskOption retval = Enum.Parse<AskOption>(value, true);
                if (Enum.IsDefined(retval)) {
                    return retval;
                }
                else {
                    return Default;
                }
            }
            catch {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type LogicErrorLevel.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public LogicErrorLevel GetSetting(string Section, string Key, LogicErrorLevel Default = LogicErrorLevel.Medium, bool DontAdd = false) {
            // get a string value, parse it to LogicErrorLevel;
            // if it doesn't parse, return default
            string value = GetSetting(Section, Key, Default.ToString(), false);
            try {
                LogicErrorLevel retval = Enum.Parse<LogicErrorLevel>(value, true);
                if (Enum.IsDefined(retval)) {
                    return retval;
                }
                else {
                    return Default;
                }
            }
            catch {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type AGICodeStyle.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <param name="DontAdd"></param>
        /// <returns></returns>
        public AGICodeStyle GetSetting(string Section, string Key, AGICodeStyle Default = AGICodeStyle.cstDefaultStyle, bool DontAdd = false) {
            // get a string value, parse it to AGICodeStyle;
            // if it doesn't parse, return default
            string value = GetSetting(Section, Key, Default.ToString(), false);
            try {
                AGICodeStyle retval = Enum.Parse<AGICodeStyle>(value, true);
                if (Enum.IsDefined(retval)) {
                    return retval;
                }
                else {
                    return Default;
                }
            }
            catch {
                return Default;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type int, optionally converting
        /// from hex value.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="hex"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public int GetSetting(string section, string key, int defaultvalue, bool hex, bool dontAdd) {
            // get the setting value; if it converts to int value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, hex ? "0x" + defaultvalue.ToString("x8") : defaultvalue.ToString(), dontAdd);
            if (value.Length == 0) {
                return defaultvalue;
            }
            else if (value.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception) {
                    return defaultvalue;
                }
            }
            else if (value.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    int retval = Convert.ToInt32(value.Right(value.Length - 2), 16);
                    // write the value in correct format
                    WriteSetting(section, key, "0x" + retval.ToString("x8"));
                    return retval;
                }
                catch (Exception) {
                    return defaultvalue;
                }
            }
            else if (int.TryParse(value, out int result)) {
                return result;
            }
            else {
                return defaultvalue;
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type uint.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public uint GetSetting(string section, string key, uint defaultvalue = 0, bool dontAdd = false) {
            // get the setting value; if it converts to uint value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, defaultvalue.ToString(), dontAdd);

            if (value.Length == 0) {
                return defaultvalue;
            }
            else if (value.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase)) {
                try {
                    return Convert.ToUInt32(value, 16);
                }
                catch (Exception) {
                    return defaultvalue;
                }
            }
            else if (value.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                try {
                    uint retval = Convert.ToUInt32(value.Right(value.Length - 2), 16);
                    // write the value in correct format
                    WriteSetting(section, key, "0x" + retval.ToString("x8"));
                    return retval;
                }
                catch (Exception) {
                    return defaultvalue;
                }
            }
            else {

                if (uint.TryParse(value, out uint result)) {
                    return result;
                }
                else {
                    return defaultvalue;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type byte.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public byte GetSetting(string section, string key, byte defaultvalue = 0, bool dontAdd = false) {
            // get the setting value; if it converts to byte value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, defaultvalue.ToString(), dontAdd);
            if (value.Length == 0) {
                return defaultvalue;
            }
            else {
                if (byte.TryParse(value, out byte result)) {
                    return result;
                }
                else {
                    return defaultvalue;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type double.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public double GetSetting(string section, string key, double defaultvalue = 0, bool dontAdd = false) {
            // get the setting value; if it converts to single value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, defaultvalue.ToString(), dontAdd);

            if (value.Length == 0) {
                return defaultvalue;
            }
            else {
                if (double.TryParse(value, out double result)) {
                    return result;
                }
                else {
                    return defaultvalue;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type float.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public float GetSetting(string section, string key, float defaultvalue = 0, bool dontAdd = false) {
            // get the setting value; if it converts to single value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, defaultvalue.ToString(), dontAdd);

            if (value.Length == 0) {
                return defaultvalue;
            }
            else {
                if (float.TryParse(value, out float result)) {
                    return result;
                }
                else {
                    return defaultvalue;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type bool.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public bool GetSetting(string section, string key, bool defaultvalue = false, bool dontAdd = false) {
            // get the setting value; if it converts to boolean value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, defaultvalue.ToString(), dontAdd);
            if (value.Length == 0) {
                return defaultvalue;
            }
            else {
                if (bool.TryParse(value, out bool result)) {
                    return result;
                }
                else {
                    return defaultvalue;
                }
            }
        }

        /// <summary>
        /// Retrieves a value from the list of type Color. Preferred format is
        /// '0xrr, 0xgg, 0xbb'
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <param name="dontAdd"></param>
        /// <returns></returns>
        public Color GetSetting(string section, string key, Color defaultvalue, bool dontAdd = false) {
            // get the setting value; if it converts to color value, use it;
            // if any kind of error, return the default value
            string value = GetSetting(section, key, "", dontAdd);
            if (value.Length == 0) {
                // for blank entries, replace with default
                if (!dontAdd) {
                    WriteSetting(section, key, EGAColors.ColorText(defaultvalue));
                }
                return defaultvalue;
            }
            // expected format is '0xrr, 0xgg, 0xbb'
            string[] comp = value.Split(",");
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
                    if (!dontAdd) {
                        WriteSetting(section, key, EGAColors.ColorText(defaultvalue));
                    }
                    return defaultvalue;
                }
            }
            else {
                Color retColor = defaultvalue;
                if (value.Left(2).Equals("0x", StringComparison.OrdinalIgnoreCase) || value.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
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
                        if (value.Left(2).Equals("&H", StringComparison.OrdinalIgnoreCase)) {
                            value = "0x" + value[2..];
                            flipit = true;
                        }
                        // assign to int number
                        int iColor = Convert.ToInt32(value, 16);
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
                    if (int.TryParse(value, out int iColor)) {
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
                if (!dontAdd) {
                    WriteSetting(section, key, EGAColors.ColorText(retColor));
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
                string linetext = Lines[i].Replace('\t', ' ').Trim();
                if (linetext.Length > 0) {
                    if (linetext[0] != '#') {
                        if (linetext[0] == '[') {
                            int pos = linetext.IndexOf(']');
                            if (pos >= 0) {
                                linetext = linetext[1..pos].Trim();
                            }
                            else {
                                continue;
                            }
                            if (linetext.Equals(Section, StringComparison.OrdinalIgnoreCase)) {
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
                string linetext = Lines[i].Replace('\t', ' ').Trim();
                if (linetext.Length > 0) {
                    if (linetext[0] != '#') {
                        if (linetext[0] == '[') {
                            // another section is found, stop here
                            Start = i;
                            return retval;
                        }
                        // look for key/value pairs
                        int split = linetext.IndexOf('=');
                        if (split >= 0) {
                            string key = linetext[..split].Trim();
                            string value = linetext[(split + 1)..].Trim();
                            int pos;
                            if (value.Length != 0) {
                                if (value[0] == '"') {
                                    // string delimiter
                                    value = value[1..];
                                    // find ending delimiter
                                    pos = value.LastIndexOf('"');
                                    if (pos >= 0) {
                                        value = value[..pos];
                                    }
                                }
                                else {
                                    pos = value.IndexOf('#');
                                    if (pos == -1) {
                                        // no delimiter or comment found; assume entire line
                                        pos = value.Length;
                                    }
                                    value = value[..pos].Trim();
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
        /// <param name="section"></param>
        public void DeleteSection(string section) {
            int sectionPos;
            string linetext;

            // find the section we are looking for
            sectionPos = FindSettingSection(section);
            if (sectionPos == -1) {
                // nothing to delete
                return;
            }
            // step through all lines in this section, deleting until another section
            // or end of list is found
            while (sectionPos < Lines.Count) {
                Lines.RemoveAt(sectionPos);
                if (sectionPos >= Lines.Count) {
                    return;
                }
                linetext = Lines[sectionPos].Replace('\t', ' ').Trim();
                if (linetext.Length > 0) {
                    if (linetext[0] == '[') {
                        // nothing to delete
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a key/value pair from the list.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        public void DeleteKey(string section, string key) {
            string linetext;
            int sectionPos, keylength;

            // find the section we are looking for
            sectionPos = FindSettingSection(section);
            // if not found,
            if (sectionPos <= 0) {
                // nothing to delete
                return;
            }
            // step through all lines in this section; find matching key
            keylength = key.Length;
            for (int i = sectionPos + 1; i < Lines.Count; i++) {
                // skip blanks, and lines starting with a comment
                linetext = Lines[i].Replace("\t", " ").Trim();
                if (linetext.Length > 0) {
                    if (linetext[0] != '#') {
                        // not a comment
                        // if another section is found, stop here
                        if (linetext[0] == '[') {
                            // nothing to delete
                            return;
                        }
                        // look for key
                        if (linetext.Left(keylength) == key) {
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
        /// <param name="section"></param>
        /// <returns></returns>
        public int FindSettingSection(string section) {
            // find the section we are looking for
            for (int i = 0; i <= Lines.Count - 1; i++) {
                // skip blanks, and lines starting with a comment
                string linetext = Lines[i].Replace("\t", " ").Trim();
                if (linetext.Length > 0) {
                    if (linetext[0] != '#') {
                        // look for a bracket
                        if (linetext[0] == '[') {
                            // find end bracket
                            int pos = linetext.IndexOf(']', 1);
                            string check;
                            if (pos > 0) {
                                check = linetext.Mid(1, pos - 1);
                            }
                            else {
                                check = linetext.Right(linetext.Length - 1);
                            }
                            if (check.Equals(section, StringComparison.OrdinalIgnoreCase)) {
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
            itemValue = itemvalue.ToArgb();
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
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
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
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
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
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
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
            itemValue = file.GetSetting(Section, Name, defaultvalue, dontadd);
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
