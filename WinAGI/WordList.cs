using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using System.IO;
using System.Diagnostics;

namespace WinAGI.Engine
{
  public class WordList : IEnumerable<WordGroup>
  {
    //collection of words (agiWord objects)

    SortedList<string, AGIWord> mWordCol;
    SortedList<int, WordGroup> mGroupCol;
    string mResFile = "";
    string mDescription = "";
    bool mInGame;
    AGIGame parent;
    bool mIsDirty;
    bool mWriteProps;
    bool mLoaded;
    readonly string strErrSource = "WinAGI.AGIWordList";
    public WordList()
    {
      //initialize the collections
      mWordCol = new SortedList<string, AGIWord>();
      mGroupCol = new SortedList<int, WordGroup>();
      // add starting words, but don't use AddWord method
      // because list is not loaded yet
      AGIWord tmpWord = new AGIWord { WordText = "a", Group = 0 };
      mWordCol.Add("a", tmpWord);
      tmpWord.WordText = "anyword";
      tmpWord.Group = 1;
      mWordCol.Add("anyword", tmpWord);
      tmpWord.WordText = "rol";
      tmpWord.Group = 9999;
      mWordCol.Add("rol", tmpWord);
      WordGroup tmpGroup = new WordGroup { mGroupNum = 0 };
      tmpGroup.AddWordToGroup("a");
      mGroupCol.Add(0, tmpGroup);
      tmpGroup = new WordGroup { mGroupNum = 1 };
      tmpGroup.AddWordToGroup("any");
      mGroupCol.Add(1, tmpGroup);
      tmpGroup = new WordGroup { mGroupNum = 9999 };
      tmpGroup.AddWordToGroup("rol");
      mGroupCol.Add(9999, tmpGroup);
    }
    internal WordList(AGIGame parent, bool Loaded = false)
    {
      //initialize the collections
      mWordCol = new SortedList<string, AGIWord>();
      mGroupCol = new SortedList<int, WordGroup>();
      //this function is only called for a vocab word list
      //that is part of a game
      //it sets the ingame flag
      mInGame = true;
      this.parent = parent;
      //if loaded property is passed, set loaded flag as well
      mLoaded = Loaded;
      //it also sets the default name to //WORDS.TOK//
      mResFile = this.parent.agGameDir + "WORDS.TOK";
    }
    public void NewWords()
    {
      //marks the resource as loaded
      //this is needed so new resources can be created and edited
      //if already loaded
      if (mLoaded)
      {
        //error

        Exception e = new(LoadResString(642))
        {
          HResult = 642
        };
        throw e;
      }

      //cant call NewResource if already in a game;
      //clear it instead
      if (mInGame)
      {

        Exception e = new(LoadResString(510))
        {
          HResult = 510
        };
        throw e;
      }
      //mark as loaded
      mLoaded = true;
      //clear resname and description
      mResFile = "";
      mDescription = "";
      //use clear method to ensure list is reset
      Clear();

      //add default groups
      AddGroup(0);
      AddWord("a", 0);
      AddGroup(1);
      AddWord("anyword", 1);
      AddGroup(9999);
      AddWord("rol", 9999);
    }
    void CompileWinAGI(string CompileFile)
    {
      /*
      //for the text file,
      //words are saved as one group to a line
      //in numerical order by group
      //each line contains the group number, and
      //the the group//s list of words in alphabetical
      //order (separated by a tab character)
      //(empty groups are skipped during the save)
  
      //first line is version identifier
      //file description is saved at end of words
  
      string strTempFile ;
      int intFile
      AGIWordGroup tmpGroup
      AGIWord tmpWord
      int i, j
      string strOutput
  
      //if no name
      if (CompileFile.Length == 0) {
        On Error GoTo 0
        //raise error
        Exception e = new(LoadResString(615))
        {
          HResult = 615
        };
        throw e;
      }
  
      //get temporary file
      strTempFile = Path.GetTempFileName();
    
      //open file for output
      intFile = FreeFile()
      Open strTempFile Output intFile
  
      //print version
      Print #intFile, WINAGI_VERSION
  
      //step through all groups
      foreach (AGIWordGroup tmpGroup in this) {
          //add group number to output line
          strOutput = CStr(tmpGroup.GroupNum)
          //step through all words
          foreach (string tmpWord in tmpGroup) {
            //add tab character and this word
            strOutput = strOutput + vbTab + tmpWord;
          } //nxt  i
          //add this group to output file
          Print #intFile, strOutput
      } //nxt  j
  
      //if there is a description
      if (mDescription.Length != 0) {
        //print eof marker
        Print #intFile, Chr$(255) + Chr$(255)
        //print description
        Print #intFile, mDescription
      }
  
      //close file
      Close intFile
  
      //if CompileFile exists
      if (File.Exists(CompileFile)) {
        //delete it
        Kill CompileFile
      }
  
      //copy tempfile to CompileFile
      FileCopy strTempFile, CompileFile
  
      //delete temp file
      Kill strTempFile
      Err.Clear
    return;
  
    ErrHandler:
      //close file
      Close intFile
      //erase the temp file
      Kill CompileFile
      //return error condition
        Exception e = new(LoadResString(582))
        {
          HResult = 582
        };
        throw e;
      */
    }
    public void Export(string ExportFile, int FileType, bool ResetDirty = true)
    {
      //exports the list of words
      //  filetype = 0 means AGI WORDS.TOK file
      //  filetype = 1 means WinAGI word list file
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      switch (FileType)
      {
      case 0: //compile agi WORDS.TOK file
        Compile(ExportFile);
        break;
      case 1:  //create WinAGI word list
        CompileWinAGI(ExportFile);
        break;
      }
      //if NOT in a game,
      if (!mInGame)
      {
        if (ResetDirty)
        {
          //clear dirty flag
          mIsDirty = false;
        }
        //save filename
        mResFile = ExportFile;
      }
      return;
    }
    internal AGIWord ItemByIndex(int Index)
    {  //used by SetWords method to retrieve words one by one
      //NOTE: no error checking is done, because ONLY
      //SetWords uses this function, and it ensures
      //Index is valid
      return mWordCol.Values[Index];
    }
    public bool Loaded
    {
      get { return mLoaded; }
    }
    bool LoadSierraFile(string LoadFile)
    {
      byte bytHigh, bytLow;
      int lngPos;
      byte[] bytData = Array.Empty<byte>();
      string strThisWord;
      string strPrevWord;
      byte bytVal;
      int lngGrpNum;
      byte bytPrevWordCharCount;
      FileStream fsWords;
      try
      {
        //open the file
        fsWords = new FileStream(LoadFile, FileMode.Open);
      }
      catch (Exception)
      {
        // no good
        throw new Exception("bad WORDS.TOK file");
      }
      //if no data,
      if (fsWords.Length == 0)
      {
        fsWords.Dispose();
        return false;
      }
      //set load flag
      mLoaded = true;
      // empty word and group columns
      mWordCol = new SortedList<string, AGIWord> { };
      mGroupCol = new SortedList<int, WordGroup> { };

      //read in entire resource
      Array.Resize(ref bytData, (int)fsWords.Length);
      fsWords.Read(bytData);
      //start at beginning of words section
      //note that words.tok file uses MSLS format for two byte-word data)
      bytHigh = bytData[0];
      bytLow = bytData[1];
      lngPos = (bytHigh << 8) + bytLow;
      //for first word, there is no previous word
      strPrevWord = "";
      //read character Count for first word
      bytPrevWordCharCount = bytData[lngPos];
      lngPos++;

      //continue loading words until end of resource is reached
      while (lngPos < bytData.Length) // Until lngPos > UBound(bytData)
      {
        //initialize word
        strThisWord = "";
        //if number of characters to Count from previous word are longer than the previous word,
        if (bytPrevWordCharCount > strPrevWord.Length)
        {
          //should never be??
          bytPrevWordCharCount = (byte)strPrevWord.Length;
        }
        //if some characters should be copied from previous word
        if (bytPrevWordCharCount > 0)
        {
          //copy //em
          strThisWord = Left(strPrevWord, bytPrevWordCharCount);
        }
        //build rest of word
        do
        {
          bytVal = bytData[lngPos];
          //if bit7 is clear
          if (bytVal < 0x80)
          {
            strThisWord += (char)(bytVal ^ 0x7F);
          }
          lngPos++;
          //continue until last character (indicated by flag) or endofresource is reached
        }
        while ((bytVal < 0x80) && (lngPos < bytData.Length)); // Loop Until (bytVal >= 0x80) || lngPos > UBound(bytData)
        //if end of file is reached before 0x80,
        if (lngPos >= bytData.Length)
        {
          //invalid words.tok file!
          throw new Exception("bad WORDS.TOK file");
        }
        //add last character (after stripping off flag)
        strThisWord += (char)(0xFF ^ bytVal);
        //convert any upper-case characters to lower-case (just in case)
        strThisWord = strThisWord.ToLower();
        //get group number
        lngGrpNum = (bytData[lngPos] << 8) + bytData[lngPos + 1];
        //set pointer to next word

        lngPos += 2;
        //if this word different to previous,
        //(this ensures no duplicates are added)
        if (strThisWord != strPrevWord)
        {
          //if this word already exists,
          if (mWordCol.ContainsKey(strThisWord))
          {
            //delete the old one
            RemoveWord(strThisWord);
          }
          //add word
          AddWord(strThisWord, lngGrpNum);
          //this word is now the previous word
          strPrevWord = strThisWord;
        }
        bytPrevWordCharCount = bytData[lngPos];
        lngPos++;
      }
      //if no words
      if (mWordCol.Count == 0)
      {
        //add default words
        AddWord("a", 0);
        AddWord("anyword", 1);
        AddWord("rol", 9999);
      }
      return true;
    }
    public void SetWords(WordList NewWords)
    {
      //copies word list from NewWords to
      //this word list
      int i, j;
      int lngGrpNum, lngCount;
      string strWord;
      WordGroup tmpGroup;
      AGIWord tmpWord;
      //if source wordlist is not loaded
      if (!NewWords.Loaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //first, clear current list
      Clear();
      //then add all groups
      lngCount = NewWords.GroupCount;
      foreach (WordGroup oldGroup in NewWords)
      {
        //create new temp group, assign the number
        tmpGroup = new WordGroup();
        lngGrpNum = oldGroup.GroupNum;
        tmpGroup.GroupNum = lngGrpNum;
        //add group to end of collection
        mGroupCol.Add(lngGrpNum, tmpGroup);
      } 

      //then add all words
      foreach (AGIWord oldWord in NewWords.mWordCol.Values)
      {
        //get word
        strWord = oldWord.WordText;
        lngGrpNum = oldWord.Group;
        //create new word item
        tmpWord.WordText = strWord;
        tmpWord.Group = lngGrpNum;
        //add word to wordcol
        mWordCol.Add(strWord, tmpWord);
        //add word to its groupcol
        //in order to access internal properties,
        //need to create a direct reference to the AGIwordgroup class
        mGroupCol[lngGrpNum].AddWordToGroup(strWord);
      } //nxt  i

      //copy description
      mDescription = NewWords.Description;
      //set dirty flags
      mIsDirty = NewWords.IsDirty;
      mWriteProps = NewWords.WriteProps;
      //copy filename
      mResFile = NewWords.ResFile;
      //set loaded flag
      mLoaded = true;
    }
    internal bool WriteProps { get { return mWriteProps; } }
    public string Description
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error

          Exception e = new(LoadResString(563))
          {
            HResult = 563
          };
          throw e;
        }
        return mDescription;
      }
      set
      {
        //if not loaded
        if (!mLoaded)
        {
          //error

          Exception e = new(LoadResString(563))
          {
            HResult = 563
          };
          throw e;
        }
        //limit description to 1K
        value = Left(value, 1024);
        //if changing
        if (value != mDescription)
        {
          mDescription = value;
          //if in a game
          if (mInGame)
          {
            parent.WriteGameSetting("WORDS.TOK", "Description", mDescription);
            //change date of last edit
            parent.agLastEdit = DateTime.Now;
          }
        }
      }
    }
    internal bool InGame
    {
      get
      {
        //only used by setword method
        return mInGame;
      }
      set
      {
        mInGame = value;
      }
    }
    public string ResFile
    {
      get
      {
        return mResFile;
      }
      set
      {
        //resfile cannot be changed if resource is part of a game
        if (mInGame)
        {
          //error- resfile is readonly for ingame resources

          Exception e = new(LoadResString(680))
          {
            HResult = 680
          };
          throw e;
        }
        else
        {
          mResFile = value;
        }
      }
    }
    public void Clear()
    {
      //clears the word group, and sets up a blank list
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //reset group and word collections
      mGroupCol = new SortedList<int, WordGroup>();
      mWordCol = new SortedList<string, AGIWord>();
      mDescription = "";
      mIsDirty = true;
    }
    public WordGroup GroupN(int groupNum)
    {
      //returns a group by its group number
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //return this group by it's number (key value)
      return mGroupCol[groupNum];
    }
    public bool GroupExists(int GroupNumber)
    {
      //if this group exists, it returns true
      return mGroupCol.Keys.Contains(GroupNumber);
    }
    public bool IsDirty
    {
      get
      {
        //if resource is dirty, or (prop values need writing AND in game)
        return (mIsDirty || (mWriteProps && mInGame));
      }
      set
      {
        mIsDirty = value;
      }
    }
    public AGIWord this[dynamic vKeyIndex]
    {
      get
      {
        //access is by word string or index number
        //if not loaded
        if (!mLoaded)
        {
          //error

          Exception e = new(LoadResString(563))
          {
            HResult = 563
          };
          throw e;
        }
        //only allow string or integer number
        if ((vKeyIndex is int) || (vKeyIndex is byte) || (vKeyIndex is short))
        {
          //retrieve via index

          //validate index
          if (vKeyIndex < 0 || vKeyIndex > mWordCol.Count - 1)
          {
            throw new IndexOutOfRangeException();
          }
          return mWordCol.Values[vKeyIndex];
        }
        else if (vKeyIndex is string)
        {
          // a string could be a number passed as a string, OR an actual word?
          // user has to make sure it's not a number passed as a string...
          //retrieve via string key - actual word, which is the key
          return mWordCol[vKeyIndex];
        }
        else
        {
          throw new Exception("invalid key/index");
        }
      }
    }
    public void Save(string SaveFile = "", int FileType = 0)
    {
      //saves wordlist
      // filetype = 0 means AGI WORDS.TOK file
      // filetype = 1 means WinAGI word list file
      //for ingame resources, SaveFile and filetype are ignored
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //if in a game,
      if (mInGame)
      {
        //compile the file
        Compile(mResFile);
        //change date of last edit
        parent.agLastEdit = DateTime.Now; ;
      }
      else
      {
        if (SaveFile.Length == 0)
        {
          SaveFile = mResFile;
        }
        switch (FileType)
        {
        case 0:  //compile agi WORDS.TOK file
          Compile(SaveFile);
          break;
        case 1:  //create WinAGI word list
          CompileWinAGI(SaveFile);
          break;
        }
        //save filename
        mResFile = SaveFile;
      }
      //mark as clean
      mIsDirty = false;
    }
    public void Unload()
    {
      //unloads the resource; same as clear, except file marked as not dirty
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      Clear();
      mLoaded = false;
      mWriteProps = false;
      mIsDirty = false;
    }
    public int WordCount
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error

          Exception e = new(LoadResString(563))
          {
            HResult = 563
          };
          throw e;
        }
        return mWordCol.Count;
      }
    }
    public bool WordExists(string aWord)
    {
      return mWordCol.ContainsKey(aWord);
    }
    public void AddGroup(int GroupNumber)
    {
      int i;
      WordGroup tmpGroup;
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //if group number is invalid
      if (GroupNumber < 0)
      {
        //error

        Exception e = new(LoadResString(575))
        {
          HResult = 575
        };
        throw e;
      }
      if (GroupNumber > MAX_GROUP_NUM)
      {
        //error

        Exception e = new(LoadResString(575))
        {
          HResult = 575
        };
        throw e;
      }
      //see if group already exists
      if (mGroupCol.ContainsKey(GroupNumber))
      {

        Exception e = new(LoadResString(576))
        {
          HResult = 576
        };
        throw e;
      }
      tmpGroup = new WordGroup
      {
        GroupNum = GroupNumber
      };
      //add it
      mGroupCol.Add(GroupNumber, tmpGroup);
      mIsDirty = true;
    }
    void RemoveWord(string aWord)
    {
      //deletes aWord
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      // if word doesn't exist
      if (!mWordCol.ContainsKey(aWord))
      {
        //word not found

        Exception e = new(LoadResString(584))
        {
          HResult = 584
        };
        throw e;
      }
      //delete this word from its assigned group BY Group number
      WordGroup tmpGroup = GroupN(mWordCol[aWord].Group);
      tmpGroup.DeleteWordFromGroup(aWord);
      //if group is now empty, delete it too
      if (tmpGroup.WordCount == 0)
      {
        mGroupCol.Remove(tmpGroup.GroupNum);
      }
      //remove it from the word collection
      mWordCol.Remove(aWord);
      //set dirty flag
      mIsDirty = true;
    }
    public int GroupCount
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error

          Exception e = new(LoadResString(563))
          {
            HResult = 563
          };
          throw e;
        }

        return mGroupCol.Count;
      }
    }
    public void RemoveGroup(int GroupNumber)
    {
      int i;
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //if group number is invalid
      if (GroupNumber < 0 || GroupNumber > MAX_GROUP_NUM)
      {
        //error

        Exception e = new(LoadResString(575))
        {
          HResult = 575
        };
        throw e;
      }
      //if group doesn't exist
      if (!GroupExists(GroupNumber))
      {
        //error

        Exception e = new(LoadResString(583))
        {
          HResult = 583
        };
        throw e;
      }
      //step through all words in main list
      // CAN'T use foreach because we need to delete 
      // some of the objects
      for (i = mWordCol.Count - 1; i >= 0; i--)
      {
        if (mWordCol.Values[i].Group == GroupNumber)
        {
          //delete this word from main list
          mWordCol.RemoveAt(i);
        }
      }
      //delete the group
      mGroupCol.Remove(GroupNumber);
      //set dirty flag
      mIsDirty = true;
    }
    public void RenumberGroup(int OldGroupNumber, int NewGroupNumber)
    {
      int i;
      WordGroup tmpGroup;
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //if oldgroup number is invalid
      if (OldGroupNumber < 0 || OldGroupNumber > MAX_GROUP_NUM)
      {
        //error

        Exception e = new(LoadResString(575))
        {
          HResult = 575
        };
        throw e;
      }
      //if new group number is invalid
      if (NewGroupNumber < 0 || NewGroupNumber > MAX_GROUP_NUM)
      {
        //error

        Exception e = new(LoadResString(575))
        {
          HResult = 575
        };
        throw e;
      }
      //if old group doesn't exist
      if (!GroupExists(OldGroupNumber))
      {
        //error

        Exception e = new(LoadResString(696))
        {
          HResult = 696
        };
        throw e;
      }
      //if new group already exists
      if (GroupExists(NewGroupNumber))
      {
        //error

        Exception e = new(LoadResString(697))
        {
          HResult = 697
        };
        throw e;
      }
      //make temp copy of old group
      tmpGroup = mGroupCol[OldGroupNumber];
      //remove group
      mGroupCol.Remove(OldGroupNumber);
      // change group number
      tmpGroup.GroupNum = NewGroupNumber;
      //change group number for all words in the group
      // CAN'T use forach, because we are adding to the collection
      for (i = 0; i < mWordCol.Count; i++)
      {
        if (mWordCol.Values[i].Group == OldGroupNumber)
        {
          AGIWord tmpWord = mWordCol.Values[i];
          tmpWord.Group = NewGroupNumber;
        }
      }
      //then re-add the group
      mGroupCol.Add(NewGroupNumber, tmpGroup);

      //set dirty flag
      mIsDirty = true;
    }
    void Compile(string CompileFile)
    {
      //compiles the word list into a Sierra WORDS.TOK file
      int i;
      int lngGroup, lngWord;
      byte CurByte;
      string strCurWord;
      string strPrevWord = "";
      byte intPrevWordCharCount;
      bool blnWordsMatch;
      string strFirstLetter;
      int[] intLetterIndex = new int[26];
      string strTempFile;
      //if no filename passed,
      if (CompileFile.Length == 0)
      {

        Exception e = new(LoadResString(616))
        {
          HResult = 616
        };
        throw e;
      }
      //if not dirty AND CompileFile=resfile
      if (!mIsDirty && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }
      //if there are no word groups to add
      if (mGroupCol.Count == 0)
      {
        throw new Exception("672, strErrSource, Replace(LoadResString(672), ARG1, no word groups to add)");
      }
      //if there are no words,
      if (mWordCol.Count == 0)
      {
        //error
        throw new Exception("672, strErrSource, Replace(LoadResString(672), ARG1, no words to add)");
      }
      //create temp file
      strTempFile = Path.GetTempFileName();
      //open the file
      FileStream fsWords;
      try
      {
        fsWords = new FileStream(strTempFile, FileMode.OpenOrCreate);
        //write letter index
        for (i = 0; i <= 51; i++)
        {
          fsWords.WriteByte(0);
        }
        //index pointer starts with 'a'
        strFirstLetter = "a";
        intLetterIndex[1] = 52;
        //now step through all words in list
        foreach (AGIWord tmpWord in mWordCol.Values)
        {
          //get next word
          strCurWord = tmpWord.WordText;
          //if first letter is not current first letter, AND it is 'b' through 'z'
          //(this ensures non letter words (such as numbers) are included in the //a// section)
          if ((strCurWord[0] != strFirstLetter[0]) && (strCurWord[0] >= 97) && (strCurWord[0] <= 122))
          {
            //reset index pointer
            strFirstLetter = Left(strCurWord, 1);
            intLetterIndex[(byte)strFirstLetter[0] - 96] = (int)fsWords.Position;
          }
          //calculate number of characters that are common to previous word
          intPrevWordCharCount = 0;
          i = 1;
          blnWordsMatch = true;
          do
          {
            //if not at end of word, AND current position is not longer than previous word,
            if ((i <= strCurWord.Length) && (strPrevWord.Length >= i))
            {
              //if characters at this position match,
              if (strPrevWord[i] == strCurWord[i])
              {
                //increment Count
                intPrevWordCharCount++;
              }
              else
              {
                blnWordsMatch = false;
              }
            }
            else
            {
              blnWordsMatch = false;
            }
            i++;
          }
          while (!blnWordsMatch);
          //write number of characters from previous word
          fsWords.WriteByte(intPrevWordCharCount);
          //set previous word Value to this word
          strPrevWord = strCurWord;
          //strip off characters that are same as previous word
          strCurWord = Right(strCurWord, strCurWord.Length - intPrevWordCharCount);
          //if there are two or more characters to write,
          if (strCurWord.Length > 1)
          {
            //write all but last character
            for (i = 0; i < strCurWord.Length - 1; i++)
            {
              //encrypt character before writing it
              CurByte = (byte)(0x7F ^ (byte)strCurWord[i]);
              fsWords.WriteByte(CurByte);
            }
          }
          //encrypt character, and set flag 0x80 for last char
          CurByte = (byte)(0x80 + (0x7F ^ (byte)strCurWord[strCurWord.Length]));
          fsWords.WriteByte(CurByte);
          //write group number (stored as 2-byte word; high byte first)
          CurByte = (byte)(tmpWord.Group / 256);
          fsWords.WriteByte(CurByte);
          CurByte = (byte)(tmpWord.Group % 256);
          fsWords.WriteByte(CurByte);
        }
        //add null character to end of file
        fsWords.WriteByte(0);
        //reset file pointer to start
        fsWords.Seek(0, SeekOrigin.Begin);
        //write index values for all 26 letters
        for (i = 1; i <= 26; i++)
        {
          //(two byte word, high byte first
          CurByte = (byte)(intLetterIndex[i] / 256);
          fsWords.WriteByte(CurByte);
          CurByte = (byte)(intLetterIndex[i] % 256);
          fsWords.WriteByte(CurByte);
        }
        //close file,
        fsWords.Dispose();
        //if CompileFile already exists
        if (File.Exists(CompileFile))
        {
          //delete it
          File.Delete(CompileFile);
        }
        //copy tempfile to CompileFile
        File.Move(strTempFile, CompileFile);
      }
      catch (Exception)
      {
        //raise the error
        throw new Exception("672, strErrSrc, Replace(LoadResString(672), ARG1, CStr(lngError) + strError)");
      }
    }
    public void Load(string LoadFile = "")
    {
      //this function loads words for the game
      //if loading from a Sierra game, it extracts words
      //from the WORDS.TOK file;
      //if not in a game, LoadFile must be specified

      //if already loaded,
      if (mLoaded)
      {
        //error- resource already loaded

        Exception e = new(LoadResString(511))
        {
          HResult = 511
        };
        throw e;
      }
      if (mInGame)
      {
        //use default filename
        LoadFile = parent.agGameDir + "WORDS.TOK";
        //attempt to load
        if (!LoadSierraFile(LoadFile))
        {
          //error

          Exception e = new(LoadResString(529))
          {
            HResult = 529
          };
          throw e;
        }
        //get description, if there is one
        mDescription = parent.agGameProps.GetSetting("WORDS.TOK", "Description", "");
      }
      else
      {
        //if not in a game, must have a valid filename
        if (LoadFile.Length == 0)
        {
          //not in game; return error

          Exception e = new(LoadResString(599))
          {
            HResult = 599
          };
          throw e;
        }
        //verify file exists
        if (!File.Exists(LoadFile))
        {
          //error
          throw new Exception("524, strErrSource, Replace(LoadResString(524), ARG1, LoadFile)");
        }
        //if extension is .agw then
        if (LoadFile.EndsWith(".agw", StringComparison.OrdinalIgnoreCase))
        {
          //assume winagi format
          if (!LoadWinAGIFile(LoadFile))
          {
            //try sierra format
            if (!LoadSierraFile(LoadFile))
            {
              //error

              Exception e = new(LoadResString(529))
              {
                HResult = 529
              };
              throw e;
            }
          }
        }
        else
        {
          //assume sierra format
          if (!LoadSierraFile(LoadFile))
          {
            //try winagi format
            if (!LoadWinAGIFile(LoadFile))
            {
              //error

              Exception e = new(LoadResString(529))
              {
                HResult = 529
              };
              throw e;
            }
          }
        }
        //save filename
        mResFile = LoadFile;
      }
      //reset dirty flag
      mIsDirty = false;
      return;
    }
    public WordGroup Group(int Index)
    {
      //returns a group by its index (NOT the same as group number)
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //if invalid index
      if (Index < 0 || Index > mGroupCol.Count - 1)
      {
        //error

        Exception e = new(LoadResString(588))
        {
          HResult = 588
        };
        throw e;
      }
      //access the group by its index
      return mGroupCol.Values[Index];
    }
    public void AddWord(string WordText, int Group)
    {
      AGIWord NewWord;
      //if not loaded
      if (!mLoaded)
      {
        //error

        Exception e = new(LoadResString(563))
        {
          HResult = 563
        };
        throw e;
      }
      //convert input to lowercase
      WordText = WordText.ToLower();
      //check to see if word is already in collection,
      if (mWordCol.ContainsKey(WordText))
      {

        Exception e = new(LoadResString(579))
        {
          HResult = 579
        };
        throw e;
      }
      //if group number is invalid (negative, or > max)
      if (Group < 0 || Group > MAX_GROUP_NUM)
      {
        //error

        Exception e = new(LoadResString(581))
        {
          HResult = 0
        };
        throw e;
      }
      //if this group does not yet exist,
      if (!GroupExists(Group))
      {
        //add this group
        AddGroup(Group);
      }
      //add word to the group
      mGroupCol[Group].AddWordToGroup(WordText);
      //now add it to the main word collection
      NewWord.WordText = WordText;
      NewWord.Group = Group;
      //add it, using the word itself as the key
      mWordCol.Add(WordText, NewWord);
      //set dirty flag
      mIsDirty = true;
    }
    bool LoadWinAGIFile(string LoadFile)
    {
      string strInput;
      string[] strWords;
      string strThisWord;
      int lngGrpNum;
      int i;

      //attempt to open the WinAGI resource
      FileStream fsWords;
      StreamReader srWords;
      try
      {
        //attempt to open the WinAGI resource
        fsWords = new FileStream(LoadFile, FileMode.Open);
        srWords = new StreamReader(fsWords);
      }
      catch (Exception)
      {
        //ignore and fail
        return false;
      }

      //first line should be loader
      strInput = srWords.ReadLine();
      switch (strInput)
      {
      case WINAGI_VERSION:
      case WINAGI_VERSION_1_2:
      case WINAGI_VERSION_1_0:
      case WINAGI_VERSION_BETA:
        //ok
        break;
      default:
        //any 1.2.x is ok
        if (Left(strInput, 4) != "1.2.")
        {
          //close file
          fsWords.Dispose();
          srWords.Dispose();
          return false;
        }
        break;
      }
      //enable inline error handling to check for invalid words/groups
      try
      {
        //begin loading word groups
        while (!srWords.EndOfStream) // Until EOF(intFile)
        {
          strInput = srWords.ReadLine();
          //check for end of input characters
          if (strInput == ((char)255 + (char)255).ToString())
          {
            break;
          }
          //split input line
          strWords = strInput.Split("\t");
          //if there is at least one word and one number,
          if (strWords.Length >= 2)
          {
            //get group number
            lngGrpNum = (int)Val(strWords[0]);
            //add this group
            AddGroup(lngGrpNum);
            //rest of strings in the list are words; add them to this group
            for (i = 1; i < strWords.Length; i++)
            {
              AddWord(strWords[i], lngGrpNum);
            }
          }
        } //Loop
      }
      catch (Exception)
      {
        //invalid word group, or other word error
        fsWords.Dispose();
        srWords.Dispose();
        // pass along error
        throw;
      }
      //if any lines left
      if (!srWords.EndOfStream)
      {
        //get first remaining line
        strThisWord = srWords.ReadLine();
        //if there are additional lines, add them as a description
        while (!srWords.EndOfStream)
        {
          strInput = srWords.ReadLine();
          strThisWord += NEWLINE + strInput;
        }
        //save description
        mDescription = strThisWord;
      }
      //close file
      fsWords.Dispose();
      srWords.Dispose();

      //if no words
      if (mWordCol.Count == 0)
      {
        //add default words
        AddWord("a", 0);
        AddWord("anyword", 1);
        AddWord("rol", 9999);
      }
      //set loaded flag
      mLoaded = true;
      return true;
    }
    public WGrpEnum GetEnumerator()
    {
      return new WGrpEnum(mGroupCol);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    IEnumerator<WordGroup> IEnumerable<WordGroup>.GetEnumerator()
    {
      return (IEnumerator<WordGroup>)GetEnumerator();
    }
  }
    public class WGrpEnum : IEnumerator<WordGroup>
  {
    public SortedList<int, WordGroup> _groups;
    int position = -1;
    public WGrpEnum(SortedList<int, WordGroup> list)
    {
      _groups = list;
    }
    object IEnumerator.Current => Current;
    public WordGroup Current
    {
      get
      {
        try
        {
          return _groups.Values[position];
        }
        catch (IndexOutOfRangeException)
        {

          throw new InvalidOperationException();
        }
      }
    }
    public bool MoveNext()
    {
      position++;
      return (position < _groups.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    public void Dispose()
    {
      _groups = null;
    }
  }
}
