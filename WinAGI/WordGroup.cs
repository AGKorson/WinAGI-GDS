using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a word group in an AGI WORDS.TOK file. All words
    /// in the group are synonyms that share the same number used by the AGI
    /// 'said' test command.
    /// </summary>
    public class WordGroup : IEnumerable<string> {
        #region Members
        internal List<string> mWords;
        internal int mGroupNum;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an empty word group.
        /// </summary>
        public WordGroup() {
            mWords = [];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the word in this wordgroup that corresponds to the 
        /// specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] {
            get {
                if (index < 0 || index >= mWords.Count) {
                    throw new ArgumentOutOfRangeException("index");
                }
                return mWords[index];
            }
        }

        public List<string> Words {
            get {
                // don't return the original, to prevent changes
                List<string> list = [.. mWords];
                return list; 
            }
        }
        
        /// <summary>
        /// Gets the group name for this word group. The group name is the first word
        /// in the group alphabetically.
        /// </summary>
        public string GroupName {
            get {
                switch (mGroupNum) {
                case 0:
                    return "<null words>";
                case 1:
                    return "<any word>";
                case 9999:
                    return "<rest of line>";
                default:
                    if (mWords.Count == 0) {
                        return "";
                    }
                    else {
                        return mWords[0];
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the group number for this word group.
        /// </summary>
        public int GroupNum {
            get {
                return mGroupNum;
            }
            internal set {
                mGroupNum = value;
            }
        }

        /// <summary>
        /// Gets the number of words in this word group.
        /// </summary>
        public int WordCount {
            get {
                return mWords.Count;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the specified word to this word group.
        /// </summary>
        /// <param name="aWord"></param>
        internal void AddWordToGroup(string aWord) {
            if (aWord.Length == 0) {
                return;
            }
            if (mWords.Contains(aWord)) {
                return;
            }
            aWord = aWord.LowerAGI();
            int i;
            if (mWords.Count == 0) {
                mWords.Add(aWord);
            }
            else {
                if (aWord[0] < 97 || aWord[0] > 122) {
                    // add extended character words and non-lettered words
                    // to beginning of list before a-z words
                    for (i = 0; i < mWords.Count; i++) {
                        if (mWords[i][0] >= 97 && mWords[i][0] <= 122) {
                            // first a-z word found; insert new word here
                            break;
                        }
                        if (string.Compare(aWord, mWords[i], true) < 0) {
                            // this is where word goes
                            break;
                        }
                    }
                }
                else {
                    // a-z words get added AFTER extended characters 
                    // and non-letters
                    i = 0;
                    while (i < mWords.Count) {
                        if (mWords[i][0] >= 97 && mWords[i][0] <= 122) {
                            break;
                        }
                        i++;
                    }
                    for (; i < mWords.Count; i++) {
                        if (string.Compare(aWord, mWords[i], true) < 0) {
                            // this is where word goes
                            break;
                        }
                    }
                }
                // add it,
                mWords.Insert(i, aWord);
            }
            return;
        }

        /// <summary>
        /// Deletes the specified word from this word group.
        /// </summary>
        /// <param name="aWord"></param>
        internal void DeleteWordFromGroup(string aWord) {
            mWords.Remove(aWord);
            return;
        }
        #endregion

        #region Enumeration
        WordEnum GetEnumerator() {
            return new WordEnum(mWords);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<string> IEnumerable<string>.GetEnumerator() {
            return (IEnumerator<string>)GetEnumerator();
        }
        /// <summary>
        /// Implements enumeration for the WordGroup class.
        /// </summary>
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
        #endregion
    }
}
