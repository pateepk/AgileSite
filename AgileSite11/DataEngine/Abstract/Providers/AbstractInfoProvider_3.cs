using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Abstract info class provider.
    /// </summary>
    public abstract class AbstractInfoProvider<TInfo, TProvider, TQuery> : IInfoProvider, ITestableProvider, IInternalProvider, IInfoCacheProvider<TInfo>
        where TInfo : AbstractInfoBase<TInfo>, new()
        where TProvider : AbstractInfoProvider<TInfo, TProvider, TQuery>, new()
        where TQuery : ObjectQueryBase<TQuery, TInfo>, new()
    {
        #region "Variables"

        // If true, provider hashtables store empty values for not found objects to reduce database calls
        private static bool? mStoreEmptyValuesInHashtables;
        private TInfo mInfoObject;
        private static readonly CMSStatic<TProvider> mProviderObject = new CMSStatic<TProvider>();
        private static readonly object providerLoadLock = new object();

        internal readonly object hashtableLock = new object();

        /// <summary>
        /// Info tables.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        protected ProviderDictionaryCollection infos;

        #endregion


        #region "Properties"

        /// <summary>
        /// Object type information.
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get;
            protected set;
        } = InfoHelper.UNKNOWN_TYPEINFO;


        /// <summary>
        /// Provider object.
        /// </summary>
        public static TProvider ProviderObject
        {
            get
            {
                if (mProviderObject.Value == null)
                {
                    lock (providerLoadLock)
                    {
                        if (mProviderObject.Value == null)
                        {
                            mProviderObject.Value = CMSExtensibilitySection.LoadProvider<TProvider>();
                        }
                    }
                }

                return mProviderObject.Value;
            }
            set
            {
                lock (providerLoadLock)
                {
                    mProviderObject.Value = value;
                }
            }
        }


        /// <summary>
        /// Gets hashtable settings
        /// </summary>
        public HashtableSettings HashtableSettings
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if the provider instance is up-to-date and can be used to manage object instances.
        /// </summary>
        public virtual bool IsValid
        {
            get;
            private set;
        } = true;


        /// <summary>
        /// Data source for the provider
        /// </summary>
        public DataQuerySource DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Info object instance the provider is working with.
        /// </summary>
        protected TInfo InfoObject
        {
            get
            {
                return mInfoObject ?? (mInfoObject = CreateInfo(null, false));
            }
            set
            {
                mInfoObject = value;
            }
        }


        /// <summary>
        /// If true, provider hashtables store empty values for not found objects to reduce database calls
        /// </summary>
        private static bool StoreEmptyValuesInHashtables
        {
            get
            {
                if (mStoreEmptyValuesInHashtables == null)
                {
                    mStoreEmptyValuesInHashtables = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStoreEmptyValuesInHashtables"], true);
                }

                return mStoreEmptyValuesInHashtables.Value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractInfoProvider()
        {
            TypeManager.RegisterGenericType(typeof(AbstractInfoProvider<TInfo, TProvider, TQuery>));
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="initialize">Indicates if provider together with hash tables should be initialized</param>
        protected AbstractInfoProvider(bool initialize)
        {
            if (!initialize)
            {
                return;
            }

            Init();
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settings">Hashtable settings</param>
        /// <param name="typeInfo">Object type information</param>
        protected AbstractInfoProvider(ObjectTypeInfo typeInfo, HashtableSettings settings = null)
        {
            Init(typeInfo, settings);
        }


        /// <summary>
        /// Initializes the provider with the given type info and hashtable settings
        /// </summary>
        /// <param name="typeInfo">Type info of the objects which the provider manages</param>
        /// <param name="settings">Hashtable settings for internal provider cache</param>
        protected void Init(ObjectTypeInfo typeInfo = null, HashtableSettings settings = null)
        {
            typeInfo = typeInfo ?? InfoObject.TypeInfo;

            InitHashtableSettings(typeInfo, settings);

            InitTypeInfoAndRegister(typeInfo);
        }


        /// <summary>
        /// Initializes the provider variables.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        protected void InitTypeInfoAndRegister(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                return;
            }

            TypeInfo = typeInfo;
            InfoProviderCache.Register(TypeInfo.ObjectType, this);
        }


        /// <summary>
        /// Creates a new info object.
        /// </summary>
        /// <param name="dr">Data to use to create new object</param>
        /// <param name="useGenerator">If true, the process allows using the generator to differentiate between particular info types based on data</param>
        protected virtual TInfo CreateInfo(DataRow dr = null, bool useGenerator = true)
        {
            var settings = new LoadDataSettings(dr);

            // Create new instance
            TInfo infoObj =
                useGenerator ?
                (TInfo)InfoObject.Generalized.NewObject(settings) :
                new TInfo();

            if (!useGenerator && (settings.Data != null))
            {
                // Init the data
                infoObj.LoadData(settings);
            }

            return infoObj;
        }

        #endregion


        #region "ID methods"

        /// <summary>
        /// Gets an instance of info object based on ID.
        /// </summary>
        /// <param name="id">Value of the record ID to look for</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoById(int id, bool useHashtable = true)
        {
            if (id <= 0)
            {
                return null;
            }

            if (!useHashtable || !HashtableSettings.ID)
            {
                return GetInfoByColumn(TypeInfo.IDColumn, id);
            }

            LoadInfos();

            // Get object from hashtable
            BaseInfo result = infos.ById[id];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByColumn(TypeInfo.IDColumn, id);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ById[id] = InfoHelper.EmptyInfo;
                }
            }

            return result as TInfo;
        }


        /// <summary>
        /// Gets the list of info objects by their IDs.
        /// </summary>
        /// <param name="ids">List of IDs to get</param>
        protected virtual SafeDictionary<int, BaseInfo> GetInfosByIds(IEnumerable<int> ids)
        {
            SafeDictionary<int, BaseInfo> result = new SafeDictionary<int, BaseInfo>();
            List<int> missingIds = new List<int>();

            // Search for existing ones in hashtable
            bool hashtable = HashtableSettings.ID;
            if (hashtable)
            {
                LoadInfos();
            }

            foreach (var id in ids)
            {
                // Get object from hashtable
                if (hashtable)
                {
                    var info = infos.ById[id];
                    if (info != null)
                    {
                        result[id] = info;
                        continue;
                    }
                }

                missingIds.Add(id);
            }

            // Get missing infos
            if (missingIds.Count > 0)
            {
                var query = GetObjectQuery()
                    .WhereIn(TypeInfo.IDColumn, missingIds)
                    .BinaryData(false);

                LoadObjectsToHashtables(query, result);
            }

            return result;
        }


        BaseInfo IInfoProvider.GetInfoById(int id)
        {
            return GetInfoById(id);
        }


        SafeDictionary<int, BaseInfo> IInfoProvider.GetInfosByIds(IEnumerable<int> ids)
        {
            return GetInfosByIds(ids);
        }

        #endregion


        #region "Code name methods"

        /// <summary>
        /// Gets an instance of info object based on the given code name.
        /// </summary>
        /// <param name="codeName">Code name</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByCodeName(string codeName, bool useHashtable = true)
        {
            AssertCodeNameColumnExists();

            // No result if code name empty
            if (String.IsNullOrEmpty(codeName))
            {
                return null;
            }

            if (!useHashtable || !HashtableSettings.Name)
            {
                return GetInfoByColumn(TypeInfo.CodeNameColumn, codeName);
            }

            LoadInfos();

            string key = codeName;

            // Get object from hashtable
            BaseInfo result = infos.ByCodeName[key];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByColumn(TypeInfo.CodeNameColumn, codeName);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ByCodeName[key] = InfoHelper.EmptyInfo;
                }
            }

            return result as TInfo;
        }


        /// <summary>
        /// Gets an instance of info object based on the given code name.
        /// </summary>
        /// <param name="codeName">Code name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByCodeName(string codeName, int siteId, bool useHashtable)
        {
            return GetInfoByCodeName(codeName, siteId, 0, useHashtable);
        }


        /// <summary>
        /// Gets an instance of info object based on the given code name.
        /// </summary>
        /// <param name="codeName">Code name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByCodeName(string codeName, int siteId, int groupId = 0, bool useHashtable = true)
        {
            AssertCodeNameColumnExists();

            // No result if code name empty
            if (String.IsNullOrEmpty(codeName))
            {
                return null;
            }

            if (!useHashtable || !HashtableSettings.Name)
            {
                return GetInfoByCodeNameInternal(codeName, siteId, groupId);
            }

            LoadInfos();

            string key = codeName + "|" + siteId;

            // Add group key part
            if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                key += "|" + groupId;
            }

            // Get object from hashtable
            BaseInfo result = infos.ByCodeName[key];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByCodeNameInternal(codeName, siteId, groupId);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ByCodeName[key] = InfoHelper.EmptyInfo;
                }
            }

            return result as TInfo;
        }


        /// <summary>
        /// Gets an instance of info object based on the given code name.
        /// </summary>
        /// <param name="codeName">Code name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="useHashtable">Indicates if value should be returned from hashtable if hashtables are used</param>
        /// <param name="searchGlobal">If true, global objects are also searched when site object is not found</param>
        protected virtual TInfo GetInfoByCodeName(string codeName, int siteId, bool useHashtable, bool searchGlobal)
        {
            AssertCodeNameColumnExists();

            TInfo result = GetInfoByCodeName(codeName, siteId, useHashtable);
            if (searchGlobal && (result == null) && (siteId > 0))
            {
                // Search for global object if site object not found
                result = GetInfoByCodeName(codeName, 0, useHashtable);
            }

            return result;
        }


        /// <summary>
        /// Gets an instance of info object based on the given code name from database.
        /// </summary>
        /// <param name="codeName">Code name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Community group ID</param>
        private TInfo GetInfoByCodeNameInternal(string codeName, int siteId, int groupId)
        {
            // Prepare the where condition for site
            var where = new WhereCondition()
                .WhereEquals(TypeInfo.CodeNameColumn, codeName)
                .WhereID(TypeInfo.SiteIDColumn, siteId);

            // Prepare the where condition for group
            if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                where.WhereID(TypeInfo.GroupIDColumn, groupId);
            }

            return GetObjectQuery().TopN(1).Where(where).FirstOrDefault();
        }


        BaseInfo IInfoProvider.GetInfoByName(string name)
        {
            return GetInfoByCodeName(name);
        }


        BaseInfo IInfoProvider.GetInfoByName(string name, int siteId)
        {
            return GetInfoByCodeName(name, siteId);
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Gets an instance of info object based on the given full name.
        /// </summary>
        /// <param name="fullName">Full name</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByFullName(string fullName, bool useHashtable = true)
        {
            // No result if code name empty
            if (String.IsNullOrEmpty(fullName))
            {
                return null;
            }

            if (!useHashtable || !HashtableSettings.FullName)
            {
                return GetInfoByFullNameInternal(fullName);
            }

            LoadInfos();

            string key = fullName;

            // Get object from hashtable
            BaseInfo result = infos.ByFullName[key];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByFullNameInternal(fullName);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ByFullName[key] = InfoHelper.EmptyInfo;
                }
            }

            return result as TInfo;
        }


        /// <summary>
        /// Gets the info by its full name from the database.
        /// </summary>
        /// <param name="fullName">Full name</param>
        private TInfo GetInfoByFullNameInternal(string fullName)
        {
            var provider = this as IFullNameInfoProvider;
            if (provider != null)
            {
                // Get the full name where condition
                string where = provider.GetFullNameWhereCondition(fullName);
                if (String.IsNullOrEmpty(where))
                {
                    return null;
                }

                // Get the object
                return GetObjectQuery().TopN(1).Where(new WhereCondition(where)).FirstOrDefault();
            }

            return null;
        }


        BaseInfo IInfoProvider.GetInfoByFullName(string fullName)
        {
            return GetInfoByFullName(fullName);
        }

        #endregion


        #region "GUID methods"

        /// <summary>
        /// Gets an instance of info object based on the given GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByGuid(Guid guid, bool useHashtable = true)
        {
            AssertGuidColumnExists();

            // No result if guid empty
            if (guid == Guid.Empty)
            {
                return null;
            }

            if (!useHashtable || !HashtableSettings.GUID)
            {
                return GetInfoByColumn(TypeInfo.GUIDColumn, guid);
            }

            LoadInfos();

            // Get object from hashtable
            BaseInfo result = infos.ByGuid[guid];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByColumn(TypeInfo.GUIDColumn, guid);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ByGuid[guid] = InfoHelper.EmptyInfo;
                }
            }
            return result as TInfo;
        }


        /// <summary>
        /// Gets an instance of info object based on the GUID.
        /// </summary>
        /// <param name="guid">GUID of the object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="useHashtable">If true, the object is taken through the hashtable</param>
        protected virtual TInfo GetInfoByGuid(Guid guid, int siteId, bool useHashtable = true)
        {
            AssertGuidColumnExists();

            // No result if GUID empty
            if (guid == Guid.Empty)
            {
                return null;
            }

            // If site column does not exist, get by method without site ID
            if (TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return GetInfoByGuid(guid, useHashtable);
            }

            if (!useHashtable || !HashtableSettings.GUID)
            {
                return GetInfoByGuidInternal(guid, siteId);
            }

            LoadInfos();

            string key = guid + "|" + siteId;

            // Get object from hashtable
            BaseInfo result = infos.ByGuidAndSite[key];
            if (result == null)
            {
                // Get object from database
                result = GetInfoByGuidInternal(guid, siteId);

                if (result != null)
                {
                    // Register the object within hashtables
                    RegisterObjectInHashtables((TInfo)result);
                }
                else if (StoreEmptyValuesInHashtables)
                {
                    // Store empty value in hashtable
                    infos.ByGuidAndSite[key] = InfoHelper.EmptyInfo;
                }
            }

            return result as TInfo;
        }


        /// <summary>
        /// Gets an instance of info object based on the given GUID from database.
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="siteId">Site ID</param>
        private TInfo GetInfoByGuidInternal(Guid guid, int siteId)
        {
            // GUID where condition
            var where = new WhereCondition().WhereEquals(TypeInfo.GUIDColumn, guid);

            // Prepare the where condition for site
            if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                where.WhereID(TypeInfo.SiteIDColumn, siteId);
            }

            return GetObjectQuery().TopN(1).Where(where).FirstOrDefault();
        }


        BaseInfo IInfoProvider.GetInfoByGuid(Guid guid)
        {
            return GetInfoByGuid(guid);
        }


        BaseInfo IInfoProvider.GetInfoByGuid(Guid guid, int siteId)
        {
            return GetInfoByGuid(guid, siteId);
        }

        #endregion


        #region "Get / Update / Insert / Delete methods"

        /// <summary>
        /// Gets an instance of info object based on the column value.
        /// </summary>
        /// <remarks>
        /// Supposed to be overriden just in <see cref="DataClassInfoProvider"/>
        /// </remarks>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value to look for</param>
        internal virtual TInfo GetInfoByColumn<T>(string columnName, T value)
        {
            return GetObjectQuery().WhereEquals(columnName, value).BinaryData(true).TopN(1).FirstObject;
        }


        /// <summary>
        /// Bulk deletes info objects based on the given condition.
        /// </summary>
        /// <param name="where">Where condition for the objects which should be deleted.</param>
        /// <param name="settings">Configuration settings</param>
        /// <remarks>
        /// <para>
        /// Note that the delete process of individual object types within dependencies does not run within transaction. If it fails, some data may be cleared and not rolled back.
        /// If you require transaction, wrap the call of this method to <see cref="CMSTransactionScope" />.
        /// </para>
        /// <para>
        /// Method is not executing any customizations for remove dependencies routine (custom code in <see cref="BaseInfo.RemoveObjectDependencies"/> or 'removedependencies' query). All customizations must be called prior this method or handled within <see cref="ObjectEvents.BulkDelete"/> event.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="BulkDeleteSettings.ObjectType"/> cannot be deleted by current provider.</exception>
        public void BulkDelete(IWhereCondition where, BulkDeleteSettings settings = null)
        {
            settings = settings ?? new BulkDeleteSettings();

            var objectType = settings.ObjectType ?? InfoObject.TypeInfo.ObjectType;

            var obj = ModuleManager.GetReadOnlyObject(objectType, true);
            if (obj.TypeInfo.ProviderObject != this)
            {
                throw new InvalidOperationException("Provider of type " + GetType().Name + " cannot delete object type " + objectType + ", use corresponding " + obj.TypeInfo.ProviderObject.GetType().Name + " instead.");
            }

            var completeWhere =
                new WhereCondition()
                    .Where(where)
                    .Where(obj.TypeInfo.TypeCondition?.GetWhereCondition());

            using (var h = TypeInfo.Events.BulkDelete.StartEvent(obj.TypeInfo, where))
            {
                if (settings.RemoveDependencies)
                {
                    new ObjectDependenciesRemover().RemoveObjectDependencies(objectType, where);
                }

                DeleteDataInternal(completeWhere);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Bulk inserts the given list of info objects.
        /// </summary>
        /// <remarks>
        /// Info object ID is not set during the bulk insert operation.
        /// </remarks>
        /// <param name="objects">List of objects</param>
        /// <param name="settings">Configuration for the bulk insert, e.g. timeout. If null, default configuration is used.</param>
        public void BulkInsertInfos(IEnumerable<TInfo> objects, BulkInsertSettings settings = null)
        {
            // Filter only not-null
            var list = objects.Where(obj => obj != null).ToArray();

            // Pre-process infos
            foreach (var infoObj in list)
            {
                infoObj.EnsureSystemFields();
                infoObj.SaveExternalColumns(false, false);
            }

            // Create the DataSet of info data
            var ds = new InfoDataSet<TInfo>(list);
            var dt = ds.Tables[0];

            var dci = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);

            using (var h = TypeInfo.Events.BulkInsert.StartEvent(TypeInfo, list))
            {
                // Point to the right database (in case of separation)
                using (!String.IsNullOrEmpty(dci.ClassConnectionString) ? new CMSConnectionScope(dci.ClassConnectionString, true) : null)
                {
                    // Bulk insert the data to database
                    ConnectionHelper.BulkInsert(dt, dci.ClassTableName, settings ?? CreateDefaultBulkInsertSettings());
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Creates bulk insert settings. By default <see cref="SqlBulkCopyOptions.CheckConstraints"/>
        /// and <see cref="SqlBulkCopyOptions.TableLock"/> flags are set.
        /// </summary>
        protected virtual BulkInsertSettings CreateDefaultBulkInsertSettings()
        {
            return new BulkInsertSettings
            {
                Options = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock
            };
        }


        /// <summary>
        /// Bulk inserts the given list of info objects
        /// </summary>
        /// <remarks>
        /// Info object ID is not set during the bulk insert operation
        /// </remarks>
        /// <param name="objects">List of objects</param>
        public void BulkInsertInfos(IEnumerable<BaseInfo> objects)
        {
            BulkInsertInfos(objects.Cast<TInfo>());
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected virtual void SetInfo(TInfo info)
        {
            CheckObject(info);

            var infoObj = info.Generalized;

            // Check the license
            var isNew = (infoObj.ObjectID <= 0);
            info.Generalized.CheckLicense(isNew ? ObjectActionEnum.Insert : ObjectActionEnum.Edit);

            // Binding
            if (infoObj.TypeInfo.IsBinding)
            {
                // Check consistency
                if (!info.IsComplete)
                {
                    throw new Exception("[AbstractInfoProvider.SetInfo]: Object IDs not set.");
                }

                // Binding with PK
                if (infoObj.ObjectID > 0)
                {
                    // Update
                    infoObj.UpdateData();
                }
                else
                {
                    // Insert / Update with binding
                    infoObj.SetData();
                }
            }
            else
            {
                // Validate the code name
                if (!ValidateCodeName(info))
                {
                    throw new CodeNameNotValidException(infoObj);
                }

                using (var tr = new CMSLateBoundTransaction(GetType()))
                {
                    // Check the code name uniqueness
                    if (infoObj.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Check only if new object or code name changed
                        if (isNew || info.ItemChanged(infoObj.CodeNameColumn))
                        {
                            // Only start transaction if validation of unique code name is required
                            if (infoObj.CheckUnique)
                            {
                                tr.BeginTransaction();

                                // Check the unique code name
                                if (!CheckUniqueCodeName(info))
                                {
                                    throw new CodeNameNotUniqueException(infoObj);
                                }
                            }

                            if (!isNew)
                            {
                                // Remove original code name from hashtable
                                RemoveOriginalCodeNameFromHashtable(info);
                            }
                        }
                    }

                    if (infoObj.ObjectID > 0)
                    {
                        // Update
                        infoObj.UpdateData();
                    }
                    else if (TypeInfo.UseUpsert)
                    {
                        // Allow for upsert
                        infoObj.SetData();
                    }
                    else
                    {
                        // Insert
                        infoObj.InsertData();
                    }

                    tr.Commit();
                }
            }

            // Invalidate the object
            if (TypeInfo.SupportsInvalidation)
            {
                infoObj.Invalidate(true);
            }
        }


        /// <summary>
        /// Checks if the given info object is not null. Throws an exception if it is.
        /// </summary>
        /// <param name="info">Info object to check</param>
        protected static void CheckObject(TInfo info)
        {
            if (info == null)
            {
                throw new NotSupportedException("[AbstractInfoProvider.CheckObject]: You must specify the object.");
            }
        }


        /// <summary>
        /// Validates the object code name. Returns true if the code name is valid.
        /// </summary>
        /// <param name="info">Object to check</param>
        public virtual bool ValidateCodeName(TInfo info)
        {
            var infoObj = info.Generalized;

            if (!infoObj.ValidateCodeName || (TypeInfo.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return true;
            }

            string codeName = infoObj.ObjectCodeName;
            if (String.IsNullOrEmpty(codeName))
            {
                return false;
            }

            return ValidationHelper.IsCodeName(codeName);
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        /// <param name="infoObj">Info object to check</param>
        public virtual bool CheckUniqueCodeName(TInfo infoObj)
        {
            return !infoObj.Generalized.CheckUnique || infoObj.CheckUniqueCodeName();
        }


        /// <summary>
        /// Checks if a record with the same column values already exists in the database. Returns true if the set of values is unique.
        /// </summary>
        /// <param name="infoObj">Info object to check</param>
        /// <param name="columns">Columns to check</param>
        public virtual bool CheckUniqueValues(TInfo infoObj, params string[] columns)
        {
            return infoObj.CheckUniqueValues(columns);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected virtual void DeleteInfo(TInfo info)
        {
            if (info == null)
            {
                return;
            }

            using (var tr = BeginTransaction())
            {
                // Load hashtables
                LoadInfos();

                // Ensure ID for binding without ID set
                var ti = info.TypeInfo;
                var infoObj = info.Generalized;
                if (ti.IsBinding && (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (infoObj.ObjectID <= 0))
                {
                    var existing = infoObj.GetExisting();
                    if (existing != null)
                    {
                        infoObj.ObjectID = existing.Generalized.ObjectID;
                    }
                }

                // Delete the object
                infoObj.DeleteData();

                // Invalidate the object
                if (TypeInfo.SupportsInvalidation)
                {
                    infoObj.Invalidate(true);
                }

                tr.Commit();
            }
        }


        /// <summary>
        /// Creates a new transaction over this provider.
        /// </summary>
        /// <returns></returns>
        protected ITransactionScope BeginTransaction()
        {
            return TransactionScopeFactory.GetTransactionScope(GetType());
        }


        /// <summary>
        /// Deletes the data in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        private void DeleteDataInternal(IWhereCondition where)
        {
            var q = GetDeleteQuery().Where(where);

            // Execute the query
            q.Execute();

            // Clear cached objects
            ClearHashtables(true);

            if (TypeInfo.TouchCacheDependencies)
            {
                // If the InfoObject managed by this InfoProvider has more TypeInfos, override the DeleteData method and touch all required cache dependencies manually
                CacheHelper.TouchKeys(new[] { TypeInfo.ObjectType + "|all" });
            }
        }


        /// <summary>
        /// Updates the data in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value]</param>
        /// <param name="useAPI">If true, data is updated using the API. If false, with bulk update query</param>
        protected virtual void UpdateData(IWhereCondition where, IEnumerable<KeyValuePair<string, object>> values, bool useAPI = false)
        {
            if (useAPI)
            {
                BulkUpdateWithAPI(where, values);
            }
            else
            {
                BulkUpdateWithQuery(where, values);
            }
        }


        /// <summary>
        /// Updates the data based on the given where condition using a database query
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value]</param>
        private void BulkUpdateWithQuery(IWhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            var changedColumns = values.Select(item => item.Key);

            using (var h = TypeInfo.Events.BulkUpdate.StartEvent(TypeInfo, where, changedColumns))
            {
                // Update the data with bulk query
                var q = new DataQuery(TypeInfo.ObjectClassName, QueryName.GENERALUPDATE).Where(where);

                q.SetUpdateQueryValues(values);
                q.Execute();

                // Clear cached objects
                ClearHashtables(true);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Updates the data in based on the given where condition using the API
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value]</param>
        private void BulkUpdateWithAPI(IWhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            // Get all matching objects
            var objects = GetObjectQuery().Where(where);

            objects.ForEachObject(o =>
            {
                // Update values
                foreach (var value in values)
                {
                    o.SetValue(value.Key, value.Value);
                }

                // Save the object
                o.Generalized.SetObject();
            });
        }


        /// <summary>
        /// Updates the data in the database based on the given where condition.
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateData(string updateExpression, QueryDataParameters parameters, string where)
        {
            UpdateData(updateExpression, parameters, new WhereCondition().Where(where, parameters));
        }


        /// <summary>
        /// Updates the data in the database based on the given where condition.
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="whereCondition">Where condition</param>
        private void UpdateData(string updateExpression, QueryDataParameters parameters, IWhereCondition whereCondition)
        {
            // Prepare parameters
            parameters = parameters ?? new QueryDataParameters();

            parameters.AddMacro(QueryMacros.VALUES, updateExpression);

            using (var h = TypeInfo.Events.BulkUpdate.StartEvent(TypeInfo, whereCondition ?? new WhereCondition()))
            {
                var e = h.EventArguments;

                ConnectionHelper.ExecuteQuery(e.TypeInfo.ObjectClassName + ".generalupdate", parameters, e.WhereCondition.ToString(true));

                ClearHashtables(true);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Gets the where condition to limit the objects to specific site.
        /// </summary>
        /// <param name="siteId">Site ID to filter by</param>
        /// <param name="combine">True - both site and global objects are included. False - only site objects are included</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public virtual WhereCondition GetSiteWhereCondition(int siteId, bool combine = false)
        {
            // Site ID where condition
            if (siteId != ProviderHelper.ALL_SITES)
            {
                return TypeInfo.GetSiteWhereCondition(siteId, combine);
            }

            return null;
        }


        /// <summary>
        /// Creates site where condition from the specified parameters and adds it to the original where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="siteId">Site ID. Set to 0 for global objects only</param>
        /// <param name="keyName">Name of the settings key which says whether global objects are used/allowed. Set to null if it doesn't depends on any settings key</param>
        /// <param name="combine">True - site objects are returned together with the global objects. False - only global objects are returned</param>
        /// <param name="op">Logic operator ("AND" or "OR") which says how the site condition is joined with the original condition</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public virtual string AddSiteWhereCondition(string where, int siteId, string keyName, bool combine, string op = "AND")
        {
            string siteWhere = "";

            // Site condition is based on the settings
            if (!string.IsNullOrEmpty(keyName))
            {
                // Allow global objects if global objects are requested
                bool allowGlobal = (siteId <= 0);

                // Allow global objects according to the site settings
                string siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);
                if (!string.IsNullOrEmpty(siteName))
                {
                    allowGlobal = SettingsKeyInfoProvider.GetBoolValue(siteName + "." + keyName);
                }

                string siteIdcolumn = TypeInfo.SiteIDColumn;
                if ((siteIdcolumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                {
                    if (allowGlobal)
                    {
                        // Include global objects
                        siteWhere = siteIdcolumn + " IS NULL";

                        if (combine && (siteId > 0))
                        {
                            // Include site objects
                            siteWhere += " OR " + siteIdcolumn + " = " + siteId;
                        }
                    }
                    else if (siteId > 0)
                    {
                        // Only site objects
                        siteWhere = siteIdcolumn + " = " + siteId;
                    }
                }
            }
            // Site condition is not based on the settings
            else
            {
                siteWhere = TypeInfo.GetSiteWhereCondition(siteId, combine).ToString(true);
            }

            return SqlHelper.AddWhereCondition(where, siteWhere, op);
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        [Obsolete("Use GetObjectQuery().TopN(1) instead.")]
        protected TQuery GetSingleObject()
        {
            return GetObjectQuery().TopN(1);
        }


        /// <summary>
        /// Gets an object query from the provider
        /// </summary>
        /// <param name="checkLicense">If true, the call checks the license</param>
        IObjectQuery IInternalProvider.GetGeneralObjectQuery(bool checkLicense)
        {
            return GetObjectQuery(checkLicense);
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        /// <param name="checkLicense">If true, the call checks the license</param>
        protected TQuery GetObjectQuery(bool checkLicense = true)
        {
            // Indicates if query should be allowed to return results.
            bool noResultsQuery = false;

            if (checkLicense)
            {
                // Prevent recursion - License check may need additional query calls, pass if recursive.
                using (var rc = new RecursionControl("GetObjectQuery|" + TypeInfo.ObjectType))
                {
                    if (rc.Continue)
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
                                InfoObject.Generalized.CheckLicense();
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
                }
            }

            var q = GetObjectQueryInternal();

            // Set the query not to return any results after license check failed.
            if (noResultsQuery)
            {
                q.ReturnNoResults();
            }

            q = ApplySource(q);

            return q;
        }


        /// <summary>
        /// If the provider has a data source specified, applies this source to the given query. Returns the modified query.
        /// </summary>
        /// <param name="query">Query object to be given the source.</param>
        private TQuery ApplySource(TQuery query)
        {
            var result = query;
            if ((DataSource != null) && (query != null))
            {
                result = query.WithSource(DataSource);
            }

            return result;
        }


        /// <summary>
        /// Gets the object query that deletes all items matching the query parameters
        /// </summary>
        protected TQuery GetDeleteQuery()
        {
            var q = GetObjectQueryInternal();
            q.QueryName = QueryName.GENERALDELETE;

            return q;
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        protected abstract TQuery GetObjectQueryInternal();

        #endregion


        #region "Hashtable methods"

        /// <summary>
        /// Initializes the <see cref="HashtableSettings"/> based on <paramref name="typeInfo"/> and using <paramref name="settings"/> as default values.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        /// <param name="settings">Hashtable settings; if not specified the settings are initialized with <see cref="DataEngine.HashtableSettings.UseWeakReferences"/> set to <c>true</c></param>
        protected void InitHashtableSettings(ObjectTypeInfo typeInfo, HashtableSettings settings = null)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (settings == null)
            {
                settings = new HashtableSettings
                {
                    UseWeakReferences = true
                };
            }

            var objectType = typeInfo.ObjectType;

            settings.ID = ProviderHelper.UseHashtable(objectType, ProviderHelper.HASHTABLE_ID, settings.ID) && (typeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            settings.Name = ProviderHelper.UseHashtable(objectType, ProviderHelper.HASHTABLE_NAME, settings.Name) && (typeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            settings.GUID = ProviderHelper.UseHashtable(objectType, ProviderHelper.HASHTABLE_GUID, settings.GUID) && (typeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            settings.FullName = ProviderHelper.UseHashtable(objectType, ProviderHelper.HASHTABLE_FULLNAME, settings.FullName);
            settings.UseWeakReferences = ProviderHelper.UseHashtableWeakReferences(objectType, settings.UseWeakReferences);

            var loadHashtables = ProviderHelper.LoadHashTables(ProviderHelper.HASHTABLE_OBJECT_TYPE_ALL, settings.Load);
            settings.Load = ProviderHelper.LoadHashTables(objectType, loadHashtables);

            HashtableSettings = settings;
        }


        /// <summary>
        /// Updates the object instance in the hashtables. Updates is different than register, because it logs task about changing object.
        /// </summary>
        void IInfoCacheProvider<TInfo>.UpdateObjectInHashtables(TInfo info)
        {
            UpdateObjectInHashtables(info);
        }


        /// <summary>
        /// Deletes the object instance from the hashtables.
        /// </summary>
        void IInfoCacheProvider<TInfo>.DeleteObjectFromHashtables(TInfo info)
        {
            DeleteObjectFromHashtables(info);
        }


        /// <summary>
        /// Loads  the objects.
        /// </summary>
        /// <returns>Returns true if the data was not empty</returns>
        private void LoadData(ProviderDictionaryCollection sender, object parameter)
        {
            // Load the Data infos
            var query = GetObjectQuery().BinaryData(false);
            LoadObjectsToHashtables(query, null);
        }


        /// <summary>
        /// Loads the objects from a DataSet to the hashtables
        /// </summary>
        /// <param name="query">Source query</param>
        /// <param name="objectsById">Table where the created objects will be placed [ObjectID -> object]</param>
        private void LoadObjectsToHashtables(TQuery query, SafeDictionary<int, BaseInfo> objectsById)
        {
            LoadInfos();

            // Load the objects to hashtable
            query.ForEachRow(row =>
            {
                TInfo infoObj = CreateInfo(row);

                // Register to the output table
                if (objectsById != null)
                {
                    objectsById[infoObj.Generalized.ObjectID] = infoObj;
                }

                // Register the object within hashtables
                RegisterObjectInHashtables(infoObj);
            });
        }


        /// <summary>
        /// Registers the object instance within the hashtables.
        /// </summary>
        /// <param name="info">Object to register</param>
        protected virtual void RegisterObjectInHashtables(TInfo info)
        {
            if (!HashtableSettings.UseHashtables || (info == null))
            {
                return;
            }

            // Make sure the infos are loaded
            LoadInfos();

            var infoObj = info.Generalized;

            // Index by object ID
            if (HashtableSettings.ID)
            {
                infos.ById[infoObj.ObjectID] = infoObj;
            }

            // Index by code name
            if (HashtableSettings.Name)
            {
                string nameKey = infoObj.ObjectCodeName;
                // Add site ID column value
                if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectSiteID;
                }
                // Add group ID column value
                if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectGroupID;
                }

                infos.ByCodeName[nameKey] = infoObj;
            }

            // Index by GUID
            if (HashtableSettings.GUID)
            {
                if (TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // GUID only
                    infos.ByGuid[infoObj.ObjectGUID] = infoObj;
                }
                else
                {
                    // GUID + Site
                    string guidKey = infoObj.ObjectGUID + "|" + infoObj.ObjectSiteID;

                    infos.ByGuidAndSite[guidKey] = infoObj;
                }
            }

            // Index by full name
            if (HashtableSettings.FullName)
            {
                string nameKey = infoObj.ObjectFullName;

                if (nameKey == null)
                {
                    throw new Exception("The object '" + infoObj.ObjectDisplayName + "' of type '" + info.TypeInfo.ObjectType + "' doesn't implement the ObjectFullName property.");
                }

                // Register if full name not empty
                if (!String.IsNullOrEmpty(nameKey))
                {
                    infos.ByFullName[nameKey] = infoObj;
                }
            }
        }


        /// <summary>
        /// Removes the original object code name from the hashtable
        /// </summary>
        /// <param name="info">Object to remove</param>
        protected virtual void RemoveOriginalCodeNameFromHashtable(TInfo info)
        {
            if (!HashtableSettings.UseHashtables)
            {
                return;
            }

            // Make sure the infos are loaded
            LoadInfos();

            // Index by code name
            if (HashtableSettings.Name)
            {
                var infoObj = info.Generalized;

                string nameKey = infoObj.OriginalObjectCodeName;
                if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectSiteID;
                }
                if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectGroupID;
                }

                infos.ByCodeName.Remove(nameKey);
            }
        }


        /// <summary>
        /// Updates the object instance in the hashtables. Update is different than <see cref="RegisterObjectInHashtables(TInfo)"/>, because it logs task about changing object.
        /// </summary>
        /// <param name="info">Object to update</param>
        protected virtual void UpdateObjectInHashtables(TInfo info)
        {
            if (!HashtableSettings.UseHashtables)
            {
                return;
            }

            // Make sure the infos are loaded
            LoadInfos();

            var infoObj = info.Generalized;

            // Index by object ID
            if (HashtableSettings.ID)
            {
                infos.ById.Update(infoObj.ObjectID, infoObj);
            }

            // Index by code name
            if (HashtableSettings.Name)
            {
                string nameKey = infoObj.ObjectCodeName;
                if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectSiteID;
                }
                if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectGroupID;
                }
                infos.ByCodeName.Update(nameKey, infoObj);
            }

            // Index by GUID
            if (HashtableSettings.GUID)
            {
                if (TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // GUID only
                    infos.ByGuid.Update(infoObj.ObjectGUID, infoObj);
                }
                else
                {
                    // GUID + SiteID
                    string guidKey = infoObj.ObjectGUID + "|" + infoObj.ObjectSiteID;

                    infos.ByGuidAndSite.Update(guidKey, infoObj);
                }
            }

            // Index by full name
            if (HashtableSettings.FullName)
            {
                string nameKey = infoObj.ObjectFullName;

                infos.ByFullName.Update(nameKey, infoObj);
            }
        }


        /// <summary>
        /// Deletes the object instance from the hashtables.
        /// </summary>
        /// <param name="info">Object to delete</param>
        /// <exception cref="ArgumentNullException">When info is null</exception>
        protected virtual void DeleteObjectFromHashtables(TInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "Info object cannot be null");
            }

            if (!HashtableSettings.UseHashtables)
            {
                return;
            }
            var infoObj = info.Generalized;

            // Index by object ID
            if (HashtableSettings.ID)
            {
                infos.ById.Delete(infoObj.ObjectID);
            }

            // Index by code name
            if (HashtableSettings.Name)
            {
                string nameKey = infoObj.ObjectCodeName;
                if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectSiteID;
                }
                if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    nameKey += "|" + infoObj.ObjectGroupID;
                }
                infos.ByCodeName.Delete(nameKey);
            }

            // Index by GUID
            if (HashtableSettings.GUID)
            {
                if (TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // GUID only
                    infos.ByGuid.Delete(infoObj.ObjectGUID);
                }
                else
                {
                    // GUID + SiteID
                    string guidKey = infoObj.ObjectGUID + "|" + infoObj.ObjectSiteID;

                    infos.ByGuidAndSite.Delete(guidKey);
                }
            }

            // Index by full name
            if (HashtableSettings.FullName)
            {
                string nameKey = infoObj.ObjectFullName;

                infos.ByFullName.Delete(nameKey);
            }
        }


        /// <summary>
        /// Loads all objects from the database to memory.
        /// </summary>
        protected void LoadInfos()
        {
            if (!HashtableSettings.UseHashtables)
            {
                return;
            }

            if (ProviderHelper.LoadTables(infos))
            {
                lock (hashtableLock)
                {
                    if (ProviderHelper.LoadTables(infos))
                    {
                        // Load default items
                        infos = CreateHashtables();
                        infos.LoadDefaultItems();
                    }
                }
            }
        }


        /// <summary>
        /// Creates the provider hashtables
        /// </summary>
        private ProviderDictionaryCollection CreateHashtables()
        {
            string objectType = TypeInfo.ObjectType;

            // Create hashtables if not present
            var colInfos = new ProviderDictionaryCollection(objectType, HashtableSettings.Load, LoadData);

            // Init the hashtables
            if (HashtableSettings.Name)
            {
                // Codename OR CodeName;SiteID;GroupID
                string cols = TypeInfo.CodeNameColumn;
                if (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    cols += ";" + TypeInfo.SiteIDColumn;
                }
                if (TypeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    cols += ";" + TypeInfo.GroupIDColumn;
                }

                colInfos.ByCodeName = new ProviderInfoDictionary<string>(objectType, cols, useWeakReferences: HashtableSettings.UseWeakReferences);
            }

            if (HashtableSettings.ID)
            {
                // ID
                colInfos.ById = new ProviderInfoDictionary<int>(objectType, TypeInfo.IDColumn, useWeakReferences: HashtableSettings.UseWeakReferences);
            }

            if (HashtableSettings.GUID)
            {
                if (TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    // GUID
                    colInfos.ByGuid = new ProviderInfoDictionary<Guid>(objectType, TypeInfo.GUIDColumn, useWeakReferences: HashtableSettings.UseWeakReferences);
                }
                else
                {
                    // GUID + SiteID
                    string cols = TypeInfo.GUIDColumn + ";" + TypeInfo.SiteIDColumn;

                    // GUID
                    colInfos.ByGuidAndSite = new ProviderInfoDictionary<string>(objectType, cols, useWeakReferences: HashtableSettings.UseWeakReferences);
                }
            }

            var provider = this as IFullNameInfoProvider;
            if (HashtableSettings.FullName && provider != null)
            {
                // Full name
                colInfos.ByFullName = provider.GetFullNameDictionary();
            }

            return colInfos;
        }


        /// <summary>
        /// Loads all infos to the hashtable
        /// </summary>
        protected void LoadAllInfos()
        {
            LoadInfos();

            lock (hashtableLock)
            {
                infos.LoadAll();
            }
        }


        /// <summary>
        /// Clears the object's hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        void IInfoCacheProvider.ClearHashtables(bool logTasks)
        {
            ClearHashtables(logTasks);
        }


        /// <summary>
        /// Clears the object's hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected virtual void ClearHashtables(bool logTasks)
        {
            if (infos == null)
            {
                return;
            }

            lock (hashtableLock)
            {
                infos.Clear(logTasks);
            }
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Executes query and returns result as a dataset.
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="where">WHERE expression</param>
        /// <param name="orderBy">Sort expression</param>
        /// <param name="topN">Top N expression</param>
        /// <param name="columns">Columns expression</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        [Obsolete("Use member CMS.DataEngine.ConnectionHelper.ExecuteQuery(string, QueryDataParameters, string, string, int, string, int, int, ref int) instead.")]
        public static InfoDataSet<TInfo> ExecuteQuery(string queryName, QueryDataParameters parameters, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            // Prepare the DataSet for result
            if (parameters == null)
            {
                parameters = new QueryDataParameters();
            }
            parameters.FillDataSet = new InfoDataSet<TInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery(queryName, parameters, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords).As<TInfo>();
        }


        /// <summary>
        /// Gets the number of items matching the given condition. Does not check the license when retrieving data.
        /// </summary>
        /// <param name="parameters">Parameters to apply to the query</param>
        [Obsolete("Use member CMS.DataEngine.Query.GetCount<TInfo>(this ObjectQuery<TInfo>) instead.")]
        public static int GetCount(Action<TQuery> parameters = null)
        {
            // Get the number of items using the given where condition
            var query = ProviderObject.GetObjectQueryInternal().Column(new AggregatedColumn(AggregationType.Count, null).As("Count"));

            // Apply parameters
            parameters?.Invoke(query);

            return query.GetScalarResult<int>();
        }

        #endregion


        #region "Order methods"

        /// <summary>
        /// Sorts the object alphabetically.
        /// </summary>
        /// <param name="ascending">If true, the resulting order will be ascending</param>
        /// <param name="parentId">Only objects under this parent object will be sorted</param>
        /// <param name="siteId">Only object within this site will be sorted</param>
        /// <param name="groupId">Only objects within this group will be sorted</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public void SortAlphabetically(bool ascending, int parentId = 0, int siteId = 0, int groupId = 0)
        {
            var fakeInfo = ModuleManager.GetObject(TypeInfo.ObjectType);
            if (fakeInfo != null)
            {
                // Set parameters which are important for the order
                fakeInfo.Generalized.ObjectParentID = parentId;
                fakeInfo.Generalized.ObjectSiteID = siteId;
                fakeInfo.Generalized.ObjectGroupID = groupId;

                fakeInfo.Generalized.SortAlphabetically(ascending, TypeInfo.OrderColumn, TypeInfo.DisplayNameColumn);
            }
        }

        #endregion


        #region "Exception helpers"

        /// <summary>
        /// Throws a <see cref="System.NotSupportedException"/> if the object does not have a code name property.
        /// </summary>
        private void AssertCodeNameColumnExists()
        {
            if (TypeInfo.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var message = $"The {TypeInfo.ObjectType} object type does not have a code name property";
                throw new NotSupportedException(message);
            }
        }


        /// <summary>
        /// Throws a <see cref="System.NotSupportedException"/> if the object type does not have a GUID property.
        /// </summary>
        private void AssertGuidColumnExists()
        {
            if (TypeInfo.GUIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var message = $"The {TypeInfo.ObjectType} object type does not have a GUID property";
                throw new NotSupportedException(message);
            }
        }

        #endregion


        #region "Provider methods"

        /// <summary>
        /// Sets this provider instance as the default provider
        /// </summary>
        public void SetAsDefaultProvider()
        {
            SetProviderTo((TProvider)this);
        }


        /// <summary>
        /// Gets the current provider instance
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ITestableProvider ITestableProvider.GetCurrentProvider()
        {
            return ProviderObject;
        }


        /// <summary>
        /// Sets the provider to the given provider object
        /// </summary>
        /// <param name="newProvider">New provider object</param>
        private static void SetProviderTo(TProvider newProvider)
        {
            var existingProvider = (mProviderObject.Value != null) ? ProviderObject : null;

            // Do not invalidate provider when new provider is the same instance.
            if (existingProvider != newProvider)
            {
                // Set new provider
                ProviderObject = newProvider;

                // Invalidate previous provider
                existingProvider?.Invalidate();
            }
        }


        /// <summary>
        /// Resets the provider to default implementation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void ITestableProvider.ResetToDefault()
        {
            SetProviderTo(null);
        }


        /// <summary>
        /// Sets this provider as invalid
        /// </summary>
        public void Invalidate()
        {
            IsValid = false;
        }


        /// <summary>
        /// Resets the provider to its original implementation
        /// </summary>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static TProvider EnsureProviderObject()
        {
            return ProviderObject;
        }

        #endregion


        #region "WebFarm methods"

        /// <summary>
        /// Creates web farm task specific for current object and action name
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        public virtual void CreateWebFarmTask(string actionName, string data)
        {
            WebFarmHelper.CreateTask(DataTaskType.ProcessObject, TypeInfo.ObjectType + "|" + actionName + "|0", data);
        }


        /// <summary>
        /// Creates web farm task specific for current object and action name
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary value</param>
        /// <param name="filePath">File path</param>
        public virtual void CreateWebFarmTask(string actionName, string data, byte[] binary, string filePath)
        {
            string binaryFlag = (binary == null) ? "0" : "1";
            WebFarmHelper.CreateIOTask(DataTaskType.ProcessObject, filePath, binary, TypeInfo.ObjectType + "|" + actionName + "|" + binaryFlag, data);
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        public virtual void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            if (actionName == ProviderHelper.INVALIDATE_COLUMN_NAMES)
            {
                TypeInfo.InvalidateColumnNames(false);
            }
            else
            {
                ProcessWebFarmTaskInternal(actionName, data, binary);
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        protected virtual void ProcessWebFarmTaskInternal(string actionName, string data, byte[] binary)
        {
            throw new NotSupportedException("This method must be overridden by the inheriting class '" + GetType().Name + "'.");
        }

        #endregion
    }
}