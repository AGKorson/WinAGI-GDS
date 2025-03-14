using System;

namespace WinAGI.Editor {
    public class WordsUndo {
        public ActionType Action;
        public string[] Group;
        public int GroupNo;
        public int OldGroupNo;
        public string Word;
        public string OldWord;
        public string Description;
        

        public enum ActionType {
            AddGroup,   // store group number that was added
            DelGroup,   // store group number AND group object that was deleted
            Renumber,   // store old group number AND new group number
            AddWord,    // store group number AND new word that was added
            DelWord,    // store group number AND old word that was deleted
            MoveWord,   // store old group number, new group number and word that was moved
            ChangeWord, // store old word and new word
            CutWord,    // same as delete
            CutGroup,   // same as delete
            PasteWord,  // store group number and old word that was pasted over
            PasteGroup, // store group number and list of old words that were pasted over
            Replace,    // same as change word
            ReplaceAll, // store list of all words changed
            Clear,      // store all words and their groups
        }

        public WordsUndo() {
            Group = Array.Empty<string>();
        }
    }
}
