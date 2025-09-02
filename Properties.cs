using DynamicTypeDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
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

        [Editor(typeof(AGIPropertyEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
        public DynamicCustomTypeDescriptor m_dctd = null;
        public LogicHdrProperties() {
            GlobalDef = "(List)";
            m_dctd = ProviderInstaller.Install(this);
            CustomPropertyDescriptor cpd = m_dctd.GetProperty("GlobalDef");
            cpd.SetIsBrowsable(EditGame.IncludeGlobals);
        }

        public int Count {
            get => EditGame.Logics.Count;
        }

        public bool IncludeIDs {
            get => EditGame.IncludeIDs;
            
            set {
                EditGame.IncludeIDs = value;
                MDIMain.UseWaitCursor = true;
                RefreshLogicIncludes();
                MDIMain.UseWaitCursor = false;
            }
        }

        public bool IncludeReserved {
            get => EditGame.IncludeReserved;
            set {
                EditGame.IncludeReserved = value;
                MDIMain.UseWaitCursor = true;
                RefreshLogicIncludes();
                MDIMain.UseWaitCursor = false;
            }
        }

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

        [Editor(typeof(AGIPropertyEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

        [ReadOnlyAttribute(false)]
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

        [Editor(typeof(AGIPropertyEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                IServiceProvider provider, object value) {
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
namespace DynamicTypeDescriptor {
    [Flags]
    public enum PropertyFlags {
        [StandardValue("None", "None of the flags should be applied to this property.")]
        None = 0,
        [StandardValue("Display name", "Display name should be retrieved from resource if possible for this property.")]
        LocalizeDisplayName = 1,
        [StandardValue("Category name", "Category name should be retrieved from resource if possible for this property.")]
        LocalizeCategoryName = 2,
        [StandardValue("Description", "Description string should be retrieved from resource if possible for this property.")]
        LocalizeDescription = 4,
        [StandardValue("Enumeration", "Enumerations' display strings should be retrieved from resource if possible  for this property if it is an enumeration type.")]
        LocalizeEnumerations = 8,
        [StandardValue("Exclusive", "Values can only be selected from a list and user are not allowed to type in the value for this property.")]
        ExclusiveStandardValues = 16,

        [StandardValue("Use resource for all string", "Use resource for all string for this property.")]
        LocalizeAllString = PropertyFlags.LocalizeDisplayName | PropertyFlags.LocalizeDescription |
              PropertyFlags.LocalizeCategoryName | PropertyFlags.LocalizeEnumerations,

        [StandardValue("Expandable", "Make property expandlabe if property type is IEnemerable")]
        ExpandIEnumerable = 32,

        [StandardValue("Supports standard values", "Property supports standard values.")]
        SupportStandardValues = 64,

        [StandardValue("All flags", "All of the flags should be applied to this property.")]
        All = PropertyFlags.LocalizeAllString | PropertyFlags.ExclusiveStandardValues | PropertyFlags.ExpandIEnumerable | PropertyFlags.SupportStandardValues,

        Default = PropertyFlags.LocalizeAllString | PropertyFlags.SupportStandardValues,
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PropertyStateFlagsAttribute : Attribute {
        public PropertyStateFlagsAttribute()
          : base() {

        }
        public PropertyStateFlagsAttribute(PropertyFlags flags)
          : base() {
            m_Flags = flags;
        }

        private PropertyFlags m_Flags = PropertyFlags.All & ~PropertyFlags.ExclusiveStandardValues;

        public PropertyFlags Flags {
            get {
                return m_Flags;
            }
            set {
                m_Flags = value;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IdAttribute : Attribute {
        public IdAttribute()
          : base() {
        }

        public IdAttribute(int propertyId, int categoryId)
          : base() {
            PropertyId = propertyId;
            CategoryId = categoryId;
        }
        private int m_PropertyId = 0;

        public int PropertyId {
            get {
                return m_PropertyId;
            }
            set {
                m_PropertyId = value;
            }
        }
        private int m_CategoryId = 0;

        public int CategoryId {
            get {
                return m_CategoryId;
            }
            set {
                m_CategoryId = value;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class StandardValueAttribute : Attribute {
        public StandardValueAttribute() {

        }

        public StandardValueAttribute(object value) {
            m_Value = value;
        }
        public StandardValueAttribute(object value, string displayName) {
            m_DisplayName = displayName;
            m_Value = value;
        }
        public StandardValueAttribute(string displayName, string description) {
            m_DisplayName = displayName;
            m_Description = description;
        }
        private string m_DisplayName = String.Empty;
        public string DisplayName {
            get {
                if (String.IsNullOrEmpty(m_DisplayName)) {
                    if (Value != null) {
                        return Value.ToString();
                    }
                }
                return m_DisplayName;
            }
            set {
                m_DisplayName = value;
            }
        }

        private bool m_Visible = true;
        public bool Visible {
            get {
                return m_Visible;
            }
            set {
                m_Visible = value;
            }
        }

        private bool m_Enabled = true;
        public bool Enabled {
            get {
                return m_Enabled;
            }
            set {
                m_Enabled = value;
            }
        }

        private string m_Description = String.Empty;
        public string Description {
            get {
                return m_Description;
            }
            set {
                m_Description = value;
            }
        }

        internal object m_Value = null;

        public object Value {
            get {
                return m_Value;
            }
        }
        public override string ToString() {
            return DisplayName;
        }
        internal static StandardValueAttribute[] GetEnumItems(Type enumType) {
            if (enumType == null) {
                throw new ArgumentNullException("'enumInstance' is null.");
            }

            if (!enumType.IsEnum) {
                throw new ArgumentException("'enumInstance' is not Enum type.");
            }

            ArrayList arrAttr = new ArrayList();
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fields) {
                StandardValueAttribute[] attr = fi.GetCustomAttributes(typeof(StandardValueAttribute), false) as StandardValueAttribute[];

                if (attr != null && attr.Length > 0) {
                    attr[0].m_Value = fi.GetValue(null);
                    arrAttr.Add(attr[0]);
                }
                else {
                    StandardValueAttribute atr = new StandardValueAttribute(fi.GetValue(null));
                    arrAttr.Add(atr);
                }
            }
            StandardValueAttribute[] retAttr = arrAttr.ToArray(typeof(StandardValueAttribute)) as StandardValueAttribute[];
            return retAttr;
        }
    }

    internal class PropertyDescriptorList : List<CustomPropertyDescriptor> {
        public PropertyDescriptorList() {

        }
    }

    internal class AttributeList : List<Attribute> {
        public AttributeList() {

        }
        public AttributeList(AttributeCollection ac) {
            foreach (Attribute attr in ac) {
                this.Add(attr);
            }
        }
        public AttributeList(Attribute[] aa) {
            foreach (Attribute attr in aa) {
                this.Add(attr);
            }
        }
    }

    public class DynamicCustomTypeDescriptor : CustomTypeDescriptor {
        private PropertyDescriptorList m_pdl = new PropertyDescriptorList();
        private object m_instance = null;
        private Hashtable m_hashRM = new Hashtable();
        public DynamicCustomTypeDescriptor(ICustomTypeDescriptor ctd, object instance)
          : base(ctd) {
            m_instance = instance;
            GetProperties();
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {

            List<CustomPropertyDescriptor> pdl = m_pdl.FindAll(pd => pd.Attributes.Contains(attributes));

            PreProcess(pdl);
            PropertyDescriptorCollection pdcReturn = new PropertyDescriptorCollection(pdl.ToArray());

            return pdcReturn;
        }

        public override PropertyDescriptorCollection GetProperties() {
            if (m_pdl.Count == 0) {
                PropertyDescriptorCollection pdc = base.GetProperties();  // this gives us a readonly collection, no good    
                foreach (PropertyDescriptor pd in pdc) {
                    if (!(pd is CustomPropertyDescriptor)) {
                        CustomPropertyDescriptor cpd = new CustomPropertyDescriptor(base.GetPropertyOwner(pd), pd);
                        m_pdl.Add(cpd);
                    }
                }
            }

            List<CustomPropertyDescriptor> pdl = m_pdl.FindAll(pd => pd != null);

            PreProcess(pdl);
            PropertyDescriptorCollection pdcReturn = new PropertyDescriptorCollection(m_pdl.ToArray());

            return pdcReturn;
        }

        private void PreProcess(List<CustomPropertyDescriptor> pdl) {
            //if (m_PropertySortOrder != CustomSortOrder.None && pdl.Count > 0) {
            //    PropertySorter propSorter = new PropertySorter();
            //    propSorter.SortOrder = m_PropertySortOrder;
            //    pdl.Sort(propSorter);
            //}
            //UpdateCategoryTabAppendCount();
            //UpdateResourceManager();
        }

        public CustomPropertyDescriptor GetProperty(string propertyName) {
            CustomPropertyDescriptor cpd = m_pdl.FirstOrDefault(a => String.Compare(a.Name, propertyName, true) == 0);
            return cpd;
        }
    }

    internal class CustomTypeDescriptionProvider : TypeDescriptionProvider {
        private TypeDescriptionProvider m_parent = null;
        private ICustomTypeDescriptor m_ctd = null;

        public CustomTypeDescriptionProvider()
          : base() {
        }

        public CustomTypeDescriptionProvider(TypeDescriptionProvider parent)
          : base(parent) {
            m_parent = parent;
        }
        public CustomTypeDescriptionProvider(TypeDescriptionProvider parent, ICustomTypeDescriptor ctd)
          : base(parent) {
            m_parent = parent;
            m_ctd = ctd;
        }
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
            return m_ctd;
        }
    }

    public static class ProviderInstaller {
        public static DynamicCustomTypeDescriptor Install(object instance) {
            TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(instance);
            ICustomTypeDescriptor parentCtd = parentProvider.GetTypeDescriptor(instance);
            DynamicCustomTypeDescriptor ourCtd = new DynamicCustomTypeDescriptor(parentCtd, instance);
            CustomTypeDescriptionProvider ourProvider = new CustomTypeDescriptionProvider(parentProvider, ourCtd);
            TypeDescriptor.AddProvider(ourProvider, instance);
            return ourCtd;
        }
    }

    public class CustomPropertyDescriptor : PropertyDescriptor {
        private static Hashtable m_hashRM = new Hashtable();

        internal object m_owner = null;
        private Type m_PropType = Type.Missing.GetType();
        private AttributeList m_Attributes = new AttributeList();
        private PropertyDescriptor m_pd = null;
        private Collection<PropertyValueUIItem> m_colUIItem = new Collection<PropertyValueUIItem>();

        internal CustomPropertyDescriptor(object owner, string sName, Type type, object value, params Attribute[] attributes)
          : base(sName, attributes) {
            this.m_owner = owner;
            this.m_value = value;
            m_PropType = type;
            m_Attributes.AddRange(attributes);

            UpdateMemberData();
        }
        internal CustomPropertyDescriptor(object owner, PropertyDescriptor pd)
          : base(pd) {
            m_pd = pd;
            m_owner = owner;
            m_Attributes = new AttributeList(pd.Attributes);
            UpdateMemberData();
        }
        private void UpdateMemberData() {

            if (m_pd != null) {
                m_value = m_pd.GetValue(m_owner);
            }

            if (PropertyType.IsEnum) {
                StandardValueAttribute[] sva = StandardValueAttribute.GetEnumItems(PropertyType);
                this.m_StatandardValues.AddRange(sva);
            }
            else if (PropertyType == typeof(bool)) {
                this.m_StatandardValues.Add(new StandardValueAttribute(true));
                this.m_StatandardValues.Add(new StandardValueAttribute(false));
            }
        }

        public override Type ComponentType {
            get {
                return m_owner.GetType();
            }
        }
        public override Type PropertyType {
            get {
                if (m_pd != null) {
                    return this.m_pd.PropertyType;
                }
                return m_PropType;
            }
        }

        protected override Attribute[] AttributeArray {
            get {
                return m_Attributes.ToArray();
            }
            set {
                m_Attributes.Clear();
                m_Attributes.AddRange(value);
            }
        }

        public override AttributeCollection Attributes {
            get {
                AttributeCollection ac = new AttributeCollection(m_Attributes.ToArray());
                return ac;
            }
        }
        protected override void FillAttributes(IList attributeList) {
            foreach (Attribute attr in m_Attributes) {
                attributeList.Add(attr);
            }
        }
        public IList<Attribute> AllAttributes {
            get {
                return m_Attributes;
            }
        }
        /// <summary>
        /// Must override abstract properties.
        /// </summary>
        /// 

        public override bool IsLocalizable {
            get {
                LocalizableAttribute attr = (LocalizableAttribute)m_Attributes.FirstOrDefault(a => a is LocalizableAttribute);
                if (attr != null) {
                    return attr.IsLocalizable;
                }
                return base.IsLocalizable;
            }
        }
        public void SetIsLocalizable(bool isLocalizable) {
            LocalizableAttribute attr = (LocalizableAttribute)m_Attributes.FirstOrDefault(a => a is LocalizableAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is LocalizableAttribute);
            }
            attr = new LocalizableAttribute(isLocalizable);
            m_Attributes.Add(attr);
        }
        public override bool IsReadOnly {
            get {
                ReadOnlyAttribute attr = (ReadOnlyAttribute)m_Attributes.FirstOrDefault(a => a is ReadOnlyAttribute);
                if (attr != null) {
                    return attr.IsReadOnly;
                }
                return false;
            }
        }
        public void SetIsReadOnly(bool isReadOnly) {
            ReadOnlyAttribute attr = (ReadOnlyAttribute)m_Attributes.FirstOrDefault(a => a is ReadOnlyAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is ReadOnlyAttribute);
            }
            attr = new ReadOnlyAttribute(isReadOnly);
            m_Attributes.Add(attr);
        }
        public override bool IsBrowsable {
            get {
                BrowsableAttribute attr = (BrowsableAttribute)m_Attributes.FirstOrDefault(a => a is BrowsableAttribute);
                if (attr != null) {
                    return attr.Browsable;
                }
                return base.IsBrowsable;
            }
        }
        public void SetIsBrowsable(bool isBrowsable) {
            BrowsableAttribute attr = (BrowsableAttribute)m_Attributes.FirstOrDefault(a => a is BrowsableAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is BrowsableAttribute);
            }
            attr = new BrowsableAttribute(isBrowsable);
            m_Attributes.Add(attr);
        }

        private string m_KeyPrefix = String.Empty;

        internal string KeyPrefix {
            get {
                return m_KeyPrefix;
            }
            set {
                m_KeyPrefix = value;
            }
        }

        public override string DisplayName {
            get {

                if (this.ResourceManager != null && (this.PropertyFlags & PropertyFlags.LocalizeDisplayName) > 0) {
                    string sKey = KeyPrefix + base.Name + "_Name";

                    string sResult = this.ResourceManager.GetString(sKey, CultureInfo.CurrentUICulture);
                    if (!String.IsNullOrEmpty(sResult)) {
                        return sResult;
                    }
                }
                DisplayNameAttribute attr = (DisplayNameAttribute)m_Attributes.FirstOrDefault(a => a is DisplayNameAttribute);
                if (attr != null) {
                    return attr.DisplayName;
                }
                return base.DisplayName;
            }
        }
        public void SetDisplayName(string displayName) {
            DisplayNameAttribute attr = (DisplayNameAttribute)m_Attributes.FirstOrDefault(a => a is DisplayNameAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is DisplayNameAttribute);
            }
            attr = new DisplayNameAttribute(displayName);
            m_Attributes.Add(attr);
        }
        public override string Category {
            get {
                string sResult = String.Empty;
                if (this.ResourceManager != null && CategoryId != 0 && (this.PropertyFlags & PropertyFlags.LocalizeCategoryName) > 0) {
                    string sKey = KeyPrefix + "Cat" + CategoryId.ToString();
                    sResult = this.ResourceManager.GetString(sKey, CultureInfo.CurrentUICulture);
                    if (!String.IsNullOrEmpty(sResult)) {
                        return sResult.PadLeft(sResult.Length + m_TabAppendCount, '\t');
                    }

                }
                CategoryAttribute attr = (CategoryAttribute)m_Attributes.FirstOrDefault(a => a is CategoryAttribute);
                if (attr != null) {
                    sResult = attr.Category;
                }
                if (String.IsNullOrEmpty(sResult)) {
                    sResult = base.Category;
                }
                return sResult.PadLeft(base.Category.Length + m_TabAppendCount, '\t');
            }
        }
        public void SetCategory(string category) {
            CategoryAttribute attr = (CategoryAttribute)m_Attributes.FirstOrDefault(a => a is CategoryAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is CategoryAttribute);
            }
            attr = new CategoryAttribute(category);
            m_Attributes.Add(attr);
        }
        public override string Description {
            get {
                if (this.ResourceManager != null && (this.PropertyFlags & PropertyFlags.LocalizeDescription) > 0) {
                    string sKey = KeyPrefix + base.Name + "_Desc";
                    string sResult = this.ResourceManager.GetString(sKey, CultureInfo.CurrentUICulture);
                    if (!String.IsNullOrEmpty(sResult)) {
                        return sResult;
                    }
                }
                DescriptionAttribute attr = (DescriptionAttribute)m_Attributes.FirstOrDefault(a => a is DescriptionAttribute);
                if (attr != null) {
                    return attr.Description;
                }
                return base.Description;
            }
        }
        public void SetDescription(string description) {
            DescriptionAttribute attr = (DescriptionAttribute)m_Attributes.FirstOrDefault(a => a is DescriptionAttribute);
            if (attr != null) {
                m_Attributes.RemoveAll(a => a is DescriptionAttribute);
            }
            attr = new DescriptionAttribute(description);
            m_Attributes.Add(attr);
        }

        public object DefaultValue {
            get {
                DefaultValueAttribute attr = (DefaultValueAttribute)m_Attributes.FirstOrDefault(a => a is DefaultValueAttribute);
                if (attr != null) {
                    return attr.Value;
                }
                return null;
            }
            set {
                DefaultValueAttribute attr = (DefaultValueAttribute)m_Attributes.FirstOrDefault(a => a is DefaultValueAttribute);
                if (attr == null) {
                    m_Attributes.RemoveAll(a => a is DefaultValueAttribute);
                }
                attr = new DefaultValueAttribute(value);
            }
        }

        public int PropertyId {
            get {
                IdAttribute rsa = (IdAttribute)m_Attributes.FirstOrDefault(a => a is IdAttribute);
                if (rsa != null) {
                    return rsa.PropertyId;
                }
                return 0;
            }
            set {
                IdAttribute rsa = (IdAttribute)m_Attributes.FirstOrDefault(a => a is IdAttribute);
                if (rsa == null) {
                    rsa = new IdAttribute();
                    m_Attributes.Add(rsa);
                }
                rsa.PropertyId = value;
            }
        }
        public int CategoryId {
            get {
                IdAttribute rsa = (IdAttribute)m_Attributes.FirstOrDefault(a => a is IdAttribute);
                if (rsa != null) {
                    return rsa.CategoryId;
                }
                return 0;
            }
            set {
                IdAttribute rsa = (IdAttribute)m_Attributes.FirstOrDefault(a => a is IdAttribute);
                if (rsa == null) {
                    rsa = new IdAttribute();
                    m_Attributes.Add(rsa);
                }
                rsa.CategoryId = value;
            }
        }
        private int m_TabAppendCount = 0;

        internal int TabAppendCount {
            get {
                return m_TabAppendCount;
            }
            set {
                m_TabAppendCount = value;
            }
        }

        private ResourceManager m_ResourceManager = null;

        internal ResourceManager ResourceManager {
            get {
                return m_ResourceManager;
            }
            set {
                m_ResourceManager = value;
            }
        }

        private object m_value = null;
        public override object GetValue(object component) {
            if (m_pd != null) {
                return m_pd.GetValue(component);
            }
            return m_value;
        }

        public override void SetValue(object component, object value) {
            if (value != null && value is StandardValueAttribute) {
                m_value = (value as StandardValueAttribute).Value;
            }
            else {
                m_value = value;
            }

            if (m_pd != null) {
                m_pd.SetValue(component, m_value);
                this.OnValueChanged(this, new EventArgs());

            }
            else {
                EventHandler eh = this.GetValueChangedHandler(m_owner);
                if (eh != null) {
                    eh.Invoke(this, new EventArgs());
                }
                this.OnValueChanged(this, new EventArgs());
            }
        }
        protected override void OnValueChanged(object component, EventArgs e) {
            MemberDescriptor md = component as MemberDescriptor;

            base.OnValueChanged(component, e);
        }

        /// <summary>
        /// Abstract base members
        /// </summary>			
        public override void ResetValue(object component) {
            DefaultValueAttribute dva = (DefaultValueAttribute)m_Attributes.FirstOrDefault(a => a is DefaultValueAttribute);
            if (dva == null) {
                return;
            }
            SetValue(component, dva.Value);
        }

        public override bool CanResetValue(object component) {
            DefaultValueAttribute dva = (DefaultValueAttribute)m_Attributes.FirstOrDefault(a => a is DefaultValueAttribute);
            if (dva == null) {
                return false;
            }
            bool bOk = (dva.Value.Equals(m_value));
            return !bOk;

        }

        public override bool ShouldSerializeValue(object component) {
            return CanResetValue(m_owner);
        }

        private List<StandardValueAttribute> m_StatandardValues = new List<StandardValueAttribute>();
        public ICollection<StandardValueAttribute> StatandardValues {
            get {
                if (PropertyType.IsEnum || PropertyType == typeof(bool)) {
                    return m_StatandardValues.AsReadOnly();
                }
                return m_StatandardValues;
            }
        }
        private Image m_ValueImage = null;

        public Image ValueImage {
            get {
                return m_ValueImage;
            }
            set {
                m_ValueImage = value;
            }
        }

        public PropertyFlags PropertyFlags {
            get {
                PropertyStateFlagsAttribute attr = (PropertyStateFlagsAttribute)m_Attributes.FirstOrDefault(a => a is PropertyStateFlagsAttribute);
                if (attr == null) {
                    attr = new PropertyStateFlagsAttribute();
                    m_Attributes.Add(attr);
                    attr.Flags = PropertyFlags.Default;
                }

                return attr.Flags;
            }
            set {
                PropertyStateFlagsAttribute attr = (PropertyStateFlagsAttribute)m_Attributes.FirstOrDefault(a => a is PropertyStateFlagsAttribute);
                if (attr == null) {
                    attr = new PropertyStateFlagsAttribute();
                    m_Attributes.Add(attr);
                    attr.Flags = PropertyFlags.Default;
                }
                attr.Flags = value;

            }
        }
    }
}
