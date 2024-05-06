using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    public class WordGroup : IEnumerable<AGIWord> {
        internal List<string> mWords;
        //internal System.Collections.Generic.SortedDictionary<string, string> mWordsD;
        //internal System.Collections.Generic.SortedSet<string> mWordsS;

        internal int mGroupNum;
        // access to word list is by index only
        public string this[byte index] { get { return mWords[index]; } }
        public WordGroup() {
            //initialze the word collection
            mWords = [];
        }
        internal void AddWordToGroup(string aWord) {
            // TODO: need to make sure passed string is byte-code (i.e. converted from
            // unicode to actual byte values- or do I do that here????

            //add word to collection of strings
            //the fact that this word DOES NOT yet exist in this
            //group has been validated BEFORE this property is called
            int i;
            //if this is the first word,
            if (mWords.Count == 0) {
                //add it, using itself as key
                mWords.Add(aWord);
            }
            else {
                // TODO: need to add extended char words
                // and non-lettered words to BEGINNING of list

                //step through all words
                for (i = 0; i < mWords.Count; i++) {
                    //if new word is less than current word
                    if (String.Compare(aWord, mWords[i], true) < 0) {
                        //this is where word goes
                        break;
                    }
                }
                //add it,
                mWords.Insert(i, aWord);
            }
            return;
        }
        internal void DeleteWordFromGroup(string aWord) {
            //delete word from group
            //the fact that this word exists in this group is
            //tested BEFORE this function is called
            mWords.Remove(aWord);
            return;
        }
        public string GroupName {
            get {
                //return first word in group
                if (mWords.Count == 0) {
                    //return empty string
                    return "";
                }
                else {
                    return mWords[0];
                }
            }
        }
        public int GroupNum {
            get {

                return mGroupNum;
            }
            internal set {
                mGroupNum = value;
            }
        }
        public int WordCount {
            get { return mWords.Count; }
        }
        WordEnum GetEnumerator() {
            return new WordEnum(mWords);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<AGIWord> IEnumerable<AGIWord>.GetEnumerator() {
            return (IEnumerator<AGIWord>)GetEnumerator();
        }
        internal class WordEnum : IEnumerator<string> {
            public List<string> _words;
            int position = -1;
            public WordEnum(List<string> list) {
                _words = list;
            }
            object IEnumerator.Current => Current;
            public string Current {
                get {
                    try {
                        return _words[position];
                    }
                    catch (IndexOutOfRangeException) {

                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return (position < _words.Count);
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _words = null;
            }
        }
    }
}
