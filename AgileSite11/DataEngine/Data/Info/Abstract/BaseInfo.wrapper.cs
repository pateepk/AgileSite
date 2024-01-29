using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base info class (only carrying the type information).
    /// </summary>
    public abstract partial class BaseInfo
    {
        /// <summary>
        /// Info object wrapper for generalized access
        /// </summary>
        public abstract class GeneralizedInfoWrapper : ICMSObject, ICMSStorage, IRelatedData
        {
            #region "General properties"

            /// <summary>
            /// If true, the parent object data is cached within object.
            /// </summary>
            public bool CacheParentData
            {
                get
                {
                    return MainObject.CacheParentData;
                }
                set
                {
                    MainObject.CacheParentData = value;
                }
            }


            /// <summary>
            /// If true, the object is cached within the system for later use
            /// </summary>
            public bool IsCachedObject
            {
                get
                {
                    return MainObject.IsCachedObject;
                }
                set
                {
                    MainObject.IsCachedObject = value;
                }
            }


            /// <summary>
            /// Returns true if the object is disconnected from the data source
            /// </summary>
            public bool IsDisconnected
            {
                get
                {
                    return MainObject.IsDisconnected;
                }
            }


            /// <summary>
            /// Type info.
            /// </summary>
            public ObjectTypeInfo TypeInfo
            {
                get
                {
                    return MainObject.TypeInfo;
                }
            }


            /// <summary>
            /// Main object for this general access object
            /// </summary>
            public virtual BaseInfo MainObject
            {
                get;
                protected set;
            }


            /// <summary>
            /// If true, externally stored columns are ignored and are stored normally in DB.
            /// </summary>
            public bool IgnoreExternalColumns
            {
                get
                {
                    return MainObject.IgnoreExternalColumns;
                }
                set
                {
                    MainObject.IgnoreExternalColumns = value;
                }
            }

            #endregion


            #region "Column names"

            /// <summary>
            /// Code name column name of the info record.
            /// </summary>
            public string CodeNameColumn
            {
                get
                {
                    return MainObject.CodeNameColumn;
                }
            }


            /// <summary>
            /// Code name column name of the info record.
            /// </summary>
            public string DisplayNameColumn
            {
                get
                {
                    return MainObject.DisplayNameColumn;
                }
            }

            #endregion


            #region "Object values"

            /// <summary>
            /// Object ID.
            /// </summary>
            public int ObjectID
            {
                get
                {
                    return MainObject.ObjectID;
                }
                set
                {
                    MainObject.ObjectID = value;
                }
            }


            /// <summary>
            /// Object last modified time.
            /// </summary>
            public DateTime ObjectLastModified
            {
                get
                {
                    return MainObject.ObjectLastModified;
                }
                set
                {
                    MainObject.ObjectLastModified = value;
                }
            }


            /// <summary>
            /// Object GUID.
            /// </summary>
            public Guid ObjectGUID
            {
                get
                {
                    return MainObject.ObjectGUID;
                }
                set
                {
                    MainObject.ObjectGUID = value;
                }
            }


            /// <summary>
            /// Object version GUID.
            /// </summary>
            public Guid ObjectVersionGUID
            {
                get
                {
                    return MainObject.ObjectVersionGUID;
                }
                set
                {
                    MainObject.ObjectVersionGUID = value;
                }
            }


            /// <summary>
            /// Object site ID.
            /// </summary>
            public int ObjectSiteID
            {
                get
                {
                    return MainObject.ObjectSiteID;
                }
                set
                {
                    MainObject.ObjectSiteID = value;
                }
            }


            /// <summary>
            /// Object parent ID.
            /// </summary>
            public int ObjectParentID
            {
                get
                {
                    return MainObject.ObjectParentID;
                }
                set
                {
                    MainObject.ObjectParentID = value;
                }
            }


            /// <summary>
            /// Object group ID.
            /// </summary>
            public int ObjectGroupID
            {
                get
                {
                    return MainObject.ObjectGroupID;
                }
                set
                {
                    MainObject.ObjectGroupID = value;
                }
            }


            /// <summary>
            /// Object code name.
            /// </summary>
            public string ObjectCodeName
            {
                get
                {
                    return MainObject.ObjectCodeName;
                }
                set
                {
                    MainObject.ObjectCodeName = value;
                }
            }


            /// <summary>
            /// Returns the original object code name
            /// </summary>
            public virtual string OriginalObjectCodeName
            {
                get
                {
                    return ObjectCodeName;
                }
            }


            /// <summary>
            /// Object full name if exists
            /// </summary>
            public string ObjectFullName
            {
                get
                {
                    return MainObject.ObjectFullName;
                }
            }


            /// <summary>
            /// Object name.
            /// </summary>
            public string ObjectDisplayName
            {
                get
                {
                    return MainObject.ObjectDisplayName;
                }
                set
                {
                    MainObject.ObjectDisplayName = value;
                }
            }


            /// <summary>
            /// Object site name.
            /// </summary>
            /// <remarks>The property returns site name of the current object. If it is not site object, then it returns site name of his parent (or further ancestors)</remarks>
            public string ObjectSiteName
            {
                get
                {
                    return MainObject.ObjectSiteName;
                }
            }


            /// <summary>
            /// Object thumbnail GUID.
            /// </summary>
            public Guid ObjectThumbnailGUID
            {
                get
                {
                    return MainObject.ObjectThumbnailGUID;
                }
                set
                {
                    MainObject.ObjectThumbnailGUID = value;
                }
            }


            /// <summary>
            /// Object icon GUID.
            /// </summary>
            public Guid ObjectIconGUID
            {
                get
                {
                    return MainObject.ObjectIconGUID;
                }
                set
                {
                    MainObject.ObjectIconGUID = value;
                }
            }


            /// <summary>
            /// Returns the order of the object among the other objects.
            /// </summary>
            public virtual int ObjectOrder
            {
                get
                {
                    return MainObject.ObjectOrder;
                }
            }

            #endregion


            #region "Object types"

            /// <summary>
            /// Parent object type.
            /// </summary>
            public string ParentObjectType
            {
                get
                {
                    return MainObject.ParentObjectType;
                }
            }

            #endregion


            #region "Object collections"

            /// <summary>
            /// The collection of the child objects for the given object.
            /// </summary>
            public InfoObjectRepository Children
            {
                get
                {
                    return MainObject.Children;
                }
            }


            /// <summary>
            /// Collection of the child dependencies for the given object.
            /// </summary>
            public InfoObjectRepository ChildDependencies
            {
                get
                {
                    return MainObject.ChildDependencies;
                }
            }


            /// <summary>
            /// The collection of the binding objects for the given object.
            /// </summary>
            public InfoObjectRepository Bindings
            {
                get
                {
                    return MainObject.Bindings;
                }
            }


            /// <summary>
            /// The collection of the other binding objects for the given object.
            /// </summary>
            public InfoObjectRepository OtherBindings
            {
                get
                {
                    return MainObject.OtherBindings;
                }
            }


            /// <summary>
            /// The collection of the metafiles of the given object.
            /// </summary>
            public IInfoObjectCollection MetaFiles
            {
                get
                {
                    return MainObject.MetaFiles;
                }
            }


            /// <summary>
            /// The collection of the processes of the given object.
            /// </summary>
            public IInfoObjectCollection Processes
            {
                get
                {
                    return MainObject.Processes;
                }
            }


            /// <summary>
            /// The collection of the scheduled tasks of the given object.
            /// </summary>
            public IInfoObjectCollection ScheduledTasks
            {
                get
                {
                    return MainObject.ScheduledTasks;
                }
            }


            /// <summary>
            /// Collection of the objects depending on this object (object which have FK to this object).
            /// </summary>
            public InfoObjectRepository ReferringObjects
            {
                get
                {
                    return MainObject.ReferringObjects;
                }
            }

            #endregion


            #region "Settings"

            /// <summary>
            /// If true, time stamp is updated on object update.
            /// </summary>
            public bool UpdateTimeStamp
            {
                get
                {
                    return MainObject.UpdateTimeStamp;
                }
                set
                {
                    MainObject.UpdateTimeStamp = value;
                }
            }


            /// <summary>
            /// If true, synchronization tasks are logged on the object update.
            /// </summary>
            public SynchronizationTypeEnum LogSynchronization
            {
                get
                {
                    return MainObject.LogSynchronization;
                }
                set
                {
                    MainObject.LogSynchronization = value;
                }
            }


            /// <summary>
            /// If true, export tasks are logged on the object update.
            /// </summary>
            public bool LogExport
            {
                get
                {
                    return MainObject.LogExport;
                }
                set
                {
                    MainObject.LogExport = value;
                }
            }


            /// <summary>
            /// Indicates if parent is allowed to be touched, if exists.
            /// </summary>
            public bool AllowTouchParent
            {
                get
                {
                    return MainObject.AllowTouchParent;
                }
                set
                {
                    MainObject.AllowTouchParent = value;
                }
            }


            /// <summary>
            /// Indicates if the object versioning is supported.
            /// </summary>
            public bool SupportsVersioning
            {
                get
                {
                    return MainObject.SupportsVersioning;
                }
                set
                {
                    MainObject.SupportsVersioning = value;
                }
            }


            /// <summary>
            /// Indicates if the object versioning is enabled by the settings.
            /// </summary>
            public bool VersioningEnabled
            {
                get
                {
                    return MainObject.VersioningEnabled;
                }
            }


            /// <summary>
            /// If true, events are logged on the object update.
            /// </summary>
            public bool LogEvents
            {
                get
                {
                    return MainObject.LogEvents;
                }
                set
                {
                    MainObject.LogEvents = value;
                }
            }


            /// <summary>
            /// If true, integration tasks are being logged.
            /// </summary>
            public bool LogIntegration
            {
                get
                {
                    return MainObject.LogIntegration;
                }
                set
                {
                    MainObject.LogIntegration = value;
                }
            }


            /// <summary>
            /// If true, web farm tasks are logged on the object update.
            /// </summary>
            public bool LogWebFarmTasks
            {
                get
                {
                    return MainObject.LogWebFarmTasks;
                }
                set
                {
                    MainObject.LogWebFarmTasks = value;
                }
            }


            /// <summary>
            /// If true, cache dependencies are touched when the object is changed.
            /// </summary>
            public bool TouchCacheDependencies
            {
                get
                {
                    return MainObject.TouchCacheDependencies;
                }
                set
                {
                    MainObject.TouchCacheDependencies = value;
                }
            }


            /// <summary>
            /// Indicates if the object is checked out.
            /// </summary>
            public bool IsCheckedOut
            {
                get
                {
                    return MainObject.IsCheckedOut;
                }
            }


            /// <summary>
            /// Gets ID of the user who checked the object out.
            /// </summary>
            public int IsCheckedOutByUserID
            {
                get
                {
                    return MainObject.IsCheckedOutByUserID;
                }
            }


            /// <summary>
            /// Indicates if the object is clone.
            /// </summary>
            public bool IsClone
            {
                get
                {
                    return MainObject.IsClone;
                }
                set
                {
                    MainObject.IsClone = value;
                }
            }


            /// <summary>
            /// If true, the code name is validated upon saving.
            /// </summary>
            public bool ValidateCodeName
            {
                get
                {
                    return MainObject.ValidateCodeName;
                }
                set
                {
                    MainObject.ValidateCodeName = value;
                }
            }


            /// <summary>
            /// If true, the code name is checked for uniqueness upon saving.
            /// </summary>
            public bool CheckUnique
            {
                get
                {
                    return MainObject.CheckUnique;
                }
                set
                {
                    MainObject.CheckUnique = value;
                }
            }


            /// <summary>
            /// Indicates if the object supports deleting to recycle bin.
            /// </summary>
            /// <remarks>
            /// Use CMS.Synchronization.ObjectVersionManager.AllowObjectRestore(GeneralizedInfo infoObj) in cases where current system configuration needs to be taken into consideration.
            /// </remarks>
            public bool AllowRestore
            {
                get
                {
                    return MainObject.AllowRestore;
                }
                set
                {
                    MainObject.AllowRestore = value;
                }
            }


            /// <summary>
            /// Indicates if the object supports cloning.
            /// </summary>
            public bool AllowClone
            {
                get
                {
                    return MainObject.AllowClone;
                }
                set
                {
                    MainObject.AllowClone = value;
                }
            }

            #endregion


            #region "Properties"

            /// <summary>
            /// Returns true if the object is considered valid.
            /// </summary>
            public bool IsObjectValid
            {
                get
                {
                    return MainObject.IsObjectValid;
                }
            }


            /// <summary>
            /// Object category.
            /// </summary>
            public BaseInfo ObjectCategory
            {
                get
                {
                    return MainObject.ObjectCategory;
                }
            }


            /// <summary>
            /// Object parent
            /// </summary>
            public BaseInfo ObjectParent
            {
                get
                {
                    return MainObject.ObjectParent;
                }
                set
                {
                    MainObject.ObjectParent = value;
                }
            }


            /// <summary>
            /// Object thumbnail
            /// </summary>
            public BaseInfo ObjectThumbnail
            {
                get
                {
                    return MainObject.ObjectThumbnail;
                }
            }


            /// <summary>
            /// Object icon
            /// </summary>
            public BaseInfo ObjectIcon
            {
                get
                {
                    return MainObject.ObjectIcon;
                }
            }


            /// <summary>
            /// Returns true if the object has it's data storage initialized already
            /// </summary>
            public bool HasData
            {
                get
                {
                    return MainObject.HasData;
                }
            }


            /// <summary>
            /// Returns true if the object has changed.
            /// </summary>
            public virtual bool HasChanged
            {
                get
                {
                    NotSupported();
                    return true;
                }
            }


            /// <summary>
            /// Returns true if the object is complete (has all columns).
            /// </summary>
            public virtual bool IsComplete
            {
                get
                {
                    NotSupported();
                    return true;
                }
            }


            /// <summary>
            /// Gets the list of properties which should be prioritized in the macro controls (IntelliSense, MacroTree).
            /// </summary>
            public List<string> PrioritizedProperties
            {
                get
                {
                    return MainObject.PrioritizedProperties;
                }
            }


            /// <summary>
            /// Gets the list of customized columns in current object
            /// </summary>
            public IReadOnlyCollection<string> CustomizedColumns
            {
                get
                {
                    return MainObject.CustomizedColumns;
                }
                set
                {
                    MainObject.CustomizedColumns = value;
                }
            }


            /// <summary>
            /// Indicates if object is custom (created by customer).
            /// </summary>
            public bool ObjectIsCustom
            {
                get
                {
                    return MainObject.ObjectIsCustom;
                }
                set
                {
                    MainObject.ObjectIsCustom = value;
                }
            }


            /// <summary>
            /// Indicates if object is customized.
            /// </summary>
            public bool ObjectIsCustomized
            {
                get
                {
                    return MainObject.ObjectIsCustomized;
                }
            }

            #endregion


            #region "Methods"

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="mainObj">Main object to wrap</param>
            protected GeneralizedInfoWrapper(BaseInfo mainObj)
            {
                MainObject = mainObj;
            }


            /// <summary>
            /// Checks the object license. Returns true if the licensing conditions for this object were matched
            /// </summary>
            /// <param name="action">Object action</param>
            /// <param name="domainName">Domain name, if not set, uses current domain</param>
            public virtual bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
            {
                return MainObject.CheckLicense(action, domainName);
            }


            /// <summary>
            /// Checks whether info object as available on given site. Available means that object is global or assigned to given site.
            /// </summary>
            /// <param name="site">Site identifier: site name or ID</param>
            public bool IsAvailableOnSite(SiteInfoIdentifier site)
            {
                return MainObject.IsAvailableOnSite(site);
            }


            /// <summary>
            /// Returns the name of the object within its parent hierarchy.
            /// </summary>
            /// <param name="includeParent">If true, the parent object name is included to the object name</param>
            /// <param name="includeSite">If true, the site information is included if available</param>
            /// <param name="includeGroup">If true, the group information is included if available</param>
            public string GetFullObjectName(bool includeParent = true, bool includeSite = true, bool includeGroup = true)
            {
                return MainObject.GetFullObjectName(includeParent, includeSite, includeGroup);
            }


            /// <summary>
            /// Returns virtual relative path for specific column
            /// </summary>
            /// <param name="externalColumnName">External column name</param>
            /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
            public string GetVirtualFileRelativePath(string externalColumnName, string versionGuid = null)
            {
                return MainObject.GetVirtualFileRelativePath(externalColumnName, versionGuid);
            }


            /// <summary>
            /// Gets the object by specified where condition.
            /// </summary>
            /// <param name="where">Where condition</param>
            public BaseInfo GetObject(IWhereCondition where)
            {
                return MainObject.GetObject(where);
            }


            /// <summary>
            /// Gets the object by specified where condition.
            /// </summary>
            /// <param name="whereCondition">Where condition</param>
            public BaseInfo GetObject(string whereCondition)
            {
                var where = new WhereCondition(whereCondition);

                return MainObject.GetObject(where);
            }


            /// <summary>
            /// Gets the object by its ID.
            /// </summary>
            /// <param name="objectId">Object ID</param>
            public BaseInfo GetObject(int objectId)
            {
                return MainObject.GetObject(objectId);
            }


            /// <summary>
            /// Returns the existing object based on current object data.
            /// </summary>
            public BaseInfo GetExisting()
            {
                return MainObject.GetExisting();
            }


            /// <summary>
            /// Deletes the object using appropriate provider.
            /// </summary>
            public void DeleteObject()
            {
                MainObject.DeleteObject();
            }


            /// <summary>
            /// Gets the child object where condition.
            /// </summary>
            /// <param name="where">Original where condition</param>
            /// <param name="objectType">Object type of the child object</param>
            public WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
            {
                return MainObject.GetChildWhereCondition(where, objectType);
            }


            /// <summary>
            /// Gets the data query for this object type
            /// </summary>
            /// <param name="includeTypeCondition">If true, the type condition is included, otherwise selects all data from the data source</param>
            /// <param name="parameters">Parameters for the data retrieval</param>
            /// <param name="checkLicense">If true, the license is checked for this query</param>
            public IDataQuery GetDataQuery(bool includeTypeCondition, Action<DataQuerySettings> parameters, bool checkLicense)
            {
                return MainObject.GetDataQuery(includeTypeCondition, parameters, checkLicense);
            }


            /// <summary>
            /// Gets the DataSet of all the objects modified from specified date.
            /// </summary>
            /// <param name="from">From time</param>
            /// <param name="parameters">Parameters for the data retrieval</param>
            public IDataQuery GetModifiedFrom(DateTime from, Action<DataQuerySettings> parameters = null)
            {
                return MainObject.GetModifiedFrom(from, parameters);
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
            public DataSet GetData(QueryDataParameters parameters, string where = null, string orderBy = null, int topN = 0, string columns = null, bool binaryData = true)
            {
                return MainObject.GetData(parameters, where, orderBy, topN, columns, binaryData);
            }


            /// <summary>
            /// Gets count of the objects filtered by given where condition.
            /// </summary>
            /// <param name="where">Where condition</param>
            /// <param name="topN">Top N records</param>
            public int GetCount(string where, int topN)
            {
                return MainObject.GetCount(where, topN);
            }



            /// <summary>
            /// Updates the data of the object from DB (updates also ObjectSettings).
            /// </summary>
            /// <param name="binaryData">Indicates whether to load also binary data</param>
            public void UpdateFromDB(bool binaryData)
            {
                MainObject.UpdateFromDB(binaryData);
            }


            /// <summary>
            /// Updates the object using appropriate provider.
            /// </summary>
            public void SetObject()
            {
                MainObject.SetObject();
            }


            /// <summary>
            /// Submits the changes in the object to the database.
            /// </summary>
            /// <param name="withCollections">If true, also submits the changes in the underlying collections of the object (Children, ChildDependencies, Bindings, OtherBindings)</param>
            public void SubmitChanges(bool withCollections)
            {
                MainObject.SubmitChanges(withCollections);
            }


            /// <summary>
            /// Returns the parent object.
            /// </summary>
            public BaseInfo GetParent()
            {
                return MainObject.GetParent();
            }


            /// <summary>
            /// Updates the parent object by saving it (to update the timestamp).
            /// </summary>
            public void TouchParent()
            {
                MainObject.TouchParent();
            }


            /// <summary>
            /// Gets collection of dependency keys to be touched when modifying the current object.
            /// </summary>
            /// <param name="key">Cache key</param>
            /// <param name="context">Cache context</param>
            [Obsolete("Use GetCacheDependencies() instead.")]
            public List<string> GetDependencyCacheKeys(string key, string context)
            {
                return MainObject.GetDependencyCacheKeys(key, context);
            }


            /// <summary>
            /// Gets collection of dependency keys to be touched when modifying the current object.
            /// </summary>
            public ICollection<string> GetCacheDependencies()
            {
                return MainObject.GetCacheDependencies();
            }


            /// <summary>
            /// Gets DataSet with physical files for current object.
            /// </summary>
            /// <param name="operationType">Operation type</param>
            /// <param name="binaryData">Get the binary data</param>
            public DataSet GetPhysicalFiles(OperationTypeEnum operationType, bool binaryData)
            {
                return MainObject.GetPhysicalFiles(operationType, binaryData);
            }


            /// <summary>
            /// Updates physical files in given DataSet for current object.
            /// </summary>
            /// <param name="dsFiles">DataSet with files data</param>
            public void UpdatePhysicalFiles(DataSet dsFiles)
            {
                MainObject.UpdatePhysicalFiles(dsFiles);
            }


            /// <summary>
            /// Stores local settings for object instance.
            /// </summary>
            public void StoreSettings()
            {
                MainObject.StoreSettings();
            }


            /// <summary>
            /// Restores local settings for object instance.
            /// </summary>
            public void RestoreSettings()
            {
                MainObject.RestoreSettings();
            }


            /// <summary>
            /// Gets the unique string key for the object.
            /// </summary>
            public string GetObjectKey()
            {
                return MainObject.GetObjectKey();
            }


            /// <summary>
            /// Gets the global lock object for all the instances of the object (locked on "objectType_objectId").
            /// </summary>
            public object GetLockObject()
            {
                return MainObject.GetLockObject();
            }


            /// <summary>
            /// Makes the object data complete.
            /// </summary>
            /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
            public void MakeComplete(bool loadFromDb)
            {
                MainObject.MakeComplete(loadFromDb);
            }


            /// <summary>
            /// Returns true if any of the specified column names has changed.
            /// </summary>
            /// <param name="columnNames">Column names</param>
            public bool AnyItemChanged(IEnumerable<string> columnNames)
            {
                foreach (string column in columnNames)
                {
                    if (ItemChanged(column))
                    {
                        return true;
                    }
                }

                return false;
            }


            /// <summary>
            /// Returns true if the item on specified column name changed.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public bool ItemChanged(string columnName)
            {
                return MainObject.ItemChanged(columnName);
            }


            /// <summary>
            /// Returns true if the object changed.
            /// </summary>
            public bool DataChanged()
            {
                string timeStampCol = TypeInfo.TimeStampColumn;

                if (timeStampCol == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    timeStampCol = null;
                }

                return MainObject.DataChanged(timeStampCol);
            }


            /// <summary>
            /// Returns true if the object changed.
            /// </summary>
            /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
            public bool DataChanged(string excludedColumns)
            {
                return MainObject.DataChanged(excludedColumns);
            }


            /// <summary>
            /// Disconnects the object from database.
            /// </summary>
            public void Disconnect()
            {
                MainObject.Disconnect();
            }


            /// <summary>
            /// Reconnects the object to database.
            /// </summary>
            public void Reconnect()
            {
                MainObject.Reconnect();
            }


            /// <summary>
            /// Ensures the code name of the object if not set
            /// </summary>
            public void EnsureCodeName()
            {
                MainObject.EnsureCodeName();
            }


            /// <summary>
            /// Ensures the GUID of the object
            /// </summary>
            public void EnsureGUID()
            {
                MainObject.EnsureGUID();
            }


            /// <summary>
            /// Returns the BinaryData object of the current instance. Has to be overridden by specific classes. Returns null by default.
            /// </summary>
            public BinaryData GetBinaryData()
            {
                return MainObject.GetBinaryData();
            }


            /// <summary>
            /// Makes sure that the binary data is loaded into binary column of the object when StoreToFileSystem is true.
            /// </summary>
            public byte[] EnsureBinaryData()
            {
                return MainObject.EnsureBinaryData();
            }


            /// <summary>
            /// Makes sure that the binary data is loaded into binary column of the object.
            /// </summary>
            /// <param name="forceLoadFromDB">If true, the data are loaded even from DB</param>
            public byte[] EnsureBinaryData(bool forceLoadFromDB)
            {
                return MainObject.EnsureBinaryData(forceLoadFromDB);
            }


            /// <summary>
            /// Checks if a record with the same column values already exists in the database. Returns true if the set of values is unique.
            /// </summary>
            /// <param name="columns">Columns to check</param>
            public bool CheckUniqueValues(params string[] columns)
            {
                return MainObject.CheckUniqueValues(columns);
            }


            /// <summary>
            /// Checks if the object has unique code name. Returns true if the object has unique code name.
            /// </summary>
            public bool CheckUniqueCodeName()
            {
                return MainObject.CheckUniqueCodeName();
            }


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
                return MainObject.CheckPermissions(permission, currentSiteName, userInfo, exceptionOnFailure);
            }


            /// <summary>
            /// Invalidates the object in the object table.
            /// </summary>
            /// <param name="keepThisInstanceValid">If true, this object is marked as updated to behave as valid</param>
            public void Invalidate(bool keepThisInstanceValid)
            {
                MainObject.Invalidate(keepThisInstanceValid);
            }


            /// <summary>
            /// Returns the clone 
            /// </summary>
            public BaseInfo Clone()
            {
                return MainObject.CloneObject();
            }


            /// <summary>
            /// Inserts the object as a new object to the DB with inner data and structure (according to given settings) cloned from the original.
            /// </summary>
            /// <param name="settings">Cloning settings</param>
            /// <returns>Returns the newly created clone</returns>
            public BaseInfo InsertAsClone(CloneSettings settings)
            {
                CloneResult result = new CloneResult();
                return MainObject.InsertAsClone(settings, result);
            }


            /// <summary>
            /// Inserts the object as a new object to the DB with inner data and structure (according to given settings) cloned from the original.
            /// </summary>
            /// <param name="settings">Cloning settings</param>
            /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
            /// <returns>Returns the newly created clone</returns>
            public BaseInfo InsertAsClone(CloneSettings settings, CloneResult result)
            {
                return MainObject.InsertAsClone(settings, result);
            }


            /// <summary>
            /// Moves the object up within the object order (if OrderColumn is specified). The move is done within the object's parent and site (if defined).
            /// </summary>
            /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
            public void MoveObjectUp(string orderColumn = null)
            {
                MainObject.SetObjectOrder(-1, true, orderColumn);
            }


            /// <summary>
            /// Moves the object down within the object order (if OrderColumn is specified). The move is done within the object's parent and site (if defined).
            /// </summary>
            /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
            public void MoveObjectDown(string orderColumn = null)
            {
                MainObject.SetObjectOrder(1, true, orderColumn);
            }


            /// <summary>
            /// Moves the object to the right position according to the custom order.
            /// </summary>
            /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
            /// <param name="nameColumn">Column by the content of which the alphabetical order will be set</param>
            public void SetObjectAlphabeticalOrder(string orderColumn = null, string nameColumn = null)
            {
                MainObject.SetObjectAlphabeticalOrder(orderColumn, nameColumn);
            }


            /// <summary>
            /// Moves the object to the specified order (if OrderColumn is specified). The move is done within the object's parent and site (if defined).
            /// </summary>
            /// <param name="targetOrder">Desired order of the object</param>
            /// <param name="relativeOrder">If true, the targetOrder parameter is taken as a relative order from current order position</param>
            /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
            public void SetObjectOrder(int targetOrder, bool relativeOrder = false, string orderColumn = null)
            {
                MainObject.SetObjectOrder(targetOrder, relativeOrder, orderColumn);
            }


            /// <summary>
            /// Returns number which will be the last order within all the other items (according to Parent, Group and Site settings).
            /// </summary>
            /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
            public int GetLastObjectOrder(string orderColumn = null)
            {
                return MainObject.GetLastObjectOrder(orderColumn);
            }


            /// <summary>
            /// Initializes the proper order of the sibling objects so the order column is consistent.
            /// </summary>
            /// <param name="orderColumn">Name of the column that is used for storing the object's order. If null, <see cref="ObjectTypeInfo.OrderColumn"/> is taken from <see cref="TypeInfo"/>.</param>
            public void InitObjectsOrder(string orderColumn = null)
            {
                MainObject.InitObjectsOrder(orderColumn);
            }


            /// <summary>
            /// Moves the object to the right position according to the custom order.
            /// </summary>
            /// <param name="asc">Indicates whether the order is ascending or descending (ascending by default).</param>
            /// <param name="orderColumn">Name of the column that is used for storing the object's order. If null, <see cref="ObjectTypeInfo.OrderColumn"/> is taken from <see cref="TypeInfo"/>.</param>
            /// <param name="nameColumn">>Name of the column by which the alphabetical order is to be set.</param>
            public void SortAlphabetically(bool asc = true, string orderColumn = null, string nameColumn = null)
            {
                MainObject.SortAlphabetically(asc, orderColumn, nameColumn);
            }


            /// <summary>
            /// Checks object for dependent objects. Returns true if there is at least one dependent object.
            /// First tries to execute checkdependencies query, if not found, an automatic process is executed.
            /// </summary>
            /// <remarks>
            /// Automated process is based on <see cref="ObjectTypeInfo.DependsOn"/> property. 
            /// Child, site and group objects are not included. Objects whose <see cref="ObjectTypeInfo.ObjectPathColumn"/> value contains a prefix matching the current object's path value are also not included.
            /// </remarks>
            /// <param name="reportAll">If false, only required dependency constraints (without default value) are returned, otherwise checks all dependency relations.</param>
            public bool CheckDependencies(bool reportAll = false)
            {
                return MainObject.CheckDependencies(reportAll);
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
            public List<string> GetDependenciesNames(bool reportAll = false, int topN = 10)
            {
                return MainObject.GetDependenciesNames(reportAll, topN);
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
                return MainObject.GetDependencies(reportAll, topN);
            }


            /// <summary>
            /// Ensures that the object has a unique code name within it's context
            /// </summary>
            public void EnsureUniqueCodeName()
            {
                MainObject.EnsureUniqueCodeName();
            }


            /// <summary>
            /// Returns the unique code name generated from current object code name.
            /// </summary>
            public string GetUniqueCodeName()
            {
                return MainObject.GetUniqueCodeName(ObjectCodeName, 0);
            }


            /// <summary>
            /// Returns the unique display name generated from current object display name.
            /// </summary>
            /// <param name="ensureLength">If true, maximal display name length is ensured</param>
            public string GetUniqueDisplayName(bool ensureLength = true)
            {
                return MainObject.GetUniqueDisplayName(ObjectDisplayName, 0, ensureLength);
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
            public string GetUniqueName(string name, int currentObjectId, string columnName, string suffixFormat, string suffixFormatRegex, bool ensureLength)
            {
                return MainObject.GetUniqueName(name, currentObjectId, columnName, suffixFormat, suffixFormatRegex, ensureLength);
            }


            /// <summary>
            /// Gets the type of the given property
            /// </summary>
            /// <param name="propertyName">Property name</param>
            public Type GetPropertyType(string propertyName)
            {
                return MainObject.GetPropertyType(propertyName);
            }


            /// <summary>
            /// Returns value for translation services. Returns unmodified field content by default.
            /// </summary>
            /// <param name="columnName">Name of the column</param>
            public object GetValueForTranslation(string columnName)
            {
                return MainObject.GetValueForTranslation(columnName);
            }


            /// <summary>
            /// Gets the object thumbnail URL
            /// </summary>
            /// <param name="width">Width</param>
            /// <param name="height">Height</param>
            /// <param name="maxSideSize">Maximum side size, width or height</param>
            public string GetThumbnailUrl(int width, int height, int maxSideSize)
            {
                return MainObject.GetThumbnailUrl(width, height, maxSideSize);
            }


            /// <summary>
            /// Gets the object icon URL
            /// </summary>
            /// <param name="width">Width</param>
            /// <param name="height">Height</param>
            /// <param name="maxSideSize">Maximum side size, width or height</param>
            public string GetIconUrl(int width, int height, int maxSideSize)
            {
                return MainObject.GetIconUrl(width, height, maxSideSize);
            }


            /// <summary>
            /// Gets the object editing page URL.
            /// </summary>
            public string GetEditingPageURL()
            {
                return MainObject.GetEditingPageURL();
            }


            /// <summary>
            /// Reverts the object changes to the original values.
            /// </summary>
            public virtual void RevertChanges()
            {
                NotSupported();
            }


            /// <summary>
            /// Resets the object changes and keeps the new values as unchanged.
            /// </summary>
            public virtual void ResetChanges()
            {
                //NotSupported();
            }


            /// <summary>
            /// Returns the original value of column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public virtual object GetOriginalValue(string columnName)
            {
                NotSupported();
                return null;
            }


            /// <summary>
            /// Gets the column type.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public virtual Type GetColumnType(string columnName)
            {
                NotSupported();
                return null;
            }


            /// <summary>
            /// Throws the not supported exception
            /// </summary>
            protected void NotSupported()
            {
                throw new NotSupportedException("[GeneralizedInfo]: This operation is not supported since the object is low-level object.");
            }


            /// <summary>
            /// Gets dependency object type
            /// </summary>
            /// <param name="dep">Object dependency settings</param>
            public virtual string GetDependencyObjectType(ObjectDependency dep)
            {
                return MainObject.GetDependencyObjectType(dep);
            }


            /// <summary>
            /// Exports the default object installation data
            /// </summary>
            /// <param name="filePath">File path for the export</param>
            /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
            public virtual void ExportDefaultData(string filePath, IEnumerable<string> excludedNames = null)
            {
                MainObject.ExportDefaultData(filePath, excludedNames);
            }


            /// <summary>
            /// Gets where condition for default data according to TypeInfo configuration.
            /// </summary>
            /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
            public virtual string GetDefaultDataWhereCondition(bool globalOnly)
            {
                return MainObject.GetDefaultDataWhereCondition(true, globalOnly);
            }


            /// <summary>
            /// Goes through the columns which are stored externally and updates DB versions with the data from external storage.
            /// </summary>
            public void UpdateExternalColumns()
            {
                MainObject.UpdateExternalColumns();
            }


            /// <summary>
            /// Goes through the columns which are stored externally and deletes all the files.
            /// </summary>
            /// <param name="updateDB">If true, DB is updated with the data from the file before it's deleted.</param>
            public void DeleteExternalColumns(bool updateDB)
            {
                MainObject.DeleteExternalColumns(updateDB);
            }


            /// <summary>
            /// Goes through the columns which are stored externally and ensures them in the external storage.
            /// </summary>
            public void SaveExternalColumns()
            {
                MainObject.SaveExternalColumns(false, false);
            }


            /// <summary>
            /// Goes through the columns which are stored externally and returns the list of particular files this object uses.
            /// </summary>
            public List<string> GetExternalFiles()
            {
                return MainObject.GetExternalFiles();
            }


            /// <summary>
            /// Returns the list of columns registered as the external columns.
            /// </summary>
            public List<string> GetExternalColumns()
            {
                return MainObject.GetExternalColumns();
            }


            /// <summary>
            /// Goes through the columns which are stored externally and checks if the data in DB is the same as in external storage. If all the columns are same returns true, otherwise false.
            /// </summary>
            public bool IsModifiedExternally()
            {
                return MainObject.IsModifiedExternally();
            }


            /// <summary>
            /// Returns true if the object is checked out by the specified user.
            /// </summary>
            /// <param name="user">User</param>
            public bool IsCheckedOutByUser(IUserInfo user)
            {
                return MainObject.IsCheckedOutByUser(user);
            }


            /// <summary>
            /// Returns default object of given object type. Returns null by default. Example is UserInfo which returns user specified in the settings or Global Administrator.
            /// </summary>
            public BaseInfo GetDefaultObject()
            {
                return MainObject.GetDefaultObject();
            }


            /// <summary>
            /// Returns true if the object is child of the given object. If parameter parent is null, returns true only if the object is not a child of any object.
            /// </summary>
            /// <param name="parent">Parent to check</param>
            public bool IsChildOf(BaseInfo parent)
            {
                return MainObject.IsChildOf(parent);
            }


            /// <summary>
            /// Updates given path column.
            /// </summary>
            /// <param name="parentCol">Column which stores parent object ID</param>
            /// <param name="column">Path column</param>
            /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
            /// <param name="updateChildren">Determines whether the parent object changed and therefore it is necessary to update all children</param>
            /// <param name="updateLevel">If true, also the level of the object is update according to the path</param>
            public void UpdatePathColumn(string parentCol, string column, string pathPartColumn, bool updateChildren, bool updateLevel = true)
            {
                MainObject.UpdatePathColumn(parentCol, column, pathPartColumn, updateChildren, updateLevel);
            }


            /// <summary>
            /// Updates the object to the database.
            /// </summary>
            public virtual void UpdateData()
            {
                throw new NotSupportedException();
            }


            /// <summary>
            /// Inserts the object to the database.
            /// </summary>
            public virtual void InsertData()
            {
                throw new NotSupportedException();
            }


            /// <summary>
            /// Deletes the object from the database.
            /// </summary>
            public virtual void DeleteData()
            {
                throw new NotSupportedException();
            }


            /// <summary>
            /// Gets the list of synchronized columns for this object.
            /// </summary>
            public IEnumerable<string> GetSynchronizedColumns()
            {
                return MainObject.GetSynchronizedColumns();
            }


            /// <summary>
            /// Creates new object of the given class
            /// </summary>
            /// <param name="settings">Data settings</param>
            public BaseInfo NewObject(LoadDataSettings settings)
            {
                return MainObject.NewObject(settings);
            }

            #endregion


            #region "IDataContainer Members"

            /// <summary>
            /// Column names.
            /// </summary>
            public List<string> ColumnNames
            {
                get
                {
                    return MainObject.ColumnNames;
                }
            }


            /// <summary>
            /// Returns value of column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            /// <param name="value">Returns the value</param>
            /// <returns>Returns true if the operation was successful (the value was present)</returns>
            public bool TryGetValue(string columnName, out object value)
            {
                return MainObject.TryGetValue(columnName, out value);
            }


            /// <summary>
            /// Returns true if the object contains specified column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public bool ContainsColumn(string columnName)
            {
                return MainObject.ContainsColumn(columnName);
            }

            #endregion


            #region "ISimpleDataContainer Members"

            /// <summary>
            /// Gets or sets the value of the column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public object this[string columnName]
            {
                get
                {
                    return MainObject[columnName];
                }
                set
                {
                    MainObject[columnName] = value;
                }
            }


            /// <summary>
            /// Returns value of column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            public object GetValue(string columnName)
            {
                return MainObject.GetValue(columnName);
            }


            /// <summary>
            /// Sets value of column.
            /// </summary>
            /// <param name="columnName">Column name</param>
            /// <param name="value">Column value</param> 
            public bool SetValue(string columnName, object value)
            {
                return MainObject.SetValue(columnName, value);
            }

            #endregion


            #region "IHierarchicalObject Members"

            /// <summary>
            /// Properties of the object available through GetProperty.
            /// </summary>
            public List<string> Properties
            {
                get
                {
                    return MainObject.Properties;
                }
            }


            /// <summary>
            /// Returns property with given name (either object or property value).
            /// </summary>
            /// <param name="columnName">Column name</param>
            public object GetProperty(string columnName)
            {
                return MainObject.GetProperty(columnName);
            }


            /// <summary>
            /// Returns property with given name (either object or property value).
            /// </summary>
            /// <param name="columnName">Column name</param>
            /// <param name="value">Returns the value</param>
            /// <returns>Returns true if the operation was successful (the value was present)</returns>
            public bool TryGetProperty(string columnName, out object value)
            {
                return MainObject.TryGetProperty(columnName, out value);
            }


            /// <summary>
            /// Returns value of property.
            /// </summary>
            /// <param name="columnName">Column name</param>
            /// <param name="value">Returns the value</param>
            /// <param name="notNull">If true, the property attempts to return non-null values, at least it returns the empty object of the correct type</param>
            /// <returns>Returns true if the operation was successful (the value was present)</returns>
            public bool TryGetProperty(string columnName, out object value, bool notNull)
            {
                return MainObject.TryGetProperty(columnName, out value, notNull);
            }

            #endregion


            #region "IRelatedData Members"

            /// <summary>
            /// Custom data connected to the object.
            /// </summary>
            public object RelatedData
            {
                get
                {
                    return MainObject.RelatedData;
                }
                set
                {
                    MainObject.RelatedData = value;
                }
            }

            #endregion
        }
    }
}
