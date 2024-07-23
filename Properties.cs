using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    /// <summary>
    /// Property accessors for the AGI Game objrct. Used on the main form to display 
    /// properties, and allows them to be edited.
    /// </summary>
    public class GameProperties {
        //string _GameID;
        public GameProperties() {}

        public string GameID {
            get
            {
                return EditGame.GameID;
            }
            set
            {
                ChangeGameID(value);
                // mark any logics that use the gameID token as dirty
                UpdateReservedToken(EditGame.ReservedGameDefines[0].Name);
                RDefLookup[91].Value = '"' + EditGame.GameID + '"';
            }
        }

        public string Author {
            get => EditGame.GameAuthor;
            set => EditGame.GameAuthor = value;
        }
        
        public string GameDir { 
            get => EditGame.GameDir; }
        
        public string ResDir { 
            get => EditGame.ResDirName;
            set {
                // validate and change resource directory
                ChangeResDir(value);
            }
        }

        [TypeConverter(typeof(IntVerConverter))]
        public string IntVer { 
            get => EditGame?.InterpreterVersion;
            set {
                // determine if a change was made:
                if (EditGame.InterpreterVersion != value) {
                    ChangeIntVersion(value);
                }
            }
        }
        public string Description { 
            get => EditGame.GameDescription;
            set => EditGame.GameDescription = value; }
        public string GameVer { 
            get => EditGame.GameVersion;
            set {
                EditGame.GameVersion = value;
                UpdateReservedToken(EditGame.ReservedGameDefines[1].Name);
                RDefLookup[92].Value = '"' + EditGame.GameVersion + '"';
            }
        }
        public string GameAbout { 
            get => EditGame.GameAbout;
            set {
                EditGame.GameAbout = value;
                UpdateReservedToken(EditGame.ReservedGameDefines[2].Name);
                RDefLookup[93].Value = '"' + EditGame.GameAbout + '"';
            }
        }
        public bool LayoutEditor { 
            get => EditGame.UseLE;
            set {
                EditGame.UseLE = value;
                UpdateLEStatus();
            }
        }
        public DateTime LastEdit { 
            get => EditGame.LastEdit; }
    }
    public class LogicHdrProperties {
        public LogicHdrProperties() {
            GlobalDef = "(List)";
        }
        public int Count { 
            get => EditGame.Logics.Count; 
        }
        public string GlobalDef { get; set; }
        public bool UseResNames { 
            get => LogicCompiler.UseReservedNames;
            // TODO: useresnames usage checks
            set => LogicCompiler.UseReservedNames = value;
        }
    }
    public class LogicProperties {
        public LogicProperties(Logic pLogic) {
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
        [ReadOnlyAttribute(false)]
        public bool IsRoom { get; set; } //readonly if room number is 0
        public bool Compiled { get;}
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
        public int CodeSize { get; }
    }
    public class PictureHdrProperties {
        public PictureHdrProperties(int count) {
            Count = count;
        }
        public int Count { get; }
    }
    public class PictureProperties {
        public PictureProperties(Picture pPicture) {
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
    public class SoundHdrProperties {
        public SoundHdrProperties(int count) {
            Count = count;
        }
        public int Count { get; }
    }
    public class SoundProperties {
        public SoundProperties(Sound pSound) {
            Number = pSound.Number;
            ID = pSound.ID;
            Description = pSound.Description;
            Volume = pSound.Volume;
            LOC = pSound.Loc;
            Size = pSound.Size;
        }
        public SoundProperties(byte number, string id, string description, int volume, int loc, int size) {
            Number = number;
            ID = id;
            Description = description;
            Volume = volume;
            LOC = loc;
            Size = size;
        }
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
    }
    public class VieweHdrProperties {
        public VieweHdrProperties(int count) {
            Count = count;
        }
        public int Count { get; }
    }
    public class ViewProperties {
        public ViewProperties(Engine.View pView) {
            Number = pView.Number;
            ID = pView.ID;
            Description = pView.Description;
            ViewDesc = pView.ViewDescription;
            Volume = pView.Volume;
            LOC = pView.Loc;
            Size = pView.Size;
        }
        [Editor(typeof(NumberEditor),
                 typeof(System.Drawing.Design.UITypeEditor))]
        public int Number { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public string ViewDesc { get; }
        public int Volume { get; }
        public int LOC { get; }
        public int Size { get; }
    }
    public class InvObjProperties {
        public InvObjProperties(InventoryList pInvObj) {
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
    public class WordListProperties {
        public WordListProperties(WordList pWordList) {
            WordCount = pWordList.WordCount;
            GroupCount = pWordList.GroupCount;
            Description = pWordList.Description;
        }
        public int GroupCount { get; }
        public int WordCount { get; }
        public string Description { get; set; }
    }


    internal class PropIntVersions {
        internal static string[] _Versions = IntVersions;
    }

    public class IntVerConverter : StringConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            // true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            // true will limit to list. false will show the list, 
            // but allow free-form entry
            return true;
        }

        public override System.ComponentModel.TypeConverter.StandardValuesCollection
               GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(PropIntVersions._Versions);
        }
    }

    public class NumberEditor : UITypeEditor {
        public override UITypeEditorEditStyle
               GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                IServiceProvider provider, object value) {
            IWindowsFormsEditorService wfes =
               provider.GetService(typeof(IWindowsFormsEditorService)) as
               IWindowsFormsEditorService;

            if (wfes != null) {
                frmGetResourceNum _frmGetResNum = new frmGetResourceNum();
                _frmGetResNum._wfes = wfes;

                wfes.ShowDialog(_frmGetResNum);
                value = _frmGetResNum.NewResNum;
            }
            return value;
        }
    }
}
