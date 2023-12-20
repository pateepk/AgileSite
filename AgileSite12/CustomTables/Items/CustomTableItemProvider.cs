using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.CustomTables
{
    using IntDictionary = SafeDictionary<string, int?>;
    using TypeInfoDictionary = SafeDictionary<string, CustomTableItemTypeInfo>;

    /// <summary>
    /// Class for retrieving custom table items.
    /// </summary>
    public class CustomTableItemProvider : AbstractInfoProvider<CustomTableItem, CustomTableItemProvider>
    {
        #region "Constants and variables"

        /// <summary>
        /// Custom table item prefix for object type.
        /// </summary>
        internal const string CUSTOM_TABLE_ITEM_PREFIX = PredefinedObjectType.CUSTOM_TABLE_ITEM_PREFIX;

        private const string GUID_COLUMN_NAME = "ItemGUID";
        private const string TIMESTAMP_COLUMN_NAME = "ItemModifiedWhen";
        private const string CREATED_COLUMN_NAME = "ItemCreatedBy";
        private const string MODIFIED_COLUMN_NAME = "ItemModifiedBy";
        private const string ORDER_COLUMN_NAME = "ItemOrder";
        private const string ID_COLUMN_NAME = "ItemID";

        /// <summary>
        /// License limitation custom table
        /// </summary>
        private static readonly IntDictionary mLicensesCustom = new IntDictionary();


        /// <summary>
        /// Custom table TypeInfo [className.ToLowerCSafe()] -> [TypeInfo]
        /// </summary>
        private static readonly CMSStatic<TypeInfoDictionary> mTypeInfos = new CMSStatic<TypeInfoDictionary>(() => new TypeInfoDictionary());

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if the provider instance is up-to-date and can be used to manage object instances.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return base.IsValid && ValidateClass(TypeInfo.ObjectClassName);
            }
        }


        /// <summary>
        /// License limitation custom table
        /// </summary>
        private static IntDictionary LicensesCustom
        {
            get
            {
                return mLicensesCustom;
            }
        }


        /// <summary>
        /// Custom table TypeInfo [className.ToLowerCSafe()] -> [TypeInfo]
        /// </summary>
        private static TypeInfoDictionary TypeInfos
        {
            get
            {
                return mTypeInfos;
            }
        }


        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor (Creates uninitialized provider.)
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public CustomTableItemProvider()
            : base(false)
        {
            // Do not initialize base provider, it will be initialized in LoadProviderInternal
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes
        /// </summary>
        /// <param name="className">Class name of the custom table</param>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public CustomTableItemProvider(string className)
            : base(null, new HashtableSettings { ID = true, GUID = true, UseWeakReferences = true })
        {
            TypeInfo = GetTypeInfo(className);
            ClassName = className;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTableItemProvider"/> class.
        /// </summary>
        /// <param name="initialize">Indicates if provider together with hash tables should be initialized.</param>
        /// <param name="className">Class name of the custom table.</param>
        protected CustomTableItemProvider(bool initialize, string className)
            : base(initialize)
        {
            TypeInfo = GetTypeInfo(className);
            ClassName = className;
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns custom table item of specified class name and item ID.
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="className">Class name</param>
        public static CustomTableItem GetItem(int itemId, string className)
        {
            // License checking
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var provider = GetProviderObject(className);
            return provider.GetItemInternal(itemId);
        }


        /// <summary>
        /// Returns custom table item of specified type.
        /// </summary>
        public static TType GetItem<TType>(int itemId)
            where TType : CustomTableItem, new()
        {
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var className = new TType().ClassName;
            var provider = GetProviderObject(className);
            return provider.GetItemInternal(itemId) as TType;
        }


        /// <summary>
        /// Returns custom table item of specified type.
        /// </summary>
        public static TType GetItem<TType>(Guid itemGuid)
            where TType : CustomTableItem, new()
        {
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var className = new TType().ClassName;
            var provider = GetProviderObject(className);
            return provider.GetItemInternal(itemGuid) as TType;
        }


        /// <summary>
        /// Returns custom table item of specified class name and item GUID.
        /// </summary>
        /// <param name="itemGuid">Item GUID</param>
        /// <param name="className">Class name</param>
        public static CustomTableItem GetItem(Guid itemGuid, string className)
        {
            // License checking
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var provider = GetProviderObject(className);
            return provider.GetItemInternal(itemGuid);
        }


        /// <summary>
        /// Deletes given custom table item
        /// </summary>
        /// <param name="item">Custom table item</param>
        public static void DeleteItem(CustomTableItem item)
        {
            var provider = GetProviderObject(item.ClassName);
            provider.DeleteItemInternal(item);
        }


        /// <summary>
        /// Sets given custom table item
        /// </summary>
        /// <param name="item">Custom table item</param>
        public static void SetItem(CustomTableItem item)
        {
            var provider = GetProviderObject(item.ClassName);
            provider.SetItemInternal(item);
        }


        /// <summary>
        /// Returns the query for all custom table items.
        /// </summary>
        /// <param name="className">Custom table class name</param>
        public static ObjectQuery<CustomTableItem> GetItems(string className)
        {
            var provider = GetProviderObject(className);
            var items = provider.GetItemsInternal();

            // License checking
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                items.ReturnNoResults();
            }

            return items;
        }


        /// <summary>
        /// Returns all custom table items of specified type.
        /// </summary>
        public static ObjectQuery<TType> GetItems<TType>()
             where TType : CustomTableItem, new()
        {
            var className = new TType().ClassName;
            var provider = GetProviderObject(className);
            var items = provider.GetItemsInternal().As<ObjectQuery<TType>>();

            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                items.ReturnNoResults();
            }

            return items;
        }


        /// <summary>
        /// Returns the query of all data record for given custom table filtered out by where condition and ordered by given expression.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        public static ObjectQuery<CustomTableItem> GetItems(string className, string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetItems(className).Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes all items for given class.
        /// </summary>
        /// <param name="className">Custom table class name</param>
        /// <param name="where">Where condition to filter the items</param>
        public static void DeleteItems(string className, string where = null)
        {
            var provider = GetProviderObject(className);
            provider.DeleteItemsInternal(@where);

            ClearLicensesCount(true);
        }


        /// <summary>
        /// Returns last item order for specified custom table.
        /// </summary>
        /// <param name="className">Custom table class name</param>
        public static int GetLastItemOrder(string className)
        {
            var provider = GetProviderObject(className);
            return provider.GetLastItemOrderInternal();
        }


        /// <summary>
        /// Gets custom table class name from given object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static string GetClassName(string objectType)
        {
            return objectType.ToLowerCSafe().Replace(CUSTOM_TABLE_ITEM_PREFIX, "");
        }


        /// <summary>
        /// Gets custom table object type from given class name.
        /// </summary>
        /// <param name="className">Class name</param>
        public static string GetObjectType(string className)
        {
            return CUSTOM_TABLE_ITEM_PREFIX + className.ToLowerCSafe();
        }


        /// <summary>
        /// Indicates if given object type represents custom table item.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsCustomTableItemObjectType(string objectType)
        {
            return objectType.StartsWithCSafe(CUSTOM_TABLE_ITEM_PREFIX, true);
        }


        /// <summary>
        /// Gets custom table item name.
        /// </summary>
        /// <param name="item">Custom table item</param>
        /// <param name="classDisplayName">Class display name</param>
        public static string GetItemName(CustomTableItem item, string classDisplayName)
        {
            if (item == null)
            {
                return null;
            }

            var provider = GetProviderObject(item.ClassName);
            return provider.GetItemNameInternal(item, classDisplayName);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes given custom table item
        /// </summary>
        /// <param name="item">Custom table item</param>
        protected virtual void DeleteItemInternal(CustomTableItem item)
        {
            DeleteInfo(item);
        }


        /// <summary>
        /// Sets given custom table item
        /// </summary>
        /// <param name="item">Custom table item</param>
        protected virtual void SetItemInternal(CustomTableItem item)
        {
            SetInfo(item);
        }


        /// <summary>
        /// Returns custom table item of specified class name and item ID.
        /// </summary>
        /// <param name="itemId">Item ID</param>
        protected virtual CustomTableItem GetItemInternal(int itemId)
        {
            return GetInfoById(itemId);
        }


        /// <summary>
        /// Returns custom table item of specified class name and item GUID.
        /// </summary>
        /// <param name="itemGuid">Item GUID</param>
        protected virtual CustomTableItem GetItemInternal(Guid itemGuid)
        {
            return GetInfoByGuid(itemGuid);
        }


        /// <summary>
        /// Returns the query for all custom table items.
        /// </summary>
        protected virtual ObjectQuery<CustomTableItem> GetItemsInternal()
        {
            return GetObjectQuery();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Deletes all items for given class.
        /// </summary>
        /// <param name="where">Where condition to filter the items</param>
        protected virtual void DeleteItemsInternal(string where)
        {
            BulkDelete(new WhereCondition(where));
        }


        /// <summary>
        /// Gets custom table item name.
        /// </summary>
        /// <param name="item">Custom table item</param>
        /// <param name="classDisplayName">Class display name</param>
        protected virtual string GetItemNameInternal(CustomTableItem item, string classDisplayName)
        {
            string value = null;

            var structure = item.TypeInfo.ClassStructureInfo;
            if (structure != null)
            {
                // Find first string column
                var stringColumns = structure.StringColumns;
                if ((stringColumns != null) && (stringColumns.Count > 0))
                {
                    // Get column value
                    value = ValidationHelper.GetString(item.GetValue(stringColumns.First()), null);
                }
            }

            // Get name
            var name = !String.IsNullOrEmpty(value) ? ResHelper.LocalizeString(value) : item.ItemID.ToString();
            return String.Format("{0} - {1}", ResHelper.LocalizeString(classDisplayName), name);
        }


        /// <summary>
        /// Returns last item order for specified custom table.
        /// </summary>
        protected virtual int GetLastItemOrderInternal()
        {
            var className = TypeInfo.ObjectClassName;
            var orderBy = String.Format("{0}{1}", ORDER_COLUMN_NAME, SqlHelper.ORDERBY_DESC);
            var ds = GetItems(className, null, orderBy, 1, ORDER_COLUMN_NAME);
            return !DataHelper.DataSourceIsEmpty(ds) ? ValidationHelper.GetInteger(ds.Tables[0].Rows[0][ORDER_COLUMN_NAME], 0) : 0;
        }

        #endregion


        #region "Licenses"

        /// <summary>
        /// License version checker.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="action">Type of action - edit, insert, delete</param>
        /// <returns>Returns true if feature is without any limitations for domain and action</returns>
        public static bool LicenseVersionCheck(string domain, ObjectActionEnum action)
        {
            // Parse domain name to remove port etc.
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }

            // Check license limitations
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, FeatureEnum.CustomTables, action != ObjectActionEnum.Insert);
            if (versionLimitations == 0)
            {
                return true;
            }

            if (LicensesCustom[domain] == null)
            {
                var siteId = LicenseHelper.GetSiteIDbyDomain(domain);
                if (siteId > 0)
                {
                    LicensesCustom[domain] = CustomTableHelper.GetCustomTableClasses(siteId).Count;
                }
            }

            try
            {
                // Try add
                if (action == ObjectActionEnum.Insert)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(LicensesCustom[domain], -1) + 1)
                    {
                        return false;
                    }
                }

                // Get status
                if (action == ObjectActionEnum.Edit)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(LicensesCustom[domain], 0))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                ClearLicensesCount(true);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        public static void CheckLicense(ObjectActionEnum action, string domain)
        {
            if (!LicenseVersionCheck(domain, action))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.CustomTables);
            }
        }


        /// <summary>
        /// Clears custom tables hash count values.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearLicensesCount(bool logTasks = false)
        {
            LicensesCustom.Clear();

            if (logTasks)
            {
                WebFarmHelper.CreateTask(new ClearCustomTableLicensesCountWebFarmTask());
            }
        }

        #endregion


        #region "Provider management methods"

        /// <summary>
        /// Gets provider object.
        /// </summary>
        private static CustomTableItemProvider GetProviderObject(string className)
        {
            return InfoProviderLoader.GetInfoProvider<CustomTableItemProvider>(GetObjectType(className));
        }


        /// <summary>
        /// Loads custom table item provider for given object type
        /// </summary>
        /// <param name="objectType">Custom table item object type</param>
        internal static IInfoProvider LoadProviderInternal(string objectType)
        {
            var provider = CMSExtensibilitySection.LoadProvider<CustomTableItemProvider>();

            // Initialize provider
            provider.ClassName = GetClassName(objectType);
            provider.InfoObject = CustomTableItem.New(provider.ClassName);

            var hashtableSettings = new HashtableSettings
            {
                ID = true,
                GUID = true,
                UseWeakReferences = true
            };

            provider.Init(null, hashtableSettings);

            return provider;
        }


        /// <summary>
        /// Invalidates specific provider.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        internal static void InvalidateProvider(string objectType)
        {
            ProviderHelper.InvalidateProvider(objectType);
        }


        /// <summary>
        /// Creates new custom table item instance
        /// </summary>
        /// <param name="dr">Data row with the object data</param>
        /// <param name="useGenerator">If true, the process allows using the generator to differentiate between particular info types based on data</param>
        protected override CustomTableItem CreateInfo(DataRow dr = null, bool useGenerator = true)
        {
            string className = TypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN ? TypeInfo.ObjectClassName : ClassName;

            return CustomTableItem.New(className, dr);
        }

        #endregion


        #region "Cached TypeInfo methods"

        /// <summary>
        /// Returns the TypeInfo for specified class.
        /// </summary>
        /// <param name="className">Class name</param>
        public static ObjectTypeInfo GetTypeInfo(string className)
        {
            // Try to get from hash table
            className = className.ToLowerCSafe();

            CustomTableItemTypeInfo existingTypeInfo;
            var exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
            if (!exists || !existingTypeInfo.IsValid)
            {
                lock (TypeInfos.SyncRoot)
                {
                    exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
                    if (!exists || !existingTypeInfo.IsValid)
                    {
                        var newInfo = CreateTypeInfo(className);

                        // Copy events from existing one
                        if (existingTypeInfo != null)
                        {
                            existingTypeInfo.CopyCustomTableItemTypeInfoEventsTo(newInfo);
                        }

                        TypeInfos[className] = newInfo;
                        return newInfo;
                    }
                }
            }

            return existingTypeInfo;
        }


        /// <summary>
        /// Clear the class info and properties lists of all object types.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        internal static void Clear(bool logTask)
        {
            TypeInfos.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearCustomTableTypeInfosWebFarmTask());
            }
        }


        /// <summary>
        /// Invalidates typeinfo specified by class name.
        /// </summary>
        internal static void InvalidateTypeInfo(string className, bool logTask)
        {
            className = className.ToLowerCSafe();
            lock (TypeInfos.SyncRoot)
            {
                var oldInfo = TypeInfos[className];

                if (oldInfo != null)
                {
                    // Invalidate type info
                    oldInfo.IsValid = false;
                }
            }

            if (logTask)
            {
                WebFarmHelper.CreateTask(new InvalidateCustomTableTypeInfoWebFarmTask { ClassName = className });
            }
        }


        /// <summary>
        /// Validates given class name if represents existing custom table.
        /// </summary>
        /// <param name="className">Class name to validate</param>
        private static bool ValidateClass(string className)
        {
            var dataClassInfo = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(className);
            return (dataClassInfo != null) && dataClassInfo.ClassIsCustomTable;
        }


        /// <summary>
        /// Creates new typeinfo.
        /// </summary>
        private static CustomTableItemTypeInfo CreateTypeInfo(string className)
        {
            // Create new class info
            var dci = DataClassFactory.NewDataClass(className);
            if (dci == null)
            {
                throw new Exception("[CustomTableItemProvider.CreateTypeInfo]: Class '" + className + "' not found.");
            }

            if (!ValidateClass(className))
            {
                throw new Exception("[CustomTableItemProvider.CreateTypeInfo]: Class '" + className + "' is not custom table.");
            }

            // Get type info
            // Get GUID column
            string guidColumn = null;
            SynchronizationTypeEnum logSynchronization = SynchronizationTypeEnum.None;
            if (dci.ContainsColumn(GUID_COLUMN_NAME))
            {
                guidColumn = GUID_COLUMN_NAME;
                logSynchronization = SynchronizationTypeEnum.LogSynchronization;
            }

            // Get timestamp column
            string timestampColumn = null;
            if (dci.ContainsColumn(TIMESTAMP_COLUMN_NAME))
            {
                timestampColumn = TIMESTAMP_COLUMN_NAME;
            }

            // Get bindings
            List<string> list = new List<string>(2);
            if (dci.ContainsColumn(CREATED_COLUMN_NAME))
            {
                list.Add(CREATED_COLUMN_NAME);
            }

            if (dci.ContainsColumn(MODIFIED_COLUMN_NAME))
            {
                list.Add(MODIFIED_COLUMN_NAME);
            }

            List<ObjectDependency> dependencies = null;
            int arraySize = list.Count;
            if (arraySize > 0)
            {
                dependencies = new List<ObjectDependency>();
                for (int i = 0; i < arraySize; ++i)
                {
                    dependencies.Add(new ObjectDependency(list[i], PredefinedObjectType.USER));
                }
            }

            string objectType = GetObjectType(className);
            // Create the type info
            var result = new CustomTableItemTypeInfo(typeof(CustomTableItemProvider), objectType, className, dci.IDColumn, timestampColumn, guidColumn, null, null, null, null, null, null)
            {
                SynchronizationSettings =
                {
                    LogSynchronization = logSynchronization,
                },
                LogEvents = true,
                TouchCacheDependencies = true,
                SupportsVersioning = false,
                DependsOn = dependencies,
                AllowRestore = false,
                ModuleName = ModuleName.CUSTOMTABLES,
                OrderColumn = dci.ContainsColumn(ORDER_COLUMN_NAME) ? ORDER_COLUMN_NAME : ID_COLUMN_NAME,
                IsDataObjectType = true,
                ImportExportSettings =
                {
                    IncludeToExportParentDataSet = IncludeToParentEnum.None,
                    LogExport = false,
                    AllowSingleExport = false
                },
                ContinuousIntegrationSettings =
                {
                    Enabled = guidColumn != null
                },
                SerializationSettings =
                {
                    ExcludedFieldNames =
                    {
                        "ItemCreatedWhen"
                    }
                }
            };

            // Initialize new type info object
            ObjectTypeManager.EnsureObjectTypeInfoDynamicList(result);
            return result;
        }

        #endregion
    }
}