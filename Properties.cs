using System;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor
{
    // property accessors; used on the main form to display 
    // properties, and allow them to be edited
    public class GameProperties
    {
        string _GameID;
        public GameProperties(AGIGame pGame)
        {
            _GameID = pGame.GameID;
            Author = pGame.GameAuthor;
            GameDir = pGame.GameDir;
            ResDir = pGame.ResDirName;
            IntVer = pGame.InterpreterVersion;
            Description = pGame.GameDescription;
            GameVer = pGame.GameVersion;
            GameAbout = pGame.GameAbout;
            LayoutEditor = pGame.UseLE;
            LastEdit = pGame.LastEdit;
        }
        public string GameID
        {
            get
            {
                return _GameID;

            }
            set
            {
                // validate new id before changing it
                if (value.Length > 5) {
                    _GameID = value[..5];
                }
                else {
                    _GameID = value;
                }
            }
        }
        public string Author
        {
            get => EditGame.GameAuthor;
            set => EditGame.GameAuthor = value;
        }
        public string GameDir { get; }
        public string ResDir { get; set; }
        public string IntVer { get; set; }
        public string Description { get; set; }
        public string GameVer { get; set; }
        public string GameAbout { get; set; }
        public bool LayoutEditor { get; set; }
        public DateTime LastEdit { get; }
    }
    public class LogicHdrProperties
        {
            public LogicHdrProperties(int count, bool useresnames)
            {
                Count = count;
                GlobalDef = true; // what to do about this one...
                UseResNames = useresnames;
            }
            public int Count { get; }
            public bool GlobalDef { get; set; } // dbl click to edit list?
            public bool UseResNames { get; set; }
        }
    public class LogicProperties
    {
        public LogicProperties(Logic pLogic)
        {
            Number = pLogic.Number;
            ID = pLogic.ID;
            Description = pLogic.Description;
            IsRoom = pLogic.IsRoom;
            Compiled = pLogic.Compiled;
            Volume = pLogic.Volume;
            LOC = pLogic.Loc;
            Size = pLogic.Size;
            CodeSize = pLogic.CodeSize;
        }
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public bool IsRoom { get; set; } //readonly if room number is 0
        public bool Compiled { get; set; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
        public int CodeSize { get; }
    }
    public class PictureHdrProperties
    {
        public PictureHdrProperties(int count)
        {
            Count = count;
        }
        public int Count { get; }
    }
    public class PictureProperties
    {
        public PictureProperties(Picture pPicture)
        {
            Number = pPicture.Number;
            ID = pPicture.ID;
            Description = pPicture.Description;
            Volume = pPicture.Volume;
            LOC = pPicture.Loc;
            Size = pPicture.Size;
        }
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
    }
    public class SoundHdrProperties
    {
        public SoundHdrProperties(int count)
        {
            Count = count;
        }
        public int Count { get; }
    }
    public class SoundProperties
    {
        public SoundProperties(Sound pSound)
        {
            Number = pSound.Number;
            ID = pSound.ID;
            Description = pSound.Description;
            Volume = pSound.Volume;
            LOC = pSound.Loc;
            Size = pSound.Size;
        }
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
    }
    public class VieweHdrProperties
    {
        public VieweHdrProperties(int count)
        {
            Count = count;
        }
        public int Count { get; }
    }
    public class ViewProperties
    {
        public ViewProperties(WinAGI.Engine.View pView)
        {
            Number = pView.Number;
            ID = pView.ID;
            Description = pView.Description;
            ViewDesc = pView.ViewDescription;
            Volume = pView.Volume;
            LOC = pView.Loc;
            Size = pView.Size;
        }
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public string ViewDesc { get; set; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
    }
    public class InvObjProperties
    {
        public InvObjProperties(InventoryList pInvObj)
        {
            ItemCount = pInvObj.Count;
            Description = pInvObj.Description;
            Encrypted = pInvObj.Encrypted;
            MaxScreenObj = pInvObj.MaxScreenObjects;
        }
        public int ItemCount { get; }
        public string Description { get; set; }
        public bool Encrypted { get; set; }
        public int MaxScreenObj { get; set; }
    }
    public class WordListProperties
    {
        public WordListProperties(WordList pWordList)
        {
            WordCount = pWordList.WordCount;
            GroupCount = pWordList.GroupCount;
            Description = pWordList.Description;
        }
        public int GroupCount { get; }
        public int WordCount { get; }
        public string Description { get; set; }
    }
}
