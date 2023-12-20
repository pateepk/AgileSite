using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    using RelatedTypesDictionary = StringSafeDictionary<List<string>>;

    /// <summary>
    /// Object type info structure.
    /// </summary>
    [DebuggerDisplay("{ObjectType}")]
    public class ObjectTypeInfo : AbstractHierarchicalObject<ObjectTypeInfo>
    {
        #region "Static variables"

        // If true, object tasks are logged when site binding changes.
        private static bool? mLogSiteBindings;

        // If true, object tasks are logged.
        private static bool? mGlobalLogSynchronization;

        // If true, object instances are tracked.
        private static bool? mTrackObjectInstances;

        // Dictionary of related typeinfos (the relationship is made from OriginalTypeInfo property, but reflects the relationship from both sides).
        private static RelatedTypesDictionary mRelatedTypes;

        /// <summary>
        /// Uknown value.
        /// </summary>
        public const string VALUE_UNKNOWN = "[Unknown]";

        /// <summary>
        /// Uknown column name.
        /// </summary>
        public const string COLUMN_NAME_UNKNOWN = "[Unknown]";

        #endregion


        #region "Column name variables"

        // ID column name.
        private string mIDColumn = COLUMN_NAME_UNKNOWN;

        // Time stamp column name.
        private string mTimeStampColumn = COLUMN_NAME_UNKNOWN;

        // GUID column name.
        private string mGUIDColumn = COLUMN_NAME_UNKNOWN;

        // Code name column.
        private string mCodeNameColumn = COLUMN_NAME_UNKNOWN;

        // Display name column.
        private string mDisplayNameColumn = COLUMN_NAME_UNKNOWN;

        // Binary column name.
        private string mBinaryColumn = COLUMN_NAME_UNKNOWN;

        // Site ID column.
        private string mSiteIDColumn = COLUMN_NAME_UNKNOWN;

        // Parent ID column.
        private string mParentIDColumn = COLUMN_NAME_UNKNOWN;

        // Community group ID column.
        private string mGroupIDColumn = COLUMN_NAME_UNKNOWN;

        // Resource ID column for dependant object resource.
        private string mResourceIDColumn = COLUMN_NAME_UNKNOWN;


        // File extension column (for objects with binary column - column which specifies extension type of binary data).
        private string mExtensionColumn = COLUMN_NAME_UNKNOWN;

        // File size column (for objects with binary column - column which specifies size of binary data).
        private string mSizeColumn = COLUMN_NAME_UNKNOWN;

        // Mime type column (for objects with binary column - column which specifies mime type of binary data).
        private string mMimeTypeColumn = COLUMN_NAME_UNKNOWN;

        // Name of the column that contains the GUID of the object thumbnail meta file.
        private string mThumbnailGUIDColumn = COLUMN_NAME_UNKNOWN;

        // Name of the meta file group of the thumbnail meta file.
        private string mThumbnailMetaFileGroup = ObjectAttachmentsCategories.THUMBNAIL;

        // Name of the column that contains the GUID of the object icon meta file.
        private string mIconGUIDColumn = COLUMN_NAME_UNKNOWN;

        // Name of the column that contains the version GUID of the object.
        private string mVersionGUIDColumn = COLUMN_NAME_UNKNOWN;

        // Name of the column that contains code of the object (markup).
        private string mCodeColumn = COLUMN_NAME_UNKNOWN;

        // Name of the column that contains CSS of the object.
        private string mCSSColumn = COLUMN_NAME_UNKNOWN;

        // Name of the meta file group of the icon meta file.
        private string mIconMetaFileGroup = ObjectAttachmentsCategories.ICON;

        // Order column name.
        private string mOrderColumn = COLUMN_NAME_UNKNOWN;

        // Column which stores cached content extracted for this Info object to be used in search indexing. Used by the method EnsureSearchContent.
        private string mSearchContentColumn = COLUMN_NAME_UNKNOWN;

        // Customized columns column name.
        private string mCustomizedColumnsColumn = COLUMN_NAME_UNKNOWN;

        // Is custom flag column name.
        private string mIsCustomColumn = COLUMN_NAME_UNKNOWN;

        // Parent TypeInfo.
        private ObjectTypeInfo mOriginalTypeInfo;

        #endregion


        #region "Variables"

        // Events handled by object of this type
        private TypeInfoEvents mEvents;

        // Returns list of related TypeInfos of this TypeInfo (the relationship is made from OriginalTypeInfo property, but reflects the relationship from both sides).
        private IEnumerable<string> mRelatedTypeInfos;

        // Returns true if related type infos were loaded
        private bool mRelatedTypeInfosLoaded;

        // Name of object collection used for the object type's data in the macro engine.
        private string mMacroCollectionName;

        // Time of the last invalidation of all of the objects.
        private DateTime mAllObjectsInvalidated = DateTime.MinValue;

        // Time of the last invalidation of any object.
        private DateTime mLastObjectInvalidated = DateTime.MinValue;

        // Table of invalidated objects [ID] -> [Invalidated when].
        private SafeDictionary<int, DateTime> mInvalidatedObjects;

        // Table of invalidated direct child objects [ParentID] -> [Invalidated when].
        private SafeDictionary<int, DateTime> mInvalidatedChildren;


        // Number of object instances of the current type.
        private int mInstanceCount;

        // Foreign key column names.
        private IEnumerable<string> mForeignKeys;

        // App settings that can disable event logging for any type.
        private bool? mLogObjectEventsAppSettings;

        // Parent type information of the info record if the object is child object (has parent).
        private ObjectTypeInfo mParentTypeInfo = InfoHelper.UNKNOWN_TYPEINFO;

        // Object type.
        private string mObjectType = VALUE_UNKNOWN;

        // Class name.
        private string mObjectClassName = VALUE_UNKNOWN;


        // ModuleInfo object retrieved by ModuleName.
        private ModuleInfo mModuleInfo;


        // Default order by clause for getting data.
        private string mDefaultOrderBy;

        // Parent type (ObjectType constant).
        private string mParentObjectType = String.Empty;

        /// <summary>
        /// Object dependencies (list of foreign keys definition of the object with the exception of SiteID column and ParentID columns).
        /// Automatically computed from Extends and DependsOn lists of all objects within the system.
        /// </summary>
        private List<ObjectDependency> mObjectDependencies;

        // Child object types
        private readonly List<string> mChildObjectTypes = new List<string>();

        // Binding object types
        private readonly List<string> mBindingObjectTypes = new List<string>();

        // Other binding types
        private readonly List<string> mOtherBindingObjectTypes = new List<string>();

        // Name of the columns (separated by semicolon) of object dependencies which should be included into the parent data (for example class data for BizForms).
        private string mChildDependencyColumns = String.Empty;

        // Site binding object (if exists).
        private BaseInfo mSiteBindingObject;

        // Site binding object type (if exists).
        private string mSiteBinding = VALUE_UNKNOWN;

        // Object category.
        private BaseInfo mObjectCategory;

        // Category ID column name.
        private string mCategoryIDColumn;

        // Determines if the object is binding.
        private bool? mIsBinding;


        // Indicates if the object can use upsert.
        private bool? mUseUpsert;

        // Indicates if the object is binding between more than two objects.
        private bool? mIsMultipleBinding;

        // Determines if the object is site binding.
        private bool? mIsSiteBinding;

        // Determines whether the permissions should be checked when the object is accessed within the macro engine as an inner object.
        private bool mCheckPermissions = true;

        // Indicates if parent is allowed to be touched, if exists.
        private bool mAllowTouchParent = true;

        // Whether to log integration tasks.
        private bool mLogIntegration = true;

        // Indicates if object can be cloned.
        private bool mSupportsCloning = true;

        // Indicates if object can be cloned to other site than the site of the original.
        private bool mSupportsCloneToOtherSite = true;

        // Logs events.
        private bool mLogEvents;

        // Update time stamp.
        private bool mUpdateTimeStamp = true;

        // If true, the object is required when imported / synchronized. If false, the object can be skipped when some error occurs.
        private bool mRequiredObject = true;

        // If true, the objects can have meta files. By default no metafiles to simplify general processes
        private bool mHasMetaFiles;

        // Specifies whether the object supports data export.
        private bool? mAllowDataExport;

        // Indicates if object can be deleted to rec. bin.
        private bool? mAllowRestore;

        // Original object type of virtual object types.
        private string mOriginalObjectType;

        // Default where condition restricting the data of this particular object type.
        private string mWhereCondition;

        // Indicates if provider supports methods customization.
        private bool? mProviderIsCustomizable;

        // Names of the columns that reference some object.
        private List<string> mReferenceColumnNames;

        // If true, the object of this type has automatic properties.
        private bool? mHasAutomaticProperties;

        /// <summary>
        /// Indicates whether UniGrids working with objects of this type will remember their state, i.e., filter, page number, page size and sorting order.
        /// </summary>
        /// <remarks>
        /// This setting can be overridden by individual UniGrids.
        /// </remarks>
        private bool? mRememberUniGridState;

        // Indicates if the object is main (= is not child of other objects).
        private bool? mIsMainObject;

        // Object path column for hierarchical objects.
        private string mObjectPathColumn = String.Empty;

        // Object ID path column. If the object has hierarchical structure.
        private string mObjectIDPathColumn = COLUMN_NAME_UNKNOWN;

        // Object name path column. If the object has hierarchical structure.
        private string mObjectNamePathColumn = COLUMN_NAME_UNKNOWN;

        // Object level column. If the object has hierarchical structure, determines the depth within the tree.
        private string mObjectLevelColumn = COLUMN_NAME_UNKNOWN;

        // Name of the column which determines whether the object is enabled or disabled within the system.
        private string mEnabledColumn = COLUMN_NAME_UNKNOWN;

        // Form definition column name
        private string mFormDefinitionColumn = COLUMN_NAME_UNKNOWN;

        // Indicates if the object may contain some macros in it's values.
        private bool? mContainsMacros;

        // Import/export settings.
        private ImportExportSettings mImportExportSettings;

        // Synchronization settings.
        private SynchronizationSettings mSynchronizationSettings;

        // Indicates if object should be deleted when removing dependencies of other object.
        private bool mDeleteAsDependency = true;

        // Class structure info of this type
        private ClassStructureInfo mClassStructureInfo;

        // Indicates if object can be searched.
        private bool? mSupportsSearch;

        // Indicates if the objects should be included in version data set of it's parent
        private bool mIncludeToVersionParentDataSet = true;

        // Indicates if the object can have object settings.
        private bool? mHasObjectSettings;

        // Defines how are the object's fields serialized into an XML.
        private SerializationSettings mSerializationSettings;

        // Defines how the object behaves within continuous integration module.
        private ContinuousIntegrationSettings mContinuousIntegrationSettings;

        private HashSet<string> mDependentObjectTypes;

        private readonly object lockObject = new object();

        #endregion


        #region "Static properties"

        /// <summary>
        /// If true, object tasks are logged when site binding changes.
        /// </summary>
        public static bool LogSiteBindings
        {
            get
            {
                if (mLogSiteBindings == null)
                {
                    mLogSiteBindings = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingLogSiteBindings"], true);
                }

                return mLogSiteBindings.Value;
            }
            set
            {
                mLogSiteBindings = value;
            }
        }


        /// <summary>
        /// If true, object tasks are logged.
        /// </summary>
        public static bool GlobalLogSynchronization
        {
            get
            {
                if (mGlobalLogSynchronization == null)
                {
                    mGlobalLogSynchronization = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingLogSynchronization"], true);
                }

                return mGlobalLogSynchronization.Value;
            }
            set
            {
                mGlobalLogSynchronization = value;
            }
        }


        /// <summary>
        /// If true, object instances are tracked.
        /// </summary>
        public static bool TrackObjectInstances
        {
            get
            {
                if (mTrackObjectInstances == null)
                {
                    mTrackObjectInstances = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugObjects"], DebugHelper.DebugEverything);
                }

                return mTrackObjectInstances.Value;
            }
            set
            {
                mTrackObjectInstances = value;
            }
        }


        /// <summary>
        /// Returns dictionary of related typeinfos (the relationship is made from OriginalTypeInfo property, but reflects the relationship from both sides). [objectType] => [List of related object types]
        /// </summary>
        private static RelatedTypesDictionary RelatedTypes
        {
            get
            {
                return mRelatedTypes ?? (mRelatedTypes = LoadRelatedTypes());
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if object should be deleted when removing dependencies of other object.
        /// </summary>
        [RegisterProperty]
        public bool DeleteAsDependency
        {
            get
            {
                return mDeleteAsDependency;
            }
            set
            {
                mDeleteAsDependency = value;
            }
        }


        /// <summary>
        /// Condition which can distinguish between several object types within one Info class. This is used for two purposes - first to generate correct type WHERE condition
        /// which is used in GetData (for example) and for correct object initialization when creating empty BaseInfo objects for the specified object type.
        /// If you need to specify a more complex WHERE condition, use the WhereCondition property which has higher priority.
        /// </summary>
        [RegisterColumn]
        public TypeCondition TypeCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Returns list of related TypeInfos of this TypeInfo (the relationship is made from OriginalTypeInfo property, but reflects the relationship from both sides).
        /// </summary>
        public IEnumerable<string> RelatedTypeInfos
        {
            get
            {
                if (!mRelatedTypeInfosLoaded)
                {
                    mRelatedTypeInfos = RelatedTypes[ObjectType] ?? Enumerable.Empty<string>();
                    mRelatedTypeInfosLoaded = true;
                }

                return mRelatedTypeInfos;
            }
        }


        /// <summary>
        /// Last time of the object invalidation.
        /// </summary>
        public DateTime LastObjectInvalidated
        {
            get
            {
                return mLastObjectInvalidated;
            }
        }


        /// <summary>
        /// Indicates whether column names are invalidated. Default is false.
        /// </summary>
        public bool ColumnsInvalidated
        {
            get;
            set;
        }


        /// <summary>
        /// Defines how Staging features work with the object type's data. Set through the properties of the SynchronizationSettings class.
        /// </summary>
        [RegisterProperty]
        public SynchronizationSettings SynchronizationSettings
        {
            get
            {
                return mSynchronizationSettings ?? (mSynchronizationSettings = new SynchronizationSettings());
            }
        }


        /// <summary>
        /// Defines how the object type works with the Export and Import features. Set through the properties of the ImportExportSettings class.
        /// </summary>
        [RegisterProperty]
        public ImportExportSettings ImportExportSettings
        {
            get
            {
                return mImportExportSettings ?? (mImportExportSettings = new ImportExportSettings(this));
            }
        }


        /// <summary>
        /// Indicates if the object is dynamic based on data in the DB (e.g. custom table item or biz form item object types).
        /// </summary>
        [RegisterColumn]
        public bool IsDataObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object is binding between more than two objects.
        /// </summary>
        [RegisterColumn]
        public bool IsMultipleBinding
        {
            get
            {
                if (mIsMultipleBinding == null)
                {
                    mIsMultipleBinding = IsBinding && !IsSiteBinding && (SiteIDColumn != COLUMN_NAME_UNKNOWN);
                }
                return mIsMultipleBinding.Value;
            }
            set
            {
                mIsMultipleBinding = value;
            }
        }


        /// <summary>
        /// Indicates if the object is main (= is not child of other objects).
        /// </summary>
        [RegisterColumn]
        public bool IsMainObject
        {
            get
            {
                if (mIsMainObject == null)
                {
                    mIsMainObject = (ParentIDColumn == COLUMN_NAME_UNKNOWN) || IsRelatedTo(ParentObjectType);
                }
                return mIsMainObject.Value;
            }
            set
            {
                mIsMainObject = value;
            }
        }


        /// <summary>
        /// Names of the columns that reference some object.
        /// </summary>
        /// <remarks>
        /// Includes the <see cref="SiteIDColumn"/>, <see cref="ParentIDColumn"/> and <see cref="ObjectDependencies"/> (except the dynamic object type dependencies, see <see cref="ObjectDependency.HasDynamicObjectType"/>).
        /// </remarks>
        [RegisterProperty]
        public List<string> ReferenceColumnNames
        {
            get
            {
                if (mReferenceColumnNames == null)
                {
                    var names = new List<string>();

                    if (SiteIDColumn != COLUMN_NAME_UNKNOWN)
                    {
                        names.Add(SiteIDColumn);
                    }

                    if (ParentIDColumn != COLUMN_NAME_UNKNOWN)
                    {
                        names.Add(ParentIDColumn);
                    }

                    // Other foreign keys
                    if (ObjectDependencies != null)
                    {
                        foreach (var dep in ObjectDependencies)
                        {
                            // Skip dynamic dependencies
                            if (dep.HasDynamicObjectType())
                            {
                                continue;
                            }

                            names.Add(dep.DependencyColumn);
                        }
                    }

                    mReferenceColumnNames = names;
                }

                return mReferenceColumnNames;
            }
        }


        /// <summary>
        /// Gets or sets name of object collection used for the object type's data in the macro engine.
        /// If not set, the <see cref="ObjectClassName"/> is used by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Should be set when info object belongs to <see cref="ObjectTypeManager.MainObjectTypes"/> and has more than 1 <see cref="ObjectTypeInfo"/>.
        /// </para>
        /// <para>
        /// If <see cref="MacroCollectionName"/> is set to 'cms.role' then macro intellisense supplies hint for user as 'Roles' because of
        /// <see cref="InfoObjectRepository{TCollection, TObject, TSettings}.GetNicePropertyName(string)"/> method used internally
        /// by <see cref="InfoObjectRepository{TCollection, TObject, TSettings}" />.
        /// </para>
        /// <para>
        /// Value of this property is used in Kentico macro engine for acquiring objects of given type, e.g. SiteObjects.Roles or GlobalObjects.Roles.
        /// </para>
        /// </remarks>
        [RegisterColumn]
        public string MacroCollectionName
        {
            get
            {
                if (mMacroCollectionName == null)
                {
                    return ObjectClassName;
                }

                return mMacroCollectionName;
            }
            set
            {
                mMacroCollectionName = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the IDs of objects (i.e. the primary key column). Can be null in rare cases, for example binding objects without an identity column.
        /// </summary>
        [RegisterColumn]
        public string IDColumn
        {
            get
            {
                return mIDColumn;
            }
            set
            {
                mIDColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the last modification date for objects.
        /// </summary>
        [RegisterColumn]
        public string TimeStampColumn
        {
            get
            {
                return mTimeStampColumn;
            }
            set
            {
                mTimeStampColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the GUID identifiers of objects.
        /// </summary>
        [RegisterColumn]
        public string GUIDColumn
        {
            get
            {
                return mGUIDColumn;
            }
            set
            {
                mGUIDColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the unique text identifiers of objects. Can be null for object types without a dedicated code name column.
        /// </summary>
        [RegisterColumn]
        public string CodeNameColumn
        {
            get
            {
                return mCodeNameColumn;
            }
            set
            {
                mCodeNameColumn = value;
            }
        }


        /// <summary>
        /// If true, the system validates the code names of objects to be unique across both global and site-related objects.
        /// If false, global objects can have the same code name as a site-specific object. The default value is false.
        /// Applies to object types with a specified SiteIDColumn that also have the SupportsGlobalObjects property set to true.
        /// </summary>
        [RegisterColumn]
        public bool NameGloballyUnique
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the class field (column) that stores the visible names of objects (names used in the administration interface or on the live site).
        /// </summary>
        [RegisterColumn]
        public string DisplayNameColumn
        {
            get
            {
                return mDisplayNameColumn;
            }
            set
            {
                mDisplayNameColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores binary data for objects.
        /// </summary>
        [RegisterColumn]
        public string BinaryColumn
        {
            get
            {
                return mBinaryColumn;
            }
            set
            {
                mBinaryColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the order of objects (for object types that have a defined order).
        /// Allows automatic actions for changing object order on listing pages (in UniGrid components).
        /// </summary>
        [RegisterColumn]
        public string OrderColumn
        {
            get
            {
                return mOrderColumn;
            }
            set
            {
                mOrderColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores cached content extracted for this Info object to be used in search indexing.
        /// </summary>
        [RegisterColumn]
        public string SearchContentColumn
        {
            get
            {
                return mSearchContentColumn;
            }
            set
            {
                mSearchContentColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that contains customized columns.
        /// </summary>
        [RegisterColumn]
        public string CustomizedColumnsColumn
        {
            get
            {
                return mCustomizedColumnsColumn;
            }
            set
            {
                mCustomizedColumnsColumn = value;
            }
        }


        /// <summary>
        /// Column name of the info record for flag IsCustom.
        /// </summary>
        [RegisterColumn]
        public string IsCustomColumn
        {
            get
            {
                return mIsCustomColumn;
            }
            set
            {
                mIsCustomColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores site IDs for site-related objects.
        /// Only use the site ID column if the object does not have a separate binding object type for the site relationship.
        /// </summary>
        [RegisterColumn]
        public string SiteIDColumn
        {
            get
            {
                return mSiteIDColumn;
            }
            set
            {
                mSiteIDColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the IDs of parent objects. Null for object types without a parent object.
        /// </summary>
        [RegisterColumn]
        public string ParentIDColumn
        {
            get
            {
                if (String.IsNullOrEmpty(ParentObjectType))
                {
                    return COLUMN_NAME_UNKNOWN;
                }
                return mParentIDColumn;
            }
            set
            {
                mParentIDColumn = value;
            }
        }


        /// <summary>
        /// Possible parent ID column name of the info record in case the parent ID column is optional (if the object can be both global and child).
        /// </summary>
        [RegisterColumn]
        public string PossibleParentIDColumn
        {
            get
            {
                return mParentIDColumn;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores group IDs for objects related to specific community groups.
        /// </summary>
        [RegisterColumn]
        public string GroupIDColumn
        {
            get
            {
                return mGroupIDColumn;
            }
            set
            {
                mGroupIDColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores ID referencing a module (resource) in Kentico. Intended for object types whose objects have a relationship with a specific module.
        /// </summary>
        [RegisterColumn]
        public string ResourceIDColumn
        {
            get
            {
                return mResourceIDColumn;
            }
            set
            {
                mResourceIDColumn = value;
            }
        }


        /// <summary>
        /// Intended for object types that store binary data. Sets the name of the class field (column) that stores the mime type of the binary data.
        /// </summary>
        [RegisterColumn]
        public string MimeTypeColumn
        {
            get
            {
                return mMimeTypeColumn;
            }
            set
            {
                mMimeTypeColumn = value;
            }
        }


        /// <summary>
        /// Intended for object types that store binary data. Sets the name of the class field (column) that stores the extension type of the binary data.
        /// </summary>
        [RegisterColumn]
        public string ExtensionColumn
        {
            get
            {
                return mExtensionColumn;
            }
            set
            {
                mExtensionColumn = value;
            }
        }


        /// <summary>
        /// The name of the class field (column) that indicates whether objects are enabled or disabled.
        /// </summary>
        [RegisterColumn]
        public string EnabledColumn
        {
            get
            {
                return mEnabledColumn;
            }
            set
            {
                mEnabledColumn = value;
            }
        }


        /// <summary>
        /// Intended for object types that store binary data. Sets the name of the class field (column) that stores the size of the binary data.
        /// </summary>
        [RegisterColumn]
        public string SizeColumn
        {
            get
            {
                return mSizeColumn;
            }
            set
            {
                mSizeColumn = value;
            }
        }


        /// <summary>
        /// Name of the column that contains the GUID of the object thumbnail meta file.
        /// </summary>
        [RegisterColumn]
        public string ThumbnailGUIDColumn
        {
            get
            {
                return mThumbnailGUIDColumn;
            }
            set
            {
                mThumbnailGUIDColumn = value;
            }
        }


        /// <summary>
        /// Name of the column that contains the GUID of the object icon meta file.
        /// </summary>
        [RegisterColumn]
        public string IconGUIDColumn
        {
            get
            {
                return mIconGUIDColumn;
            }
            set
            {
                mIconGUIDColumn = value;
            }
        }


        /// <summary>
        /// Name of the meta file group of the thumbnail meta file.
        /// </summary>
        [RegisterColumn]
        public string ThumbnailMetaFileGroup
        {
            get
            {
                return mThumbnailMetaFileGroup;
            }
            set
            {
                mThumbnailMetaFileGroup = value;
            }
        }


        /// <summary>
        /// Name of the meta file group of the icon meta file.
        /// </summary>
        [RegisterColumn]
        public string IconMetaFileGroup
        {
            get
            {
                return mIconMetaFileGroup;
            }
            set
            {
                mIconMetaFileGroup = value;
            }
        }


        /// <summary>
        /// The code name of the module under which the object type belongs. Always required for all object type information definitions.
        /// </summary>
        [RegisterColumn]
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the class field (column) that stores the GUID identifiers for individual versions of objects.
        /// Intended for object types that support versioning.
        /// </summary>
        [RegisterColumn]
        public string VersionGUIDColumn
        {
            get
            {
                return mVersionGUIDColumn;
            }
            set
            {
                mVersionGUIDColumn = value;
            }
        }


        /// <summary>
        /// Name of the column that contains code of the object (markup).
        /// </summary>
        [RegisterColumn]
        public string CodeColumn
        {
            get
            {
                return mCodeColumn;
            }
            set
            {
                mCodeColumn = value;
            }
        }


        /// <summary>
        /// Name of the column that contains CSS of the object.
        /// </summary>
        [RegisterColumn]
        public string CSSColumn
        {
            get
            {
                return mCSSColumn;
            }
            set
            {
                mCSSColumn = value;
            }
        }


        /// <summary>
        /// Gets the ModuleInfo object based on the value of the ModuleName property.
        /// </summary>
        public ModuleInfo ModuleInfo
        {
            get
            {
                if (mModuleInfo == null)
                {
                    if (!String.IsNullOrEmpty(ModuleName))
                    {
                        mModuleInfo = ModuleEntryManager.GetModuleInfo(ModuleName);
                    }
                }
                return mModuleInfo;
            }
        }


        /// <summary>
        /// The name of the class field (column) that stores the assembly name of a related class in the project's code.
        /// </summary>
        [RegisterColumn]
        public string AssemblyNameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Form definition column name
        /// </summary>
        public string FormDefinitionColumn
        {
            get
            {
                return mFormDefinitionColumn;
            }
            set
            {
                mFormDefinitionColumn = value;
            }
        }


        /// <summary>
        /// Determines how the system serializes objects of the given type from the database into XML files. Set through the properties of the <see cref="SerializationSettings"/> class.
        /// </summary>
        [RegisterProperty]
        public SerializationSettings SerializationSettings
        {
            get
            {
                return mSerializationSettings ?? (mSerializationSettings = new SerializationSettings(this));
            }
        }


        /// <summary>
        /// Determines how the object behaves within continuous integration. Set through the properties of the <see cref="ContinuousIntegrationSettings"/> class.
        /// </summary>
        [RegisterProperty]
        public ContinuousIntegrationSettings ContinuousIntegrationSettings
        {
            get
            {
                return mContinuousIntegrationSettings ?? (mContinuousIntegrationSettings = new ContinuousIntegrationSettings(this));
            }
        }


        /// <summary>
        /// Default order by clause for getting data.
        /// </summary>
        [RegisterColumn]
        public string DefaultOrderBy
        {
            get
            {
                if (mDefaultOrderBy == null)
                {
                    var orderBy = GetFirstKnownColumn(OrderColumn, DisplayNameColumn, CodeNameColumn);
                    if (orderBy == null)
                    {
                        if (IsBinding)
                        {
                            // For binding, use list of all binding columns as order
                            orderBy = SqlHelper.JoinColumnList(GetBindingColumns());
                        }
                        else
                        {
                            orderBy = GetFirstKnownColumn(IDColumn, ParentIDColumn, SiteIDColumn, GUIDColumn);
                        }
                    }

                    mDefaultOrderBy = orderBy;
                }

                return mDefaultOrderBy;
            }
            set
            {
                mDefaultOrderBy = value;
            }
        }


        /// <summary>
        /// The type of the object's InfoProvider class. Required for all object types.
        /// </summary>
        public Type ProviderType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the ObjectTypeInfo instance representing the parent object type.
        /// </summary>
        public ObjectTypeInfo ParentTypeInfo
        {
            get
            {
                // If parent type info is unknown, get the parent type info
                if ((mParentTypeInfo != null) && (mParentTypeInfo.ObjectType == VALUE_UNKNOWN))
                {
                    mParentTypeInfo = !String.IsNullOrEmpty(ParentObjectType) ? ObjectTypeManager.GetTypeInfo(ParentObjectType) : null;
                }

                return mParentTypeInfo;
            }
        }


        /// <summary>
        /// Gets an instance of the provider class set for the object type (specified by the ProviderType property).
        /// </summary>
        public IInfoProvider ProviderObject
        {
            get
            {
                return InfoProviderLoader.GetInfoProvider(OriginalObjectType);
            }
        }


        /// <summary>
        /// The primary identifier string for the object type.
        /// This identifier is used to select object types in the API, user interface components (UniGrid, UniSelector), REST calls, etc.
        /// Required for all object types.
        /// </summary>
        [RegisterColumn]
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;
            }
        }


        /// <summary>
        /// The code name assigned to the matching module class in the Kentico administration interface. Identifies the object types's definition in the database. Required for all object types.
        /// </summary>
        [RegisterColumn]
        public string ObjectClassName
        {
            get
            {
                return mObjectClassName;
            }
            set
            {
                mObjectClassName = value;
            }
        }


        /// <summary>
        /// Class structure information. If not set explicitly, it is retrieved automatically by class name.
        /// </summary>
        [RegisterColumn]
        public virtual ClassStructureInfo ClassStructureInfo
        {
            get
            {
                if (mClassStructureInfo != null)
                {
                    return mClassStructureInfo;
                }

                return ClassStructureInfo.GetClassInfo(ObjectClassName);
            }
            set
            {
                mClassStructureInfo = value;
            }
        }


        /// <summary>
        /// The object type name of the parent (as defined in the type information of the parent object type). Null for object types without a parent object.
        /// </summary>
        [RegisterColumn]
        public string ParentObjectType
        {
            get
            {
                return mParentObjectType;
            }
            set
            {
                mParentObjectType = value;
            }
        }


        /// <summary>
        /// Name of the columns (separated by semicolon) of object dependencies which should be included into the parent data (for example class data for BizForms).
        /// </summary>
        [RegisterColumn]
        public string ChildDependencyColumns
        {
            get
            {
                return mChildDependencyColumns;
            }
            set
            {
                mChildDependencyColumns = value;
            }
        }


        /// <summary>
        /// Object dependencies (list of foreign keys definition of the object with the exception of SiteID column and ParentID columns).
        /// Automatically computed from <see cref="DependsOn"/> and <see cref="Extends"/> lists of all objects within the system.
        /// </summary>
        internal List<ObjectDependency> ObjectDependenciesInternal
        {
            get
            {
                return LockHelper.Ensure(ref mObjectDependencies, GetObjectDependencies, lockObject);
            }
        }


        /// <summary>
        /// Object dependencies (list of foreign keys definition of the object with the exception of SiteID column and ParentID columns).
        /// Automatically computed from <see cref="DependsOn"/> and <see cref="Extends"/> lists of all objects within the system.
        /// </summary>
        [RegisterProperty]
        public IEnumerable<ObjectDependency> ObjectDependencies
        {
            get
            {
                return ObjectDependenciesInternal;
            }
        }


        /// <summary>
        /// Returns true if object is a composite object. Object consists of several partial objects.
        /// </summary>
        [RegisterColumn]
        public bool IsComposite
        {
            get
            {
                return (ConsistsOf != null);
            }
        }


        /// <summary>
        /// Composite object type - type of the object which is a composite object for this object type.
        /// </summary>
        [RegisterColumn]
        public string CompositeObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Defines the list of object types that are part of this composite object. If the object is not a composite object, this property is not initialized.
        /// </summary>
        [RegisterProperty]
        public virtual ICollection<string> ConsistsOf
        {
            get;
            set;
        }


        /// <summary>
        /// List of ExtraColumn instances that extend other object types by defining a reference (foreign key) pointing to this object type.
        /// Allows the system to maintain referential integrity of relationships when importing or staging objects, or perform chain deleting of objects together with referenced objects.
        /// </summary>
        /// <remarks>
        /// If possible, use the <see cref="ObjectTypeInfo.DependsOn"/> property to define the reference directly in the type info of the related object type.
        /// </remarks>
        [RegisterProperty]
        public IEnumerable<ExtraColumn> Extends
        {
            get;
            set;
        }


        /// <summary>
        /// List of ObjectDependency instances that define references (foreign keys) pointing to other objects that are directly visible.
        /// Allows the system to maintain referential integrity of relationships when importing or staging objects, or perform chain deleting of objects together with referenced objects.
        /// Do not include site or parent references that are specified by the SiteIDCOlumn and ParentIDColumn properties.
        /// </summary>
        /// <remarks>
        /// For foreign keys of objects belonging to other modules not visible by this module, define the reference externally using the <see cref="ObjectTypeInfo.Extends"/>
        /// property in the type info of the related object.
        /// </remarks>
        [RegisterProperty]
        public IEnumerable<ObjectDependency> DependsOn
        {
            get;
            set;
        }


        /// <summary>
        /// List of object types on which this object type depends on indirectly. Indirectly means there is no column with reference pointing to object of specified object type.
        /// Allows the system to maintain chain of dependencies between object types.
        /// Do not include direct dependencies like parent, site or dependencies specified by <see cref="DependsOn"/>.
        /// </summary>
        [RegisterProperty]
        public IEnumerable<string> DependsOnIndirectly
        {
            get;
            set;
        }


        /// <summary>
        /// Gets list of child object types.
        /// </summary>
        [RegisterProperty]
        public virtual List<string> ChildObjectTypes
        {
            get
            {
                return mChildObjectTypes;
            }
        }


        /// <summary>
        /// Gets list of binding object types.
        /// </summary>
        [RegisterProperty]
        public virtual List<string> BindingObjectTypes
        {
            get
            {
                return mBindingObjectTypes;
            }
        }


        /// <summary>
        /// Gets list of other binding types - binding types where this object participate but is not parent object of those binding types.
        /// </summary>
        [RegisterProperty]
        public virtual List<string> OtherBindingObjectTypes
        {
            get
            {
                return mOtherBindingObjectTypes;
            }
        }


        /// <summary>
        /// Gets the Info class of the binding class that stores relationships between the given class and sites (if such a site binding class exists).
        /// </summary>
        public BaseInfo SiteBindingObject
        {
            get
            {
                return InfoHelper.EnsureInfo(ref mSiteBindingObject, GetSiteBindingObject);
            }
        }


        /// <summary>
        /// Gets the object type name of the binding class that stores relationships between the given class and sites (if such a site binding class exists).
        /// </summary>
        [RegisterColumn]
        public string SiteBinding
        {
            get
            {
                if (mSiteBinding == VALUE_UNKNOWN)
                {
                    mSiteBinding = (SiteBindingObject != null) ? SiteBindingObject.TypeInfo.ObjectType : null;
                }

                return mSiteBinding;
            }
        }


        /// <summary>
        /// Empty read-only instance of the category object (if this type has a category object)
        /// For example settings key will have a SettingsCategoryInfo object here.
        /// </summary>
        public BaseInfo CategoryObject
        {
            get
            {
                return EnsureCategoryInfo();
            }
        }


        /// <summary>
        /// Category ID column name.
        /// </summary>
        public string CategoryIDColumn
        {
            get
            {
                if (mCategoryIDColumn == null)
                {
                    EnsureCategoryInfo();
                }

                return mCategoryIDColumn;
            }
        }


        /// <summary>
        /// If true, timestamp of the object is updated when saved.
        /// </summary>
        [RegisterColumn]
        public bool UpdateTimeStamp
        {
            get
            {
                return (SettingsHelper.AllowUpdateTimeStamps && mUpdateTimeStamp);
            }
            set
            {
                mUpdateTimeStamp = value;
            }
        }


        /// <summary>
        /// Determines whether the system logs integration bus synchronization tasks for objects of the type. True by default.
        /// </summary>
        [RegisterColumn]
        public bool LogIntegration
        {
            get
            {
                return mLogIntegration;
            }
            set
            {
                mLogIntegration = value;
            }
        }


        /// <summary>
        /// Indicates if parent is allowed to be touched, if exists. Default is true.
        /// </summary>
        [RegisterColumn]
        public bool AllowTouchParent
        {
            get
            {
                return mAllowTouchParent;
            }
            set
            {
                mAllowTouchParent = value;
            }
        }


        /// <summary>
        /// Determines whether the object type supports object versioning. The default value is false.
        /// </summary>
        [RegisterColumn]
        public bool SupportsVersioning
        {
            get;
            set;
        }


        /// <summary>
        /// Determines if objects of the given type are included in the version data of parent objects. True by default.
        /// </summary>
        public bool IncludeToVersionParentDataSet
        {
            get
            {
                return mIncludeToVersionParentDataSet;
            }
            set
            {
                mIncludeToVersionParentDataSet = value;
            }
        }


        /// <summary>
        /// Determines whether the object type supports object locking (check out and check in). The default value is false.
        /// </summary>
        [RegisterColumn]
        public bool SupportsLocking
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the object type has fields that can contain macro expressions in their values. Determines if objects are processed when resigning macros in the system.
        /// True by default. You can set the value to False to optimize the performance of the resigning process (during manual resigning and upgrades).
        /// Does not affect the resolving of macros.
        /// </summary>
        [RegisterColumn]
        public bool ContainsMacros
        {
            get
            {
                if (mContainsMacros != null)
                {
                    return mContainsMacros.Value;
                }

                return !IsBinding;
            }
            set
            {
                mContainsMacros = value;
            }
        }


        /// <summary>
        /// Indicates whether the system allows cloning for objects of the given type. True by default.
        /// </summary>
        [RegisterColumn]
        public bool SupportsCloning
        {
            get
            {
                return mSupportsCloning;
            }
            set
            {
                mSupportsCloning = value;
            }
        }


        /// <summary>
        /// Indicates whether objects of the type can be cloned to a different site than the site of the original object. True by default.
        /// Applies to object types with a specified SiteIDColumn.
        /// </summary>
        [RegisterColumn]
        public bool SupportsCloneToOtherSite
        {
            get
            {
                return mSupportsCloneToOtherSite;
            }
            set
            {
                mSupportsCloneToOtherSite = value;
            }
        }


        /// <summary>
        /// Indicates whether the system logs events into the Event log when objects of the type are updated. False by default.
        /// </summary>
        [RegisterColumn]
        public bool LogEvents
        {
            get
            {
                if (!mLogObjectEventsAppSettings.HasValue)
                {
                    mLogObjectEventsAppSettings = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogObjectEvents:" + ObjectType], true);
                }
                return mLogEvents && mLogObjectEventsAppSettings.Value;
            }
            set
            {
                mLogEvents = value;
            }
        }


        /// <summary>
        /// Indicates whether the corresponding dummy cache keys (dependencies) are touched when an object of the type is modified.
        /// This causes the cache to delete all items that depend on the given dummy keys. The default value is false.
        /// </summary>
        [RegisterColumn]
        public bool TouchCacheDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the object is required when imported / synchronized. If false, the object can be skipped when some error occurs. Default is true.
        /// </summary>
        [RegisterColumn]
        public bool RequiredObject
        {
            get
            {
                return mRequiredObject;
            }
            set
            {
                mRequiredObject = value;
            }
        }


        /// <summary>
        /// If true, the objects can have meta files. By default no metafiles to simplify general processes
        /// </summary>
        [RegisterColumn]
        public bool HasMetaFiles
        {
            get
            {
                return !IsBinding && mHasMetaFiles;
            }
            set
            {
                mHasMetaFiles = value;
            }
        }


        /// <summary>
        /// If true, the processes can be run on the objects. By default no processes to simplify general processes.
        /// </summary>
        [RegisterColumn]
        public bool HasProcesses
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the object has externally stored columns. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool HasExternalColumns
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the object can be targeted by triggers. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool IsTriggerTarget
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the scheduled tasks can be run on the objects. By default no scheduled tasks to simplify general processes.
        /// </summary>
        [RegisterColumn]
        public bool HasScheduledTasks
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true for site-related object types.
        /// is true if the SiteIDColumn or GroupIDColumn property is set in the type information, or if the object type has a site-related parent.
        /// </summary>
        [RegisterColumn]
        public bool IsSiteObject
        {
            get
            {
                if (SiteIDColumn != COLUMN_NAME_UNKNOWN)
                {
                    // Object with SiteID column is site object
                    return true;
                }
                if (GroupIDColumn != COLUMN_NAME_UNKNOWN)
                {
                    // Group object is always site object since Group is a site object
                    return true;
                }
                if (!String.IsNullOrEmpty(ParentObjectType) && !ParentObjectType.EqualsCSafe(ObjectType))
                {
                    // Check the parent
                    var parent = ModuleManager.GetReadOnlyObject(ParentObjectType);
                    if (parent != null)
                    {
                        return parent.TypeInfo.IsSiteObject;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates whether the object type represents a relationship between two or more object types. Setting to true forces the object type to behave as a binding.
        /// </summary>
        [RegisterColumn]
        public bool IsBinding
        {
            get
            {
                if (mIsBinding != null)
                {
                    return mIsBinding.Value;
                }

                return (IDColumn == COLUMN_NAME_UNKNOWN);
            }
            set
            {
                mIsBinding = value;
            }
        }


        /// <summary>
        /// Indicates whether the object can use upsert. If not set, the value is inferred from <see cref="IsBinding"/> (only binding objects can use upsert by default).
        /// Setting this property to any value overrides the default behavior.
        /// </summary>
        /// <remarks>
        /// When set to true, objects with ID value set are updated as usual. If their ID is not set, then upsert is used.
        /// </remarks>
        [RegisterColumn]
        public bool UseUpsert
        {
            get
            {
                if (mUseUpsert != null)
                {
                    return mUseUpsert.Value;
                }

                return IsBinding;
            }
            set
            {
                mUseUpsert = value;
            }
        }


        /// <summary>
        /// Indicates whether the object type represents a category for organizing other objects in a tree hierarchy (for example web part categories).
        /// The default value is false.
        /// </summary>
        [RegisterColumn]
        public bool IsCategory
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the object is always deleted using API methods in the remove dependencies phase, not the generated queries. Use for objects where additional actions are performed during deletion. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool DeleteObjectWithAPI
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the system prevents objects of the type from being deleted if they are the target of a required reference for at least one other object in the system.
        /// Applies to object types that are the target of a Required type reference from another object type.
        /// The CheckDependencies procedure is first called within the remove dependencies procedure and if there are some, the remove dependencies procedure
        /// throws an exception and does not continue.
        /// </summary>
        [RegisterColumn]
        public bool CheckDependenciesOnDelete
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the system checks read permissions for the given module when accessing objects of the type in the macro engine. True by default.
        /// The permissions are checked for the user who saved the macro, not the user viewing the result.
        /// </summary>
        [RegisterColumn]
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Returns true if the object is site binding.
        /// </summary>
        [RegisterColumn]
        public bool IsSiteBinding
        {
            get
            {
                if (mIsSiteBinding != null)
                {
                    return mIsSiteBinding.Value;
                }

                return IsBinding && (SiteIDColumn != COLUMN_NAME_UNKNOWN) && !HasBindingTypeDependencies();
            }
            set
            {
                mIsSiteBinding = value;
            }
        }


        /// <summary>
        /// Identifies binding classes that connect two objects of the same type = the binding's parent type is the same as the object dependency type
        /// </summary>
        [RegisterColumn]
        public bool IsSelfBinding
        {
            get
            {
                // Cannot be self binding if is not binding or object dependencies are not defined
                if (!IsBinding || (ObjectDependencies == null))
                {
                    return false;
                }

                // Check dependencies
                string parentType = ParentObjectType;
                foreach (var dep in ObjectDependencies)
                {
                    // Skip dynamic dependencies
                    if (dep.HasDynamicObjectType())
                    {
                        continue;
                    }

                    // If the dependency is same as parent, self binding
                    string type = dep.DependencyObjectType;
                    if (type.Equals(parentType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates whether the system allows users to export the object type's data into files of various formats from listings (UniGrid).
        /// True by default for all objects except for binding objects.
        /// </summary>
        [RegisterColumn]
        public bool AllowDataExport
        {
            get
            {
                if (mAllowDataExport != null)
                {
                    return mAllowDataExport.Value;
                }
                else
                {
                    return !IsBinding;
                }
            }
            set
            {
                mAllowDataExport = value;
            }
        }


        /// <summary>
        /// If true, the object is automatically invalidated upon it's change. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool SupportsInvalidation
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the system allows both site-related objects and global objects whose value in the site ID column is null (for example polls).
        /// Applies to object types with a specified SiteIDColumn. False by default. Must be true if you wish to use export or staging for global objects of the given type.
        /// </summary>
        [RegisterColumn]
        public bool SupportsGlobalObjects
        {
            get;
            set;
        }


        /// <summary>
        /// Get/sets info's feature for license check
        /// </summary>
        [RegisterColumn]
        public FeatureEnum Feature
        {
            get;
            set;
        }


        /// <summary>
        /// Table of invalidated objects [ID] -> [Invalidated when].
        /// </summary>
        private SafeDictionary<int, DateTime> InvalidatedObjects
        {
            get
            {
                return mInvalidatedObjects ?? (mInvalidatedObjects = new SafeDictionary<int, DateTime>());
            }
        }


        /// <summary>
        /// Table of invalidated direct child objects [ParentID] -> [Invalidated when].
        /// </summary>
        private SafeDictionary<int, DateTime> InvalidatedChildren
        {
            get
            {
                return mInvalidatedChildren ?? (mInvalidatedChildren = new SafeDictionary<int, DateTime>());
            }
        }


        /// <summary>
        /// Gets the number of registered isntances.
        /// </summary>
        public int InstanceCount
        {
            get
            {
                return mInstanceCount;
            }
        }


        /// <summary>
        /// List of the foreign keys columns.
        /// </summary>
        [RegisterProperty]
        private IEnumerable<string> ForeignKeys
        {
            get
            {
                return mForeignKeys ?? (mForeignKeys = InitializeForeignKeys());
            }
        }


        /// <summary>
        /// Determines whether the system stores deleted objects of the type in the recycle bin.
        /// By default the value is true for all objects with allowed synchronization or versioning except for binding objects.
        /// </summary>
        [RegisterColumn]
        public bool AllowRestore
        {
            get
            {
                if (mAllowRestore == null)
                {
                    return !IsBinding && (SupportsVersioning || SynchronizationSettings.LogSynchronization != SynchronizationTypeEnum.None);
                }
                else
                {
                    return mAllowRestore.Value;
                }
            }
            set
            {
                mAllowRestore = value;
            }
        }


        /// <summary>
        /// Indicates if provider supports methods customization
        /// </summary>
        [RegisterColumn]
        public bool ProviderIsCustomizable
        {
            get
            {
                if (mProviderIsCustomizable == null)
                {
                    try
                    {
                        mProviderIsCustomizable = (ProviderObject != null);
                    }
                    catch
                    {
                        mProviderIsCustomizable = false;
                    }
                }

                return mProviderIsCustomizable.Value;
            }
        }


        /// <summary>
        /// Indicates if the object type info is used as read-only data source for listing controls such as grids or selectors.
        /// </summary>
        [RegisterColumn]
        public bool IsListingObjectTypeInfo
        {
            get;
            protected set;
        }


        /// <summary>
        /// Original object type of virtual object types. For normal object type it's null.
        /// </summary>
        [RegisterProperty]
        public ObjectTypeInfo OriginalTypeInfo
        {
            get
            {
                return mOriginalTypeInfo;
            }
            set
            {
                if (mEvents != null)
                {
                    throw new NotSupportedException("[ObjectTypeInfo.OriginalTypeInfo]: Unable to set original type info at this point, type info events were already initialized.");
                }

                mOriginalTypeInfo = value;
            }
        }


        /// <summary>
        /// Original object type of virtual object types. For normal object type it's same as object type.
        /// </summary>
        [RegisterColumn]
        public string OriginalObjectType
        {
            get
            {
                return mOriginalObjectType ?? (mOriginalObjectType = GetOriginalObjectType());
            }
        }


        /// <summary>
        /// Default where condition restricting the data of this particular object type.
        /// </summary>
        [RegisterColumn]
        public string WhereCondition
        {
            get
            {
                return mWhereCondition ?? (mWhereCondition = GetTypeWhereCondition().ToString(true));
            }
        }


        /// <summary>
        /// If true, the object of this type has got some automatic properties
        /// </summary>
        [RegisterColumn]
        public bool HasAutomaticProperties
        {
            get
            {
                if (mHasAutomaticProperties != null)
                {
                    return mHasAutomaticProperties.Value;
                }

                return (ObjectDependencies != null);
            }
            set
            {
                mHasAutomaticProperties = value;
            }
        }


        /// <summary>
        /// Indicates whether listing pages (UniGrids) that display the object type's data remember the listing state for individual users
        /// The listing state includes filtering options, page number, page size and item order.
        /// </summary>
        /// <remarks>
        /// This property can be overridden by individual UniGrids.
        /// </remarks>
        [RegisterColumn]
        public bool? RememberUniGridState
        {
            get
            {
                return mRememberUniGridState;
            }
            set
            {
                mRememberUniGridState = value;
            }
        }


        /// <summary>
        /// Maximum length of the object code name.
        /// </summary>
        [RegisterColumn]
        public int MaxCodeNameLength
        {
            get;
            set;
        }


        /// <summary>
        /// Default data exported within installation. Affects default data and web template data export.
        /// </summary>
        /// <remarks>
        /// This property is for internal use only and should not be used in custom code.
        /// </remarks>
        //[RegisterProperty]
        public DefaultDataSettings DefaultData
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if object is stored in DB or only in memory.
        ///
        /// If true, object is stored in memory and cannot be queried by SQL. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool IsVirtualObject
        {
            get;
            set;
        }


        /// <summary>
        /// Intended for objects that have a hierarchical tree structure. Sets the name of the class field (column) that stores the path of objects in the hierarchy structure.
        /// You can also set the path column separately for paths built out of objects names or object IDs through the ObjectNamePathColumn and ObjectIDPathColumn properties.
        /// </summary>
        [RegisterColumn]
        public string ObjectPathColumn
        {
            get
            {
                if (String.IsNullOrEmpty(mObjectPathColumn))
                {
                    if (ObjectIDPathColumn != COLUMN_NAME_UNKNOWN)
                    {
                        return ObjectIDPathColumn;
                    }

                    return ObjectNamePathColumn;
                }

                return mObjectPathColumn;
            }
            set
            {
                mObjectPathColumn = value;
            }
        }


        /// <summary>
        /// Intended for objects that have a hierarchical tree structure.
        /// Sets the name of the class field (column) that stores the path of objects in the hierarchy structure, consisting of object IDs.
        /// </summary>
        [RegisterColumn]
        public string ObjectIDPathColumn
        {
            get
            {
                if (String.IsNullOrEmpty(mObjectIDPathColumn))
                {
                    return COLUMN_NAME_UNKNOWN;
                }

                return mObjectIDPathColumn;
            }
            set
            {
                mObjectIDPathColumn = value;
            }
        }


        /// <summary>
        /// Intended for objects that have a hierarchical tree structure.
        /// Sets the name of the class field (column) that stores the path of objects in the hierarchy structure, consisting of object names.
        /// </summary>
        [RegisterColumn]
        public string ObjectNamePathColumn
        {
            get
            {
                if (String.IsNullOrEmpty(mObjectNamePathColumn))
                {
                    return COLUMN_NAME_UNKNOWN;
                }

                return mObjectNamePathColumn;
            }
            set
            {
                mObjectNamePathColumn = value;
            }
        }


        /// <summary>
        /// Intended for objects that have a hierarchical tree structure. Sets the name of the class field (column) that stores the depth of objects in the hierarchy structure.
        /// </summary>
        [RegisterColumn]
        public string ObjectLevelColumn
        {
            get
            {
                if (String.IsNullOrEmpty(mObjectLevelColumn))
                {
                    return COLUMN_NAME_UNKNOWN;
                }

                return mObjectLevelColumn;
            }
            set
            {
                mObjectLevelColumn = value;
            }
        }


        /// <summary>
        /// If set (= not null), then the automatic process of registering the object type in the ChildObjectTypes list is bypassed and the registration is
        /// forced to the types specified in this list.
        /// Only use if you need to register multiple parent types (if the parent has multiple type info definitions).
        /// </summary>
        public List<string> RegisterAsChildToObjectTypes
        {
            get;
            set;
        }


        /// <summary>
        /// If set (= not null), than the automatic process of registering the object type in the BindingObjectTypes list is bypassed and the registration is
        /// forced to the specified types in this list.
        /// </summary>
        public List<string> RegisterAsBindingToObjectTypes
        {
            get;
            set;
        }


        /// <summary>
        /// If set (= not null), than the automatic process of registering the object type in the OtherBindingObjectTypes list is bypassed and the registration is
        /// forced to the specified types in this list.
        /// </summary>
        public List<string> RegisterAsOtherBindingToObjectTypes
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies a list of class fields (columns) that the system excludes from the macro engine, data retrieved by the REST service and event logging.
        /// Can be used to protect fields with sensitive values, for example passwords.
        /// </summary>
        [RegisterProperty]
        public List<string> SensitiveColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if object can be searched. Default is true for main objects.
        /// </summary>
        [RegisterColumn]
        public bool SupportsSearch
        {
            get
            {
                if (mSupportsSearch != null)
                {
                    return mSupportsSearch.Value;
                }

                return !IsBinding;
            }
            set
            {
                mSupportsSearch = value;
            }
        }


        /// <summary>
        /// List of nested info object types that contain the data internally
        /// </summary>
        public List<string> NestedInfoTypes
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object can have object settings. Default is false.
        /// </summary>
        [RegisterColumn]
        public bool HasObjectSettings
        {
            get
            {
                if (mHasObjectSettings == null)
                {
                    mHasObjectSettings = SupportsVersioning;
                }

                return mHasObjectSettings.Value;
            }
            set
            {
                mHasObjectSettings = value;
            }
        }


        /// <summary>
        /// Specifies object types somehow dependent on the current object type (as parent, site, group or via foreign key). Skips inherited object types.
        /// </summary>
        internal HashSet<string> DependentObjectTypes
        {
            get
            {
                return mDependentObjectTypes ?? (mDependentObjectTypes = LoadDependentObjectTypes());
            }
            set
            {
                mDependentObjectTypes = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="providerType">The type of the object's InfoProvider class. Required for all object types.</param>
        /// <param name="objectType">The primary identifier string for the object type. Required for all object types.</param>
        /// <param name="objectClassName">The code name assigned to the matching module class in the administration interface. Required for all object types.</param>
        /// <param name="idColumn">The name of the class field (column) that stores the IDs of objects (primary key). Can be null in rare cases, for example binding objects without an identity column.</param>
        /// <param name="timeStampColumn">The name of the class field (column) that stores the last modification date for objects.</param>
        /// <param name="guidColumn">The name of the class field (column) that stores the GUID identifiers of objects.</param>
        /// <param name="codeNameColumn">The name of the class field (column) that stores the unique text identifiers of objects. Can be null for object types without a dedicated code name column.</param>
        /// <param name="displayNameColumn">The name of the class field (column) that stores the names of objects used in the administration interface and on the live site.</param>
        /// <param name="binaryColumn">The name of the class field (column) that stores binary data for objects.</param>
        /// <param name="siteIDColumn">The name of the class field (column) that stores site IDs for site-related objects. Only use the site ID column if the object does not have a separate binding object type for the site relationship.</param>
        /// <param name="parentIDColumn">The name of the class field (column) that stores the IDs of parent objects. Null for object types without a parent object.</param>
        /// <param name="parentObjectType">The object type name of the parent. Null for object types without a parent object.</param>
        public ObjectTypeInfo(Type providerType, string objectType, string objectClassName, string idColumn, string timeStampColumn, string guidColumn, string codeNameColumn, string displayNameColumn, string binaryColumn, string siteIDColumn, string parentIDColumn, string parentObjectType)
        {
            // Provider type
            ProviderType = providerType;

            // Base type
            if (objectType != null)
            {
                mObjectType = objectType;
            }
            if (objectClassName != null)
            {
                mObjectClassName = objectClassName;
            }

            // Column names
            if (idColumn != null)
            {
                mIDColumn = idColumn;
            }
            if (timeStampColumn != null)
            {
                mTimeStampColumn = timeStampColumn;
            }
            if (guidColumn != null)
            {
                mGUIDColumn = guidColumn;
            }
            if (codeNameColumn != null)
            {
                mCodeNameColumn = codeNameColumn;
            }
            if (displayNameColumn != null)
            {
                mDisplayNameColumn = displayNameColumn;
            }
            if (binaryColumn != null)
            {
                mBinaryColumn = binaryColumn;
            }
            if (siteIDColumn != null)
            {
                mSiteIDColumn = siteIDColumn;
            }
            if (parentIDColumn != null)
            {
                mParentIDColumn = parentIDColumn;
            }

            // Depending types
            if (parentObjectType != null)
            {
                mParentObjectType = parentObjectType;

                SynchronizationSettings.LogSynchronization = SynchronizationTypeEnum.TouchParent;

                SynchronizationSettings.IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete;

                ImportExportSettings.IncludeToExportParentDataSet = IncludeToParentEnum.Complete;
            }

            // Set required object
            mRequiredObject = mParentIDColumn.ToLowerCSafe() == COLUMN_NAME_UNKNOWN.ToLowerCSafe();

            DependsOnIndirectly = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a new instance of listing object type info
        /// </summary>
        /// <remarks>
        /// Listing object type info is used as read-only data source for listing controls such as grids or selectors.
        /// </remarks>
        /// <param name="objectType">Listing object type</param>
        /// <param name="sourceInfo">Source object type info</param>
        public static ObjectTypeInfo CreateListingObjectTypeInfo(string objectType, ObjectTypeInfo sourceInfo)
        {
            var listInfo = new ObjectTypeInfo(sourceInfo.ProviderType, objectType, sourceInfo.ObjectClassName, sourceInfo.IDColumn, sourceInfo.TimeStampColumn, sourceInfo.GUIDColumn, sourceInfo.CodeNameColumn, sourceInfo.DisplayNameColumn, sourceInfo.BinaryColumn, sourceInfo.SiteIDColumn, sourceInfo.ParentIDColumn, sourceInfo.ParentObjectType);

            InitListingObjectTypeInfo(sourceInfo, listInfo);

            return listInfo;
        }


        /// <summary>
        /// Initializes given <paramref name="listInfo"/> type info as listing type with values from <paramref name="sourceInfo"/>.
        /// </summary>
        /// <param name="sourceInfo">Source type info</param>
        /// <param name="listInfo">Listing type info</param>
        protected static void InitListingObjectTypeInfo(ObjectTypeInfo sourceInfo, ObjectTypeInfo listInfo)
        {
            listInfo.IsListingObjectTypeInfo = true;
            listInfo.OriginalTypeInfo = sourceInfo;

            listInfo.TouchCacheDependencies = sourceInfo.TouchCacheDependencies;

            listInfo.SupportsVersioning = sourceInfo.SupportsVersioning;
            listInfo.SupportsLocking = sourceInfo.SupportsLocking;
            listInfo.SupportsCloning = sourceInfo.SupportsCloning;

            listInfo.mAllowRestore = sourceInfo.mAllowRestore;
            listInfo.mLogEvents = sourceInfo.mLogEvents;

            listInfo.ContainsMacros = sourceInfo.ContainsMacros;
            listInfo.ModuleName = sourceInfo.ModuleName;

            var listInfoSettings = listInfo.ImportExportSettings;
            var sourceInfoSettings = sourceInfo.ImportExportSettings;

            listInfoSettings.IncludeToExportParentDataSet = sourceInfoSettings.IncludeToExportParentDataSet;
            listInfoSettings.LogExport = sourceInfoSettings.LogExport;
            listInfoSettings.AllowSingleExport = sourceInfoSettings.AllowSingleExport;

            sourceInfo.SynchronizationSettings.CopyPropertiesTo(listInfo.SynchronizationSettings);
        }


        /// <summary>
        /// Returns list of <see cref="ObjectDependency"/> of current <see cref="ObjectTypeInfo"/> that is combination
        /// of <see cref="DependsOn"/>, <see cref="Extends"/> and the other object types that extends current <see cref="ObjectTypeInfo"/>.
        /// </summary>
        private List<ObjectDependency> GetObjectDependencies()
        {
            var list = new List<ObjectDependency>();

            if (DependsOn != null)
            {
                list.AddRange(DependsOn);
            }

            // Touching object types in ObjectTypeManager when not registered causes infinite loop
            if (ObjectTypeManager.ObjectTypesRegistered)
            {
                foreach (var objectTypeInfo in GetExtendedDependencyObjectTypes())
                {
                    list.AddRange(objectTypeInfo.Extends.Where(ec => ec.ExtendedObjectType.EqualsCSafe(ObjectType, true))
                        .Select(ec => new ObjectDependency(ec.ColumnName, objectTypeInfo.ObjectType, true, ec.DependencyType)));
                }
            }

            return list;
        }


        /// <summary>
        /// Returns all object types with extended dependency on current object type
        /// </summary>
        private IEnumerable<ObjectTypeInfo> GetExtendedDependencyObjectTypes()
        {
            return ObjectTypeManager.ExistingObjectTypes
                                    .Select(objectType => ObjectTypeManager.GetTypeInfo(objectType))
                                    .Where(typeInfo => typeInfo != null && typeInfo.Extends != null && !typeInfo.IsListingObjectTypeInfo && typeInfo.ObjectType != ObjectType);
        }


        /// <summary>
        /// Returns true if this object type is the same or has the same original type.
        /// </summary>
        /// <remarks>
        /// Object type is considered related if either <see cref="ObjectType"/>, <see cref="OriginalObjectType"/> or <see cref="RelatedTypeInfos"/> equal to the given object type.
        /// </remarks>
        /// <param name="objType">ObjectType to compare with</param>
        internal bool IsRelatedTo(string objType)
        {
            return (ObjectType == objType) ||
                   (OriginalObjectType == objType) ||
                   ((RelatedTypeInfos != null) && RelatedTypeInfos.Contains(objType));
        }


        /// <summary>
        /// Returns true if this object type is a composite that consists of given type (<see cref="ConsistsOf"/>).
        /// </summary>
        /// <param name="objType">Component type to check</param>
        internal bool HostsComponentType(string objType)
        {
            return IsComposite && ConsistsOf.Contains(objType, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Returns true is this object type is related to (<see cref="IsRelatedTo"/>) or hosts (<see cref="HostsComponentType"/>) the given type.
        /// </summary>
        /// <param name="objType">ObjectType to compare with</param>
        internal bool RepresentsType(string objType)
        {
            return IsRelatedTo(objType) || HostsComponentType(objType);
        }


        /// <summary>
        /// Gets the original object type of virtual object types. For normal object type it's same as object type.
        /// </summary>
        private string GetOriginalObjectType()
        {
            return (OriginalTypeInfo != null) ? OriginalTypeInfo.OriginalObjectType : ObjectType;
        }


        /// <summary>
        /// Returns all direct dependency relations of current object.
        /// </summary>
        /// <remarks>
        /// Related types of dependent types are not returned, only the primary dependent type.
        /// Dependent object types consists of types defined in <see cref="ObjectTypeInfo.DependsOn"/> property, and child types.
        /// In case that current object type represents site or group, then also site or group objects are included.
        /// </remarks>
        internal IEnumerable<DependencyReference> GetDependencyReferences()
        {
            var isSite = RepresentsType(PredefinedObjectType.SITE);
            var isGroup = RepresentsType(PredefinedObjectType.GROUP);

            // Process all dependent object types
            foreach (var objType in DependentObjectTypes)
            {
                // Skip processing of my own object type if it has an IDPath column (it means it's hierarchical object and needs to be processed differently)
                if ((objType == ObjectType) && ObjectPathColumn != COLUMN_NAME_UNKNOWN)
                {
                    continue;
                }

                var obj = ModuleManager.GetReadOnlyObject(objType, true);
                var objTypeInfo = obj.TypeInfo;
                var processedColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                // Check parent column
                var parentIDColumn = objTypeInfo.ParentIDColumn;
                if (RepresentsType(objTypeInfo.ParentObjectType) && !processedColumns.Contains(parentIDColumn))
                {
                    processedColumns.Add(parentIDColumn);
                    yield return new DependencyReference(objType, objTypeInfo.ParentObjectType, parentIDColumn, ObjectDependencyEnum.Required);
                }

                // Check site ID column
                var siteIDColumn = objTypeInfo.SiteIDColumn;
                if (isSite && (objTypeInfo.SiteIDColumn != COLUMN_NAME_UNKNOWN) && !processedColumns.Contains(siteIDColumn))
                {
                    processedColumns.Add(siteIDColumn);
                    yield return new DependencyReference(objType, PredefinedObjectType.SITE, siteIDColumn, ObjectDependencyEnum.Required);
                }

                // Check group ID column
                var groupIDColumn = objTypeInfo.GroupIDColumn;
                if (isGroup && (objTypeInfo.GroupIDColumn != COLUMN_NAME_UNKNOWN) && !processedColumns.Contains(groupIDColumn))
                {
                    processedColumns.Add(groupIDColumn);
                    yield return new DependencyReference(objType, PredefinedObjectType.GROUP, groupIDColumn, ObjectDependencyEnum.Required);
                }

                var dependencies = objTypeInfo.ObjectDependencies;

                // Check dependencies
                foreach (var dep in dependencies)
                {
                    var depObjType = obj.GetDependencyObjectType(dep);

                    // Skip the group dependencies, group objects are handled separately with GroupIDColumn and their own typeinfo
                    if ((depObjType == PredefinedObjectType.GROUP) || !RepresentsType(depObjType))
                    {
                        continue;
                    }

                    var depColumn = dep.DependencyColumn;
                    var required = dep.DependencyType;

                    if (!processedColumns.Contains(depColumn))
                    {
                        processedColumns.Add(depColumn);

                        yield return new DependencyReference(objType, depObjType, depColumn, required, isDependencyByExtension: dep.IsDependencyByExtension);
                    }
                }
            }
        }


        /// <summary>
        /// Returns all dynamic dependency relations of current object.
        /// </summary>
        /// <remarks>
        /// All dynamic dependencies of all objects in the system are returned with following exceptions:
        /// Metafiles dependency is returned only when <see cref="HasMetaFiles"/> is set to true.
        /// Automation workflow trigger dependency is returned only when <see cref="IsTriggerTarget"/> is set to true.
        /// </remarks>
        internal IEnumerable<DynamicDependencyReference> GetDynamicDependencyReferences()
        {
            if (IDColumn == COLUMN_NAME_UNKNOWN)
            {
                return Enumerable.Empty<DynamicDependencyReference>();
            }

            var relevantDynamicTypes = ObjectTypeManager.ObjectTypesWithDynamicDependency.Where(i =>
                                                               i != MetaFileInfo.OBJECT_TYPE && i != PredefinedObjectType.AUTOMATIONWORKFLOWTRIGGER
                                                            || i == MetaFileInfo.OBJECT_TYPE && HasMetaFiles
                                                            || i == PredefinedObjectType.AUTOMATIONWORKFLOWTRIGGER && IsTriggerTarget);

            return from dynamicDependentObjType in relevantDynamicTypes
                   let dependentTypeInfo = ObjectTypeManager.GetTypeInfo(dynamicDependentObjType, true)
                   from dependency in dependentTypeInfo.ObjectDependencies.Where(x => x.HasDynamicObjectType())
                   select new DynamicDependencyReference(dynamicDependentObjType, dependency.ObjectTypeColumn, dependency.DependencyColumn, dependency.DependencyType);
        }


        /// <summary>
        /// Gets the object types somehow dependent on the current object type (as parent, site, group or via foreign key). Skips inherited object types.
        /// </summary>
        private HashSet<string> LoadDependentObjectTypes()
        {
            bool isSite = RepresentsType(PredefinedObjectType.SITE);
            bool isGroup = RepresentsType(PredefinedObjectType.GROUP);

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Process all object types
            foreach (var objectTypeInfo in ObjectTypeManager.AllObjectTypes
                                                         .Select(objectType => ObjectTypeManager.GetTypeInfo(objectType))
                                                         .Where(typeInfo => (typeInfo != null) && !typeInfo.IsListingObjectTypeInfo))
            {
                // Check parent column
                if (RepresentsType(objectTypeInfo.ParentObjectType))
                {
                    AddDependentObjectType(result, objectTypeInfo);
                    continue;
                }

                // Check site ID column
                if (isSite && (objectTypeInfo.SiteIDColumn != COLUMN_NAME_UNKNOWN))
                {
                    AddDependentObjectType(result, objectTypeInfo);
                    continue;
                }

                // Check group ID column
                if (isGroup && (objectTypeInfo.GroupIDColumn != COLUMN_NAME_UNKNOWN))
                {
                    AddDependentObjectType(result, objectTypeInfo);
                    continue;
                }

                // Check dependencies
                var dependencies = objectTypeInfo.ObjectDependencies
                                              .Where(dep => !dep.HasDynamicObjectType())
                                              .Select(dep => dep.DependencyObjectType)
                                              // Skip the group dependencies, group objects are handled separately with GroupIDColumn and their own typeinfo
                                              .Where(depObjType => (depObjType != PredefinedObjectType.GROUP))
                                              .Where(RepresentsType);

                if (dependencies.Any())
                {
                    AddDependentObjectType(result, objectTypeInfo);
                }
            }

            return result;
        }


        /// <summary>
        /// When component (partial) object type claims that he depends on current type, then composite object (complete object) is dependent on the type.
        /// When current object is part of the same composite then preserve object type of dependent type.
        /// Component object types can have relations that don't exists between instances of composite objects.
        /// </summary>
        private void AddDependentObjectType(ISet<string> types, ObjectTypeInfo typeInfo)
        {
            if ((typeInfo.CompositeObjectType != null) && (typeInfo.CompositeObjectType != CompositeObjectType))
            {
                types.Add(typeInfo.CompositeObjectType);
            }
            else
            {
                types.Add(typeInfo.ObjectType);
            }
        }


        /// <summary>
        /// Loads the related types for all type infos [objectType] => [List of related object types]
        /// </summary>
        private static RelatedTypesDictionary LoadRelatedTypes()
        {
            var result = new RelatedTypesDictionary();

            foreach (var objType in ObjectTypeManager.AllObjectTypes)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(objType);

                if ((typeInfo != null) && (typeInfo.OriginalTypeInfo != null))
                {
                    var originalObjectType = typeInfo.OriginalTypeInfo.ObjectType;

                    // Add binding from original
                    var relatedOriginal = result[originalObjectType] ?? (result[originalObjectType] = new List<string>());

                    relatedOriginal.Add(objType);

                    // Add binding to original
                    var related = result[objType] ?? (result[objType] = new List<string>());

                    related.Add(originalObjectType);
                }
            }

            return result;
        }


        /// <summary>
        /// Initializes the category info.
        /// </summary>
        private BaseInfo EnsureCategoryInfo()
        {
            return InfoHelper.EnsureInfo(ref mObjectCategory, GetCategoryInfo);
        }


        /// <summary>
        /// Gets the category info for current object.
        /// </summary>
        private BaseInfo GetCategoryInfo()
        {
            // Find category object in object dependencies
            var dependencies = ObjectDependencies;
            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                {
                    // Check the dependency for being category
                    string dependencyColumn = dep.DependencyColumn;
                    string dependencyType = dep.DependencyObjectType;

                    if (!String.IsNullOrEmpty(dependencyType))
                    {
                        var dependencyObj = ModuleManager.GetReadOnlyObject(dependencyType);
                        if ((dependencyObj != null) && dependencyObj.TypeInfo.IsCategory)
                        {
                            mCategoryIDColumn = dependencyColumn;
                            return dependencyObj;
                        }
                    }
                }
            }

            // No category found
            mCategoryIDColumn = COLUMN_NAME_UNKNOWN;
            return null;
        }


        /// <summary>
        /// Gets the site binding object
        /// </summary>
        private BaseInfo GetSiteBindingObject()
        {
            BaseInfo siteBindingObject = null;

            // Search the binding types
            foreach (string bindingType in BindingObjectTypes)
            {
                BaseInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                if ((bindingObj != null) && bindingObj.TypeInfo.IsSiteBinding)
                {
                    // If site binding end the evaluation
                    siteBindingObject = bindingObj;
                    break;
                }
            }
            return siteBindingObject;
        }


        /// <summary>
        /// Returns true if the object has at least one Object dependency of type Binding.
        /// </summary>
        private bool HasBindingTypeDependencies()
        {
            if (ObjectDependencies != null)
            {
                foreach (var dep in ObjectDependencies)
                {
                    if (dep.DependencyType == ObjectDependencyEnum.Binding)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Copies the event's hooks from current ObjectTypeInfo to specified one.
        /// </summary>
        /// <param name="info">Target.</param>
        /// <remarks>Do not use in custom code!</remarks>
        protected void CopyEventsTo(ObjectTypeInfo info)
        {
            info.mEvents = Events;
            info.OnLoadRelatedData = OnLoadRelatedData;
            info.OnLogObjectChange = OnLogObjectChange;
        }


        /// <summary>
        /// Gets the first known column from the given list of columns
        /// </summary>
        /// <param name="columns">Columns to select from</param>
        public static string GetFirstKnownColumn(params string[] columns)
        {
            foreach (var column in columns)
            {
                if (!String.IsNullOrEmpty(column) && (column != COLUMN_NAME_UNKNOWN))
                {
                    return column;
                }
            }

            return null;
        }


        /// <summary>
        /// Generates default WHERE condition according to GroupColumn and TypeCondition settings.
        /// </summary>
        private WhereCondition GetTypeWhereCondition()
        {
            var tc = TypeCondition;
            if (tc != null)
            {
                return tc.GetWhereCondition();
            }

            return new WhereCondition();
        }


        /// <summary>
        /// Applies condition defined in TypeCondition property to the given info. Returns info without changes if TypeCondition is null.
        /// </summary>
        /// <param name="info">Info to apply condition to</param>
        public void ApplyTypeCondition(IDataContainer info)
        {
            if (TypeCondition == null)
            {
                return;
            }

            foreach (var conditionColumn in TypeCondition.ConditionColumns)
            {
                info.SetValue(conditionColumn, TypeCondition.GetFieldValue(conditionColumn));
            }
        }


        /// <summary>
        /// Gets the object type for the given column or null if the object type is not found or unknown.
        /// </summary>
        /// <param name="columnName">Column name to check</param>
        public string GetObjectTypeForColumn(string columnName)
        {
            // Check parent
            if (!String.IsNullOrEmpty(ParentObjectType) && ParentIDColumn.EqualsCSafe(columnName, true))
            {
                return ParentObjectType;
            }

            // Check site
            if (SiteIDColumn.EqualsCSafe(columnName, true))
            {
                return PredefinedObjectType.SITE;
            }

            // Check group
            if (GroupIDColumn.EqualsCSafe(columnName, true))
            {
                return PredefinedObjectType.GROUP;
            }

            // Dependencies
            var dep = GetDependencyForColumn(columnName);
            if (dep != null)
            {
                return dep.DependencyObjectType;
            }

            return null;
        }


        /// <summary>
        /// Gets the dependency settings for the given column or null if the settings is not found or unknown.
        /// </summary>
        /// <param name="columnName">Column name to check</param>
        public ObjectDependency GetDependencyForColumn(string columnName)
        {
            // Dependencies
            if (ObjectDependencies != null)
            {
                foreach (var dep in ObjectDependencies)
                {
                    // If column found, return its object type
                    string colName = dep.DependencyColumn;
                    if (colName.EqualsCSafe(columnName, true))
                    {
                        return dep;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns column name collection of specified object type.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        public ICollection<string> GetTypeColumns(ObjectTypeInfo typeInfo)
        {
            var result = GetTypeColumns(typeInfo.ObjectType);
            if ((result.Count == 0) && (typeInfo.OriginalObjectType != typeInfo.ObjectType))
            {
                result = GetTypeColumns(typeInfo.OriginalObjectType);
            }

            return result;
        }


        /// <summary>
        /// Returns column name collection of specified object type.
        /// </summary>
        /// <param name="objectType">Object type name</param>
        private ICollection<string> GetTypeColumns(string objectType)
        {
            var result = new List<string>();

            // Site ID column
            if (string.Equals(objectType, PredefinedObjectType.SITE, StringComparison.OrdinalIgnoreCase) && (SiteIDColumn != COLUMN_NAME_UNKNOWN))
            {
                result.Add(SiteIDColumn);
            }

            // ID column
            if (string.Equals(objectType, ObjectType, StringComparison.OrdinalIgnoreCase) && (IDColumn != COLUMN_NAME_UNKNOWN))
            {
                result.Add(IDColumn);
            }

            // Parent ID column
            if (string.Equals(objectType, ParentObjectType, StringComparison.OrdinalIgnoreCase) && (ParentIDColumn != COLUMN_NAME_UNKNOWN))
            {
                result.Add(ParentIDColumn);
            }

            // Dependencies
            if (ObjectDependencies != null)
            {
                foreach (var dep in ObjectDependencies.Where(d => !string.Equals(d.DependencyObjectType, PredefinedObjectType.SITE, StringComparison.OrdinalIgnoreCase)
                                                               && !d.HasDynamicObjectType()))
                {
                    // If type matches, add the dependency column
                    if (string.Equals(dep.DependencyObjectType, objectType, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(dep.DependencyColumn);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets value of appropriate include to parent data set property due to operation.
        /// </summary>
        /// <param name="operation">Operation type</param>
        public IncludeToParentEnum IncludeToParentDataSet(OperationTypeEnum operation)
        {
            switch (operation)
            {
                case OperationTypeEnum.Synchronization:
                case OperationTypeEnum.Integration:
                    return SynchronizationSettings.IncludeToSynchronizationParentDataSet;

                case OperationTypeEnum.Export:
                case OperationTypeEnum.ExportSelection:
                    return ImportExportSettings.IncludeToExportParentDataSet;

                case OperationTypeEnum.Versioning:
                    return IncludeToVersionParentDataSet ? IncludeToParentEnum.Complete : IncludeToParentEnum.None;

                default:
                    return IncludeToParentEnum.None;
            }
        }


        /// <summary>
        /// Invalidates specific object.
        /// </summary>
        /// <param name="objectId">Object ID to invalidate</param>
        /// <param name="logTask">If true web farm task is logged</param>
        public void ObjectInvalidated(int objectId, bool logTask = true)
        {
            ObjectInvalidatedInternal(objectId, DateTime.Now);

            // Log web farm task
            if (logTask)
            {
                WebFarmHelper.CreateTask(new InvalidateObjectWebFarmTask {
                    ObjectId = objectId,
                    TaskTarget = ObjectType
                });
            }
        }


        /// <summary>
        /// Invalidates specific object to the specific time.
        /// </summary>
        /// <param name="objectId">Object ID to invalidate</param>
        /// <param name="now">Time of invalidation</param>
        internal void ObjectInvalidatedInternal(int objectId, DateTime now)
        {
            InvalidatedObjects[objectId] = now;

            mLastObjectInvalidated = now;
        }


        /// <summary>
        /// Invalidates direct child objects of specific parent.
        /// </summary>
        /// <param name="parentId">Parent object ID</param>
        /// <param name="logTask">If true web farm task is logged</param>
        public void ChildrenInvalidated(int parentId, bool logTask = true)
        {
            ChildrenInvalidatedInternal(parentId, DateTime.Now);

            // Log web farm task
            if (logTask)
            {
                WebFarmHelper.CreateTask(new InvalidateChildrenWebFarmTask {
                    ParentId = parentId,
                    TaskTarget = ObjectType });
            }
        }


        /// <summary>
        /// Invalidates direct child objects of specific parent to the specific time.
        /// </summary>
        /// <param name="parentId">Parent object ID</param>
        /// <param name="now">Time of invalidation</param>
        internal void ChildrenInvalidatedInternal(int parentId, DateTime now)
        {
            InvalidatedChildren[parentId] = now;
        }


        /// <summary>
        /// Invalidates all objects of this type.
        /// </summary>
        public void InvalidateAllObjects()
        {
            InvalidateAllObjects(true);
        }


        /// <summary>
        /// Invalidates all objects of this type.
        /// </summary>
        /// <param name="logTask">If true, web farm task is logged</param>
        public void InvalidateAllObjects(bool logTask)
        {
            InvalidateAllObjectsInternal(DateTime.Now);

            // Log web farm task
            if (logTask)
            {
                WebFarmHelper.CreateTask(new InvalidateAllWebFarmTask { TaskTarget = ObjectType });
            }
        }


        /// <summary>
        /// Invalidates all objects of this type to specific time.
        /// </summary>
        /// <param name="now">Time of invalidation</param>
        internal void InvalidateAllObjectsInternal(DateTime now)
        {
            mAllObjectsInvalidated = now;
            mLastObjectInvalidated = now;
        }


        /// <summary>
        /// Invalidates column names.
        /// </summary>
        public void InvalidateColumnNames()
        {
            InvalidateColumnNames(true);
        }


        /// <summary>
        /// Invalidates column names.
        /// </summary>
        public void InvalidateColumnNames(bool logTask)
        {
            ColumnsInvalidated = true;

            // Log web farm task
            if (logTask)
            {
                ProviderObject.CreateWebFarmTask(ProviderHelper.INVALIDATE_COLUMN_NAMES, null);
            }
        }


        /// <summary>
        /// Returns true if the object is invalid.
        /// </summary>
        /// <param name="objectId">Object ID</param>
        /// <param name="lastValid">Time when the object was last known as valid</param>
        public bool IsObjectInvalid(int objectId, DateTime lastValid)
        {
            // If older than invalidation of all objects, the object is always invalid
            if (mAllObjectsInvalidated >= lastValid)
            {
                return true;
            }

            // Check if the object was ever invalidated
            var invalidationTime = InvalidatedObjects[objectId];
            if (invalidationTime == DateTimeHelper.ZERO_TIME)
            {
                return false;
            }

            // Check the invalidation time
            return (invalidationTime >= lastValid);
        }


        /// <summary>
        /// Returns true if the direct child objects are invalid.
        /// </summary>
        /// <param name="parentId">Parent object ID</param>
        /// <param name="lastValid">Time when the objects was last known as valid</param>
        public bool AreChildrenInvalid(int parentId, DateTime lastValid)
        {
            // Check if the direct child objects were ever invalidated
            var invalidationTime = InvalidatedChildren[parentId];
            if (invalidationTime == DateTimeHelper.ZERO_TIME)
            {
                return false;
            }

            // Check the invalidation time
            return (invalidationTime >= lastValid);
        }


        /// <summary>
        /// Increase the number of instances
        /// </summary>
        internal void IncrementInstanceCount()
        {
            Interlocked.Increment(ref mInstanceCount);
        }


        /// <summary>
        /// Decrease the number of instances
        /// </summary>
        internal void DecrementInstanceCount()
        {
            Interlocked.Decrement(ref mInstanceCount);
        }


        /// <summary>
        /// Gets the site where condition for this query
        /// </summary>
        /// <param name="siteIdentifier">Identifier of an existing site, or null.</param>
        /// <param name="includeGlobal">Include global objects</param>
        public WhereCondition GetSiteWhereCondition(SiteInfoIdentifier siteIdentifier, bool includeGlobal)
        {
            // Handle all sites constant
            if (siteIdentifier?.ObjectID == ProviderHelper.ALL_SITES)
            {
                if (includeGlobal)
                {
                    // All sites + global = no restrictions
                    return null;
                }

                if (SiteIDColumn != COLUMN_NAME_UNKNOWN)
                {
                    // All with site ID set
                    return new WhereCondition().WhereNotNull(SiteIDColumn);
                }

                // No site ID column = no restrictions (all is available for all sites)
                return null;
            }

            var where = new WhereCondition();
            var globalWhere = new WhereCondition();

            // Check the site ID column
            var hasSiteId = (SiteIDColumn != COLUMN_NAME_UNKNOWN);

            // Check the site binding
            var siteBinding = SiteBindingObject;
            var hasSiteBinding = (siteBinding != null);

            if (hasSiteId)
            {
                if (siteIdentifier?.ObjectID > 0)
                {
                    // SiteID = 123 -> Specific site
                    where.Where(SiteIDColumn, QueryOperator.Equals, siteIdentifier.ObjectID);
                }
                else if (siteIdentifier?.ObjectID == 0)
                {
                    where.NoResults();
                }

                if (includeGlobal || siteIdentifier == null || siteIdentifier.ObjectID < 0 || hasSiteBinding)
                {
                    // SiteID IS NULL -> Global objects
                    globalWhere = globalWhere.WhereNull(SiteIDColumn);
                }
            }

            if (hasSiteBinding)
            {
                // Get the site binding object
                if (siteIdentifier?.ObjectID > 0)
                {
                    var siteQuery = new IDQuery(SiteBinding)
                    .Where(siteBinding.TypeInfo.SiteIDColumn, QueryOperator.Equals, siteIdentifier.ObjectID);

                    // Site binding
                    globalWhere.WhereIn(IDColumn, siteQuery);
                }
                else if (siteIdentifier?.ObjectID == 0)
                {
                    globalWhere.NoResults();
                }
            }

            // Merge where conditions
            if (!globalWhere.WhereIsEmpty && where.ReturnsNoResults)
            {
                return globalWhere;
            }

            where.Or(globalWhere);

            return where;
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific path.
        /// </summary>
        /// <param name="path">ID path</param>
        public string GetObjectPathWhereCondition(string path)
        {
            if (ObjectPathColumn != COLUMN_NAME_UNKNOWN)
            {
                path = SqlHelper.EscapeQuotes(path.TrimEnd('/'));
                return String.Format("(({0} = N'{1}') OR ({0} LIKE '{2}/%'))", ObjectPathColumn, path, SqlHelper.EscapeLikeText(path));
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific binding dependencies. Use this method for 3 and more-keys bindings.
        /// </summary>
        /// <param name="bindingType">Binding object type name</param>
        /// <param name="dependencies">Pairs of binding dependencies to filter by (first variable in each pair - dependency object type, second variable in each pair - dependency value)</param>
        /// <exception cref="ObjectTypeColumnNotFoundException">If dependency column is not found by object type</exception>
        public string GetBindingWhereCondition(string bindingType, params object[] dependencies)
        {
            // If at least one binding dependency is specified, build the condition
            if (dependencies.Length > 0)
            {
                GeneralizedInfo binding = ModuleManager.GetReadOnlyObject(bindingType);
                if (binding != null)
                {
                    var bindingTypeInfo = binding.TypeInfo;
                    if (bindingTypeInfo.IsBinding)
                    {
                        // Get the binding class
                        var dci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(bindingTypeInfo.ObjectClassName);

                        // Get the binding dependency column which references object id column
                        string dependencyIdColumn = bindingTypeInfo.GetTypeColumns(ObjectType).SingleOrDefault();

                        var sb = new StringBuilder();

                        // ObjectID IN (SELECT BindingDependencyIDColumn FROM Binding_Table WHERE
                        sb.Append(IDColumn);
                        sb.Append(" IN (SELECT ");
                        sb.Append(dependencyIdColumn);
                        sb.Append(" FROM ");
                        sb.Append(dci.ClassTableName);
                        sb.Append(" WHERE ");

                        // Is 2-keys binding (only 1 pair is specified)?
                        bool isTwoKeysBinding = (dependencies.Length == 2);

                        // BindingDependencyColumn1 = <bindingDependencyIDValue1> AND BindingDependencyColumn2 = <bindingDependencyIDValue2> AND ...
                        for (int i = 1; i < dependencies.Length; i = i + 2)
                        {
                            // Dependency object type for N-keys binding
                            string dependencyObjectType = (string)dependencies[i - 1];

                            // Dependency object type for 2-keys binding
                            if (isTwoKeysBinding && (dependencyObjectType == null))
                            {
                                dependencyObjectType = GetBindingDependencyObjectType(bindingTypeInfo);
                            }

                            // Dependency column name and value
                            var columnName = bindingTypeInfo.GetTypeColumns(dependencyObjectType).SingleOrDefault();
                            if (String.IsNullOrEmpty(columnName))
                            {
                                throw new ObjectTypeColumnNotFoundException("Column for dependency object type '" + dependencyObjectType + "' was not found.", dependencyObjectType);
                            }

                            int? columnIdValue = (int?)dependencies[i];

                            if (i > 1)
                            {
                                sb.Append(" AND ");
                            }

                            sb.Append(columnName);
                            if (columnIdValue != null)
                            {
                                sb.Append(" = ");
                                sb.Append(columnIdValue);
                            }
                            else
                            {
                                sb.Append(" IS NULL ");
                            }
                        }

                        // Close IN (...) expression
                        sb.Append(")");

                        return sb.ToString();
                    }
                }
            }

            return "";
        }


        /// <summary>
        /// Gets the list of columns representing binding.
        /// </summary>
        /// <param name="includeParent">If true, the result includes the parent column</param>
        /// <param name="includeSite">If true, the result includes the site column</param>
        public List<string> GetBindingColumns(bool includeParent = true, bool includeSite = true)
        {
            var result = new List<string>();

            // Add parent
            if (includeParent && (ParentIDColumn != COLUMN_NAME_UNKNOWN))
            {
                result.Add(ParentIDColumn);
            }

            // Add site
            if (includeSite && (SiteIDColumn != COLUMN_NAME_UNKNOWN))
            {
                result.Add(SiteIDColumn);
            }

            // Find all binding columns
            foreach (var dep in ObjectDependencies)
            {
                if (dep.DependencyType == ObjectDependencyEnum.Binding)
                {
                    result.Add(dep.DependencyColumn);
                }
            }

            return result;
        }


        /// <summary>
        /// Gets a new empty where condition for this object type
        /// </summary>
        public WhereCondition CreateWhereCondition()
        {
            var query = GetDataQuery(null, false);
            var where = new WhereCondition
            {
                ParentQuery = query
            };

            return where;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        /// <param name="parameters">Parameters for the data retrieval</param>
        /// <param name="checkLicense">If true, the license is checked for this query</param>
        private IDataQuery GetDataQuery(Action<DataQuerySettings> parameters, bool checkLicense)
        {
            var infoObj = ModuleManager.GetReadOnlyObject(ObjectType);
            var query = infoObj.GetDataQuery(true, parameters, checkLicense);

            return query;
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific dependencies.
        /// </summary>
        /// <param name="dependencyType">Dependency object type name</param>
        /// <param name="op">Operator</param>
        /// <param name="dependencies">Pairs of dependencies to filter by (first item in each pair - dependency object type, second item in each pair - dependency value)</param>
        /// <exception cref="ObjectTypeColumnNotFoundException">If dependency column is not found by object type</exception>
        public WhereCondition GetDependencyWhereCondition(string dependencyType, string op, params Tuple<string, int?>[] dependencies)
        {
            // If at least one dependency is specified, build the condition
            if (dependencies.Length > 0)
            {
                GeneralizedInfo dependency = ModuleManager.GetReadOnlyObject(dependencyType);
                if (dependency != null)
                {
                    if (op == null)
                    {
                        op = "AND";
                    }

                    var depTypeInfo = dependency.TypeInfo;

                    // Get the dependency column which references object id column
                    string dependencyIdColumn = GetTypeColumns(dependencyType).First();

                    var depCols = TypeHelper.GetTypes(dependencyIdColumn);

                    var where = CreateWhereCondition();

                    foreach (var depCol in depCols)
                    {
                        var dependencyObjectQuery = new ObjectQuery(depTypeInfo.ObjectType, false).Column(depTypeInfo.IDColumn);

                        foreach (var tuple in dependencies)
                        {
                            if (tuple != null)
                            {
                                // Dependency object type
                                string dependencyObjectType = tuple.Item1;

                                // Dependency column name and value
                                var columnName = depTypeInfo.GetTypeColumns(dependencyObjectType).SingleOrDefault();
                                if (String.IsNullOrEmpty(columnName))
                                {
                                    throw new ObjectTypeColumnNotFoundException("Column for dependency object type '" + dependencyObjectType + "' was not found.", dependencyObjectType);
                                }

                                int? columnIdValue = tuple.Item2;

                                if (op == "OR")
                                {
                                    dependencyObjectQuery.Or();
                                }

                                if (columnIdValue != null)
                                {
                                    dependencyObjectQuery.WhereEquals(columnName, columnIdValue);
                                }
                                else
                                {
                                    dependencyObjectQuery.WhereNull(columnName);
                                }
                            }
                        }

                        where.Or().WhereIn(depCol, dependencyObjectQuery);
                    }

                    return where;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns first object type from binding dependencies, parent, and site which holds reference to the different object type than the object type of the provider.
        /// Use only for 2-keys bindings.
        /// </summary>
        private string GetBindingDependencyObjectType(ObjectTypeInfo bindingType)
        {
            var dependencies = bindingType.ObjectDependencies;
            if (dependencies != null)
            {
                string providerObjectType = ObjectType.ToLowerCSafe();

                // Search the dependencies
                foreach (var dep in dependencies)
                {
                    if (dep != null)
                    {
                        string dependencyObjectType = dep.DependencyObjectType;

                        // If dependency object type is different from provider object type, get the dependency object type
                        if (dependencyObjectType.ToLowerCSafe() != providerObjectType)
                        {
                            return dependencyObjectType;
                        }
                    }
                }

                // Dependency not found, get the parent
                if ((bindingType.ParentObjectType != null) && (bindingType.ParentObjectType.ToLowerCSafe() != providerObjectType))
                {
                    return bindingType.ParentObjectType;
                }

                // Parent not found, get the site
                if ((bindingType.SiteIDColumn != COLUMN_NAME_UNKNOWN) && (providerObjectType != PredefinedObjectType.SITE))
                {
                    return PredefinedObjectType.SITE;
                }
            }

            return "";
        }


        /// <summary>
        /// Initializes foreign key columns
        /// </summary>
        private IEnumerable<string> InitializeForeignKeys()
        {
            // Add dependency ID columns
            var cols = new HashSet<string>();

            if (ObjectDependencies != null)
            {
                foreach (var dep in ObjectDependencies)
                {
                    string colName = dep.DependencyColumn;
                    cols.Add(colName.ToLowerCSafe());
                }
            }

            // Add parent ID column
            if (ParentIDColumn != COLUMN_NAME_UNKNOWN)
            {
                cols.Add(ParentIDColumn.ToLowerCSafe());
            }

            // Add site ID column
            if (SiteIDColumn != COLUMN_NAME_UNKNOWN)
            {
                cols.Add(SiteIDColumn.ToLowerCSafe());
            }

            return cols;
        }


        /// <summary>
        /// Indicates if given column is foreign key.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool IsForeignKey(string columnName)
        {
            return columnName != null && ForeignKeys.Contains(columnName.ToLowerCSafe());
        }


        /// <summary>
        /// Gets the nice object type name for this type
        /// </summary>
        public virtual string GetNiceObjectTypeName()
        {
            return TypeHelper.GetNiceObjectTypeName(ObjectType);
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Events handled by object of this type
        /// </summary>
        public TypeInfoEvents Events
        {
            get
            {
                return LockHelper.Ensure(ref mEvents, () => new TypeInfoEvents(this), lockObject);
            }
        }


        /// <summary>
        /// Object load related data event handler.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        public delegate object ObjectLoadRelatedDataEventHandler(BaseInfo infoObj);


        /// <summary>
        /// Fires when search content is requested. You can modify content value which is saved to the search index.
        /// </summary>
        /// <param name="obj">Currently indexed object</param>
        /// <param name="content">Current content value</param>
        public delegate string OnGetContentEventHandler(object obj, string content);


        /// <summary>
        /// Logs the object change.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        /// <param name="taskType">Type of the task</param>
        public delegate void OnLogObjectChangeEventHandler(BaseInfo infoObj, TaskTypeEnum taskType);


        /// <summary>
        /// Fires when the object change should be logged - Handler for particular type info
        /// </summary>
        public event OnLogObjectChangeEventHandler OnLogObjectChange;


        /// <summary>
        /// Fires when the object change should be logged - Global handler
        /// </summary>
        public static event OnLogObjectChangeEventHandler OnLogGlobalObjectChange;


        /// <summary>
        /// Fires when the related data should be loaded to the object.
        /// </summary>
        public event ObjectLoadRelatedDataEventHandler OnLoadRelatedData;


        /// <summary>
        /// Raises the on load related data event.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        public object RaiseOnLoadRelatedData(BaseInfo infoObj)
        {
            return OnLoadRelatedData?.Invoke(infoObj);
        }


        /// <summary>
        /// Raises the OnLogObjectChange event.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="taskType">Type of the task</param>
        public void RaiseOnLogObjectChange(BaseInfo infoObj, TaskTypeEnum taskType)
        {
            if (OnLogObjectChange != null)
            {
                OnLogObjectChange(infoObj, taskType);
            }
            else
            {
                OnLogGlobalObjectChange?.Invoke(infoObj, taskType);
            }
        }

        #endregion

    }
}