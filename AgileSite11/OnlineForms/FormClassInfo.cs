using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.OnlineForms;

[assembly: RegisterObjectType(typeof(FormClassInfo), FormClassInfo.OBJECT_TYPE_FORM)]

namespace CMS.OnlineForms
{
    /// <summary>
    /// <see cref="BizFormInfo"/> stores information about General, Autoresponder, Email notification and similar tabs. 
    /// <see cref="FormClassInfo"/> stores the structure of the form. That means for example form definition (Fields tab) and search fields settings.
    /// <see cref="BizFormItem"/> stores the data that visitors fill on the website.
    /// </summary>
    public class FormClassInfo : DataClassInfo
    {
        #region "Type information properties"

        /// <summary>
        /// Object type for form
        /// </summary>
        public const string OBJECT_TYPE_FORM = PredefinedObjectType.FORMCLASS;


        /// <summary>
        /// Type information for forms.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOFORM = new ObjectTypeInfo(typeof(DataClassInfoProvider), OBJECT_TYPE_FORM, "CMS.Class", "ClassID", "ClassLastModified", "ClassGUID", "ClassName", "ClassDisplayName", null, null, null, null)
        {
            MacroCollectionName = "BizForm",
            OriginalTypeInfo = TYPEINFO,
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            HasExternalColumns = true,
            DeleteObjectWithAPI = true,
            VersionGUIDColumn = "ClassVersionGUID",
            FormDefinitionColumn = "ClassFormDefinition",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            TypeCondition = new TypeCondition().WhereEquals("ClassIsForm", true),
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("ClassFormDefinition"),
                    new StructuredField<SearchSettings>("ClassSearchSettings"),
                    new StructuredField("ClassContactMapping")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private static RegisteredProperties<DataClassInfo> mLocalRegisteredProperties;
        private IInfoObjectCollection mItems;

        #endregion


        #region "Properties"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return TYPEINFOFORM;
            }
        }


        /// <summary>
        /// Returns the items of this collection
        /// </summary>
        public IInfoObjectCollection Items
        {
            get
            {
                return mItems ?? (mItems = new InfoObjectCollection(BizFormItemProvider.GetObjectType(ClassName)));
            }
        }


        /// <summary>
        /// Local registered properties
        /// </summary>
        protected override RegisteredProperties<DataClassInfo> RegisteredProperties
        {
            get
            {
                return mLocalRegisteredProperties ?? (mLocalRegisteredProperties = new RegisteredProperties<DataClassInfo>(RegisterProperties));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public FormClassInfo()
            : base(true)
        {
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Items", m => ((FormClassInfo)m).Items);
        }

        #endregion
    }
}
