using System;
using System.Collections.Generic;
using System.IO;
using static WinAGI.Common.Base;

namespace WinAGI.Engine
{
    using System.Drawing;

    public class SettingsList(string filename)
    {
        //elements of a settings list file:
        //
        //  #comments begin with hashtag; all characters on line after hashtag are ignored
        //  comments can be added to end of valid section or key / value line
        //  blank lines are ignored
        //  [::BEGIN group::] marker to indicate a group of sections
        //  [::END group::]   marker to indicate end of a group
        //  [section]         sections indicated by square brackets; anything else on the line gets ignored
        //  key=value         key/value pairs separated by an equal sign; no quotes around values means only
        //                      single word; use quotes for multiword strings
        //  if string is multline, use '\n' control code (and use multiline option)
        internal List<string> Lines = new List<string>();
        public string Filename
        {
            get;
            set;
        } = filename;

        public void WriteSetting(string Section, string Key, dynamic Value, string Group = "")
        {
            int lngPos, i;
            string strLine, strCheck;
            int lngSectionEnd = 0;
            int lenKey; bool blnFound = false;
            int lngGrpStart, lngGrpEnd, lngInsertLine;
            if (Value == null) {
                strCheck = "\"\"";
            }
            else {  //convert to string
                strCheck = Value.ToString();
            }
            //if value contains spaces, it must be enclosed in quotes
            if (strCheck.IndexOf(" ") > 0) {
                if (strCheck[0] != '"') {
                    strCheck = "\"" + strCheck;
                }
                if ((strCheck[strCheck.Length - 1] != '"')) {
                    strCheck += "\"";
                }
            }
            //if value contains any carriage returns, replace them with control characters
            if (strCheck.Contains("\r\n", StringComparison.OrdinalIgnoreCase)) {
                strCheck = strCheck.Replace("\r\n", "\\n");
            }
            if (strCheck.Contains("\r", StringComparison.OrdinalIgnoreCase)) {
                strCheck = strCheck.Replace("\r", "\\n");
            }
            if (strCheck.Contains("\n", StringComparison.OrdinalIgnoreCase)) {
                strCheck = strCheck.Replace("\n", "\\n");
            }
            //if nullstring, include quotes
            if (strCheck.Length == 0) {
                strCheck = "\"\"";
            }
            //if a group is provided, we will add new items within the group;
            //existing items, even if within the group, will be left where they are
            lngGrpStart = -1;
            lngGrpEnd = -1;
            lngPos = -1;
            if (Group.Length > 0) {
                //********** we will have to make adjustments to group start
                //           and end positions later on as we add lines
                //           during the key update! don't forget to do that!!
                for (i = 1; i <= Lines.Count - 1; i++) {
                    //skip blanks, and lines starting with a comment
                    strLine = Lines[i].Replace("\t", " ").Trim();
                    if (strLine.Length > 0) {
                        //skip empty lines
                        if (strLine[0] != '#') {
                            //skip comments
                            //if not found yet, look for the starting marker
                            if (!blnFound) {
                                //is this the group marker?
                                if (strLine.Equals("[::BEGIN " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                    lngGrpStart = i;
                                    blnFound = true;
                                    //was the end found earlier? if so, we are done
                                    if (lngGrpEnd >= 0) {
                                        break;
                                    }
                                }
                            }
                            else {
                                //start is found; make sure we find end before
                                //finding another start
                                if (Left(strLine, 9).Equals("[::BEGIN ", StringComparison.OrdinalIgnoreCase)) {
                                    //mark position of first new start, so we can move the end marker here
                                    if (lngPos < 0) {
                                        lngPos = i;
                                    }
                                }
                            }
                            //we check for end marker here even if start not found
                            //just in case they are backwards
                            if (strLine.Equals("[::END " + Group + "::]", StringComparison.OrdinalIgnoreCase)) {
                                lngGrpEnd = i;
                                //and if we also have a start, we can exit the loop
                                if (blnFound) {
                                    break;
                                }
                            }
                        }
                    }
                }

                //possible outcomes:
                // - start and end both found; start before end
                //   this is what we want
                //
                // - start and end both found, but end before start
                //   this is backwards; we fix by moving end to
                //   the line after start
                //
                // - start found, but no end; we add end
                //   to just before next group start, or
                //   to end of file if no other group start
                //
                // - end found, but no start; we fix by putting
                //   start right in front of end

                // if both found
                if (lngGrpStart >= 0 && lngGrpEnd >= 0) {
                    //if backwards, move end to line after start
                    if (lngGrpEnd < lngGrpStart) {
                        Lines.Insert(lngGrpStart + 1, Lines[lngGrpEnd]);
                        Lines.RemoveAt(lngGrpEnd);
                        lngGrpStart -= 1;
                        lngGrpEnd = lngGrpStart + 1;
                    }
                }
                // if only start found
                else if (lngGrpStart >= 0) {
                    //means end not found
                    //if there was another start found, insert end there
                    if (lngPos > 0) {
                        Lines.Insert(lngPos, "[::END " + Group + "::]");
                        Lines.Insert(lngPos + 1, "");
                        lngGrpEnd = lngPos;
                    }
                    else {
                        //otherwise insert group end at end of file
                        lngGrpEnd = Lines.Count;
                        Lines.Add("[::END " + Group + "::]");
                    }
                }
                // if end found but no start
                else if (lngGrpEnd >= 0) {
                    //means start not found
                    //insert start in front of end
                    lngGrpStart = lngGrpEnd;
                    Lines.Insert(lngGrpStart, "[::START " + Group + "::]");
                    lngGrpEnd = lngGrpStart + 1;
                }
                else {
                    //means neither found
                    //make sure at least one blank line
                    if (Lines[Lines.Count - 1].Trim().Length > 0) {
                        Lines.Add("");
                    }
                    lngGrpStart = Lines.Count;
                    Lines.Add("[::BEGIN " + Group + "::]");
                    Lines.Add("[::END " + Group + "::]");
                    lngGrpEnd = lngGrpStart + 1;
                }
            }
            //reset the found flag
            blnFound = false;
            //find the section we are looking for
            int lngSectionStart = FindSettingSection(Section);
            //if not found, create it at end of group (if group is provided)
            //otherwise at end of list
            if (lngSectionStart <= 0) {
                if (lngGrpStart >= 0) {
                    lngInsertLine = lngGrpEnd;
                }
                else {
                    lngInsertLine = Lines.Count;
                }
                //make sure there is at least one blank line (unless this is first line in list)
                if (lngInsertLine > 0) {
                    if (Lines[lngInsertLine - 1].Trim().Length != 0) {
                        Lines.Insert(lngInsertLine, "");
                        lngInsertLine++;
                    }
                }
                Lines.Insert(lngInsertLine, "[" + Section + "]");
                Lines.Insert(lngInsertLine + 1, "   " + Key + " = " + strCheck);
                //no need to check for location of section within group;
                //we just added it to the group (if one is needed)
            }
            else {
                //now step through all lines in this section; find matching key
                lenKey = Key.Length;
                for (i = lngSectionStart + 1; i < Lines.Count; i++) {
                    //skip blanks, and lines starting with a comment
                    strLine = Lines[i].Replace("\t", " ").Trim();
                    if (strLine.Length > 0) {
                        if (strLine[0] != '#') {
                            //if another section is found, stop here
                            if (strLine[0] == '[') {
                                //if part of a group; last line of the section
                                //is line prior to the new section
                                if (lngGrpStart >= 0) {
                                    lngSectionEnd = i - 1;
                                }
                                //if not already added, add it now
                                if (!blnFound) {
                                    //back up until a nonblank line is found
                                    for (lngPos = i - 1; lngPos >= lngSectionStart; lngPos--) {
                                        if (Lines[lngPos].Trim().Length > 0) {
                                            break;
                                        }
                                    }
                                    //add the key and value at this pos
                                    Lines.Insert(lngPos + 1, "   " + Key + " = " + strCheck);
                                    //this also bumps down the section end
                                    lngSectionEnd++;
                                    //it also may bump down group start/end
                                    if (lngGrpStart >= lngPos + 1) {
                                        lngGrpStart++;
                                    }
                                    if (lngGrpEnd >= lngPos + 1) {
                                        lngGrpEnd++;
                                    }
                                }
                                //we are done, but if part of a group
                                //we need to verify the section is in
                                //the group
                                if (lngGrpStart >= 0) {
                                    blnFound = true;
                                    break;
                                }
                                else {
                                    return;
                                }
                            }
                            //if not already found,  look for 'key'
                            if (!blnFound) {
                                if (Left(strLine, lenKey).Equals(Key, StringComparison.OrdinalIgnoreCase) && (strLine.Substring(lenKey + 1, 1) == " " || strLine.Substring(lenKey + 1, 1) == "=")) {
                                    //found it- change key value to match new value
                                    //(if there is a comment on the end, save it)
                                    strLine = Right(strLine, strLine.Length - lenKey).Trim();
                                    if (strLine.Length > 0) {
                                        //expect an equal sign
                                        if (strLine[0] == '=') {
                                            //remove it
                                            strLine = Right(strLine, strLine.Length - 1).Trim();
                                        }
                                    }
                                    if (strLine.Length > 0) {
                                        if (strLine[0] == '"') {
                                            //string delimiter; find ending delimiter
                                            lngPos = strLine.IndexOf('"', 1) + 1;
                                        }
                                        else {
                                            //look for comment marker
                                            lngPos = strLine.IndexOf("#", 1);
                                            // if none found, look for space delimiter
                                            if (lngPos == -1) {
                                                //look for a space as a delimiter
                                                lngPos = strLine.IndexOf(" ", 1);
                                            }
                                            ////look for a space as a delimiter
                                            //lngPos = strLine.IndexOf(" ", 1);
                                            //if (lngPos == -1)
                                            //{
                                            //  //could be a case where a comment is at end of text, without a space
                                            //  //if so we need to keep the delimiter
                                            //  lngPos = strLine.IndexOf("#", 2) - 1;
                                            //}
                                        }
                                        //no delimiter found; assume entire line
                                        if (lngPos <= 0) {
                                            lngPos = strLine.Length;
                                        }
                                        //now strip off the old value, leaving potential comment
                                        strLine = Right(strLine, strLine.Length - lngPos).Trim();
                                    }
                                    //if something left, maks sure it's a comment
                                    if (strLine.Length > 0) {
                                        if (strLine[0] != '#') {
                                            strLine = "#" + strLine;
                                        }
                                        //make sure it starts with a space
                                        strLine = "   " + strLine;
                                    }
                                    strLine = "   " + Key + " = " + strCheck + strLine;
                                    Lines[i] = strLine;
                                    //we are done, but if part of a group
                                    //we need to keep going to find end so
                                    //we can validate section is in the group
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
                }
                //if not found (will only happen if this the last section in the
                //list, probably NOT in a group, but still possible (if the
                //section is outside the defined group)
                if (!blnFound) {
                    //back up until a nonblank line is found
                    for (lngPos = i - 1; i >= lngSectionStart; i--) {
                        if (Lines[lngPos].Trim().Length > 0) {
                            break;
                        }
                    }
                    //add the key and value at this pos
                    Lines.Insert(lngPos + 1, "   " + Key + " = " + strCheck);
                    //we SHOULD be done, but just in case this section is
                    //out of position, we still check for the group
                    if (lngGrpStart < 0) {
                        //no group - all done!
                        return;
                    }
                    //note that we don't need to bother adjusting group
                    //start/end, because we only added a line to the
                    //end of the file, and we know that the group
                    //start/end markers MUST be before the start
                    //of this section
                }
                //found marker ONLY set if part a group so let's verify
                //the section is in the group, moving it if necessary

                //if this was last section, AND section is NOT in its group
                //then then section end won't be set yet
                if (lngSectionEnd <= 0) {
                    lngSectionEnd = Lines.Count - 1;
                }
                //if the section is not in the group, then move it
                //(depends on whether section is BEFORE or AFTER group start)
                if (lngSectionStart < lngGrpStart) {
                    //make sure at least one blank line above the group end
                    if (Lines[lngGrpEnd - 1].Trim().Length > 0) {
                        Lines.Insert(lngGrpEnd, "");
                        lngGrpEnd++;
                    }
                    //add the section to end of group
                    for (i = lngSectionStart; i <= lngSectionEnd; i++) {
                        Lines.Insert(lngGrpEnd, Lines[i]);
                        lngGrpEnd++;
                    }
                    //then delete the section from it's current location
                    for (i = lngSectionStart; i <= lngSectionEnd; i++) {
                        Lines.RemoveAt(lngSectionStart);
                    }
                }
                else if (lngSectionStart > lngGrpEnd) {
                    //make sure at least one blank line above the group end
                    if (Lines[lngGrpEnd - 1].Trim().Length > 0) {
                        Lines.Insert(lngGrpEnd, "");
                        lngGrpEnd++;
                        lngSectionStart++;
                        lngSectionEnd++;
                    }
                    //add the section to end of group
                    for (i = lngSectionEnd; i >= lngSectionStart; i--) {
                        Lines.Insert(lngGrpEnd, Lines[lngSectionEnd]);
                        //delete the line in current location
                        Lines.RemoveAt(lngSectionEnd + 1);
                    }
                }
            }
        }
        internal void Save()
        {
            //open temp file
            string TempFile = Path.GetTempFileName();
            try {
                using StreamWriter cfgSR = new(TempFile);
                //now output the results to the file
                foreach (string line in Lines)
                    cfgSR.WriteLine(line);
                //// close it
                //cfgSR.Close();
                //dispose it
                cfgSR.Dispose();
                // now copy new to final destination
                File.Move(TempFile, Filename, true);
            }
            catch (Exception) {
                // do we care if there is a file error?
                throw;
            }
        }
        internal void Open(bool CreateNew = true)
        {
            // opens this SettingsList

            // if file does not exist, a blank list object is created
            // if the CreateNew flag is set, the blank file is also saved to disk
            FileStream fsConfig;
            StreamWriter swConfig;
            StreamReader srConfig;

            if (File.Exists(Filename) || CreateNew) {
                //open the config file for create/write
                fsConfig = new FileStream(Filename, FileMode.OpenOrCreate);
                long lngLen = fsConfig.Length;
                //if this is an empty file (either previously empty or created by this call)
                if (lngLen == 0) {
                    swConfig = new StreamWriter(fsConfig);
                    //add a single comment to the file
                    Lines.Add("#");
                    // and write it to the file
                    swConfig.WriteLine("#");
                    swConfig.Dispose();
                }
                else {
                    //grab the file data
                    srConfig = new StreamReader(fsConfig);
                    while (!srConfig.EndOfStream) {

                        string strInput = srConfig.ReadLine();
                        Lines.Add(strInput);
                    }
                    srConfig.Dispose();
                }
                fsConfig.Dispose();
            }
            else {
                //if file doesn't exist, and NOT forcing new file creation
                //just add a single comment as first line
                Lines.Add("#");
            }
        }
        public string GetSetting(string Section, string Key, string Default = "", bool DontAdd = false)
        {
            //need to make sure there is a list to read from
            if (Lines.Count == 0) {
                //return the default
                return Default;
            }
            //elements of a settings file:
            //
            //  #comments begin with hashtag; all characters on line after hashtag are ignored
            //  //comments can be added to end of valid section or key/value line
            //  blank lines are ignored
            //  [::BEGIN group::] marker to indicate a group of sections
            //  [::END group::] marker to indicate end of a group
            //  [section] sections indicated by square brackets; anything else on the line gets ignored
            //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
            //    single word; use quotes for multiword strings
            //  if string is multline, use '\n' control code (and use multiline option)
            string strLine;
            int i;
            int lngPos;
            //find the section we are looking for (skip 1st line; it's the filename)
            int lngSection = FindSettingSection(Section);
            //if not found,
            if (lngSection < 0) {
                if (!DontAdd)
                {
                    //add the section and the value
                    WriteSetting(Section, Key, Default, "");
                }
                //and return the default value
                return Default;
            }
            else {
                //step through all lines in this section; find matching key
                int lenKey = Key.Length;
                for (i = lngSection + 1; i < Lines.Count; i++) {
                    //skip blanks, and lines starting with a comment
                    strLine = Lines[i].Replace("\t", " ").Trim();
                    if (strLine.Length > 0) {
                        if (strLine[0] != '#') { //not a comment
                                                 //if another section is found, stop here
                            if (strLine[0] == '[') {
                                break;
                            }
                            //look for 'key'
                            //validate that this is an exact match, and not a key that starts with
                            //the same letters by verifying next char is either a space, or an equal sign
                            if (Left(strLine, lenKey).Equals(Key, StringComparison.OrdinalIgnoreCase) && (strLine.Substring(lenKey, 1) == " " || strLine.Substring(lenKey, 1) == "=")) {
                                //found it- extract value (if there is a comment on the end, drop it)
                                //strip off key
                                strLine = Right(strLine, strLine.Length - lenKey).Trim();
                                //check for nullstring, incase line has ONLY the key and nothing else
                                if (strLine.Length > 0) {
                                    //expect an equal sign
                                    if (strLine[0] == '=') {
                                        //remove it
                                        strLine = Right(strLine, strLine.Length - 1).Trim();
                                    }
                                    if (strLine.Length > 0) {
                                        if (strLine[0] == '"') {
                                            //string delimiter; find ending delimiter
                                            // (don't add 1; we want to drop the ending quote)
                                            lngPos = strLine.IndexOf("\"", 1);
                                        }
                                        else {
                                            //look for comment marker (don't add to result -
                                            // the coment marker gets stripped off)
                                            lngPos = strLine.IndexOf("#", 1);
                                        }
                                        //no delimiter found; assume entire line
                                        if (lngPos <= 0) {
                                            lngPos = strLine.Length;
                                        }
                                        //now strip off anything past value (including quote delimiter)
                                        strLine = Left(strLine, lngPos).Trim();
                                        if (strLine.Length > 0) {
                                            //if a leading quote, remove it
                                            if (strLine[0] == '"') {
                                                strLine = Right(strLine, strLine.Length - 1);
                                            }
                                        }
                                        //should never have an end quote; it will be caught as the ending delimiter
                                        if (strLine.Length > 0) {
                                            if (Right(strLine, 1)[0] == '"') {
                                                strLine = Left(strLine, strLine.Length - 1);
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
                }
                // not found - 
                if (DontAdd)
                {
                    //return the default
                    return Default;
                }
                //stach the default value unmodified by control codes
                string sReturn = Default;
                // add teh default value here
                //back up until a nonblank line is found
                for (lngPos = i - 1; lngPos >= lngSection; lngPos--) {
                    if (Lines[lngPos].Trim().Length > 0) {
                        break;
                    }
                }
                //add the key and default value at this pos
                //if value contains spaces, it must be enclosed in quotes
                if (Default.IndexOf(" ", 0) >= 0) {
                    if (Default[0] != '"') {
                        Default = "\"" + Default;
                    }
                    if (Right(Default, 1)[0] != '"') {
                        Default += "\"";
                    }
                }
                //if Default contains any carriage returns, replace them with control characters
                if (Default.IndexOf("\r\n", 0) >= 0) {
                    Default = Default.Replace("\r\n", "\\n");
                }
                if (Default.IndexOf("\r", 0) >= 0) {
                    Default = Default.Replace("\r", "\\n");
                }
                if (Default.IndexOf("\n", 0) >= 0) {
                    Default = Default.Replace("\n", "\\n");
                }
                if (Default.Length == 0) {
                    Default = "\"\"";
                }
                Lines.Insert(lngPos + 1, "   " + Key + " = " + Default);
                return sReturn;
            }
        }
        public int GetSetting(string Section, string Key, int Default = 0, bool hex = false, bool DontAdd = false)
        {
            //get the setting value; if it converts to long value, use it;
            //if any kind of error, return the default value
            string strValue = GetSetting(Section, Key, hex ? "0x" + Default.ToString("x8") : Default.ToString(), DontAdd);
            if (strValue.Length == 0)
            {
                return Default;
            }
            else if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return Convert.ToInt32(strValue, 16);
                }
                catch (Exception)
                {
                    return Default;
                }
            }
            else if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase))
            {
                try
                { // TODO: check all number conversions for correct type
                    int retval = Convert.ToInt32(Right(strValue, strValue.Length - 2), 16);
                    //write the value in correct format
                    WriteSetting(Section, Key, "0x" + retval.ToString("x8"));
                    return retval;
                }
                catch (Exception)
                {
                    return Default;
                }
            }
            else
            {
                if (int.TryParse(strValue, out int iResult))
                {
                    return iResult;
                }
                else
                {
                    return Default;
                }
            }
        }
        public uint GetSetting(string Section, string Key, uint Default = 0, bool DontAdd = false)
        {
            //get the setting value; if it converts to long value, use it;
            //if any kind of error, return the default value
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
        public byte GetSetting(string Section, string Key, byte Default = 0, bool DontAdd = false)
        {
            //get the setting value; if it converts to byte value, use it;
            //if any kind of error, return the default value
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
        public double GetSetting(string Section, string Key, double Default = 0, bool DontAdd = false)
        {
            //get the setting value; if it converts to single value, use it;
            //if any kind of error, return the default value
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
        public bool GetSetting(string Section, string Key, bool Default = false, bool DontAdd = false)
        {
            //get the setting value; if it converts to boolean value, use it;
            //if any kind of error, return the default value
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
        public Color GetSetting(string Section, string Key, Color Default, bool DontAdd = false)
        {
            //get the setting value; if it converts to long value, use it;
            //if any kind of error, return the default value
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
                    try {
                        // convert hex integer color value; assume it is '0xaarrggbb' or '&Haarrggbb';
                        // we can't just convert it to a number and then translate it;
                        // the translator expects colors to be in '0xaabbggrr' format
                        // also, we ignore the alpha; all colors are returned with only
                        // rgb components set; the alpha channel is always left to default

                        // assign to int number
                        int iColor = Convert.ToInt32(strValue, 16);
                        //parse it into rgb components
                        int r, g, b;
                        b = iColor % 0x100;
                        g = (iColor >> 8) % 0x100;
                        r = (iColor >> 16) % 0x100;
                        retColor = Color.FromArgb(r, g, b);
                    }
                    catch (Exception) {
                        //keep default
                    }
                }
                else {
                    if (int.TryParse(strValue, out int iColor)) {
                        // it might be a non-hex color number
                        try {
                            //parse it into rgb components
                            int r, g, b;
                            b = (int)(unchecked((uint)iColor) % 0x100);
                            g = (int)(unchecked(((uint)iColor) >> 8) % 0x100);
                            r = (int)(unchecked(((uint)iColor) >> 16) % 0x100);
                            retColor = Color.FromArgb(r, g, b);
                        }
                        catch (Exception) {
                            //keep default;
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
        public void DeleteSection(string Section)
        {
            //elements of a settings file:
            //
            //  #comments begin with hashtag; all characters on line after hashtag are ignored
            //  //comments can be added to end of valid section or key/value line
            //  blank lines are ignored
            //  [section] sections indicated by square brackets; anything else on the line gets ignored
            //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
            //    single word; use quotes for multiword strings
            //  if string is multline, use '\n' control code (and use multiline option)

            int lngPos, lngSection = 0, i;
            string strLine, strCheck;
            //find the section we are looking for (skip 1st line; it's the filename)
            for (i = 1; i < Lines.Count; i++) {
                //skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        //look for a bracket
                        if (strLine[0] == '[') {
                            //find end bracket
                            lngPos = strLine.IndexOf("]", 1);
                            if (lngPos >= 0) {
                                strCheck = Mid(strLine, 1, lngPos - 2);
                            }
                            else {
                                strCheck = Right(strLine, strLine.Length - 1);
                            }
                            if (strCheck.Equals(Section, StringComparison.OrdinalIgnoreCase)) {
                                //found it
                                lngSection = i;
                                break;
                            }
                        }
                    }
                }
            }
            //if not found,
            if (lngSection == 0) {
                //nothing to delete
                return;
            }
            //step through all lines in this section, deleting until another section or end of list is found
            do {  //delete this line
                Lines.RemoveAt(lngSection);

                //at end?
                if (lngSection >= Lines.Count) {
                    return;
                }
                //or another section found?
                strLine = Lines[lngSection].Replace('\t', ' ').Trim();
                if (strLine.Length > 0) {
                    //if another section is found, stop here
                    if (strLine[0] == (char)91) {
                        //nothing to delete
                        return;
                    }
                }
            } while (true);
        }
        public void DeleteKey(string Section, string Key)
        {
            //elements of a settings file:
            //
            //  #comments begin with hashtag; all characters on line after hashtag are ignored
            //  //comments can be added to end of valid section or key/value line
            //  blank lines are ignored
            //  [section] sections indicated by square brackets; anything else on the line gets ignored
            //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
            //    single word; use quotes for multiword strings
            //  if string is multline, use '\n' control code (and use multiline option)

            int i;
            string strLine;
            int lngSection, lenKey;
            //find the section we are looking for (skip 1st line; it's the filename)
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
        private int FindSettingSection(string Section)
        {
            int i, lngPos;
            string strLine;
            //find the section we are looking for (skip 1st line; it's the filename)
            for (i = 1; i <= Lines.Count - 1; i++) {
                //skip blanks, and lines starting with a comment
                strLine = Lines[i].Replace("\t", " ").Trim();
                if (strLine.Length > 0) {
                    if (strLine[0] != '#') {
                        //look for a bracket
                        if (strLine[0] == '[') {
                            //find end bracket
                            lngPos = strLine.IndexOf("]", 1);
                            string strCheck;
                            if (lngPos > 0) {
                                strCheck = Mid(strLine, 1, lngPos - 1);
                            }
                            else {
                                strCheck = Right(strLine, strLine.Length - 1);
                            }
                            if (strCheck.Equals(Section, StringComparison.OrdinalIgnoreCase)) {
                                //found it
                                return i;
                            }
                        }
                    }
                }
            }
            // not found
            return -1;
        }
    }
}
