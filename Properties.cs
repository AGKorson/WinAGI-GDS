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
    /// Property accessors for the AGI Game object. Used on the main form to display 
    /// properties, and allows them to be edited.
    /// </summary>
    public class GameProperties {
        //string _GameID;
        public GameProperties() {
            
        }

        public string GameID {
            get
            {
                return EditGame.GameID;
            }
            set
            {
                if (ChangeGameID(value)) {
                    // mark any logics that use the gameID token as dirty
                    UpdateReservedToken(EditGame.ReservedGameDefines[0].Name);
                    RDefLookup[91].Value = '"' + EditGame.GameID + '"';
                }
            }
        }

        public string Author {
            get => EditGame.GameAuthor;
            set => EditGame.GameAuthor = value;
        }
        
        public string GameDir { 
            get => EditGame.GameDir;
        }
        
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

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get => EditGame.GameDescription;
            set => EditGame.GameDescription = value;
        }

        //[Editor(typeof(AGIPropertyEditor),
        //        typeof(System.Drawing.Design.UITypeEditor))]
        public string GameVer {
            get => EditGame.GameVersion;
            set {
                EditGame.GameVersion = value;
                UpdateReservedToken(EditGame.ReservedGameDefines[1].Name);
                RDefLookup[92].Value = '"' + EditGame.GameVersion + '"';
            }
        }

        //[Editor(typeof(AGIPropertyEditor),
        //        typeof(System.Drawing.Design.UITypeEditor))]
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

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string GlobalDef { get => "(List)"; set { }
        }

        public bool UseResNames { 
            get => LogicCompiler.UseReservedNames;
            // TODO: useresnames usage checks
            set => LogicCompiler.UseReservedNames = value;
        }
    }
    
    public class LogicProperties {
        Logic pLogic;

        public LogicProperties(Logic logic) {
            pLogic = logic;
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public int Number {
            get {
                return pLogic.Number;
            }
            set {
                MessageBox.Show("new number: " + value.ToString());
            }
        }


        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get {
                return pLogic.ID;
            } 
            set {
                MessageBox.Show("new ID: " + value.ToString());
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get {
                return pLogic.Description;
            }
            set {
                MessageBox.Show("new Description: " + value.ToString());
            }
        }

        [ReadOnlyAttribute(false)]
        public bool IsRoom {
            //readonly if room number is 0
            get {
                return pLogic.IsRoom; 
            } 
            set {
                MessageBox.Show("new IsRoom: " + value.ToString());
            }
        } 

        public bool Compiled {
            get => pLogic.Compiled;
        }

        public int Volume {
            get => pLogic.Volume; 
        }

        public int LOC {
            get => pLogic.Loc;
        }

        public int Size {
            get => pLogic.Size;
        }

        public int CodeSize {
            get => pLogic.CodeSize;
        }
    }
    
    public class PictureHdrProperties {
        public int Count { get => EditGame.Pictures.Count; }
    }
    
    public class PictureProperties {
        Picture pPicture;

        public PictureProperties(Picture picture) {
            pPicture = picture;
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public int Number {
            get {
                return pPicture.Number;
            }
            set {
                MessageBox.Show("new number: " + value.ToString());
            }
        }


        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get {
                return pPicture.ID;
            }
            set {
                MessageBox.Show("new ID: " + value.ToString());
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get {
                return pPicture.Description;
            }
            set {
                MessageBox.Show("new Description: " + value.ToString());
            }
        }

        public int Volume {
            get => pPicture.Volume;
        }

        public int LOC {
            get => pPicture.Loc;
        }

        public int Size {
            get => pPicture.Size;
        }
    }

    public class SoundHdrProperties {
        public int Count { get => EditGame.Sounds.Count; }
    }
    
    public class SoundProperties {
        Sound pSound;

        public SoundProperties(Sound sound) {
            pSound = sound;
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public int Number {
            get {
                return pSound.Number;
            }
            set {
                MessageBox.Show("new number: " + value.ToString());
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get {
                return pSound.ID;
            }
            set {
                MessageBox.Show("new ID: " + value.ToString());
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get {
                return pSound.Description;
            }
            set {
                MessageBox.Show("new Description: " + value.ToString());
            }
        }

        public int Volume {
            get => pSound.Volume;
        }

        public int LOC {
            get => pSound.Loc;
        }

        public int Size {
            get => pSound.Size;
        }
    }

    public class ViewHdrProperties {
        public int Count { get => EditGame.Views.Count; }
    }
    
    public class ViewProperties {
        Engine.View pView;

        public ViewProperties(Engine.View view) {
            pView = view;
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public int Number {
            get {
                return pView.Number;
            }
            set {
                MessageBox.Show("new number: " + value.ToString());
            }
        }


        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get {
                return pView.ID;
            }
            set {
                MessageBox.Show("new ID: " + value.ToString());
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get {
                return pView.Description;
            }
            set {
                MessageBox.Show("new Description: " + value.ToString());
            }
        }

        public string ViewDesc {
            get;
        }

        public int Volume {
            get => pView.Volume;
        }

        public int LOC {
            get => pView.Loc;
        }

        public int Size {
            get => pView.Size;
        }
    }

    public class InvObjProperties {
        public InvObjProperties(InventoryList pInvObj) {
            ItemCount = pInvObj.Count;
            Description = pInvObj.Description;
            Encrypted = pInvObj.Encrypted;
            MaxScreenObj = pInvObj.MaxScreenObjects;
        }
        public int ItemCount { get; }
        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
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
        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
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

        public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(PropIntVersions._Versions);
        }
    }

    public class AGIPropertyEditor : UITypeEditor {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                IServiceProvider provider, object value) {
            switch (context.PropertyDescriptor.Name) {
            case "Number":
                //IWindowsFormsEditorService wfes =
                //   provider.GetService(typeof(IWindowsFormsEditorService)) as
                //   IWindowsFormsEditorService;

                //if (wfes != null) {
                //    frmGetResourceNum _frmGetResNum = new frmGetResourceNum();
                //    _frmGetResNum._wfes = wfes;

                //    wfes.ShowDialog(_frmGetResNum);
                //    value = _frmGetResNum.NewResNum;
                //}
                // note: value has to change to fire the property set method!
                value = 1;
                return value;
            case "ID":
                MessageBox.Show("ID: " + SelResType.ToString() + " " + SelResNum);
                value = "newid";
                return value;
            case "Description":
                if (SelResType == AGIResType.Game) {
                    MessageBox.Show("open game properties window");
                }
                else {
                    MessageBox.Show("Description: " + SelResType.ToString() + " " + SelResNum);
                }
                return value;
            case "GlobalDef":
                // same as clicking globals menu item
                OpenGlobals(GEInUse);
                return value;
            default:
                // to avoid compile error, need a default case
                // even though it will never be reached
                return value;
            }
        }
    }
}
