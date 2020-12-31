using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public class AGIWordGroup
  {
    internal List<string> mWords;
    //internal System.Collections.Generic.SortedDictionary<string, string> mWordsD;
    //internal System.Collections.Generic.SortedSet<string> mWordsS;

    internal int mGroupNum;
    internal readonly string strErrSource = "WinAGI.AGIWordGroup";
      //access to word list is by index only
    public string this[byte index]
    { get { return mWords[index]; } }

    internal void AddWordToGroup(string aWord)
    {
      //add word to collection of strings
      //the fact that this word DOES NOT yet exist in this
      //group has been validated BEFORE this property is called
      int i;
      //if this is the first word,
      if (mWords.Count == 0)
      {
        //add it, using itself as key
        mWords.Add(aWord);
      } 
      else
      {
        //step through words in reverse order
        for (i = 0; i <= mWords.Count; i++)
        {
          //if new word is less than current word
          if (String.Compare(aWord, mWords[i], true) < 0)
          {
            //this is where word goes
            break;
          }
        }
        //add it, 
        mWords.Insert(i, aWord);
      }
    return;
    }
    internal void DeleteWordFromGroup(string aWord)
    {
      //delete word from group
      //the fact that this word exists in this group is
      //tested BEFORE this function is called
      mWords.Remove(aWord);
    return;
    }
    public string GroupName
    {
      get
      {
        //return first word in group
        if (mWords.Count == 0)
        {
          //return empty string
          return "";
        }
        else
        {
          return mWords[0];
        }
      }
    }
    public int GroupNum
    {
      get
      {

        return mGroupNum;
      }
      internal set
      {
        mGroupNum = value;
      }
    }
    public int WordCount
    {
      get { return mWords.Count; }
    }
  }
}
