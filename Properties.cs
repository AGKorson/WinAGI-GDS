using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    /// <summary>
    /// Property accessors for the AGI Game object. Used on the main form to display 
    /// properties, and allows them to be edited.
    /// </summary>
    public class GameProperties {
        public string GameID {
            get
            {
                return EditGame.GameID;
            }
            set
            {
                if (ChangeGameID(value)) {
                    // mark any logics that use the gameID token as changed
                    UpdateReservedToken(EditGame.ReservedDefines.GameInfo[0].Name);
                    EditGame.ReservedDefines.GameInfo[0].Value = '"' + EditGame.GameID + '"';
                    
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
            get => EditGame.SrcResDirName;
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
            set { }
        }

        public string GameVer {
            get => EditGame.GameVersion;
            set {
                EditGame.GameVersion = value;
                UpdateReservedToken(EditGame.ReservedDefines.GameInfo[1].Name);
                EditGame.ReservedDefines.GameInfo[1].Value = '"' + EditGame.GameVersion + '"';
            }
        }

        public string GameAbout { 
            get => EditGame.GameAbout;
            set {
                EditGame.GameAbout = value;
                UpdateReservedToken(EditGame.ReservedDefines.GameInfo[2].Name);
                EditGame.ReservedDefines.GameInfo[2].Value = '"' + EditGame.GameAbout + '"';
            }
        }

        [ReadOnly(false)]
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
        public int Count {
            get => EditGame.Logics.Count;
        }

        [ReadOnly(false)]
        public bool IncludeIDs {
            get => EditGame.IncludeIDs;
            
            set {
                EditGame.IncludeIDs = value;
                MDIMain.UseWaitCursor = true;
                RefreshLogicIncludes();
                MDIMain.UseWaitCursor = false;
            }
        }

        [ReadOnly(false)]
        public bool IncludeReserved {
            get => EditGame.IncludeReserved;
            set {
                EditGame.IncludeReserved = value;
                MDIMain.UseWaitCursor = true;
                RefreshLogicIncludes();
                MDIMain.UseWaitCursor = false;
            }
        }

        [ReadOnly(false)]
        public bool IncludeGlobals {
            get => EditGame.IncludeGlobals;
            set {
                EditGame.IncludeGlobals = value;
                MDIMain.UseWaitCursor = true;
                RefreshLogicIncludes();
                MDIMain.UseWaitCursor = false;
                MDIMain.propertyGrid1.SelectedObject = new LogicHdrProperties();
            }
        }

        [ReadOnly(false)]
        [Editor(typeof(AGIPropertyEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
        public string GlobalDef {
            get => "(List)"; set { }
        }
    }

    public class LogicProperties {
        Logic pLogic;

        public LogicProperties(Logic logic) {
            pLogic = logic;
        }

        [Editor(typeof(AGIPropertyEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
        public byte Number {
            get => pLogic.Number;
            set {
                if (value != pLogic.Number) {
                    RenumberResource(AGIResType.Logic, pLogic.Number, value);
                }
            }
        }

        [Editor(typeof(AGIPropertyEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get => pLogic.ID;
            set { }
        }

        [Editor(typeof(AGIPropertyEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get => pLogic.Description;
            set { }
        }

        [ReadOnly(false)]
        public bool IsRoom {
            get {
                return pLogic.IsRoom; 
            }
            set {
                if (value != pLogic.IsRoom) {
                    pLogic.IsRoom = value;
                    pLogic.SaveProps();
                    if (EditGame.UseLE) {
                        UpdateReason reason;
                        if (pLogic.IsRoom) {
                            reason = UpdateReason.ShowRoom;
                        }
                        else {
                            reason = UpdateReason.HideRoom;
                        }
                        // update layout editor to show new room status
                        UpdateExitInfo(reason, SelResNum, pLogic);
                    }
                }
                // update editor
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.LogicNumber == SelResNum) {
                        frm.EditLogic.IsRoom = value;
                        break;
                    }
                }
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
            get {
                bool unload = !pLogic.Loaded;
                if (unload) {
                    pLogic.Load();
                }
                int retval = pLogic.CodeSize;
                if (unload) {
                    pLogic.Unload();
                }
                return retval;
            }
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
        public byte Number {
            get => pPicture.Number;
            set {
               if (value != pPicture.Number) {
                    RenumberResource(AGIResType.Picture, pPicture.Number, value);
                }
            }
        }


        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get => pPicture.ID;
            set { }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get => pPicture.Description;
            set { }
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
        public byte Number {
            get => pSound.Number;
            set {
                if (value != pSound.Number) {
                    RenumberResource(AGIResType.Sound, pSound.Number, value);
                }
            }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get => pSound.ID;
            set { }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get => pSound.Description;
            set { }
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
        public byte Number {
            get => pView.Number;
            set {
                if (value != pView.Number) {
                    RenumberResource(AGIResType.View, pView.Number, value);
                }
            }
        }


        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string ID {
            get => pView.ID;
            set { }
        }

        [Editor(typeof(AGIPropertyEditor),
                typeof(System.Drawing.Design.UITypeEditor))]
        public string Description {
            get => pView.Description;
            set { }
        }

        public string ViewDesc {
            get => pView.ViewDescription;
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
        public bool Encrypted { get; }
        public int MaxScreenObj { get; }
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
            if (context?.PropertyDescriptor?.IsReadOnly == true) {
                // don't show a button if readonly
                return UITypeEditorEditStyle.None;
            }
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                IServiceProvider provider, object value) {
            // ignore if readonly
            if (context?.PropertyDescriptor?.IsReadOnly == true) {
                return value;
            }

            AGIResType restype = 0;
            byte resnum = 0;
            AGIResource thisres = null;
            frmGetResourceNum frm;

            switch (context.PropertyDescriptor.Name) {
            case "Number":
                bool isroom = false;
                switch (context.Instance.ToString()) {
                case "WinAGI.Editor.LogicProperties":
                    restype = AGIResType.Logic;
                    resnum = ((LogicProperties)context.Instance).Number;
                    isroom = ((LogicProperties)context.Instance).IsRoom;
                    break;
                case "WinAGI.Editor.PictureProperties":
                    restype = AGIResType.Picture;
                    resnum = ((PictureProperties)context.Instance).Number;
                    break;
                case "WinAGI.Editor.SoundProperties":
                    restype = AGIResType.Sound;
                    resnum = ((SoundProperties)context.Instance).Number;
                    break;
                case "WinAGI.Editor.ViewProperties":
                    restype = AGIResType.View;
                    resnum = ((ViewProperties)context.Instance).Number;
                    break;
                }
                frm = new(isroom ? GetRes.RenumberRoom : GetRes.Renumber, restype, resnum);
                if (frm.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    value = frm.NewResNum;
                }
                else {
                    value = (byte)SelResNum;
                }
                // need to unbox the value object before converting to byte
                return (byte)value;
            case "ID":
                switch (context.Instance.ToString()) {
                case "WinAGI.Editor.LogicProperties":
                    restype = AGIResType.Logic;
                    resnum = ((LogicProperties)context.Instance).Number;
                    thisres = EditGame.Logics[resnum];
                    break;
                case "WinAGI.Editor.PictureProperties":
                    restype = AGIResType.Picture;
                    resnum = ((PictureProperties)context.Instance).Number;
                    thisres = EditGame.Pictures[resnum];
                    break;
                case "WinAGI.Editor.SoundProperties":
                    restype = AGIResType.Sound;
                    resnum = ((SoundProperties)context.Instance).Number;
                    thisres = EditGame.Sounds[resnum];
                    break;
                case "WinAGI.Editor.ViewProperties":
                    restype = AGIResType.View;
                    resnum = ((ViewProperties)context.Instance).Number;
                    thisres = EditGame.Views[resnum];
                    break;
                }
                MDIMain.EditSelectedItemProperties(1);
                return thisres.ID;
            case "Description":
                switch (context.Instance.ToString()) {
                case "WinAGI.Editor.GameProperties":
                    MDIMain.ShowProperties(false, "Version", nameof(frmGameProperties.txtGameDescription));
                    return EditGame.GameDescription;

                case "WinAGI.Editor.LogicProperties":
                    restype = AGIResType.Logic;
                    resnum = ((LogicProperties)context.Instance).Number;
                    thisres = EditGame.Logics[resnum];
                    break;
                case "WinAGI.Editor.PictureProperties":
                    restype = AGIResType.Picture;
                    resnum = ((PictureProperties)context.Instance).Number;
                    thisres = EditGame.Pictures[resnum];
                    break;
                case "WinAGI.Editor.SoundProperties":
                    restype = AGIResType.Sound;
                    resnum = ((SoundProperties)context.Instance).Number;
                    thisres = EditGame.Sounds[resnum];
                    break;
                case "WinAGI.Editor.ViewProperties":
                    restype = AGIResType.View;
                    resnum = ((ViewProperties)context.Instance).Number;
                    thisres = EditGame.Views[resnum];
                    break;
                case "WinAGI.Editor.InvObjProperties":
                    break;
                case "WinAGI.Editor.WordListProperties":
                    break;
                }
                MDIMain.EditSelectedItemProperties(1);
                switch (context.Instance.ToString()) {
                case "WinAGI.Editor.InvObjProperties":
                    return EditGame.InvObjects.Description;
                case "WinAGI.Editor.WordListProperties":
                    return EditGame.WordList.Description;
                default:
                    return thisres.Description;
                }
            case "GlobalDef":
                if (EditGame.IncludeGlobals) {
                    // same as clicking globals menu item
                    OpenGlobals(GEInUse);
                }
                return value;
            default:
                // to avoid compile error, need a default case
                // even though it will never be reached
                return value;
            }
        }
    }
}
