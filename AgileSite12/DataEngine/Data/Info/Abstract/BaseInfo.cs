using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Core;
using CMS.DataEngine.Internal;
using CMS.DataEngine.CollectionExtensions;
using CMS.Helpers;
using CMS.Base;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base info class (only carrying the type information).
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{GetType()}: {ObjectCodeName} ({ObjectDisplayName}) - {InstanceGUID}")]
    public abstract partial class BaseInfo : AbstractObject, IInfo
    {
        #region "Constants"

        /// <summary>
        /// Site group
        /// </summary>
        protected const string SITE = "##SITE##";

        /// <summary>
        /// Tools group
        /// </summary>
        protected const string SOCIALANDCOMMUNITY = "##SOCIALANDCOMMUNITY##";

        /// <summary>
        /// Global group
        /// </summary>
        protected const string GLOBAL = "##GLOBAL##";

        /// <summary>
        /// Administration group
        /// </summary>
        protected const string CONFIGURATION = "##CONFIGURATION##";

        /// <summary>
        /// Development group
        /// </summary>
        protected const string DEVELOPMENT = "##DEVELOPMENT##";

        /// <summary>
        /// Content management group
        /// </summary>
        protected const string CONTENTMANAGEMENT = "##CONTENTMANAGEMENT##";

        #endregion


        #region "Enums"

        /// <summary>
        /// Type of the object for permission check.
        /// </summary>
        protected enum PermissionObjectType
        {
            /// <summary>
            /// Only global administrator can access the object.
            /// </summary>
            OnlyAdmin,

            /// <summary>
            /// Permission should be checked against current site.
            /// </summary>
            CurrentSite,

            /// <summary>
            /// Permission should be checked against one of the specified sites (at least one has to be allowed for the user to be able to access the object).
            /// </summary>
            SpecifiedSite,
        }

        #endregion


        #region "Generalized properties"

        /// <summary>
        /// Returns true if the object has it's data storage initialized already
        /// </summary>
        [DatabaseMapping(false)]
        protected virtual bool HasData
        {
            get
            {
                // Lower object always has the data ready
                return true;
            }
        }


        /// <summary>
        /// Returns true if the object is global object. False if the object belongs to specific site only.
        /// </summary>
        public virtual bool IsGlobal
        {
            get
            {
                return (ObjectSiteID == 0);
            }
        }


        /// <summary>
        /// Returns true if the object is complete (has all columns).
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool IsComplete
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// If true, the object allows partial updates.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool AllowPartialUpdate
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException("Setting AllowPartialUpdate is not supported for this object.");
            }
        }


        /// <summary>
        /// Generalized interface of this object.
        /// </summary>
        public virtual GeneralizedInfo Generalized
        {
            get
            {
                return mGeneralized ?? (mGeneralized = GetGeneralizedInfo());
            }
        }


        /// <summary>
        /// Implicit operator for conversion from GeneralizedInfo class to BaseInfo
        /// </summary>
        /// <param name="info">Info object</param>
        public static implicit operator BaseInfo(GeneralizedInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return info.MainObject;
        }


        /// <summary>
        /// Implicit operator for conversion from BaseInfo class to GeneralizedInfo
        /// </summary>
        /// <param name="info">Info object</param>
        public static implicit operator GeneralizedInfo(BaseInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return info.Generalized;
        }


        /// <summary>
        /// Gets the generalized info for this object
        /// </summary>
        protected virtual GeneralizedInfo GetGeneralizedInfo()
        {
            return new GeneralizedInfo(this);
        }

        #endregion


        #region "Variables"

        // Object type information.
        internal ObjectTypeInfo mTypeInfo;

        // If true, timestamp of the object is updated when saved.
        private bool mUpdateTimeStamp = true;

        // If true, synchronization tasks are logged on the object update.
        private SynchronizationTypeEnum mLogSynchronization = SynchronizationTypeEnum.Default;

        // If true, export tasks are logged on the object update.
        private bool mLogExport = true;

        // Indicates if parent is allowed to be touched, if exists.
        private bool? mAllowTouchParent;

        // Indicates if the object versioning is supported.
        private bool mSupportsVersioning = true;

        // If true, events are logged on the object update.
        private bool mLogEvents = true;

        // Whether to log integration tasks.
        private bool mLogIntegration = true;

        // If true, web farm tasks are logged on the object update.
        private bool mLogWebFarmTasks = true;

        // If true, cache keys are touched on the object update.
        private bool mTouchCacheDependencies = true;

        // If true, the code name is validated upon saving.
        private bool mValidateCodeName = true;

        // If true, the code name is checked for uniqueness upon saving.
        private bool mCheckUnique = true;

        // Indicates if the object supports deleting to recycle bin.
        private bool mAllowRestore = true;

        // Indicates if the object supports cloning.
        private bool mAllowClone = true;

        // Object site name.
        private string mObjectSiteName;

        // Object parent.
        private BaseInfo mObjectParent;

        // Object thumbnail metafile.
        private BaseInfo mObjectThumbnail;

        // Object icon metafile.
        private BaseInfo mObjectIcon;

        // Object site ID.
        private int mObjectSiteID = -1;

        // If true, the parent data is cached.
        private bool mCacheParentData;

        // Related data is loaded.
        private bool mRelatedDataLoaded;

        // Custom data connected to the object.
        private object mRelatedData;

        // Local object settings.
        private object[] mLocalSettings;

        // Child objects of the object.
        private InfoObjectRepository mChildren;

        // Child dependency objects of the object.
        private InfoObjectRepository mChildDependencies;

        // Binding objects of the object.
        private BindingRepository mBindings;

        // Other binding objects of the object.
        private OtherBindingRepository mOtherBindings;

        // Depending objects of the object.
        private InfoObjectRepository mReferringObjects;

        // Collection of properties of all object types, pairs [object type -> properties]
        private static readonly Hashtable mProperties = new Hashtable();

        /// <summary>
        /// Status of the object.
        /// </summary>
        protected ObjectStatusEnum mStatus = ObjectStatusEnum.Changed;

        // Generalized interface of this object
        private GeneralizedInfo mGeneralized;

        // Number of disconnected references for this collection
        private int mDisconnectedCount;

        private DateTime mLastUpdated = DateTime.Now;
        private IReadOnlyCollection<string> mCustomizedColumns;
        private ObjectSettingsInfo mObjectSettings;
        private bool mIsCachedObject;
        private readonly Guid mInstanceGUID = Guid.NewGuid();

        // Gets an object that can be used to synchronize access to the static objects
        private static readonly object globalLockObject = new object();

        // Object for locking this instance context
        private readonly object lockObject = new object();

        #endregion


        #region "References to TypeInfo (protected, accessible from Generalized)"

        /// <summary>
        /// Code name column name of the info record.
        /// </summary>
        /// <remarks>If the code name column is not known, name of <see cref="ObjectTypeInfo.GUIDColumn"/> is returned.</remarks>
        protected virtual string CodeNameColumn
        {
            get
            {
                if (TypeInfo.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return TypeInfo.GUIDColumn;
                }

                return TypeInfo.CodeNameColumn;
            }
        }


        /// <summary>
        /// Display name column name of the info record.
        /// </summary>
        /// <remarks>If the display name column is not known, name of <see cref="CodeNameColumn"/> is returned.</remarks>
        protected virtual string DisplayNameColumn
        {
            get
            {
                if (TypeInfo.DisplayNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return CodeNameColumn;
                }

                return TypeInfo.DisplayNameColumn;
            }
        }


        /// <summary>
        /// Parent object type.
        /// </summary>
        protected virtual string ParentObjectType
        {
            get
            {
                return TypeInfo.ParentObjectType;
            }
        }

        #endregion


        #region "General properties (protected, accessible from Generalized)"

        /// <summary>
        /// Returns true if the object is considered valid.
        /// </summary>
        protected virtual bool IsObjectValid
        {
            get
            {
                return !IsObjectInvalid(mLastUpdated);
            }
        }


        /// <summary>
        /// Returns the object instance GUID
        /// </summary>
        protected virtual Guid InstanceGUID
        {
            get
            {
                return mInstanceGUID;
            }
        }


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        protected virtual bool IsCachedObject
        {
            get
            {
                return mIsCachedObject;
            }
            set
            {
                mIsCachedObject = value;
            }
        }


        /// <summary>
        /// Returns true if this collection is disconnected from the database
        /// </summary>
        protected virtual bool IsDisconnected
        {
            get
            {
                return ((mDisconnectedCount > 0) || CMSActionContext.CurrentDisconnected);
            }
        }


        /// <summary>
        /// If true, externally stored columns are ignored and are stored normally in DB.
        /// </summary>
        protected virtual bool IgnoreExternalColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the current status of the object.
        /// </summary>
        [DatabaseMapping(false)]
        protected virtual ObjectStatusEnum ObjectStatus
        {
            get
            {
                if (ObjectID <= 0)
                {
                    return ObjectStatusEnum.New;
                }

                return mStatus;
            }
            set
            {
                mStatus = value;
            }
        }


        /// <summary>
        /// If true, the parent object data is cached within object.
        /// </summary>
        protected bool CacheParentData
        {
            get
            {
                return mCacheParentData;
            }
            set
            {
                mCacheParentData = value;

                if (value == false)
                {
                    mObjectParent = null;
                }
            }
        }


        /// <summary>
        /// Object ID.
        /// </summary>
        protected virtual int ObjectID
        {
            get
            {
                var idColumn = TypeInfo.IDColumn;
                if (idColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return 0;
                }

                return GetIntegerValue(idColumn, 0);
            }
            set
            {
                var idColumn = TypeInfo.IDColumn;
                if (idColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(idColumn, value, (value > 0));
                }
            }
        }


        /// <summary>
        /// Last modified time.
        /// </summary>
        protected virtual DateTime ObjectLastModified
        {
            get
            {
                var timeStampColumn = TypeInfo.TimeStampColumn;
                if (timeStampColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return DateTime.MinValue;
                }

                return ValidationHelper.GetDateTime(GetValue(timeStampColumn), DateTime.MinValue);
            }
            set
            {
                var timeStampColumn = TypeInfo.TimeStampColumn;
                if (timeStampColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(timeStampColumn, value, DateTime.MinValue);
                }
            }
        }


        /// <summary>
        /// Object GUID.
        /// </summary>
        protected virtual Guid ObjectGUID
        {
            get
            {
                var guidColumn = TypeInfo.GUIDColumn;
                if (guidColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return Guid.Empty;
                }

                return ValidationHelper.GetGuid(GetValue(guidColumn), Guid.Empty);
            }
            set
            {
                var guidColumn = TypeInfo.GUIDColumn;
                if (guidColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(guidColumn, value, Guid.Empty);
                }
            }
        }


        /// <summary>
        /// Object version GUID.
        /// </summary>
        protected virtual Guid ObjectVersionGUID
        {
            get
            {
                var versionGuidColumn = TypeInfo.VersionGUIDColumn;
                if (versionGuidColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return Guid.Empty;
                }

                return ValidationHelper.GetGuid(GetValue(versionGuidColumn), Guid.Empty);
            }
            set
            {
                var versionGuidColumn = TypeInfo.VersionGUIDColumn;
                if (versionGuidColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(versionGuidColumn, value, Guid.Empty);
                }
            }
        }


        /// <summary>
        /// Object site ID.
        /// </summary>
        protected virtual int ObjectSiteID
        {
            get
            {
                var siteIdColumn = TypeInfo.SiteIDColumn;
                if (siteIdColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // In case site ID is taken from parent, use cached value if available
                    if (mObjectSiteID < 0)
                    {
                        // Try to get from parent
                        GeneralizedInfo parent = ObjectParent;
                        mObjectSiteID = (parent != null) ? parent.ObjectSiteID : 0;
                    }

                    return mObjectSiteID;
                }

                // Get from site ID column
                return GetIntegerValue(siteIdColumn, 0);
            }
            set
            {
                var siteIdColumn = TypeInfo.SiteIDColumn;
                if (siteIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(siteIdColumn, value, (value > 0));
                }
            }
        }


        /// <summary>
        /// Object parent ID.
        /// </summary>
        protected virtual int ObjectParentID
        {
            get
            {
                // If unknown parent ID column, return 0 as no parent
                var possibleParentIdColumn = TypeInfo.PossibleParentIDColumn;
                if (possibleParentIdColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return 0;
                }

                return GetIntegerValue(possibleParentIdColumn, 0);
            }
            set
            {
                var possibleParentIdColumn = TypeInfo.PossibleParentIDColumn;
                if (possibleParentIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(possibleParentIdColumn, value, (value > 0));
                }
            }
        }


        /// <summary>
        /// Object community group ID.
        /// </summary>
        protected virtual int ObjectGroupID
        {
            get
            {
                // If unknown group ID column, return 0 as standard object
                var groupIdColumn = TypeInfo.GroupIDColumn;
                if (groupIdColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return 0;
                }

                return GetIntegerValue(groupIdColumn, 0);
            }
            set
            {
                var groupIdColumn = TypeInfo.GroupIDColumn;
                if (groupIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(groupIdColumn, value, (value > 0));
                }
            }
        }


        /// <summary>
        /// Object display name.
        /// </summary>
        /// <remarks>
        /// <para>The returned value is evaluated from first known column in following column sequence:
        /// <see cref="DisplayNameColumn" />,
        /// <see cref="CodeNameColumn" />,
        /// <see cref="ObjectTypeInfo.GUIDColumn" />,
        /// <see cref="ObjectTypeInfo.IDColumn"/>,
        /// <see cref="ObjectTypeInfo.ObjectType" />.
        /// </para>
        /// <para>
        /// When evaluating on <see cref="ObjectTypeInfo.IDColumn"/>,
        /// returned value consists of both <see cref="ObjectTypeInfo.ObjectType" /> and <see cref="ObjectTypeInfo.IDColumn"/>.
        /// </para>
        /// </remarks>
        protected virtual string ObjectDisplayName
        {
            get
            {
                if (DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // <display name>
                    var name = GetStringValue(DisplayNameColumn, "");
                    if (!String.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }

                var ti = TypeInfo;
                if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // <object type> (<id>)
                    return ti.ObjectType + " (" + ObjectID + ")";
                }

                // <object type>
                return ti.ObjectType;
            }
            set
            {
                if (DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(DisplayNameColumn, value);
                }
            }
        }


        /// <summary>
        /// Object code name.
        /// </summary>
        /// <remarks>
        /// The returned value is evaluated from either <see cref="CodeNameColumn"/> or <see cref="ObjectTypeInfo.GUIDColumn"/> column.
        /// If none of the columns is known, <see cref="String.Empty"/> is returned.
        /// </remarks>
        protected virtual string ObjectCodeName
        {
            get
            {
                if (CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return "";
                }

                return GetStringValue(CodeNameColumn, "");
            }
            set
            {
                if (CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(CodeNameColumn, value);
                }
            }
        }


        /// <summary>
        /// Object full name if exists
        /// </summary>
        protected virtual string ObjectFullName
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Object thumbnail GUID.
        /// </summary>
        protected virtual Guid ObjectThumbnailGUID
        {
            get
            {
                var thumbnailGuidColumn = TypeInfo.ThumbnailGUIDColumn;
                if (thumbnailGuidColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return Guid.Empty;
                }

                return ValidationHelper.GetGuid(GetValue(thumbnailGuidColumn), Guid.Empty);
            }
            set
            {
                var thumbnailGuidColumn = TypeInfo.ThumbnailGUIDColumn;
                if (thumbnailGuidColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(thumbnailGuidColumn, value, Guid.Empty);
                }
            }
        }


        /// <summary>
        /// Object icon GUID.
        /// </summary>
        protected virtual Guid ObjectIconGUID
        {
            get
            {
                var iconGuidColumn = TypeInfo.IconGUIDColumn;
                if (iconGuidColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return Guid.Empty;
                }

                return ValidationHelper.GetGuid(GetValue(iconGuidColumn), Guid.Empty);
            }
            set
            {
                var iconGuidColumn = TypeInfo.IconGUIDColumn;
                if (iconGuidColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(iconGuidColumn, value, Guid.Empty);
                }
            }
        }


        /// <summary>
        /// Object parent.
        /// </summary>
        protected virtual BaseInfo ObjectParent
        {
            get
            {
                if (ParentObjectType == null)
                {
                    return null;
                }

                // If parent data is not cached, get parent directly
                if (!CacheParentData || !IsParentValid())
                {
                    mObjectParent = null;

                    return GetParent();
                }

                return mObjectParent;
            }
            set
            {
                if (value != null)
                {
                    // Set the parent ID and site ID (child may not have different site ID than parent)
                    ObjectParentID = value.ObjectID;

                    if (!TypeInfo.IsSiteBinding // Do not adopt site ID by the site binding to not change its meaning, it is not a true parent-child relationship
                        && (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) // Object must have site ID column
                        && (value.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) // Parent object must have site ID column
                        )
                    {
                        ObjectSiteID = value.ObjectSiteID;
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if the currently hosted parent object is valid
        /// </summary>
        private bool IsParentValid()
        {
            var parent = mObjectParent;

            // If not set, then not valid
            if ((parent == null) || (parent is NotImplementedInfo))
            {
                return false;
            }

            // If doesn't support invalidation, then always valid
            if (!parent.TypeInfo.SupportsInvalidation)
            {
                return true;
            }

            // Otherwise, ask the object
            return parent.IsObjectValid;
        }


        /// <summary>
        /// Object category.
        /// </summary>
        protected virtual BaseInfo ObjectCategory
        {
            get
            {
                var ti = TypeInfo;

                var categoryObject = ti.CategoryObject;
                if (categoryObject != null)
                {
                    var id = ValidationHelper.GetInteger(GetValue(ti.CategoryIDColumn), 0);
                    if (id > 0)
                    {
                        return categoryObject.GetObject(id);
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Returns the object site.
        /// </summary>
        protected virtual BaseInfo ObjectSite
        {
            get
            {
                int siteId = ObjectSiteID;
                if (siteId > 0)
                {
                    return ProviderHelper.GetInfoById(PredefinedObjectType.SITE, siteId);
                }

                return null;
            }
        }


        /// <summary>
        /// Returns the order of the object among the other objects.
        /// </summary>
        protected virtual int ObjectOrder
        {
            get
            {
                var orderColumn = TypeInfo.OrderColumn;
                if (orderColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return GetIntegerValue(orderColumn, 0);
                }
                return 0;
            }
        }


        /// <summary>
        /// Gets the list of customized columns in current object
        /// </summary>
        protected virtual IReadOnlyCollection<string> CustomizedColumns
        {
            get
            {
                var customizedColumnsColumn = TypeInfo.CustomizedColumnsColumn;

                if ((customizedColumnsColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (mCustomizedColumns == null))
                {
                    string columns = GetStringValue(customizedColumnsColumn, String.Empty);
                    mCustomizedColumns = new ReadOnlyCollection<string>(columns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }

                return mCustomizedColumns;
            }
            set
            {
                if (TypeInfo.CustomizedColumnsColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return;
                }

                mCustomizedColumns = value;
                SetValue(TypeInfo.CustomizedColumnsColumn, value.Join(";"));
            }
        }


        /// <summary>
        /// Indicates if object is custom.
        /// </summary>
        protected bool ObjectIsCustom
        {
            get
            {
                var isCustomColumn = TypeInfo.IsCustomColumn;
                if (isCustomColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return GetBooleanValue(isCustomColumn, false);
                }

                return false;
            }
            set
            {
                var isCustomColumn = TypeInfo.IsCustomColumn;
                if (isCustomColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    SetValue(isCustomColumn, value);
                }
            }
        }


        /// <summary>
        /// Indicates if object is customized.
        /// </summary>
        protected bool ObjectIsCustomized
        {
            get
            {
                return ObjectIsCustom || (CustomizedColumns.Count > 0);
            }
        }


        /// <summary>
        /// Object site name.
        /// </summary>
        protected virtual string ObjectSiteName
        {
            get
            {
                // Load the site name
                return mObjectSiteName ?? (mObjectSiteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, ObjectSiteID) ?? string.Empty);
            }
        }


        /// <summary>
        /// If true, timestamp of the object is updated when saved.
        /// </summary>
        protected virtual bool UpdateTimeStamp
        {
            get
            {
                return (TypeInfo.UpdateTimeStamp && mUpdateTimeStamp && CMSActionContext.CurrentUpdateTimeStamp);
            }
            set
            {
                mUpdateTimeStamp = value;
            }
        }


        /// <summary>
        /// Indicates how should be handled the logging of synchronization tasks on the object update.
        /// </summary>
        protected virtual SynchronizationTypeEnum LogSynchronization
        {
            get
            {
                if (!CMSActionContext.CurrentLogSynchronization)
                {
                    return SynchronizationTypeEnum.None;
                }

                if (mLogSynchronization == SynchronizationTypeEnum.Default)
                {
                    return TypeInfo.SynchronizationSettings.LogSynchronization;
                }

                return mLogSynchronization;
            }
            set
            {
                mLogSynchronization = value;
            }
        }


        /// <summary>
        /// If true, export tasks are logged on the object update.
        /// </summary>
        protected virtual bool LogExport
        {
            get
            {
                return (TypeInfo.ImportExportSettings.LogExport && mLogExport && CMSActionContext.CurrentLogExport);
            }
            set
            {
                mLogExport = value;
            }
        }


        /// <summary>
        /// Indicates if parent is allowed to be touched, if exists.
        /// </summary>
        protected virtual bool AllowTouchParent
        {
            get
            {
                if (!CMSActionContext.CurrentTouchParent)
                {
                    return false;
                }

                if (mAllowTouchParent == null)
                {
                    var ti = TypeInfo;

                    bool logSynchronization = (LogSynchronization == SynchronizationTypeEnum.TouchParent);
                    bool includeToExport = (ti.ImportExportSettings.IncludeToExportParentDataSet != IncludeToParentEnum.None);

                    // Do not touch parent for site bindings
                    bool touchParent = !ti.IsSiteBinding && (logSynchronization || includeToExport);

                    return ti.AllowTouchParent && touchParent;
                }

                return mAllowTouchParent.Value;
            }
            set
            {
                mAllowTouchParent = value;
            }
        }


        /// <summary>
        /// Indicates if the object versioning is supported. Default false
        /// </summary>
        protected virtual bool SupportsVersioning
        {
            get
            {
                return (TypeInfo.SupportsVersioning && mSupportsVersioning && CMSActionContext.CurrentCreateVersion);
            }
            set
            {
                mSupportsVersioning = value;
            }
        }


        /// <summary>
        /// Indicates if the object versioning is enabled by the settings.
        /// </summary>
        protected virtual bool VersioningEnabled
        {
            get
            {
                string key = "CMSEnableVersioning" + TypeInfo.ObjectType.Replace(".", "");

                return SettingsKeyInfoProvider.GetBoolValue("CMSEnableObjectsVersioning") && SettingsKeyInfoProvider.GetBoolValue(ObjectSiteName + "." + key);
            }
        }


        /// <summary>
        /// If true, Events tasks are logged on the object update.
        /// </summary>
        protected virtual bool LogEvents
        {
            get
            {
                return TypeInfo.LogEvents && mLogEvents && CMSActionContext.CurrentLogEvents;
            }
            set
            {
                mLogEvents = value;
            }
        }


        /// <summary>
        /// If true, integration tasks are being logged.
        /// </summary>
        protected virtual bool LogIntegration
        {
            get
            {
                return TypeInfo.LogIntegration && mLogIntegration && CMSActionContext.CurrentLogIntegration;
            }
            set
            {
                mLogIntegration = value;
            }
        }


        /// <summary>
        /// If true, web farm tasks are logged on the object update.
        /// </summary>
        protected virtual bool LogWebFarmTasks
        {
            get
            {
                return mLogWebFarmTasks && CMSActionContext.CurrentLogWebFarmTasks;
            }
            set
            {
                mLogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// If true, cache dependencies are touched when the object is changed.
        /// </summary>
        protected virtual bool TouchCacheDependencies
        {
            get
            {
                return TypeInfo.TouchCacheDependencies && mTouchCacheDependencies && CMSActionContext.CurrentTouchCacheDependencies;
            }
            set
            {
                mTouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// Indicates if the object is checked out.
        /// </summary>
        protected virtual bool IsCheckedOut
        {
            get
            {
                if (!TypeInfo.SupportsLocking || (ObjectID <= 0))
                {
                    return false;
                }

                return ObjectSettings.IsCheckedOut;
            }
        }


        /// <summary>
        /// Gets ID of the user who checked the object out.
        /// </summary>
        protected virtual int IsCheckedOutByUserID
        {
            get
            {
                if (!TypeInfo.SupportsLocking || (ObjectID <= 0))
                {
                    return 0;
                }

                return ObjectSettings.ObjectCheckedOutByUserID;
            }
        }


        /// <summary>
        /// Indicates if the object is clone.
        /// </summary>
        protected virtual bool IsClone
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the code name is validated upon saving.
        /// </summary>
        protected virtual bool ValidateCodeName
        {
            get
            {
                return mValidateCodeName;
            }
            set
            {
                mValidateCodeName = value;
            }
        }


        /// <summary>
        /// If true, the code name is checked for uniqueness upon saving.
        /// </summary>
        protected virtual bool CheckUnique
        {
            get
            {
                return mCheckUnique;
            }
            set
            {
                mCheckUnique = value;
            }
        }


        /// <summary>
        /// Indicates if the object supports deleting to recycle bin.
        /// </summary>
        protected virtual bool AllowRestore
        {
            get
            {
                return mAllowRestore && TypeInfo.AllowRestore;
            }
            set
            {
                mAllowRestore = value;
            }
        }


        /// <summary>
        /// Indicates if the object supports cloning.
        /// </summary>
        protected virtual bool AllowClone
        {
            get
            {
                return mAllowClone && TypeInfo.SupportsCloning && !TypeInfo.IsBinding;
            }
            set
            {
                mAllowClone = value;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary with the methods for clearing the internal object cache [columnName] => [clearCacheAction]
        /// </summary>
        protected internal abstract IDictionary<string, Action<BaseInfo>> ClearCacheMethods
        {
            get;
        }


        /// <summary>
        /// Object settings
        /// </summary>
        /// <exception cref="System.Exception">Thrown when <see cref="ObjectID"/> is not set.</exception>
        [DatabaseMapping(false)]
        public ObjectSettingsInfo ObjectSettings
        {
            get
            {
                return EnsureObjectSettings();
            }
        }


        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        public virtual List<string> Properties
        {
            get
            {
                // Try to get from hashtable
                string objType = TypeInfo.ObjectType.ToLowerInvariant();
                List<string> properties = (List<string>)mProperties[objType];

                if (properties == null)
                {
                    lock (mProperties)
                    {
                        properties = new List<string>();

                        // Add registered properties
                        List<string> registeredProperties = GetRegisteredProperties();
                        if (registeredProperties != null)
                        {
                            properties.AddRange(registeredProperties);
                        }

                        // Add custom properties
                        List<string> customProperties = GetCustomProperties();
                        if (customProperties != null)
                        {
                            properties.AddRange(customProperties);
                        }

                        // Add all columns
                        List<string> columns = new List<string>();

                        foreach (string col in GetColumnNames())
                        {
                            // Add column
                            columns.Add(col);
                        }

                        // Order the properties alphabetically
                        properties.Sort();

                        // Order the columns alphabetically
                        columns.Sort();

                        properties.AddRange(columns);

                        // Cache it
                        mProperties[objType] = properties;
                    }
                }

                return properties;
            }
        }


        /// <summary>
        /// Object type information.
        /// </summary>
        public virtual ObjectTypeInfo TypeInfo
        {
            get
            {
                if (mTypeInfo == null)
                {
                    return InfoHelper.UNKNOWN_TYPEINFO;
                }
                else
                {
                    return mTypeInfo;
                }
            }
            protected internal set
            {
                mTypeInfo = value;
            }
        }


        /// <summary>
        /// Object parent
        /// </summary>
        public virtual BaseInfo Parent
        {
            get
            {
                return ObjectParent;
            }
        }


        /// <summary>
        /// Object site
        /// </summary>
        public virtual BaseInfo Site
        {
            get
            {
                return ObjectSite;
            }
        }


        /// <summary>
        /// Object thumbnail
        /// </summary>
        public virtual BaseInfo Thumbnail
        {
            get
            {
                return ObjectThumbnail;
            }
        }


        /// <summary>
        /// Object icon
        /// </summary>
        public virtual BaseInfo Icon
        {
            get
            {
                return ObjectIcon;
            }
        }


        /// <summary>
        /// Collection of the sites to which the object is associated via site bindings (M:N relationships).
        /// </summary>
        public virtual IInfoObjectCollection AssignedSites
        {
            get
            {
                var ti = TypeInfo;

                if ((Bindings == null) || string.IsNullOrEmpty(ti.SiteBinding))
                {
                    return null;
                }

                // Get the site binding collection
                var bindings = Bindings[ti.SiteBinding];
                if (bindings == null)
                {
                    return null;
                }

                // Transform to the sites
                var siteBinding = ti.SiteBindingObject;

                return bindings.FieldsAsObjects[siteBinding.TypeInfo.SiteIDColumn];
            }
        }


        /// <summary>
        /// Collection of the binding objects for the given object where the current object is a parent of the binding.
        /// </summary>
        public virtual BindingRepository Bindings
        {
            get
            {
                return GetBindings();
            }
        }


        /// <summary>
        /// Collection of the binding objects for the given object where the current object is not a parent of the binding (parent object is on the second side).
        /// </summary>
        public virtual OtherBindingRepository OtherBindings
        {
            get
            {
                return GetOtherBindings();
            }
        }


        /// <summary>
        /// Collection of the objects depending on this object (object which have FK to this object).
        /// </summary>
        protected virtual InfoObjectRepository ReferringObjects
        {
            get
            {
                return GetReferringObjects();
            }
        }


        /// <summary>
        /// Collection of the child objects for the given object.
        /// </summary>
        public virtual InfoObjectRepository Children
        {
            get
            {
                return GetObjectChildren();
            }
        }


        /// <summary>
        /// Collection of the child dependencies for the given object. These are objects which should be included into the parent data (for example class data for BizForms), but aren't direct child of the object.
        /// </summary>
        protected virtual InfoObjectRepository ChildDependencies
        {
            get
            {
                return GetChildDependencies();
            }
        }


        /// <summary>
        /// Object thumbnail metafile.
        /// </summary>
        protected virtual MetaFileInfo ObjectThumbnail
        {
            get
            {
                return (MetaFileInfo)InfoHelper.EnsureInfo(ref mObjectThumbnail, GetThumbnailInfo);
            }
        }


        /// <summary>
        /// Object icon metafile.
        /// </summary>
        protected virtual MetaFileInfo ObjectIcon
        {
            get
            {
                return (MetaFileInfo)InfoHelper.EnsureInfo(ref mObjectIcon, GetIconInfo);
            }
        }


        /// <summary>
        /// Collection of the metafiles belonging to the object.
        /// </summary>
        public virtual IInfoObjectCollection MetaFiles
        {
            get
            {
                if (!TypeInfo.HasMetaFiles)
                {
                    return null;
                }
                return ReferringObjects[MetaFileInfo.OBJECT_TYPE];
            }
        }


        /// <summary>
        /// Collection of the processes belonging to the object.
        /// </summary>
        public virtual IInfoObjectCollection Processes
        {
            get
            {
                if (!TypeInfo.HasProcesses)
                {
                    return null;
                }

                return ReferringObjects[PredefinedObjectType.AUTOMATIONSTATE];
            }
        }


        /// <summary>
        /// Collection of the scheduled tasks belonging to the object.
        /// </summary>
        public virtual IInfoObjectCollection ScheduledTasks
        {
            get
            {
                if (!TypeInfo.HasScheduledTasks)
                {
                    return null;
                }

                return ReferringObjects[PredefinedObjectType.OBJECTSCHEDULEDTASK];
            }
        }


        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                if (!mRelatedDataLoaded)
                {
                    // Load the related data to the object
                    mRelatedDataLoaded = true;
                    mRelatedData = TypeInfo.RaiseOnLoadRelatedData(this);
                }

                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
                mRelatedDataLoaded = true;
            }
        }


        /// <summary>
        /// Gets the list of properties which should be prioritized in the macro controls (IntelliSense, MacroTree).
        /// </summary>
        protected virtual List<string> PrioritizedProperties
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseInfo()
        {
            // Un-register the instance
            if ((mTypeInfo != null) && (mTypeInfo != InfoHelper.UNKNOWN_TYPEINFO))
            {
                mTypeInfo.DecrementInstanceCount();
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        protected BaseInfo(ObjectTypeInfo typeInfo)
        {
            InitTypeInfo(typeInfo);
        }


        /// <summary>
        /// Initializes the object by the given type info
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        private void InitTypeInfo(ObjectTypeInfo typeInfo)
        {
            mTypeInfo = typeInfo;

            // Ignore external column of objects that do not support them
            if ((typeInfo != null) && (typeInfo != InfoHelper.UNKNOWN_TYPEINFO))
            {
                IgnoreExternalColumns = !typeInfo.HasExternalColumns;

                typeInfo.IncrementInstanceCount();
            }
        }


        /// <summary>
        /// Throws serialization not supported exception
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown always.</exception>
        protected void SerializationNotSupported()
        {
            throw new NotSupportedException("This class is abstract and doesn't support direct serialization.");
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        protected BaseInfo(SerializationInfo info, StreamingContext context)
            : this(info, context, null)
        {
            SerializationNotSupported();
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected BaseInfo(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
        {
            mLogSynchronization = (SynchronizationTypeEnum)info.GetValue("LogSynchronization", typeof(SynchronizationTypeEnum));

            // Load the type info
            if (mTypeInfo == null)
            {
                string objectType = (string)info.GetValue("ObjectType", typeof(string));
                if (!String.IsNullOrEmpty(objectType))
                {
                    var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                    if (typeInfo == null)
                    {
                        throw new Exception("Type information for the object type '" + objectType + "' was not found.");
                    }

                    InitTypeInfo(typeInfo);
                }
            }

            mUpdateTimeStamp = (bool)info.GetValue("UpdateTimeStamp", typeof(bool));
            mLastUpdated = (DateTime)info.GetValue("LastUpdated", typeof(DateTime));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LogSynchronization", mLogSynchronization);

            // Object type
            string objectType = null;
            if (mTypeInfo != null)
            {
                objectType = mTypeInfo.ObjectType;
            }
            info.AddValue("ObjectType", objectType);

            info.AddValue("UpdateTimeStamp", mUpdateTimeStamp);
            info.AddValue("LastUpdated", mLastUpdated);
        }


        /// <summary>
        /// Loads the object using the given settings
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal abstract void LoadData(LoadDataSettings settings);


        /// <summary>
        /// Creates new object of the given class based on the given settings
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected abstract BaseInfo NewObject(LoadDataSettings settings);


        /// <summary>
        /// Executes the given action using original data of the object
        /// </summary>
        /// <param name="action">Action to execute</param>
        public abstract void ExecuteWithOriginalData(Action action);

        #endregion


        #region "External files methods"

        /// <summary>
        /// Goes through the columns which are stored externally and updates DB versions with the data from external storage.
        /// </summary>
        protected virtual void UpdateExternalColumns()
        {
            throw new NotImplementedException("[BaseInfo.DeleteExternalColumns]: Should be implemented in the child class.");
        }


        /// <summary>
        /// Goes through the columns which are stored externally and deletes all the files.
        /// </summary>
        /// <param name="updateDB">If true, DB is updated with the data from the file before it's deleted.</param>
        protected virtual void DeleteExternalColumns(bool updateDB)
        {
            throw new NotImplementedException("[BaseInfo.DeleteExternalColumns]: Should be implemented in the child class.");
        }


        /// <summary>
        /// Goes through the columns which are stored externally and ensures them in the external storage.
        /// </summary>
        /// <param name="deleteUnusedFiles">If true, the old files are deleted when the path of the columnFile has changed</param>
        /// <param name="onlyIfChanged">Tries to modify the external file only if the column is marked as changed</param>
        protected internal virtual void SaveExternalColumns(bool deleteUnusedFiles, bool onlyIfChanged)
        {
            throw new NotImplementedException("[BaseInfo.SaveExternalColumns]: Should be implemented in the child class.");
        }


        /// <summary>
        /// Goes through the columns which are stored externally and returns the list of particular files this object uses.
        /// </summary>
        protected virtual List<string> GetExternalFiles()
        {
            throw new NotImplementedException("[BaseInfo.GetExternalFiles]: Should be implemented in the child class.");
        }


        /// <summary>
        /// Returns the list of columns registered as the external columns.
        /// </summary>
        protected virtual List<string> GetExternalColumns()
        {
            return null;
        }


        /// <summary>
        /// Goes through the columns which are stored externally and checks if the data in DB is the same as in external storage. If all the columns are same returns true, otherwise false.
        /// </summary>
        protected virtual bool IsModifiedExternally()
        {
            return false;
        }


        /// <summary>
        /// Gets DataSet with physical files.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="binaryData">If true, gets the binary data to the DataSet</param>
        protected virtual DataSet GetPhysicalFiles(OperationTypeEnum operationType, bool binaryData)
        {
            return null;
        }


        /// <summary>
        /// Saves physical files.
        /// </summary>
        /// <param name="dsFiles">DataSet with files data</param>
        protected virtual void UpdatePhysicalFiles(DataSet dsFiles)
        {
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// Ensures the GUID of the object
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected virtual string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            throw new Exception("GetVirtualFileRelativePath method must be overridden by child class.");
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the dictionary of methods for clearing the cache inside the info object [columnName] => [clearCacheAction]
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        protected static IDictionary<string, Action<BaseInfo>> GetClearCacheMethods(ObjectTypeInfo typeInfo)
        {
            var methods = new Dictionary<string, Action<BaseInfo>>(StringComparer.InvariantCultureIgnoreCase);

            // Clear customized columns property to refresh its value
            if (typeInfo.CustomizedColumnsColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                methods[typeInfo.CustomizedColumnsColumn] = o =>
                {
                    o.mCustomizedColumns = null;
                };
            }

            // Clear cached site ID and site name when site changes
            if (typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                methods[typeInfo.SiteIDColumn] = o =>
                {
                    o.mObjectSiteID = -1;
                    o.mObjectSiteName = null;
                };
            }

            // Clear cached parent and site when parent changes
            if (typeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                methods[typeInfo.ParentIDColumn] = o =>
                {
                    o.mObjectParent = null;
                    o.mObjectSiteID = -1;

                    o.mObjectSiteName = null;
                };
            }

            // Clear cached thumbnail if thumbnail changes
            if (typeInfo.ThumbnailGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                methods[typeInfo.ThumbnailGUIDColumn] = o =>
                {
                    o.mObjectThumbnail = null;
                };
            }

            // Clear cached thumbnail if thumbnail changes
            if (typeInfo.IconGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                methods[typeInfo.IconGUIDColumn] = o =>
                {
                    o.mObjectIcon = null;
                };
            }

            return methods;
        }


        /// <summary>
        /// Clears the cached values depending on the given column value.
        /// </summary>
        /// <param name="columnName">Column name, if not provided, all cached values are cleared.</param>
        protected void ClearCachedValues(string columnName = null)
        {
            if (columnName == null)
            {
                // Clear all caches
                foreach (var method in ClearCacheMethods.Values)
                {
                    method(this);
                }
            }
            else
            {
                // Clear only cache depending on given column
                Action<BaseInfo> method;
                if (ClearCacheMethods.TryGetValue(columnName, out method))
                {
                    method(this);
                }
            }
        }


        /// <summary>
        /// Clears hashtable with cached properties of all object types.
        /// </summary>
        public static void Clear()
        {
            if (mProperties != null)
            {
                mProperties.Clear();
            }
        }


        /// <summary>
        /// Clears data from the object.
        /// </summary>
        public virtual void ClearData()
        {
            foreach (string column in Properties)
            {
                SetValue(column, null);
            }
        }


        /// <summary>
        /// Ensures the GUID of the object
        /// </summary>
        protected virtual Guid EnsureGUID()
        {
            if (TypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var guid = ObjectGUID;
                if (guid == Guid.Empty)
                {
                    ObjectGUID = guid = Guid.NewGuid();
                }

                return guid;
            }

            return Guid.Empty;
        }


        /// <summary>
        /// Ensures the last modified time stamp of the object
        /// </summary>
        protected virtual void EnsureLastModified()
        {
            var ti = TypeInfo;
            if (ti.TimeStampColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Ensure / update the time stamp
                if (UpdateTimeStamp || (ObjectLastModified == DateTime.MinValue))
                {
                    SetValue(ti.TimeStampColumn, DateTime.Now);
                }
            }
        }


        /// <summary>
        /// Submits the changes in the object to the database.
        /// </summary>
        /// <param name="withCollections">If true, also submits the changes in the underlying collections of the object (Children, ChildDependencies, Bindings, OtherBindings)</param>
        public virtual void SubmitChanges(bool withCollections)
        {
            // Do the changes in a transaction
            using (var tr = TransactionScopeFactory.GetTransactionScope(TypeInfo.ProviderType))
            {
                switch (ObjectStatus)
                {
                    case ObjectStatusEnum.ToBeDeleted:
                        // Delete the object
                        DeleteObject();
                        break;


                    case ObjectStatusEnum.New:
                    case ObjectStatusEnum.Changed:
                        // Save the object to the database
                        SetObject();
                        break;

                    case ObjectStatusEnum.Unchanged:
                        // Do nothing
                        break;

                    default:
                        throw new NotImplementedException("[BaseInfo.SubmitChanges]: Unknown status.");
                }

                // Submit the changes in the children
                if (withCollections)
                {
                    if (mChildren != null)
                    {
                        mChildren.SubmitChanges();
                    }

                    if (mChildDependencies != null)
                    {
                        mChildDependencies.SubmitChanges();
                    }

                    if (mBindings != null)
                    {
                        mBindings.SubmitChanges();
                    }

                    if (mOtherBindings != null)
                    {
                        mOtherBindings.SubmitChanges();
                    }
                }

                // Commit the transaction
                tr.Commit();
            }
        }


        /// <summary>
        /// Updates the database entity using appropriate provider
        /// </summary>
        public virtual void Update()
        {
            if ((ObjectID <= 0) && (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                throw new Exception(String.Format("[BaseInfo.Update]: Object ID ({0}) is not set, unable to update this object.", TypeInfo.IDColumn));
            }

            SetObject();
        }


        /// <summary>
        /// Deletes the object using appropriate provider
        /// </summary>
        public virtual bool Delete()
        {
            if ((ObjectID <= 0) && (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                throw new Exception(String.Format("[BaseInfo.Delete]: Object ID ({0}) is not set, unable to delete this object.", TypeInfo.IDColumn));
            }

            DeleteObject();

            return true;
        }


        /// <summary>
        /// Destroys the object including its version history using appropriate provider
        /// </summary>
        public virtual bool Destroy()
        {
            if ((ObjectID <= 0) && (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                throw new Exception(String.Format("[BaseInfo.Destroy]: Object ID ({0}) is not set, unable to destroy this object.", TypeInfo.IDColumn));
            }

            // Delete without ensuring the version
            using (new CMSActionContext { CreateVersion = false })
            {
                DeleteObject();
            }

            return true;
        }


        /// <summary>
        /// Inserts the object using appropriate provider
        /// </summary>
        public virtual void Insert()
        {
            ObjectID = 0;

            SetObject();
        }


        /// <summary>
        /// Checks if a record with the same column values already exists in the database. Returns true if the set of values is unique.
        /// </summary>
        /// <param name="columns">Columns to check</param>
        public virtual bool CheckUniqueValues(params string[] columns)
        {
            // Get the data
            var q = GetDataQuery(
                true,
                s =>
                {
                    s.TopN(1).Column(TypeInfo.IDColumn);

                    // Build the where condition
                    foreach (string column in columns)
                    {
                        s.Where(column, QueryOperator.Equals, GetValue(column));
                    }
                },
                false
            );

            var ds = q.Result;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0) == ObjectID;
            }

            return true;
        }


        /// <summary>
        /// Gets dependency object type
        /// </summary>
        /// <param name="dep">Object dependency settings</param>
        public virtual string GetDependencyObjectType(ObjectDependency dep)
        {
            string objectType = dep.DependencyObjectType;

            // Dynamic object type
            if (dep.HasDynamicObjectType())
            {
                string colName = dep.ObjectTypeColumn;
                objectType = ValidationHelper.GetString(GetValue(colName), "");
            }

            return objectType;
        }

        #endregion


        #region "Name methods"

        /// <summary>
        /// Ensures the code name of the object if not set
        /// </summary>
        protected virtual void EnsureCodeName()
        {
            string column = TypeInfo.CodeNameColumn;
            if (column != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Check code name for emptiness or special value
                string value = ValidationHelper.GetString(GetValue(column), "");
                if (String.IsNullOrEmpty(value) || (value == InfoHelper.CODENAME_AUTOMATIC))
                {
                    // Ensure automatic code name
                    value = GetAutomaticCodeName();
                    value = GetUniqueCodeName(value, ObjectID);

                    SetValue(column, value);
                }
            }
        }


        /// <summary>
        /// Gets the automatic code name for the object
        /// </summary>
        protected virtual string GetAutomaticCodeName()
        {
            var value = ValidationHelper.GetCodeName(CoreServices.Localization.LocalizeString(ObjectDisplayName), useUnicode: false);
            if (String.IsNullOrEmpty(value))
            {
                // Ensure some value if result empty
                value = TypeHelper.GetNiceName(TypeInfo.ObjectType);
            }
            return value;
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        public virtual bool CheckUniqueCodeName()
        {
            string codeNameColumn = TypeInfo.CodeNameColumn;

            // If code name column does not exist, automatically pass
            if (codeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return true;
            }

            // Get name collisions
            var collisions = GetNameCollisions(ObjectID, codeNameColumn, ObjectCodeName, true);

            return !collisions.Any();
        }


        /// <summary>
        /// Ensures that the object has a unique code name within it's context
        /// </summary>
        protected virtual void EnsureUniqueCodeName()
        {
            ObjectCodeName = GetUniqueCodeName(ObjectCodeName, ObjectID);
        }


        /// <summary>
        /// Returns the unique code name for the specified object, does not check duplicity if duplicity occurs within the currentObjectId.
        /// </summary>
        /// <param name="codeName">Original code name</param>
        /// <param name="currentObjectId">Current object ID</param>
        protected virtual string GetUniqueCodeName(string codeName, int currentObjectId)
        {
            return GetUniqueName(codeName, currentObjectId, TypeInfo.CodeNameColumn, "_{0}", "[_](\\d+)$", true);
        }


        /// <summary>
        /// Returns the unique display name for the specified object.
        /// </summary>
        /// <param name="displayName">Original display name</param>
        /// <param name="currentObjectId">Current object ID</param>
        /// <param name="ensureLength">If true, maximal display name length is ensured</param>
        protected virtual string GetUniqueDisplayName(string displayName, int currentObjectId, bool ensureLength = true)
        {
            return GetUniqueName(displayName, currentObjectId, TypeInfo.DisplayNameColumn, " ({0})", "[ ](\\(\\d+\\))$", ensureLength);
        }


        /// <summary>
        /// Returns the unique code name for the specified object, does not check duplicity if duplicity occurs within the currentObjectId.
        /// </summary>
        /// <param name="name">Original code name</param>
        /// <param name="currentObjectId">Current object ID</param>
        /// <param name="columnName">Name of the column in which the uniqueness should be preserved (CodeNameColumn/DisplayNameColumn)</param>
        /// <param name="suffixFormat">Formatting string for the suffix (for example '_{0}' for code name or ' ({0})' for display name</param>
        /// <param name="suffixFormatRegex">Regex for formatting suffix (needed to remove suffix before finding the possible match in DB)</param>
        /// <param name="ensureLength">If true, maximal code name length is ensured</param>
        protected string GetUniqueName(string name, int currentObjectId, string columnName, string suffixFormat, string suffixFormatRegex, bool ensureLength)
        {
            // If nodeName not specified, do not process
            if ((name == null) || (columnName == ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || string.IsNullOrEmpty(columnName) || columnName.Contains(";"))
            {
                return "";
            }

            // Check localization macros
            bool wasLocalizable = false;
            if (name.StartsWith("{$", StringComparison.Ordinal) && name.EndsWith("$}", StringComparison.Ordinal))
            {
                name = CoreServices.Localization.LocalizeString(name);
                wasLocalizable = true;
            }

            name = name.Trim();

            // Cut to the allowed length
            if (ensureLength)
            {
                name = TypeHelper.EnsureMaxCodeNameLength(name, TypeInfo.MaxCodeNameLength);
            }

            // Get original name
            string originalName = name;
            if (!string.IsNullOrEmpty(suffixFormatRegex))
            {
                originalName = Regex.Replace(originalName, suffixFormatRegex, "");
                if (originalName == "")
                {
                    originalName = name;
                }
            }

            int maxLength = TypeHelper.GetMaxCodeNameLength(TypeInfo.MaxCodeNameLength);

            // Get all which may possibly match (supported up to "_999")
            string searchName = originalName;
            if (ensureLength && (searchName.Length > maxLength - 4))
            {
                searchName = searchName.Substring(0, maxLength - 4);
            }

            // Get the possible name collisions
            var collisions = GetNameCollisions(currentObjectId, columnName, searchName, false).ToList();
            if (collisions.Any())
            {
                var names = collisions.ToHashSetCollection(StringComparer.InvariantCultureIgnoreCase);

                bool unique = false;
                int uniqueIndex = 0;
                do
                {
                    // Get matching collisions
                    if (!names.Contains(name) && !wasLocalizable)
                    {
                        // If no collision found, consider as unique
                        unique = true;
                    }
                    else
                    {
                        // If match (duplicity found), create new name
                        uniqueIndex++;
                        string uniqueString = string.Format(suffixFormat, uniqueIndex);
                        int originalLength = maxLength - uniqueString.Length;
                        if (ensureLength && (originalName.Length > originalLength))
                        {
                            name = originalName.Substring(0, originalLength) + uniqueString;
                        }
                        else
                        {
                            name = originalName + uniqueString;
                        }

                        // Flip this flag to false (this flag is used to ensure, that delocalized names are appended with the suffix even though they are unique without the suffix)
                        wasLocalizable = false;
                    }
                }
                while (!unique);
            }
            else if (ensureLength)
            {
                name = TypeHelper.EnsureMaxCodeNameLength(name, TypeInfo.MaxCodeNameLength);
            }

            return name;
        }


        /// <summary>
        /// Gets the list of the name collisions of the given object
        /// </summary>
        /// <param name="currentObjectId">Current object ID</param>
        /// <param name="columnName">Column name</param>
        /// <param name="searchName">Search name for collision</param>
        /// <param name="exactMatch">If true, the names must match exactly</param>
        protected IEnumerable<string> GetNameCollisions(int currentObjectId, string columnName, string searchName, bool exactMatch)
        {
            var ti = TypeInfo;

            // If group ID not specified (otherwise site provided through group), add site where
            WhereCondition siteWhere = null;
            if (ObjectGroupID <= 0)
            {
                // Site where condition
                if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    siteWhere = GetSiteWhereCondition();

                    // If name is unique globally, add also option for global search
                    if (ti.NameGloballyUnique)
                    {
                        if (ObjectSiteID > 0)
                        {
                            // Include global items to search for site object
                            siteWhere.Or().WhereNull(ti.SiteIDColumn);
                        }
                        else
                        {
                            // Do not restrict just to global (check all sites) for global object
                            siteWhere = null;
                        }
                    }
                }
            }

            WhereCondition groupWhere = null;
            WhereCondition parentWhere = null;

            if (!ti.NameGloballyUnique)
            {
                // Group where condition
                groupWhere = GetGroupWhereCondition();

                // Parent where condition, do not apply if parent ID is group ID
                if (ti.GroupIDColumn != ti.ParentIDColumn)
                {
                    parentWhere = GetIdentifyingParentWhereCondition();
                }
            }

            // Name where condition
            var nameWhere = GetUniqueNameWhereCondition(columnName, searchName, currentObjectId, exactMatch);

            // Check duplicity
            var result = GetDataQuery(true, s => s
                .Where(siteWhere, groupWhere, parentWhere, nameWhere)
                .Column(columnName),
                false
            );

            return result.Select(dr => ValidationHelper.GetString(dr[columnName], ""));
        }


        /// <summary>
        /// Constructs base where condition for checking column value uniqueness. This method can be overridden in child classes to add more conditions.
        /// </summary>
        /// <param name="columnName">Name of the column in which the uniqueness should be preserved (CodeNameColumn/DisplayNameColumn)</param>
        /// <param name="searchName">Name which should be saved in the column (eventually with suffix)</param>
        /// <param name="currentObjectId">ID of the current object (this object will be excluded from the search for duplicate names)</param>
        /// <param name="exactMatch">If true, the names must match exactly</param>
        /// <returns>Where condition used to check for name uniqueness</returns>
        protected virtual WhereCondition GetUniqueNameWhereCondition(string columnName, string searchName, int currentObjectId, bool exactMatch)
        {
            var where = new WhereCondition(TypeInfo.IDColumn, QueryOperator.NotEquals, currentObjectId);

            if (exactMatch)
            {
                // Exact match
                where.Where(columnName, QueryOperator.Equals, searchName);
            }
            else
            {
                // Prefix match
                where.Where(columnName, QueryOperator.Like, searchName + "%");
            }

            return where;
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Checks whether the specified user has permissions for this object.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="currentSiteName">Name of the current context site. Permissions are checked on this site only when the site name cannot be obtained directly from the info object (from SiteIDColumn or site binding).</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        public bool CheckPermissions(PermissionsEnum permission, string currentSiteName, IUserInfo userInfo, bool exceptionOnFailure = false)
        {
            return CheckPermissionsWithHandler(permission, currentSiteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. Outcome of this method is determined by combining results of CheckPermissions event and CheckPermissionsInternal method.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="currentSiteName">Name of the current context site. Permissions are checked on this site only when the site name cannot be obtained directly from the info object (from SiteIDColumn or site binding).</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected virtual bool CheckPermissionsWithHandler(PermissionsEnum permission, string currentSiteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            // For site objects use their site instead of current site
            if (ObjectSiteID > 0)
            {
                currentSiteName = ObjectSiteName;
            }

            var e = new ObjectSecurityEventArgs
            {
                Object = this,
                User = userInfo,
                SiteName = currentSiteName,
                Permission = permission
            };

            // Handle the event
            using (var h = TypeInfo.Events.CheckPermissions.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Combine with default result
                    e.Result = e.Result.CombineWith(CheckPermissionsInternal(permission, currentSiteName, userInfo, exceptionOnFailure));
                }

                // Finish the event
                h.FinishEvent();
            }

            // Convert result to bool
            var boolResult = e.Result.ToBoolean();

            if (exceptionOnFailure)
            {
                PermissionCheckException(permission, currentSiteName, boolResult);
            }

            return boolResult;
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected virtual bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            // Get the real permission to check
            var checkPermission = GetPermissionToCheck(permission);
            if (checkPermission != permission)
            {
                // If different, fallback to the new permission to make sure overridden methods act accordingly
                return CheckPermissionsInternal(checkPermission, siteName, userInfo, exceptionOnFailure);
            }

            var boolResult = CheckDefaultPermissions(permission, siteName, userInfo).ToBoolean();

            // Throw an exception on failure
            if (exceptionOnFailure)
            {
                PermissionCheckException(permission, siteName, boolResult);
            }

            return boolResult;
        }


        /// <summary>
        /// Check universal permissions for an object
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User</param>
        protected AuthorizationResultEnum CheckDefaultPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo)
        {
            // Always return true for global administrator with global access
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return AuthorizationResultEnum.Allowed;
            }

            // Check for global DestroyObjects permission if action is Destroy.
            if ((permission == PermissionsEnum.Destroy) && (userInfo.IsAuthorizedPerResource("CMS.GlobalPermissions", "DestroyObjects", siteName, false)))
            {
                return AuthorizationResultEnum.Allowed;
            }

            var result = AuthorizationResultEnum.Insignificant;

            if (!string.IsNullOrEmpty(TypeInfo.ModuleName))
            {
                // Check permissions for given module with standard permission name
                string permissionName = GetPermissionName(permission);
                if (!String.IsNullOrEmpty(permissionName))
                {
                    List<SiteInfoIdentifier> sites;
                    string permissionToCheck;
                    PermissionObjectType type = GetPermissionObjectType(permissionName, out sites, out permissionToCheck);
                    switch (type)
                    {
                        case PermissionObjectType.OnlyAdmin:
                            // This object is only for global administrator, in this branch of code it's automatic denial, because global admin is checked already
                            result = AuthorizationResultEnum.Denied;
                            break;

                        case PermissionObjectType.CurrentSite:
                            result = result.CombineWith(userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, permissionToCheck, siteName, false));
                            break;

                        case PermissionObjectType.SpecifiedSite:
                            if (sites != null)
                            {
                                bool allowed = false;
                                foreach (var site in sites)
                                {
                                    // Check all the sites to which the object is assigned
                                    if (userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, permissionToCheck, site, false))
                                    {
                                        allowed = true;
                                        break;
                                    }
                                }
                                result = result.CombineWith(allowed);
                            }
                            else
                            {
                                // No sites to check, deny access
                                result = AuthorizationResultEnum.Denied;
                            }
                            break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the permission object type of the object (checks the SiteID column and SiteBinding columns). According to this value, the permission check is performed.
        /// </summary>
        protected virtual PermissionObjectType GetPermissionObjectType(string permissionName, out List<SiteInfoIdentifier> sites, out string permissionToCheck)
        {
            sites = null;
            permissionToCheck = permissionName;

            // Objects with SiteID column
            var siteBindingObject = TypeInfo.SiteBindingObject;
            if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (ObjectSiteID > 0)
                {
                    sites = new List<SiteInfoIdentifier> { ObjectSiteID };
                    return PermissionObjectType.SpecifiedSite;
                }

                if (siteBindingObject == null)
                {
                    // Object with only SiteID column with no site defined and no site bindings
                    permissionToCheck = GetGlobalPermissionName(permissionName);
                    return PermissionObjectType.CurrentSite;
                }
            }

            // Object with site bindings (special handling is only for existing objects, new objects with no ID have always no bindings, therefore the check should be against current site)
            if ((siteBindingObject != null) && (ObjectID > 0))
            {
                // Get all site IDs to which the object is assigned
                var siteBindingTypeInfo = siteBindingObject.TypeInfo;

                string siteCol = siteBindingTypeInfo.SiteIDColumn;

                var siteBindingWhere = new WhereCondition(siteBindingTypeInfo.ParentIDColumn, QueryOperator.Equals, ObjectID);
                var siteBindings = siteBindingObject.GetData(siteBindingWhere.Parameters, siteBindingWhere.WhereCondition, null, -1, siteCol, false);

                if (DataHelper.DataSourceIsEmpty(siteBindings))
                {
                    // Object is not assigned to any of the sites - only global administrator can access such object
                    return PermissionObjectType.OnlyAdmin;
                }
                else
                {
                    sites = new List<SiteInfoIdentifier>();
                    sites.AddRange(DataHelper.GetIntegerValues(siteBindings.Tables[0], siteCol).Select(x => new SiteInfoIdentifier(x)));

                    return PermissionObjectType.SpecifiedSite;
                }
            }

            // Default for objects with no site bindings, no site column
            return PermissionObjectType.CurrentSite;
        }


        /// <summary>
        /// Returns name of the global permission name corresponding to the given permission name.
        /// By default, "Global" + permissionName is returned.
        /// </summary>
        /// <param name="permissionName">Name of the original permission</param>
        protected virtual string GetGlobalPermissionName(string permissionName)
        {
            return "Global" + permissionName;
        }


        /// <summary>
        /// Fires an exception in case authorization result is false (denied or insignificant)
        /// </summary>
        /// <param name="permission">Checked permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="result">Result of the permission check</param>
        protected void PermissionCheckException(PermissionsEnum permission, string siteName, bool result)
        {
            // Convert result to bool
            if (!result)
            {
                var permissionName = GetPermissionName(permission);

                throw new PermissionCheckException(TypeInfo.ModuleName, permissionName, siteName);
            }
        }


        /// <summary>
        /// Gets permission to check for the object. By default the <see cref="PermissionsEnum.Create"/> and <see cref="PermissionsEnum.Delete"/> permission
        /// is changed to <see cref="PermissionsEnum.Modify"/>.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        protected virtual PermissionsEnum GetPermissionToCheck(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                    return PermissionsEnum.Modify;
            }

            return permission;
        }


        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called.
        ///
        /// Derived classes can override this method to change permission which is checked (for example check for global permissions if object is global).
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected virtual string GetPermissionName(PermissionsEnum permission)
        {
            permission = GetPermissionToCheck(permission);

            return permission.ToString();
        }

        #endregion


        #region "Property methods"

        /// <summary>
        /// Returns value for translation services. Returns unmodified field content by default.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        protected virtual object GetValueForTranslation(string columnName)
        {
            return GetValue(columnName);
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="columnName">Property name</param>
        protected virtual Type GetPropertyType(string columnName)
        {
            // Search by property name + ID (such as SiteDefaultStylesheet[ID])
            columnName = columnName + "ID";
            if (ContainsColumn(columnName))
            {
                string bindingType = GetObjectTypeForColumn(columnName);
                if (bindingType != null)
                {
                    // Get object
                    BaseInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                    if (bindingObj != null)
                    {
                        // Get the object type
                        return bindingObj.GetType();
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the double value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual double GetDoubleValue(string columnName, double defaultValue)
        {
            return ValidationHelper.GetDouble(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the decimal value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual decimal GetDecimalValue(string columnName, decimal defaultValue)
        {
            return ValidationHelper.GetDecimal(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the integer value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual int GetIntegerValue(string columnName, int defaultValue)
        {
            return ValidationHelper.GetInteger(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the DateTime value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual DateTime GetDateTimeValue(string columnName, DateTime defaultValue)
        {
            return ValidationHelper.GetDateTime(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the guid value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual Guid GetGuidValue(string columnName, Guid defaultValue)
        {
            return ValidationHelper.GetGuid(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the boolean value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual bool GetBooleanValue(string columnName, bool defaultValue)
        {
            return ValidationHelper.GetBoolean(GetValue(columnName), defaultValue);
        }


        /// <summary>
        /// Gets the string value from the object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual string GetStringValue(string columnName, string defaultValue)
        {
            return ValidationHelper.GetString(GetValue(columnName), defaultValue);
        }

        #endregion


        #region "Change management methods

        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public virtual void MakeComplete(bool loadFromDb)
        {
            // Do nothing in base class
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ItemChanged(string columnName)
        {
            return false;
        }


        /// <summary>
        /// Returns list of column names which values were changed.
        /// </summary>
        /// <returns>List of column names</returns>
        public virtual List<string> ChangedColumns()
        {
            return ColumnNames;
        }

        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
        public virtual bool DataChanged(string excludedColumns)
        {
            return true;
        }


        /// <summary>
        /// Returns the original value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetOriginalValue(string columnName)
        {
            return null;
        }

        #endregion


        #region "Hierarchy methods"

        /// <summary>
        /// Ensures the IDPath and Level columns.
        /// </summary>
        protected virtual void EnsureHierarchyColumns()
        {
            var ti = TypeInfo;

            // Get column settings
            string column = ti.ObjectIDPathColumn;
            string namePathColumn = ti.ObjectNamePathColumn;

            bool hasIDPath = (column != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            bool hasNamePath = (namePathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

            if (hasIDPath || hasNamePath)
            {
                string parentCol = null;

                if (ti.IsRelatedTo(ParentObjectType))
                {
                    parentCol = ParentObjectType;
                }
                else
                {
                    // Find hierarchy column in the object dependencies
                    foreach (var dep in ti.ObjectDependencies)
                    {
                        if (ti.IsRelatedTo(dep.DependencyObjectType))
                        {
                            parentCol = dep.DependencyColumn;
                            break;
                        }
                    }
                }

                if (parentCol != null)
                {
                    bool parentChanged = ItemChanged(parentCol);

                    // Update ID path
                    if (hasIDPath)
                    {
                        if (string.IsNullOrEmpty(GetStringValue(column, "")) || parentChanged)
                        {
                            UpdatePathColumn(parentCol, column, ti.IDColumn, parentChanged);
                        }
                    }

                    // Update name path
                    if (hasNamePath)
                    {
                        string namePathPartColumn = GetNamePathPartColumn();
                        if (string.IsNullOrEmpty(GetStringValue(namePathColumn, "")) || parentChanged || ItemChanged(namePathPartColumn))
                        {
                            UpdatePathColumn(parentCol, namePathColumn, namePathPartColumn, parentChanged || ItemChanged(namePathPartColumn));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Updates given path column.
        /// </summary>
        /// <param name="parentCol">Column which stores parent object ID</param>
        /// <param name="column">Path column</param>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        /// <param name="updateChildren">Determines whether the parent object changed and therefore it is necessary to update all children</param>
        /// <param name="updateLevel">If true, also the level of the object is update according to the path</param>
        private void UpdatePathColumn(string parentCol, string column, string pathPartColumn, bool updateChildren, bool updateLevel = true)
        {
            int level;
            var ti = TypeInfo;

            string levelColumn = ti.ObjectLevelColumn;
            string path = BuildObjectPath(parentCol, column, levelColumn, pathPartColumn, out level);

            // Update Level column
            if (updateLevel && (levelColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                SetValue(levelColumn, level);
            }

            // Update IDPath column
            SetValue(column, path);

            // Update all children IDPath and level
            if (updateChildren)
            {
                string originalValue = ValidationHelper.GetString(GetOriginalValue(column), "");
                if (!string.IsNullOrEmpty(originalValue))
                {
                    var dci = DataClassInfoProvider.GetDataClassInfo(ti.ObjectClassName);
                    if (dci != null)
                    {
                        QueryDataParameters parameters = new QueryDataParameters();
                        parameters.Add("UpdateLevel", updateLevel);

                        parameters.AddMacro("##TABLENAME##", dci.ClassTableName);
                        parameters.AddMacro("##PATHCOLUMN##", SqlHelper.AddSquareBrackets(column));
                        parameters.AddMacro("##LEVELCOLUMN##", SqlHelper.AddSquareBrackets(levelColumn));
                        parameters.AddMacro("##IDCOLUMN##", SqlHelper.AddSquareBrackets(ti.IDColumn));
                        parameters.AddMacro("##OLDPARENTID##", ObjectID.ToString());
                        parameters.AddMacro("##NEWPARENTID##", GetIntegerValue(parentCol, 0).ToString());
                        parameters.AddMacro("##OLDPREFIX##", SqlHelper.EscapeQuotes(originalValue + "/"));
                        parameters.AddMacro("##CURRENTITEM##", SqlHelper.EscapeQuotes(GetCurrentObjectPathPart(pathPartColumn)));

                        ConnectionHelper.ExecuteQuery("cms.class.UpdatePath", parameters);
                    }
                }
            }
        }


        /// <summary>
        /// Returns the length of a part of IDPath.
        /// </summary>
        protected virtual int GetIDPathPartLength()
        {
            return 8;
        }


        /// <summary>
        /// Returns the name of the column which is used to build the NamePath
        /// </summary>
        protected virtual string GetNamePathPartColumn()
        {
            return CodeNameColumn;
        }


        /// <summary>
        /// Builds the path from the given column.
        /// </summary>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        protected virtual string GetCurrentObjectPathPart(string pathPartColumn)
        {
            if (pathPartColumn == TypeInfo.IDColumn)
            {
                return ObjectID.ToString().PadLeft(GetIDPathPartLength(), '0');
            }

            return GetStringValue(pathPartColumn, "");
        }


        /// <summary>
        /// Builds the path from the given column.
        /// </summary>
        /// <param name="parentColumName">Column of the parent ID</param>
        /// <param name="pathColumnName">Column name to build the path from</param>
        /// <param name="levelColumnName">Column name of the level</param>
        /// <param name="level">Level of the object within the tree hierarchy</param>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        protected virtual string BuildObjectPath(string parentColumName, string pathColumnName, string levelColumnName, string pathPartColumn, out int level)
        {
            BaseInfo parent = GetObject(GetIntegerValue(parentColumName, 0));
            if (parent != null)
            {
                level = parent.GetIntegerValue(levelColumnName, 0) + 1;
                return parent.GetStringValue(pathColumnName, "").TrimEnd('/') + "/" + GetCurrentObjectPathPart(pathPartColumn);
            }

            level = 0;
            return "/";
        }

        #endregion


        #region "Protected generalized methods (accessible from Generalized wrapper)"

        /// <summary>
        /// Gets the list of synchronized columns for this object.
        /// </summary>
        /// <param name="excludeColumns">When true values is passed, columns from <see cref="SynchronizationSettings.ExcludedStagingColumns"/> are removed</param>
        protected virtual IEnumerable<string> GetSynchronizedColumns(bool excludeColumns = true)
        {
            var columns = ColumnNames.ToList();

            // Id is never synchronized
            columns.Remove(TypeInfo.IDColumn);

            // Excluded columns are not synchronized as well
            if (excludeColumns && TypeInfo.SynchronizationSettings.ExcludedStagingColumns != null)
            {
                columns.RemoveAll(i => TypeInfo.SynchronizationSettings.ExcludedStagingColumns.Contains(i, StringComparer.OrdinalIgnoreCase));
            }

            return columns;
        }


        /// <summary>
        /// Returns true if the object is child of the given object. If parameter parent is null, returns true only if the object is not a child of any object.
        /// </summary>
        /// <param name="parent">Parent to check</param>
        protected virtual bool IsChildOf(BaseInfo parent)
        {
            if (parent == null)
            {
                return (ObjectParentID == 0);
            }

            // Parent type doesn't match
            if (TypeInfo.ParentObjectType != parent.TypeInfo.ObjectType)
            {
                return false;
            }

            return (ObjectParentID == parent.Generalized.ObjectID);
        }


        /// <summary>
        /// Returns default object of given object type. Has to be overridden in specific info. Returns null by default. Example is UserInfo which returns user specified in the settings or Global Administrator.
        /// </summary>
        protected internal virtual BaseInfo GetDefaultObject()
        {
            // No default object by default
            return null;
        }


        /// <summary>
        /// Returns the default text representation in the macros.
        /// </summary>
        public virtual string ToMacroString()
        {
            if (DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return ObjectDisplayName;
            }

            return ToString();
        }


        /// <summary>
        /// Returns the text representation of the object
        /// </summary>
        public override string ToString()
        {
            var result = String.Format("{0} ({1})", GetType().FullName, TypeInfo.ObjectType);

            // Append display name
            if (DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                result += " - " + ObjectDisplayName;
            }

            return result;
        }


        /// <summary>
        /// By default, BaseInfo has no special macro representation.
        /// </summary>
        public virtual object MacroRepresentation()
        {
            return this;
        }


        /// <summary>
        /// Updates the data of the object from DB (updates also ObjectSettings).
        /// </summary>
        /// <param name="binaryData">Indicates whether to load also binary data</param>
        protected virtual void UpdateFromDB(bool binaryData)
        {
            var idColumn = TypeInfo.IDColumn;
            if (idColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return;
            }

            var q = GetDataQuery(true, s => s
                .Where(idColumn, QueryOperator.Equals, ObjectID)
                .TopN(1),
                false
            );

            q.IncludeBinaryData = binaryData;

            var ds = q.Result;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                LoadData(new LoadDataSettings(ds.Tables[0].Rows[0]));
            }

            // Get current object settings and forcibly update from DB
            var objectSettings = EnsureObjectSettings();
            if (objectSettings != null)
            {
                objectSettings.UpdateFromDB(binaryData);
            }

        }


        /// <summary>
        /// Disconnects the collection from the database.
        /// </summary>
        protected void Disconnect()
        {
            Interlocked.Increment(ref mDisconnectedCount);
        }


        /// <summary>
        /// Reconnects the collection to the database.
        /// </summary>
        protected void Reconnect()
        {
            Interlocked.Decrement(ref mDisconnectedCount);

            if (mDisconnectedCount < 0)
            {
                mDisconnectedCount = 0;
            }
        }


        /// <summary>
        /// Gets the object identifier that uniquely identifies the object
        /// </summary>
        protected string GetObjectIdentifier()
        {
            string id = null;

            var objId = ObjectID;
            if (objId > 0)
            {
                // Try object ID
                id = objId.ToString();
            }
            else
            {
                // Try object GUID
                var guid = ObjectGUID;
                if (guid != Guid.Empty)
                {
                    id = guid.ToString();
                }
            }

            return id;
        }


        /// <summary>
        /// Gets the unique string key for the object.
        /// </summary>
        protected virtual string GetObjectKey()
        {
            var id = GetObjectIdentifier();

            return String.Format("{0}_{1}", TypeInfo.ObjectType, id);
        }


        /// <summary>
        /// Gets the global lock object for all the instances of the object (locked on "objectType_objectId").
        /// </summary>
        protected virtual object GetLockObject()
        {
            string key = GetObjectKey();

            return LockHelper.GetLockObject(key);
        }


        /// <summary>
        /// Updates the parent object, enables to update the data that is overridden in the parent object before or after it is saved
        /// </summary>
        /// <param name="parent">Parent object that will be saved</param>
        protected virtual void SetParent(GeneralizedInfo parent)
        {
            // Set object
            parent.SetObject();
        }


        /// <summary>
        /// Updates the parent object by saving it (to update the timestamp).
        /// </summary>
        protected virtual void TouchParent()
        {
            if (CMSActionContext.CurrentTouchParent)
            {
                GeneralizedInfo parent = ObjectParent;
                if (parent != null)
                {
                    using (CMSActionContext context = new CMSActionContext())
                    {
                        // Disable creating version of parent object type if child object is versioned itself
                        // MetaFile is Exception, changing metafile causes parent to create new version always
                        context.CreateVersion = CMSActionContext.CurrentCreateVersion && ((TypeInfo.ObjectType == MetaFileInfo.OBJECT_TYPE) || !SupportsVersioning || !VersioningEnabled);

                        // Do not log events when the parent is just touched
                        context.LogEvents = false;

                        // Update the parent object
                        SetParent(parent);
                    }
                }
            }
        }


        /// <summary>
        /// Returns the parent object.
        /// </summary>
        protected virtual BaseInfo GetParent()
        {
            // Do not get parent if disconnected
            if (IsDisconnected)
            {
                return null;
            }

            int parentId = ObjectParentID;
            if (parentId > 0)
            {
                // Get the parent object by original object type
                var parInfo = ModuleManager.GetReadOnlyObject(ParentObjectType);
                if (parInfo != null)
                {
                    var parent = ProviderHelper.GetInfoById(parInfo.TypeInfo.OriginalObjectType, parentId);

                    return parent;
                }
            }

            return null;
        }


        /// <summary>
        /// Initialize the thumbnail info.
        /// </summary>
        private BaseInfo GetThumbnailInfo()
        {
            if (IsDisconnected)
            {
                return null;
            }

            // Get the GUID
            Guid metaFileGuid = ObjectThumbnailGUID;

            return (metaFileGuid != Guid.Empty) ? ProviderHelper.GetInfoByGuid(MetaFileInfo.OBJECT_TYPE, metaFileGuid, ObjectSiteID) : null;
        }


        /// <summary>
        /// Initialize the icon info.
        /// </summary>
        private BaseInfo GetIconInfo()
        {
            if (IsDisconnected)
            {
                return null;
            }

            // Get the GUID
            Guid metaFileGuid = ObjectIconGUID;

            return (metaFileGuid != Guid.Empty) ? ProviderHelper.GetInfoByGuid(MetaFileInfo.OBJECT_TYPE, metaFileGuid, ObjectSiteID) : null;
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected abstract void DeleteObject();


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected abstract void SetObject();


        /// <summary>
        /// Creates the clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public abstract BaseInfo CloneObject(bool clear = false);


        /// <summary>
        /// Copies memory properties
        /// </summary>
        /// <param name="infoObj">Target object instance</param>
        protected void CopyMemoryProperties(BaseInfo infoObj)
        {
            infoObj.LogSynchronization = mLogSynchronization;
            infoObj.UpdateTimeStamp = mUpdateTimeStamp;
            infoObj.LogExport = mLogExport;
            infoObj.SupportsVersioning = mSupportsVersioning;
            infoObj.LogEvents = mLogEvents;
            infoObj.LogIntegration = mLogIntegration;
            infoObj.LogWebFarmTasks = mLogWebFarmTasks;
            infoObj.TouchCacheDependencies = mTouchCacheDependencies;
            infoObj.ValidateCodeName = mValidateCodeName;
            infoObj.CheckUnique = mCheckUnique;
            infoObj.AllowRestore = mAllowRestore;
            infoObj.AllowClone = mAllowClone;
        }


        /// <summary>
        /// Invalidates the object in the object table.
        /// </summary>
        /// <param name="keepThisInstanceValid">If true, this object is marked as updated to behave as valid</param>
        protected virtual void Invalidate(bool keepThisInstanceValid)
        {
            // Check if the invalidation is supported by the object
            if (!TypeInfo.SupportsInvalidation)
            {
                throw new Exception("[BaseInfo.Invalidate]: Object type '" + TypeInfo.ObjectType + "' does not support invalidation. To enable it, set the SupportsInvalidation property of the object TypeInfo to true.");
            }

            TypeInfo.ObjectInvalidated(ObjectID);

            if (keepThisInstanceValid)
            {
                mLastUpdated = DateTime.Now;
            }
        }


        /// <summary>
        /// Returns true if the object is invalid.
        /// </summary>
        /// <param name="lastValid">Time when the object was last known as valid</param>
        protected virtual bool IsObjectInvalid(DateTime lastValid)
        {
            // Check if the invalidation is supported by the object
            if (!TypeInfo.SupportsInvalidation)
            {
                throw new Exception("[BaseInfo.IsObjectInvalid]: Object type '" + TypeInfo.ObjectType + "' does not support invalidation. To enable it, set the SupportsInvalidation property of the object TypeInfo to true.");
            }

            // Check invalidation from parent side
            var parentInfo = TypeInfo.ParentTypeInfo;
            if (parentInfo != null)
            {
                // All direct children are invalid
                if (parentInfo.AreChildrenInvalid(ObjectParentID, lastValid))
                {
                    return true;
                }
            }

            // Check when the last invalidation occurred
            if (TypeInfo.LastObjectInvalidated < lastValid)
            {
                return false;
            }

            return TypeInfo.IsObjectInvalid(ObjectID, lastValid);
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected virtual ICollection<string> GetCacheDependencies()
        {
            List<string> result = new List<string>();

            // Fill in the dependency array
            string objectType = TypeInfo.ObjectType.ToLowerInvariant();
            result.Add(objectType + "|all");

            // By ID
            if (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                result.Add(String.Format("{0}|byid|{1}", objectType, ObjectID));
            }

            // By code name
            if (TypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                result.Add(String.Format("{0}|byname|{1}", objectType, ObjectCodeName.ToLowerInvariant()));
            }

            // By code name
            if (TypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                result.Add(String.Format("{0}|byguid|{1}", objectType, ObjectGUID.ToString().ToLowerInvariant()));
            }

            // Object children
            int parentId = ObjectParentID;
            if (parentId > 0)
            {
                result.Add(String.Format("{0}|byid|{1}|children", ParentObjectType, parentId));

                result.Add(String.Format("{0}|byid|{1}|children|{2}", ParentObjectType, parentId, objectType));
            }

            return result;
        }


        /// <summary>
        /// Stores local settings for object instance.
        /// </summary>
        protected virtual void StoreSettings()
        {
            mLocalSettings = new object[]
                {
                    LogSynchronization,
                    LogWebFarmTasks,
                    TouchCacheDependencies,
                    UpdateTimeStamp,
                    LogEvents,
                    SupportsVersioning,
                    LogIntegration
                };
        }


        /// <summary>
        /// Restores local settings for object instance.
        /// </summary>
        protected virtual void RestoreSettings()
        {
            if (mLocalSettings != null)
            {
                LogSynchronization = (SynchronizationTypeEnum)mLocalSettings[0];
                LogWebFarmTasks = (bool)mLocalSettings[1];
                TouchCacheDependencies = (bool)mLocalSettings[2];
                UpdateTimeStamp = (bool)mLocalSettings[3];
                LogEvents = (bool)mLocalSettings[4];
                SupportsVersioning = (bool)mLocalSettings[5];
                LogIntegration = (bool)mLocalSettings[6];
            }
        }


        /// <summary>
        /// Gets the object editing page URL.
        /// </summary>
        protected virtual string GetEditingPageURL()
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);
            return dci.ClassEditingPageURL;
        }


        /// <summary>
        /// Returns true if the object is checked out by the specified user.
        /// </summary>
        /// <param name="user">User</param>
        protected virtual bool IsCheckedOutByUser(IUserInfo user)
        {
            // If locking not supported returns true automatically, otherwise return true if object is locked by user
            return !Generalized.TypeInfo.SupportsLocking || (IsCheckedOutByUserID == user.UserID);
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        /// <remarks>
        /// When overriding this method you should take into consideration <see cref="CMSActionContext.CurrentCheckLicense"/> flag.
        /// In case of failing license check you should throw <see cref="LicenseException"/> instead of returning false.
        /// </remarks>
        protected virtual bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            if (TypeInfo.Feature != FeatureEnum.Unknown)
            {
                return ObjectFactory<ILicenseService>.StaticSingleton().CheckLicense(TypeInfo.Feature, domainName);
            }

            return true;
        }


        /// <summary>
        /// Checks whether info object as available on given site. Available means that object is global or assigned to given site.
        /// </summary>
        /// <param name="site">Site identifier: site name or ID</param>
        protected bool IsAvailableOnSite(SiteInfoIdentifier site)
        {
            // Invalid site
            if (site <= 0)
            {
                return false;
            }

            // Check Site ID column.
            if (site == ObjectSiteID)
            {
                return true;
            }

            // Check binding objects
            BaseInfo bindingObject = TypeInfo.SiteBindingObject;

            // Object is global
            if ((bindingObject == null) && (ObjectSiteID <= 0))
            {
                return true;
            }

            // Try to find binding
            if (bindingObject != null)
            {
                var bindingTypeInfo = bindingObject.TypeInfo;

                var q = bindingObject.GetDataQuery(
                    true,
                    s => s
                        .Columns(bindingTypeInfo.SiteIDColumn)
                        .Where(bindingTypeInfo.ParentIDColumn, QueryOperator.Equals, ObjectID)
                        .Where(bindingTypeInfo.SiteIDColumn, QueryOperator.Equals, (int)site)
                        .TopN(1),
                    false
                );

                return !DataHelper.DataSourceIsEmpty(q.Result);
            }

            return false;
        }

        #endregion


        #region "Binary data methods"

        /// <summary>
        /// Makes sure that the binary data is loaded into binary column of the object when StoreToFileSystem is true.
        /// </summary>
        protected virtual byte[] EnsureBinaryData()
        {
            return EnsureBinaryData(false);
        }


        /// <summary>
        /// Returns the BinaryData object of the current instance. Has to be overridden by specific classes. Returns null by default.
        /// </summary>
        protected virtual BinaryData GetBinaryData()
        {
            return null;
        }


        /// <summary>
        /// Makes sure that the binary data is loaded into binary column of the object.
        /// </summary>
        /// <param name="forceLoadFromDB">If true, the data are loaded even from DB</param>
        protected internal virtual byte[] EnsureBinaryData(bool forceLoadFromDB)
        {
            var ti = TypeInfo;

            var binaryColumn = ti.BinaryColumn;
            if (binaryColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if ((forceLoadFromDB || (GetValue(binaryColumn) == null)) && ObjectID > 0)
                {
                    // If object has empty binary column, load the binary data from DB
                    var q = GetDataQuery(true, s => s
                        .Where(ti.IDColumn, QueryOperator.Equals, ObjectID)
                        .TopN(1)
                        .Column(binaryColumn),
                        false
                    );

                    var ds = q.Result;
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Save the data into the object
                        byte[] data = ds.Tables[0].Rows[0][0] as byte[];
                        SetValue(binaryColumn, data);
                        return data;
                    }
                }

                return GetValue(binaryColumn) as byte[];
            }
            return null;
        }

        #endregion


        #region "Get data methods"

        /// <summary>
        /// Gets the object by specified where condition.
        /// </summary>
        /// <param name="includeTypeCondition">If true, the type condition is included, otherwise selects all data from the data source</param>
        /// <param name="where">Where condition</param>
        protected BaseInfo GetObject(IWhereCondition where, bool includeTypeCondition = true)
        {
            // Get the data
            var q = GetDataQuery(includeTypeCondition, s => s
                    .Where(where)
                    .TopN(1),
                false
            );

            q.IncludeBinaryData = false;

            var ds = q.Result;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ModuleManager.GetObject(ds.Tables[0].Rows[0], TypeInfo.ObjectType);
            }

            return null;
        }


        /// <summary>
        /// Gets the object by its ID.
        /// </summary>
        /// <param name="objectId">Object ID</param>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ObjectTypeInfo.IDColumn"/> is not set.</exception>
        public BaseInfo GetObject(int objectId)
        {
            var ti = TypeInfo;
            if (ti.IDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                throw new InvalidOperationException("Object ID column for type '" + ti.ObjectType + "' is not specified.");
            }

            var where = new WhereCondition(ti.IDColumn, QueryOperator.Equals, objectId);

            // Get the object, we get the object specifically by ID, so type condition is not used in this call
            return GetObject(where, false);
        }


        /// <summary>
        /// Gets count of the objects filtered by given where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="topN">Top N records</param>
        protected int GetCount(string where, int topN)
        {
            var q = GetDataQuery(
                true,
                s => s
                    .Where(where)
                    .TopN(topN)
                    .Column(new AggregatedColumn(AggregationType.Count, TypeInfo.IDColumn)),
                false
            );

            return (int)q.Tables[0].Rows[0][0];
        }


        /// <summary>
        /// Gets the DataSet of all the objects modified from specified date.
        /// </summary>
        /// <param name="from">From time</param>
        /// <param name="parameters">Parameters for the data retrieval</param>
        protected IDataQuery GetModifiedFrom(DateTime from, Action<DataQuerySettings> parameters = null)
        {
            Action<DataQuerySettings> settings = null;

            // Add the time stamp where condition
            var ti = TypeInfo;

            var applyFrom = (from != DateTime.MinValue) && (ti.TimeStampColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

            if (applyFrom || (parameters != null))
            {
                settings = s =>
                {
                    // Time condition
                    if (applyFrom)
                    {
                        s.Where(ti.TimeStampColumn, QueryOperator.LargerOrEquals, from);
                    }

                    // Parameters
                    if (parameters != null)
                    {
                        parameters(s);
                    }
                };
            }

            var q = GetDataQuery(true, settings, false);

            q.IncludeBinaryData = false;

            return q;
        }


        /// <summary>
        /// Returns the data according to the set of input parameters.
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="binaryData">If true, binary data are returned in the result</param>
        protected virtual DataSet GetData(QueryDataParameters parameters, string where, string orderBy, int topN, string columns, bool binaryData)
        {
            // Get all if no time specified
            var query = GetDataQuery(true, s => s
                .Where(where)
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(columns),
                false
            );

            query.Parameters = parameters;
            query.IncludeBinaryData = binaryData;

            return query.Result;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        /// <param name="includeTypeCondition">If true, the type condition is included, otherwise selects all data from the data source</param>
        /// <param name="parameters">Parameters for the data retrieval</param>
        /// <param name="checkLicense">If true, the license is checked for this query</param>
        internal IDataQuery GetDataQuery(bool includeTypeCondition, Action<DataQuerySettings> parameters, bool checkLicense)
        {
            // Indicates if query should be allowed to return results.
            bool noResultsQuery = false;

            if (checkLicense)
            {
                using (var context = new CMSActionContext())
                {
                    // If query should return empty results when license check fails, disable license redirect.
                    if (CMSActionContext.CurrentEmptyDataForInvalidLicense)
                    {
                        context.AllowLicenseRedirect = false;
                        context.LogLicenseWarnings = false;
                    }

                    try
                    {
                        // Check the license for this type of objects.
                        CheckLicense();
                    }
                    catch (LicenseException)
                    {
                        // Set no results flag when queries should return empty results when license check fails.
                        if (CMSActionContext.CurrentEmptyDataForInvalidLicense)
                        {
                            noResultsQuery = true;
                        }
                        // Otherwise propagate exception
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            var query = GetDataQueryInternal();

            EnsureDefaultOrderIfRequired(query);

            // Set the query not to return any results after license check failed.
            if (noResultsQuery)
            {
                query.ReturnNoResults();
            }

            // Include type where condition
            if (includeTypeCondition || (parameters != null))
            {
                query.ApplySettings(s =>
                {
                    if (includeTypeCondition)
                    {
                        s.Where(TypeInfo.WhereCondition);
                    }

                    if (parameters != null)
                    {
                        parameters(s);
                    }
                });
            }

            return query;
        }


        private void EnsureDefaultOrderIfRequired(IDataQuery query)
        {
            // object query handles default order itself
            if (query is IObjectQuery)
            {
                return;
            }

            query.DefaultOrderByColumns = TypeInfo.DefaultOrderBy;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected virtual IDataQuery GetDataQueryInternal()
        {
            var provider = (IInternalProvider)TypeInfo.ProviderObject;
            return provider.GetGeneralObjectQuery(false);
        }


        /// <summary>
        /// Gets the parameterized query to get siblings of the object (If there is no parent-child hierarchy, query is parameterized for all objects)
        /// </summary>
        /// <param name="parameters">Parameters for the data retrieval</param>
        protected virtual IDataQuery GetSiblingsQueryInternal(Action<DataQuerySettings> parameters)
        {
            return GetDataQuery(true, s =>
            {
                // Apply siblings condition parameters
                s.Where(GetSiblingsWhereCondition());

                // Apply external parameters
                if (parameters != null)
                {
                    parameters(s);
                }
            }, false);
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected virtual BaseInfo GetExisting()
        {
            var where = GetExistingWhereCondition();

            return GetExistingBase(where);
        }


        /// <summary>
        /// Returns query that selects objects matching with <paramref name="whereCondition"/> based on <paramref name="isGuidColumnKnown"/>..
        /// </summary>
        /// <remarks>
        /// Meant for use in <see cref="GetExistingBase"/> only.
        /// <para>
        /// If <paramref name="isGuidColumnKnown"/> is <c>true</c>, all objects selected by the <paramref name="whereCondition"/> will be returned,
        /// otherwise only the very first object matching the <paramref name="whereCondition"/> will be returned by the resulting <see cref="IDataQuery"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetDataQuery(bool, Action{DataQuerySettings}, bool)"/>
        private IDataQuery GetDataQueryForExisting(IWhereCondition whereCondition, bool includeTypeCondition, bool isGuidColumnKnown)
        {
            IDataQuery query;
            if (isGuidColumnKnown)
            {
                // GUID column is known, GUID matching should take place, all matching objects needs to be selected
                query = GetDataQuery(
                    includeTypeCondition,
                    s => s.Where(whereCondition),
                    false);
            }
            else
            {
                // GUID column is not known, GUID matching is not an option, single matching object is enough
                query = GetDataQuery(
                    includeTypeCondition,
                    s => s.Where(whereCondition).TopN(1),
                    false);
            }
            query.IncludeBinaryData = false;

            return query;
        }


        /// <summary>
        /// Returns first data row from the <paramref name="query"/>'s result that has
        /// same value in column with <paramref name="guidColumnName"/> as this <see cref="BaseInfo"/>
        /// (provided the <paramref name="isGuidColumnKnown"/> is <c>true</c>).
        /// Otherwise, first row is return.
        /// </summary>
        /// <remarks>
        /// Meant for use in <see cref="GetExistingBase"/> only.
        /// <para>
        /// Provided <paramref name="query"/> is supposed to contain <see cref="BaseInfo"/>s of same <see cref="BaseInfo.TypeInfo"/>
        /// representing same object (matched via <see cref="ObjectTypeInfo.GUIDColumn"/> and/or <see cref="ObjectTypeInfo.CodeNameColumn"/>).
        /// </para>
        /// <para>If there are no rows in the first table of the <paramref name="query"/>'s result, <c>null</c> is returned.</para>
        /// </remarks>
        /// <seealso cref="GetDataQueryForExisting(IWhereCondition, bool, bool)"/>
        private DataRow GetDataRowWithSameGuidOrFirst(IDataQuery query, bool isGuidColumnKnown, string guidColumnName)
        {
            var dataSet = query.Result;
            if (DataHelper.DataSourceIsEmpty(dataSet))
            {
                // No matching data found
                return null;
            }

            DataRow dataRow = null;
            var table = dataSet
                .Tables[0]
                .AsEnumerable();

            if (isGuidColumnKnown)
            {
                var guid = GetGuidValue(guidColumnName, Guid.Empty);
                if (guid != Guid.Empty)
                {
                    dataRow = table.FirstOrDefault(row =>
                    {
                        var rowGuid = ValidationHelper.GetGuid(row[guidColumnName], Guid.Empty);

                        return rowGuid != Guid.Empty && rowGuid == guid;
                    });
                }
            }

            return dataRow ?? table.FirstOrDefault();
        }


        /// <summary>
        /// Returns single info object and ensures correct GUID vs. code name priority is used (if applicable) after resolving provided <paramref name="whereCondition"/>.
        /// <para>If no object match provided <paramref name="whereCondition"/>, null is returned.</para>
        /// </summary>
        protected BaseInfo GetExistingBase(IWhereCondition whereCondition, bool includeTypeCondition = true)
        {
            var guidColumnName = TypeInfo.GUIDColumn;
            var isGuidColumnKnown = guidColumnName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN;

            IDataQuery query = GetDataQueryForExisting(whereCondition, includeTypeCondition, isGuidColumnKnown);

            DataRow dataRow = GetDataRowWithSameGuidOrFirst(query, isGuidColumnKnown, guidColumnName);

            if (dataRow == null)
            {
                // No matching row found, no object
                return null;
            }
            return ModuleManager.GetObject(dataRow, TypeInfo.ObjectType);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        /// <param name="applyTypeCondition">If true, type condition is applied to the resulting where condition</param>
        protected WhereCondition GetExistingWhereCondition(bool applyTypeCondition)
        {
            var where = GetExistingWhereCondition();

            // Apply type condition
            if (applyTypeCondition)
            {
                if (where == null)
                {
                    where = new WhereCondition();
                }

                where.Where(TypeInfo.WhereCondition);
            }

            return where;
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        protected virtual WhereCondition GetExistingWhereCondition()
        {
            var ti = TypeInfo;

            // Add site ID condition only when the object has a direct link to site. Site is handled through parent ID for objects with parent.
            var hasSiteId = (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

            var where = hasSiteId ? GetSiteWhereCondition() : new WhereCondition();

            // Add parent ID condition
            where.Where(GetIdentifyingParentWhereCondition());

            bool whereValid = false;
            var codeNameWhere = new WhereCondition();

            // GUID (primary)
            if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                Guid guid = ObjectGUID;
                if (guid != Guid.Empty)
                {
                    codeNameWhere.Where(ti.GUIDColumn, QueryOperator.Equals, guid);
                    whereValid = true;
                }
            }

            // Code name (secondary)
            if (ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                string codeName = ObjectCodeName;
                if (!string.IsNullOrEmpty(codeName))
                {
                    codeNameWhere.Or().Where(CodeNameColumn, QueryOperator.Equals, codeName);
                    whereValid = true;
                }
            }

            where.Where(codeNameWhere);

            // If where condition not valid
            if (!whereValid)
            {
                if (!ti.IsBinding && (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                {
                    // ID column as last resort
                    where.Where(ti.IDColumn, QueryOperator.Equals, ObjectID);
                }
                else
                {
                    // For bindings, combination of IDs to connected objects is used to get an existing object
                    var dependencies = ti.ObjectDependencies;
                    if (dependencies != null)
                    {
                        // Dependencies
                        foreach (var dep in dependencies)
                        {
                            if (dep.DependencyType == ObjectDependencyEnum.Binding)
                            {
                                string columnName = dep.DependencyColumn;
                                where.Where(columnName, QueryOperator.Equals, GetValue(columnName));
                            }
                        }
                    }
                }
            }

            return where;
        }


        /// <summary>
        /// Gets the where condition for the parent object when parent object required to distinguish two objects of current type.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> if the object represents <see cref="ObjectTypeInfo.IsMainObject"/> and therefore can be uniquely identified without parent.
        /// </remarks>
        private WhereCondition GetIdentifyingParentWhereCondition()
        {
            if (TypeInfo.IsMainObject)
            {
                return null;
            }

            return new WhereCondition().WhereID(TypeInfo.PossibleParentIDColumn, ObjectParentID);
        }


        /// <summary>
        /// Gets the child object where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="objectType">Object type of the child object</param>
        protected virtual WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
        {
            // There is no special where condition by default, return given
            return where ?? new WhereCondition();
        }


        /// <summary>
        /// Gets the site where condition for the object
        /// </summary>
        protected WhereCondition GetSiteWhereCondition()
        {
            return new WhereCondition().WhereID(TypeInfo.SiteIDColumn, ObjectSiteID);
        }


        /// <summary>
        /// Gets the group where condition for the object
        /// </summary>
        protected WhereCondition GetGroupWhereCondition()
        {
            return new WhereCondition().WhereID(TypeInfo.GroupIDColumn, ObjectGroupID);
        }

        #endregion


        #region "Default data export methods"

        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected virtual string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            string where = null;

            var ti = TypeInfo;

            var settings = ti.DefaultData;
            if (settings != null)
            {
                where = settings.Where;

                // Handle site dependency
                if (globalOnly && (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                {
                    where = SqlHelper.AddWhereCondition(where, ti.SiteIDColumn + " IS NULL");
                }

                // Always exclude testing objects
                where = AddColumnPrefixesWhereCondition(where, DisplayNameColumn, excludedNames);
                where = AddColumnPrefixesWhereCondition(where, CodeNameColumn, excludedNames);

                if (recursive)
                {
                    // Add category where condition
                    var objectCategory = ObjectCategory;
                    if (objectCategory != null)
                    {
                        where = AddDependencyDefaultDataWhereCondition(where, objectCategory, ti.CategoryIDColumn, objectCategory.TypeInfo.ObjectType != ti.ObjectType, "AND", null, excludedNames);
                    }

                    // Add parent where condition
                    BaseInfo parent = ModuleManager.GetReadOnlyObject(ParentObjectType);
                    if ((parent != null) && (ti.ParentIDColumn != ti.CategoryIDColumn))
                    {
                        where = AddDependencyDefaultDataWhereCondition(where, parent, ti.ParentIDColumn, parent.TypeInfo.ObjectType != ti.ObjectType, "AND", null, excludedNames);
                    }

                    if (ti.DependsOn != null)
                    {
                        foreach (ObjectDependency dependency in ti.DependsOn)
                        {
                            if ((dependency.DependencyColumn != ti.ParentIDColumn) && (dependency.DependencyColumn != ti.CategoryIDColumn))
                            {
                                switch (dependency.DependencyType)
                                {
                                    case ObjectDependencyEnum.Binding:
                                    case ObjectDependencyEnum.Required:
                                        // Add dependency where condition
                                        {
                                            BaseInfo dependencyInfo = ModuleManager.GetReadOnlyObject(dependency.DependencyObjectType);
                                            where = AddDependencyDefaultDataWhereCondition(where, dependencyInfo, dependency.DependencyColumn, dependency.DependencyObjectType != ti.ObjectType);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return where;
        }


        /// <summary>
        /// Adds default data where condition of given dependency on given <paramref name="dependencyIDColumn"/> to given <paramref name="where" /> condition.
        /// Returns original <paramref name="where"/> condition if no such dependency exists.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="dependencyInfo"><see cref="BaseInfo"/> of dependency object type.</param>
        /// <param name="dependencyIDColumn">Column with ID to specified dependency object.</param>
        /// <param name="recursive">Indicates whether added dependency where condition should process its dependencies</param>
        /// <param name="op">Operator used to connect old where with the new one. By default this is 'AND'.</param>
        /// <param name="dependencyTargetIDColumn">Name of the column on dependency target object that contains dependency ID. By default ID column of the target object is used.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected static string AddDependencyDefaultDataWhereCondition(string where, BaseInfo dependencyInfo, string dependencyIDColumn, bool recursive, string op = "AND", string dependencyTargetIDColumn = null, IEnumerable<string> excludedNames = null)
        {
            if ((dependencyInfo != null) && !String.IsNullOrEmpty(dependencyIDColumn))
            {
                string dependencyWhere = dependencyInfo.GetDefaultDataWhereCondition(recursive, true, excludedNames);
                if (!String.IsNullOrEmpty(dependencyWhere))
                {
                    var depTypeInfo = dependencyInfo.TypeInfo;

                    // Get parent DataClass
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(depTypeInfo.ObjectClassName);

                    if (String.IsNullOrEmpty(dependencyTargetIDColumn))
                    {
                        dependencyTargetIDColumn = depTypeInfo.IDColumn;
                    }

                    dependencyWhere = String.Format("{0} IN (SELECT {1} FROM {2} WHERE {3})", dependencyIDColumn, dependencyTargetIDColumn, dci.ClassTableName, dependencyWhere);
                    dependencyWhere = SqlHelper.AddWhereCondition(dependencyIDColumn + " IS NULL", dependencyWhere, "OR");

                    return SqlHelper.AddWhereCondition(where, dependencyWhere, op);
                }

                if (dependencyInfo.TypeInfo.DefaultData == null)
                {
                    // Do not export objects for object type without default data settings
                    where = SqlHelper.AddWhereCondition(where, SqlHelper.NO_DATA_WHERE);
                }
            }

            return where;
        }


        /// <summary>
        /// Adds restriction to given <paramref name="where" /> filtering out all rows where given column starts with one of excluded prefixes found in TypeInfo default data settings.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="columnName">Column that must not start with excluded prefixes.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected string AddColumnPrefixesWhereCondition(string where, string columnName, IEnumerable<string> excludedNames = null)
        {
            var ti = TypeInfo;

            if ((columnName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (columnName != ti.GUIDColumn))
            {
                IEnumerable<string> prefixes = ti.DefaultData.ExcludedPrefixes;
                if (excludedNames != null)
                {
                    prefixes = excludedNames.Union(prefixes);
                }
                foreach (string prefix in prefixes)
                {
                    where = SqlHelper.AddWhereCondition(where, String.Format("{0} NOT LIKE '{1}%'", columnName, prefix));
                }
            }

            return where;
        }


        /// <summary>
        /// Returns names of all columns that should be exported with default data as a comma separated string.
        /// </summary>
        protected virtual string GetDefaultDataExportColumns()
        {
            var ti = TypeInfo;
            var csi = ti.ClassStructureInfo;

            IEnumerable<string> columns = csi.ColumnNames;

            var excludedColumns = ti.DefaultData.ExcludedColumns;
            if (excludedColumns != null)
            {
                columns = columns.Except(excludedColumns);
            }

            return SqlHelper.JoinColumnList(columns);
        }


        /// <summary>
        /// Exports the default object installation data
        /// </summary>
        /// <param name="filePath">File path for the export</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected virtual void ExportDefaultData(string filePath, IEnumerable<string> excludedNames = null)
        {
            var data = GetDefaultData(excludedNames);
            WriteDefaultData(filePath, data);
        }


        /// <summary>
        /// Returns the default object installation data
        /// </summary>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected virtual DataSet GetDefaultData(IEnumerable<string> excludedNames = null)
        {
            var settings = TypeInfo.DefaultData;

            if (settings != null)
            {
                string where = GetDefaultDataWhereCondition(true, true, excludedNames);

                var query = GetDataQuery(true, s => s
                    .Where(where)
                    .OrderBy(settings.OrderBy),
                    false
                    );

                var columns = GetDefaultDataExportColumns();

                query.SelectColumnsList.Load(columns);

                // Get the data
                query.IncludeBinaryData = true;

                DataSet result = query.Result;

                RecomputeChildCountColumns(settings, result, excludedNames);

                return result;
            }

            return null;
        }


        /// <summary>
        /// Re-computes count of child objects.
        /// </summary>
        /// <param name="settings">Default data settings.</param>
        /// <param name="defaultData">Default data whose child count is recomputed.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out in child objects (if they need to be fetched).</param>
        private void RecomputeChildCountColumns(DefaultDataSettings settings, DataSet defaultData, IEnumerable<string> excludedNames = null)
        {
            var childDependencies = settings.ChildDependencies;
            if ((childDependencies != null) && !DataHelper.DataSourceIsEmpty(defaultData))
            {
                DataTable defaultDataTable = defaultData.Tables[0];

                // Avoid possible multiple enumeration
                IEnumerable<string> excludedNamesEnumerated = (excludedNames == null) ? null : excludedNames as IList<string> ?? excludedNames.ToList();

                foreach (DefaultDataChildDependency childDependency in childDependencies)
                {
                    string childObjectType = childDependency.ChildObjectType;
                    DataTable childDataTable = null;

                    if (TypeInfo.ObjectType.Equals(childObjectType, StringComparison.OrdinalIgnoreCase))
                    {
                        // Recursive dependency
                        childDataTable = defaultDataTable;
                    }
                    else
                    {
                        // Dependency on different object type
                        var childObject = ModuleManager.GetReadOnlyObject(childObjectType);
                        DataSet childDefaultData = childObject.GetDefaultData(excludedNamesEnumerated);

                        if (!DataHelper.DataSourceIsEmpty(childDefaultData))
                        {
                            childDataTable = childDefaultData.Tables[0];
                        }
                    }

                    DataHelper.RecomputeChildCount(defaultDataTable, childDependency.IdColumn, childDependency.ChildCountColumn, childDataTable, childDependency.ParentIdColumn);
                }
            }
        }


        /// <summary>
        /// Writes the default object installation data into xml file
        /// </summary>
        /// <param name="filePath">File path for the export</param>
        /// <param name="data">Default data to write</param>
        protected virtual void WriteDefaultData(string filePath, DataSet data)
        {
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);
                data.Tables[0].TableName = dci.ClassTableName;

                // If other type sharing the same database table has already been exported, merge the data
                if (File.Exists(filePath))
                {
                    DataSet otherObjectType = new DataSet();

                    // ReadXml does not support zip provider
                    SystemIO.Stream fileStream = File.OpenRead(filePath);
                    try
                    {
                        otherObjectType.ReadXml(fileStream, XmlReadMode.ReadSchema);
                    }
                    finally
                    {
                        fileStream.Close();
                    }

                    data.Merge(otherObjectType);
                }

                // Save the data
                SystemIO.Stream output = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                try
                {
                    data.WriteXml(output, XmlWriteMode.WriteSchema);
                }
                finally
                {
                    output.Close();
                }
            }
        }

        #endregion


        #region "IDataContainer methods"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Sets the object value to the nullable column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        /// <param name="condition">Condition for the valid value, if false, NULL is saved</param>
        public virtual void SetValue(string columnName, object value, bool condition)
        {
            // If condition is not met, set null
            if (!condition)
            {
                value = null;
            }

            SetValue(columnName, value);
        }


        /// <summary>
        /// Sets the object value to the nullable column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        /// <param name="nullValue">Null value, if matched, NULL is saved</param>
        public virtual void SetValue(string columnName, object value, object nullValue)
        {
            // If value equals null value, set null
            if ((value != null) && value.Equals(nullValue))
            {
                value = null;
            }

            SetValue(columnName, value);
        }


        /// <summary>
        /// Sets the object value.
        /// </summary>
        public abstract bool SetValue(string columnName, object value);


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public abstract bool TryGetValue(string columnName, out object value);


        /// <summary>
        /// Locks the object as a read-only
        /// </summary>
        protected internal abstract void SetReadOnly();


        /// <summary>
        /// Gets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public abstract List<string> ColumnNames
        {
            get;
        }


        /// <summary>
        /// Returns true if the object contains given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public abstract bool ContainsColumn(string columnName);


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected abstract List<string> GetColumnNames();


        /// <summary>
        /// Gets list of custom properties.
        /// </summary>
        protected virtual List<string> GetCustomProperties()
        {
            return null;
        }


        /// <summary>
        /// Gets list of registered properties.
        /// </summary>
        protected virtual List<string> GetRegisteredProperties()
        {
            return null;
        }

        #endregion


        #region "IHierarchicalDataContainer methods"

        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <param name="notNull">If true, the property attempts to return non-null values, at least it returns the empty object of the correct type</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetProperty(string columnName, out object value, bool notNull)
        {
            bool result = TryGetProperty(columnName, out value);

            // Handle the null value
            if (notNull && result && (value == null))
            {
                Type propertyType = GetPropertyType(columnName);

                value = ClassHelper.GetEmptyObject(propertyType);
            }

            return result;
        }


        /// <summary>
        /// Gets the object type for the given column or null if the object type is not found or unknown.
        /// </summary>
        /// <param name="columnName">Column name to check</param>
        protected virtual string GetObjectTypeForColumn(string columnName)
        {
            return TypeInfo.GetObjectTypeForColumn(columnName);
        }


        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present or at least property exists)</returns>
        public virtual bool TryGetProperty(string columnName, out object value)
        {
            // Try to get the regular field value
            return TryGetValue(columnName, out value);
        }


        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetProperty(string columnName)
        {
            // Get the value
            object value;
            TryGetProperty(columnName, out value);

            return value;
        }

        #endregion


        #region "IAdvancedDataContainer members"

        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        public abstract bool HasChanged
        {
            get;
        }


        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        public abstract void RevertChanges();


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged according to the asUnchanged parameter.
        /// </summary>
        public abstract void ResetChanges();

        #endregion


        #region "IComparable Members"

        /// <summary>
        /// Returns the code name of the object
        /// </summary>
        protected virtual string Name
        {
            get
            {
                string name = ObjectCodeName;
                if (String.IsNullOrEmpty(name))
                {
                    name = ObjectID.ToString();
                }

                return name;
            }
        }


        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        public int CompareTo(object obj)
        {
            var info = obj as BaseInfo;
            if (info != null)
            {
                return String.Compare(Name, info.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            throw new Exception("[BaseInfo.CompareTo]: Cannot compare BaseInfo with other type.");
        }

        #endregion


        #region "Nested repositories methods"

        /// <summary>
        /// Clears the nested cached objects
        /// </summary>
        protected void ClearCache()
        {
            mChildren = null;
            mChildDependencies = null;

            mBindings = null;
            mOtherBindings = null;

            mReferringObjects = null;
        }


        /// <summary>
        /// Gets the object children repository
        /// </summary>
        protected InfoObjectRepository GetObjectChildren()
        {
            var ti = TypeInfo;
            if (ti.ChildObjectTypes == null)
            {
                return null;
            }

            if (mChildren == null)
            {
                lock (lockObject)
                {
                    if (mChildren == null)
                    {
                        // Create new repository for the child objects
                        InfoObjectRepository children = new InfoObjectRepository(this);

                        AddCollections(children, ti.ChildObjectTypes, ObjectHelper.GetParentIdColumn, (col, childTypeInfo) =>
                        {
                            col.WhereCondition = GetChildWhereCondition(null, childTypeInfo.ObjectType);
                            col.OrderBy = childTypeInfo.DefaultOrderBy;
                        });

                        mChildren = children;
                    }
                }
            }

            return mChildren;
        }


        /// <summary>
        /// Gets the child dependencies repository
        /// </summary>
        protected InfoObjectRepository GetChildDependencies()
        {
            var ti = TypeInfo;
            var objectDependencies = ti.ObjectDependencies;

            if (String.IsNullOrEmpty(ti.ChildDependencyColumns) || (objectDependencies == null))
            {
                return null;
            }

            if (mChildDependencies == null)
            {
                lock (lockObject)
                {
                    if (mChildDependencies == null)
                    {
                        // Create new repository for the child objects
                        var dependencies = new InfoObjectRepository((ICMSStorage)Generalized);

                        var objectIDs = new Dictionary<string, List<int>>();

                        // Collect IDs for each dependency object type (more dependency columns can have same object type)
                        var depColumns = ti.ChildDependencyColumns.Split(';');

                        // Process all columns
                        foreach (string depColumn in depColumns)
                        {
                            // Process all dependencies
                            foreach (var dep in objectDependencies)
                            {
                                var dependencyColumn = dep.DependencyColumn;
                                if (dependencyColumn.Equals(depColumn, StringComparison.OrdinalIgnoreCase))
                                {
                                    var dependencyType = GetDependencyObjectType(dep);

                                    if (!string.IsNullOrEmpty(dependencyType))
                                    {
                                        List<int> ids;
                                        // Ensure the list of IDs
                                        if (!objectIDs.TryGetValue(dependencyType, out ids))
                                        {
                                            ids = new List<int>();
                                            objectIDs[dependencyType] = ids;
                                        }

                                        // Add ID to the list
                                        var value = ValidationHelper.GetInteger(GetValue(dependencyColumn), 0);
                                        if (value > 0)
                                        {
                                            ids.Add(value);
                                        }
                                    }
                                }
                            }
                        }

                        // Create the collection for each object type
                        foreach (var obj in objectIDs)
                        {
                            var dependency = ModuleManager.GetReadOnlyObject(obj.Key);
                            if (dependency != null)
                            {
                                var depTypeInfo = dependency.TypeInfo;

                                var where = new WhereCondition().WhereIn(depTypeInfo.IDColumn, obj.Value);

                                string orderBy = null;
                                if (dependency.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                {
                                    orderBy = dependency.DisplayNameColumn;
                                }

                                string name = depTypeInfo.MacroCollectionName;

                                var settings = new InfoCollectionSettings(name, obj.Key)
                                {
                                    WhereCondition = where,
                                    OrderBy = orderBy
                                };

                                dependencies.AddCollection(settings);
                            }
                        }

                        mChildDependencies = dependencies;
                    }
                }
            }

            return mChildDependencies;
        }


        /// <summary>
        /// Gets the repository of depending objects
        /// </summary>
        protected InfoObjectRepository GetReferringObjects()
        {
            if (mReferringObjects == null)
            {
                lock (lockObject)
                {
                    if (mReferringObjects == null)
                    {
                        var referring = new InfoObjectRepository();

                        var ti = TypeInfo;

                        // Process all object types except the binding ones
                        foreach (var objType in ObjectTypeManager.AllExceptBindingObjectTypes)
                        {
                            var obj = ModuleManager.GetReadOnlyObject(objType);

                            if (obj != null)
                            {
                                var dependencies = obj.TypeInfo.ObjectDependencies;

                                if (dependencies != null)
                                {
                                    var types = new SafeDictionary<string, string>();

                                    foreach (var dep in dependencies)
                                    {
                                        string depObjType = dep.DependencyObjectType;
                                        string baseWhere = "";

                                        if (string.IsNullOrEmpty(depObjType))
                                        {
                                            baseWhere = dep.ObjectTypeColumn + " = N'" + ti.ObjectType + "' AND " + dep.DependencyColumn + " = ##ID##";
                                        }
                                        else if (depObjType == ti.ObjectType)
                                        {
                                            baseWhere = dep.DependencyColumn + " = ##ID##";
                                        }

                                        if (!string.IsNullOrEmpty(baseWhere))
                                        {
                                            types[objType] = SqlHelper.AddWhereCondition(types[objType], baseWhere, "OR");
                                        }
                                    }

                                    foreach (var type in types.TypedKeys)
                                    {
                                        var registeredType = type.Equals(objType, StringComparison.OrdinalIgnoreCase) ? obj : ModuleManager.GetReadOnlyObject(type);

                                        var col = referring.AddCollection(new InfoCollectionSettings(registeredType.TypeInfo.ObjectClassName, type));

                                        var where = types[type];
                                        col.DynamicWhere = () => where.Replace("##ID##", ObjectID.ToString());
                                    }
                                }
                            }
                        }

                        mReferringObjects = referring;
                    }
                }
            }
            return mReferringObjects;
        }


        /// <summary>
        /// Gets the bindings repository for the object
        /// </summary>
        private BindingRepository GetBindings()
        {
            var ti = TypeInfo;
            if (ti.BindingObjectTypes == null)
            {
                return null;
            }

            if (mBindings == null)
            {
                lock (lockObject)
                {
                    if (mBindings == null)
                    {
                        // Create new repository for the child objects
                        var bindings = new BindingRepository(this);

                        // Prepare site ID for child collections, we only want bindings to the objects on the same site
                        int siteId = ObjectSiteID;
                        if (ti.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            siteId = -1;
                        }

                        // Add bindings collections to the repository
                        AddCollections(bindings, ti.BindingObjectTypes, ObjectHelper.GetParentIdColumn, (col, bindingTypeInfo) => { col.SiteID = siteId; });

                        mBindings = bindings;
                    }
                }
            }

            return mBindings;
        }


        /// <summary>
        /// Gets the repository of other bindings
        /// </summary>
        protected OtherBindingRepository GetOtherBindings()
        {
            var ti = TypeInfo;
            if (ti.OtherBindingObjectTypes == null)
            {
                return null;
            }

            if (mOtherBindings == null)
            {
                lock (lockObject)
                {
                    if (mOtherBindings == null)
                    {
                        // Create new repository for the child objects
                        var bindings = new OtherBindingRepository(this);

                        // Add other bindings collections to the repository
                        AddCollections(bindings, ti.OtherBindingObjectTypes, ObjectHelper.GetOtherBindingParentIdColumn);

                        mOtherBindings = bindings;
                    }
                }
            }

            return mOtherBindings;
        }


        /// <summary>
        /// Adds the list of collections into the given repository
        /// </summary>
        /// <param name="repository">Repository</param>
        /// <param name="objectTypes">Object types to add</param>
        /// <param name="getParentColumn">Function to get the parent column for a specific collection</param>
        /// <param name="collectionSetup">Optional extra collection setup</param>
        private void AddCollections(InfoObjectRepository repository, IEnumerable<string> objectTypes, Func<ObjectTypeInfo, ObjectTypeInfo, string> getParentColumn, Action<InfoCollectionSettings, ObjectTypeInfo> collectionSetup = null)
        {
            var ti = TypeInfo;

            // Add collection for each child type
            foreach (string objectType in objectTypes)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                if (typeInfo != null)
                {
                    var parentIdColumn = getParentColumn(typeInfo, ti);

                    // Register the collection of children
                    string name = typeInfo.MacroCollectionName;

                    var col = repository.AddCollection(new InfoCollectionSettings(name, objectType));
                    if (collectionSetup != null)
                    {
                        collectionSetup(col, typeInfo);
                    }

                    // Use parent object type for getting the correct ID column. The reference may point to just sub-part of composite object
                    var parentObjectType = typeInfo.GetObjectTypeForColumn(parentIdColumn);
                    var idColumn = ObjectTypeManager.GetTypeInfo(parentObjectType).IDColumn;

                    // Add dynamic where condition on parent
                    col.DynamicWhere = () => parentIdColumn + " = " + GetIntegerValue(idColumn, 0);
                }
            }
        }

        #endregion


        #region "Icon / Thumbnail methods"

        /// <summary>
        /// Gets the object thumbnail URL
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="maxSideSize">Maximum side size, width or height</param>
        protected virtual string GetThumbnailUrl(int width, int height, int maxSideSize)
        {
            return GetMetaFileUrl(TypeInfo.ThumbnailGUIDColumn, width, height, maxSideSize);
        }


        /// <summary>
        /// Gets the object icon URL
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="maxSideSize">Maximum side size, width or height</param>
        protected virtual string GetIconUrl(int width, int height, int maxSideSize)
        {
            return GetMetaFileUrl(TypeInfo.IconGUIDColumn, width, height, maxSideSize);
        }


        /// <summary>
        /// Gets the object meta file URL
        /// </summary>
        /// <param name="columnName">Meta file column name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="maxSideSize">Maximum side size, width or height</param>
        private string GetMetaFileUrl(string columnName, int width, int height, int maxSideSize)
        {
            string fileGuid = null;
            if (columnName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                fileGuid = GetStringValue(columnName, null);
            }

            if (!string.IsNullOrEmpty(fileGuid))
            {
                StringBuilder sb = new StringBuilder();

                // Serve file through the metafile
                sb.Append("~/CMSPages/GetMetafile.aspx?fileguid=");
                sb.Append(GetValue(columnName));

                if (maxSideSize > 0)
                {
                    // Max side size
                    sb.Append("&maxsidesize=");
                    sb.Append(maxSideSize);
                }
                else
                {
                    // Width / height
                    if (width > 0)
                    {
                        sb.Append("&width=");
                        sb.Append(width);
                    }
                    if (height > 0)
                    {
                        sb.Append("&height=");
                        sb.Append(height);
                    }
                }

                return sb.ToString();
            }

            return null;
        }

        #endregion


        #region "Cloning methods"

        /// <summary>
        /// Inserts the object as a new object to the DB with inner data and structure (according to given settings) cloned from the original.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <returns>Returns the newly created clone</returns>
        protected virtual BaseInfo InsertAsClone(CloneSettings settings, CloneResult result)
        {
            return InfoCloneHelper.InsertAsClone(this, settings, result);
        }


        /// <summary>
        /// This method is called on cloned object prepared in memory by InsertAsClone method.
        /// Override if you need to do further actions before inserting actual object to DB (insert special objects, modify foreign keys, copy files, etc.).
        /// Calls Insert() by default.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected internal virtual void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            Insert();
        }


        /// <summary>
        /// This method is called once the object is completely cloned (with all children, bindings, etc.).
        /// Override if you need to do further actions after the object has been cloned.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected internal virtual void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // No actions needed by default
        }

        #endregion


        #region "Order methods"

        /// <summary>
        /// Inits the proper order of the sibling objects so the order column is consistent.
        /// </summary>
        /// <param name="orderColumn">Name of the column that is used for storing the object's order. If null, <see cref="ObjectTypeInfo.OrderColumn"/> is taken from <see cref="TypeInfo"/>.</param>
        /// <param name="nameColumn">>Name of the column by which the order is to be set. <see cref="ObjectTypeInfo.DisplayNameColumn"/> is used if not given.</param>
        /// <param name="ascending">Indicates whether the order is ascending or descending (ascending order is used by default).</param>
        protected virtual void InitObjectsOrder(string orderColumn, string nameColumn = null, bool ascending = true)
        {
            if (string.IsNullOrEmpty(orderColumn))
            {
                orderColumn = TypeInfo.OrderColumn;
            }
            if (orderColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var where = GetSiblingsWhereCondition();
                var bulkUpdateArgs = new BulkUpdateEventArgs(TypeInfo, where, new[] { orderColumn });
                using (var bulkUpdateEvent = TypeInfo.Events.BulkUpdate.StartEvent(bulkUpdateArgs))
                {
                    if (bulkUpdateEvent.CanContinue())
                    {
                        InitObjectsOrderInternal(orderColumn, nameColumn, where, ascending);
                    }

                    bulkUpdateEvent.FinishEvent();
                }
            }
        }


        /// <summary>
        /// Copy value of external columns directly via set value
        /// </summary>
        /// <param name="target">Target info class</param>
        protected internal void CopyExternalColumns(BaseInfo target)
        {
            if (mTypeInfo.HasExternalColumns && !IgnoreExternalColumns)
            {
                foreach (String columnName in GetExternalColumns())
                {
                    object val = GetValue(columnName);
                    target.SetValue(columnName, val);
                }
            }
        }


        /// <summary>
        /// Method called after the InitObjectOrder method is called. Override this to do further actions after order initialization.
        /// </summary>
        protected virtual void InitObjectsOrderPostprocessing()
        {
            ProviderHelper.ClearHashtables(TypeInfo.ObjectType, true);
        }


        /// <summary>
        /// Returns number which will be the last order within all the other items (according to Parent, Group and Site settings).
        /// I.e. returns largest existing order + 1.
        /// </summary>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        protected virtual int GetLastObjectOrder(string orderColumn)
        {
            var ti = TypeInfo;

            string orderCol = (string.IsNullOrEmpty(orderColumn) ? ti.OrderColumn : orderColumn);
            if (orderCol != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var q = GetSiblingsQueryInternal(s =>
                {
                    s.OrderByDescending(orderCol);
                    s.TopN(1);
                    s.Column(orderCol);

                    // Do not include object itself
                    s.WhereNot(GetOrderIdentityWhereCondition());
                });

                var ds = q.Result;

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0) + 1;
                }

                return 1;
            }
            return 0;
        }


        /// <summary>
        /// Sort objects alphabetically.
        /// </summary>
        /// <param name="ascending">Indicates whether the order will be ascending or descending.</param>
        /// <param name="orderColumn">Name of the column that is used for storing the object's order. If null, <see cref="ObjectTypeInfo.OrderColumn"/> is taken from <see cref="TypeInfo"/>.</param>
        /// <param name="nameColumn">>Name of the column by which the alphabetical order will be set.</param>
        protected void SortAlphabetically(bool ascending, string orderColumn, string nameColumn)
        {
            var e = new ObjectSortEventArgs(this, ascending, orderColumn, nameColumn);

            // Handle the event
            using (var h = TypeInfo.Events.Sort.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    InitObjectsOrder(orderColumn, nameColumn, ascending);
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Moves the object to the right position according to the custom order.
        /// </summary>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        /// <param name="nameColumn">Column by the content of which the alphabetical order will be set</param>
        protected void SetObjectAlphabeticalOrder(string orderColumn, string nameColumn)
        {
            var ti = TypeInfo;

            var orderCol = (string.IsNullOrEmpty(orderColumn) ? ti.OrderColumn : orderColumn);
            if (orderCol != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Ensure the name column (by default it's display name column)
                if (string.IsNullOrEmpty(nameColumn))
                {
                    if (ti.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        nameColumn = ti.DisplayNameColumn;
                    }
                    else if (ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        nameColumn = ti.CodeNameColumn;
                    }
                    else
                    {
                        // No automatic column to order items, do nothing
                        return;
                    }
                }

                var q = GetSiblingsQueryInternal(s =>
                {
                    s.OrderBy(orderCol, nameColumn);
                    s.Column(nameColumn);
                    // Do not include object itself
                    s.WhereNot(GetOrderIdentityWhereCondition());
                });

                var ds = q.Result;

                int index = 1;
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    index = ds.Tables[0].Rows.Count + 1;
                    string currentValue = GetStringValue(nameColumn, "");
                    int i = 0;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        i++;

                        string name = ValidationHelper.GetString(dr[0], "");

                        if (string.Compare(name, currentValue, StringComparison.CurrentCulture) > 0)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                SetObjectOrder(index, false, orderColumn);
            }
        }


        /// <summary>
        /// Moves the object to the specified order. The move is done within the object's siblings.
        /// </summary>
        /// <param name="targetOrder">Desired order of the object.</param>
        /// <param name="relativeOrder">If true, the <paramref name="targetOrder"/> is taken as a relative order from current order position.</param>
        /// <param name="orderColumn">Name of the order column. If null, <see cref="ObjectTypeInfo.OrderColumn"/> from <see cref="TypeInfo"/> is taken</param>
        protected void SetObjectOrder(int targetOrder, bool relativeOrder, string orderColumn)
        {
            if (String.IsNullOrEmpty(orderColumn))
            {
                orderColumn = TypeInfo.OrderColumn;
            }
            if (orderColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return;
            }

            var e = new ObjectChangeOrderEventArgs(this, targetOrder, relativeOrder, orderColumn);
            using (var h = TypeInfo.Events.ChangeOrder.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    var where = GetSiblingsWhereCondition();
                    var bulkUpdateArgs = new BulkUpdateEventArgs(TypeInfo, where, new[] { orderColumn });
                    using (var bulkUpdateEvent = TypeInfo.Events.BulkUpdate.StartEvent(bulkUpdateArgs))
                    {
                        if (bulkUpdateEvent.CanContinue())
                        {
                            // Init the proper item order so the order is consistent - internal method do not fire the bulk update event again
                            InitObjectsOrderInternal(orderColumn, null, where);

                            // Move the object to specified position
                            var parameters = GetOrderQueryParameters(orderColumn);
                            parameters.Add("@ObjectID", GetObjectOrderID());
                            parameters.Add("@NewOrder", targetOrder);
                            parameters.Add("@RelativeOrder", relativeOrder);
                            parameters.IncludeDataParameters(where.Parameters, null);
                            var data = ConnectionHelper.ExecuteQuery("cms.class.SetObjectOrder", parameters, where.WhereCondition);
                            int newOrder = ValidationHelper.GetInteger(data.Tables[0].Rows[0][0], 0);

                            SetValue(orderColumn, newOrder);

                            SetObjectOrderPostprocessing();
                        }

                        bulkUpdateEvent.FinishEvent();
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Returns ID of the item being ordered. By default return ObjectID. This is overridden in TreeNode, where NodeID has to be supplied.
        /// </summary>
        protected virtual int GetObjectOrderID()
        {
            return Generalized.ObjectID;
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected virtual void SetObjectOrderPostprocessing()
        {
            var ti = TypeInfo;

            // Generate staging tasks for all involved objects
            var q = GetSiblingsQueryInternal(s => s.Column(ti.IDColumn));

            var ds = q.Result;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int id = ValidationHelper.GetInteger(dr[0], 0);
                    if (id > 0)
                    {
                        var obj = GetObject(id);
                        if (obj.LogSynchronization == SynchronizationTypeEnum.TouchParent)
                        {
                            obj.TouchParent();

                            // Update is always within only one parent, therefore it is enough if the parent is touched once, not for every object
                            break;
                        }

                        ti.RaiseOnLogObjectChange(obj, TaskTypeEnum.UpdateObject);
                    }
                }
            }

            // Clear hashtables and create web farm task
            ProviderHelper.ClearHashtables(ti.ObjectType, true);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected virtual WhereCondition GetSiblingsWhereCondition()
        {
            var parent = Parent;

            var ti = TypeInfo;
            // Make sure WHERE condition is surrounded by brackets because of complex conditions with OR operator
            var where = new WhereCondition().Where(ti.WhereCondition);

            if ((ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (parent != null))
            {
                where.WhereEquals(ti.ParentIDColumn, parent.Generalized.ObjectID);
            }

            if (ti.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (ObjectGroupID == 0)
                {
                    where.WhereNull(ti.GroupIDColumn);
                }
                else
                {
                    where.WhereEquals(ti.GroupIDColumn, ObjectGroupID);
                }
            }

            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (ObjectSiteID == 0)
                {
                    where.WhereNull(ti.SiteIDColumn);
                }
                else
                {
                    where.WhereEquals(ti.SiteIDColumn, ObjectSiteID);
                }
            }

            return where;
        }


        /// <summary>
        /// Gets order identity where condition to identify the object to be ordered
        /// </summary>
        protected virtual WhereCondition GetOrderIdentityWhereCondition()
        {
            var ti = TypeInfo;
            var where = new WhereCondition();

            // Do not include object itself
            if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                where.WhereEquals(ti.IDColumn, ObjectID);
            }

            return where;
        }


        /// <summary>
        /// Creates QueryDataParameters with special macros for object order management.
        /// </summary>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        /// <param name="nameColumn">Name of the column by which the order should be initialized (if not set, displayname column is used)</param>
        /// <param name="asc">If true the order will be ascending (default is true)</param>
        protected virtual QueryDataParameters GetOrderQueryParameters(string orderColumn, string nameColumn = null, bool asc = true)
        {
            var dci = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);
            if (dci != null)
            {
                string orderCol = (string.IsNullOrEmpty(orderColumn) ? TypeInfo.OrderColumn : orderColumn);
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.AddMacro("##ORDERCOLUMN##", orderCol);
                parameters.AddMacro("##IDColumn##", TypeInfo.IDColumn);
                parameters.AddMacro("##TABLESELECT##", dci.ClassTableName);
                parameters.AddMacro("##TABLE##", dci.ClassTableName);

                string tableDefinition;
                string orderByDefinition;
                string selectDefinition;
                if (string.IsNullOrEmpty(nameColumn))
                {
                    tableDefinition = "[" + orderCol + "] int";
                    selectDefinition = "[" + orderCol + "]";
                    if (TypeInfo.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        tableDefinition += ", \n[" + TypeInfo.DisplayNameColumn + "] varchar(250)";
                        selectDefinition += ", [" + TypeInfo.DisplayNameColumn + "]";
                    }
                    if (TypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        tableDefinition += ", \n[" + TypeInfo.CodeNameColumn + "] varchar(250)";
                        selectDefinition += ", [" + TypeInfo.CodeNameColumn + "]";
                    }
                    if (TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        tableDefinition += ", \n[" + TypeInfo.IDColumn + "] int";
                        selectDefinition += ", [" + TypeInfo.IDColumn + "]";
                    }
                    orderByDefinition = selectDefinition + (asc ? SqlHelper.ORDERBY_ASC : SqlHelper.ORDERBY_DESC);
                }
                else
                {
                    // Specified order
                    orderByDefinition = nameColumn + (asc ? SqlHelper.ORDERBY_ASC : SqlHelper.ORDERBY_DESC) + ", [" + orderCol + "], [" + TypeInfo.IDColumn + "]";
                    selectDefinition = "[" + TypeInfo.IDColumn + "], [" + orderCol + "], [" + nameColumn + "]";
                    tableDefinition = "[" + TypeInfo.IDColumn + "] int, \n [" + orderCol + "] int, \n[" + nameColumn + "] varchar(250)";
                }

                parameters.AddMacro("##TABLEDEF##", tableDefinition);
                parameters.AddMacro("##ORDERBYDEF##", orderByDefinition);
                parameters.AddMacro("##SELECTDEF##", selectDefinition);
                return parameters;
            }

            throw new Exception("[BaseInfo.GetOrderQueryParameters]: Data class of the object not found.");
        }


        /// <summary>
        /// Inits the proper order of the sibling objects so the order column is consistent.
        /// </summary>
        /// <param name="orderColumn">Name of the column that is used for storing the object's order. If null, <see cref="ObjectTypeInfo.OrderColumn"/> is taken from <see cref="TypeInfo"/>.</param>
        /// <param name="nameColumn">>Name of the column by which the order is to be set. <see cref="ObjectTypeInfo.DisplayNameColumn"/> is used if not set.</param>
        /// <param name="where">Where condition that defines all the objects that can be reordered.</param>
        /// <param name="ascending">Indicates whether the order is ascending or descending (ascending order is used by default).</param>
        private void InitObjectsOrderInternal(string orderColumn, string nameColumn, WhereCondition where, bool ascending = true)
        {
            var parameters = GetOrderQueryParameters(orderColumn, nameColumn, ascending);
            parameters.IncludeDataParameters(where.Parameters, null);
            ConnectionHelper.ExecuteQuery("cms.class.InitObjectsOrder", parameters, where.WhereCondition);

            InitObjectsOrderPostprocessing();
        }

        #endregion


        #region "Check and remove dependencies methods"

        #region "Check dependencies"

        /// <summary>
        /// Checks object for dependent objects. Returns true if there is at least one dependent object.
        /// First tries to execute checkdependencies query, if not found, an automatic process is executed.
        /// </summary>
        /// <remarks>
        /// Automated process is based on <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// Child, site and group objects are not included. Objects whose <see cref="ObjectTypeInfo.ObjectPathColumn"/> value contains a prefix matching the current object's path value are also not included.
        /// </remarks>
        /// <param name="reportAll">If false, only required dependency constraints (without default value) are returned, otherwise checks all dependency relations.</param>
        protected virtual bool CheckDependencies(bool reportAll = true)
        {
            string queryName = TypeInfo.ObjectClassName + ".checkdependencies";
            if (QueryInfoProvider.GetQueryInfo(queryName, false) != null)
            {
                // Use the procedure if exists
                var deps = GetDependenciesNames(reportAll, 1);
                return (deps != null) && (deps.Count > 0);
            }
            else
            {
                // Use more efficient method which does not return formatted names
                var deps = GetDependencies(reportAll, 1);
                return (deps != null) && (deps.Count > 0);
            }
        }


        /// <summary>
        /// Returns a list of object names which depend on this object.
        /// First tries to execute checkdependencies query, if not found, an automatic process is executed.
        /// </summary>
        /// <remarks>
        /// Automated process is based on <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// Child, site and group objects are not included. Objects whose <see cref="ObjectTypeInfo.ObjectPathColumn"/> value contains a prefix matching the current object's path value are also not included.
        /// </remarks>
        /// <param name="reportAll">If false, only required dependency constraints (without default value) are returned, otherwise checks all dependency relations.</param>
        /// <param name="topN">Number of dependent objects to return, 0 means no limitation.</param>
        protected virtual List<string> GetDependenciesNames(bool reportAll = true, int topN = 10)
        {
            // Load dependencies with custom query
            string queryName = TypeInfo.ObjectClassName + ".checkdependencies";
            if (QueryInfoProvider.GetQueryInfo(queryName, false) != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ID", ObjectID);

                // Get the result
                List<string> names = new List<string>();
                DataSet ds = ConnectionHelper.ExecuteQuery(queryName, parameters, null, null, topN);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        // Get object names
                        StringBuilder sb = new StringBuilder();
                        sb.Append(ValidationHelper.GetString(row[0], "N/A"));
                        int length = row.ItemArray.Length;
                        if (length > 1)
                        {
                            sb.AppendFormat(" ({0}", ValidationHelper.GetString(row.ItemArray[1], "N/A"));
                            for (int i = 2; i < row.ItemArray.Length; ++i)
                            {
                                sb.AppendFormat(", {0}", ValidationHelper.GetString(row.ItemArray[i], "N/A"));
                            }
                            sb.Append(")");
                        }
                        names.Add(sb.ToString());
                    }
                }
                return names;
            }
            else
            {
                return GetDependenciesNamesAuto(reportAll, topN);
            }
        }


        /// <summary>
        /// Returns a list of object names which depend on this object.
        /// </summary>
        /// <remarks>
        /// List of objects is based on <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// Child, site and group objects are not included. Objects whose <see cref="ObjectTypeInfo.ObjectPathColumn"/> value contains a prefix matching the current object's path value are also not included.
        /// </remarks>
        /// <param name="reportAll">If false, only required dependency constraints (without default value) are returned, otherwise checks all dependency relations.</param>
        /// <param name="topN">Number of dependent objects to return, 0 means no limitation.</param>
        protected virtual List<string> GetDependenciesNamesAuto(bool reportAll, int topN)
        {
            var dependencies = GetDependencies(reportAll, topN);
            if ((dependencies == null) || (dependencies.Count == 0))
            {
                return new List<string>();
            }

            // Use hashset to prevent duplicates from the list
            var result = new HashSet<string>();
            foreach (var dependency in dependencies)
            {
                string objType = dependency.ObjectType;
                int objId = dependency.ObjectID;

                BaseInfo obj = ModuleManager.GetReadOnlyObject(objType);
                if (obj == null)
                {
                    continue;
                }

                // Do not include partial object types in a list of dependencies
                if (!string.IsNullOrEmpty(obj.TypeInfo.CompositeObjectType))
                {
                    continue;
                }

                obj = obj.GetObject(objId);
                if (obj == null)
                {
                    continue;
                }

                string name = obj.GetFullObjectName(true, true, true);
                result.Add(name);
            }

            return result.ToList();
        }


        /// <summary>
        /// Returns list of objects which have (direct) dependency on this object.
        /// </summary>
        /// <remarks>
        /// List of objects is based on <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// Child, site and group objects are not included. Objects whose <see cref="ObjectTypeInfo.ObjectPathColumn"/> value contains a prefix matching the current object's path value are also not included.
        /// </remarks>
        /// <param name="reportAll">If false, only required dependency constraints (without default value) are returned, otherwise checks all dependency relations.</param>
        /// <param name="topN">Number of dependent objects to return, 0 means no limitation.</param>
        internal List<DependencyInfo> GetDependencies(bool reportAll = false, int topN = 0)
        {
            var result = new List<DependencyInfo>();

            foreach (var objType in ObjectTypeManager.AllObjectTypes)
            {
                var obj = ModuleManager.GetReadOnlyObject(objType);
                if (obj != null)
                {
                    // Exclude inherited and binding types (bindings don't have ID column and cannot be displayed anyway)
                    var ti = obj.TypeInfo;

                    if (!ti.IsListingObjectTypeInfo && !ti.IsBinding && (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        var dependencies = ti.ObjectDependencies;
                        if (dependencies != null)
                        {
                            // Check dependencies
                            foreach (var dep in dependencies)
                            {
                                string depObjType = obj.GetDependencyObjectType(dep);

                                if (TypeInfo.IsRelatedTo(depObjType))
                                {
                                    // We found an object type which depends on object's type and was not processed yet, we need to check it
                                    var required = dep.DependencyType;
                                    if (reportAll || (required == ObjectDependencyEnum.Binding) || (required == ObjectDependencyEnum.Required))
                                    {
                                        string depColumn = dep.DependencyColumn;

                                        // Get all objects with reference to current object
                                        var ds = obj.GetDataQuery(true, s => s
                                            .Where(depColumn, QueryOperator.Equals, ObjectID)
                                            .TopN(topN)
                                            .Column(ti.IDColumn),
                                            false
                                        ).Result;

                                        AddItemsToDependecyList(ds, objType, result, topN);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Adds items to the list, reflects topN setting.
        /// </summary>
        /// <param name="ds">DataSet with data to add</param>
        /// <param name="objType">Object type of the data</param>
        /// <param name="result">List to add the items to</param>
        /// <param name="topN">Maximal allowed number of items in the list</param>
        private static void AddItemsToDependecyList(DataSet ds, string objType, List<DependencyInfo> result, int topN)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int id = ValidationHelper.GetInteger(dr[0], 0);
                    result.Add(new DependencyInfo(objType, id));

                    if ((topN > 0) && (result.Count == topN))
                    {
                        // Number of items done, quit processing
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Returns object name combining object type name and object display name.
        /// </summary>
        protected virtual string GetObjectName()
        {
            return TypeInfo.GetNiceObjectTypeName() + " '" + ObjectDisplayName + "'";
        }


        /// <summary>
        /// Returns the name of the object within its parent hierarchy.
        /// </summary>
        /// <param name="includeParent">If true, the parent object name is included to the object name</param>
        /// <param name="includeSite">If true, the site information is included if available</param>
        /// <param name="includeGroup">If true, the group information is included if available</param>
        protected virtual string GetFullObjectName(bool includeParent, bool includeSite, bool includeGroup)
        {
            string name = GetObjectName();

            // Include parent
            if (includeParent)
            {
                var parent = Parent;
                if (parent != null)
                {
                    string parentName = parent.GetFullObjectName(true, false, false);
                    if (!string.IsNullOrEmpty(parentName))
                    {
                        name += " " + string.Format(CoreServices.Localization.GetString("general.dependencycheck.objectin"), parentName);
                    }
                }
            }

            // Include group
            if (includeGroup && (ObjectGroupID > 0))
            {
                name += " " + GetFullObjectName(PredefinedObjectType.GROUP, ObjectGroupID, "general.dependencycheck.ingroup");
            }

            // Include site
            if (includeSite && (ObjectSiteID > 0))
            {
                name += " " + GetFullObjectName(PredefinedObjectType.SITE, ObjectSiteID, "general.dependencycheck.onsite");
            }

            return name;
        }


        /// <summary>
        /// Returns formatted name of the given object using specified resource string.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="objectId">ID of the object</param>
        /// <param name="resString">Formatting resource string</param>
        private static string GetFullObjectName(string objectType, int objectId, string resString)
        {
            var infoObj = ProviderHelper.GetInfoById(objectType, objectId);

            return infoObj != null ? string.Format(CoreServices.Localization.GetString(resString), infoObj.ObjectDisplayName) : "";
        }

        #endregion


        #region "Remove dependencies"

        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearCache">If true, caches of all object types which were potentially modified are cleared (provider hashtables and object type cache dummy keys)</param>
        protected virtual void RemoveObjectDependencies(bool deleteAll = false, bool clearCache = true)
        {
            new ObjectDependenciesRemover().RemoveObjectDependencies(this, deleteAll, clearCache);
        }

        #endregion

        #endregion


        #region "Search methods"

        /// <summary>
        /// Gets the id column value which is used as search id by default.
        /// </summary>
        public virtual string GetSearchID()
        {
            return ObjectID.ToString();
        }

        #endregion


        #region "Object settings methods"

        /// <summary>
        /// Ensures the object settings
        /// </summary>
        /// <exception cref="System.Exception">Thrown when <see cref="ObjectID"/> is not set.</exception>
        private ObjectSettingsInfo EnsureObjectSettings()
        {
            var ti = TypeInfo;
            if (ti.ObjectType == ObjectSettingsInfo.OBJECT_TYPE)
            {
                return null;
            }

            // Validate the object ID
            int objectId = ObjectID;
            if (objectId <= 0)
            {
                throw new Exception(String.Format("[SynchronizedInfo.ObjectSettings]: Cannot provide general settings for the object '{0}' of type '{1}'. The object must have valid ID.", ObjectDisplayName, ti.ObjectType));
            }

            // If settings available and valid, return
            var result = mObjectSettings;
            if ((result == null) || !result.IsObjectValid)
            {
                lock (globalLockObject)
                {
                    result = mObjectSettings;
                    if ((result == null) || !result.IsObjectValid)
                    {
                        string objectType = ti.ObjectType;

                        // Try to get existing object settings
                        result = ObjectSettingsInfoProvider.GetObjectSettingsInfo(objectType, objectId);

                        if (result == null)
                        {
                            // Ensure new object settings
                            result = new ObjectSettingsInfo
                            {
                                ObjectSettingsObjectType = objectType,
                                ObjectSettingsObjectID = objectId
                            };

                            ObjectSettingsInfoProvider.SetObjectSettingsInfo(result);
                        }

                        mObjectSettings = result;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}