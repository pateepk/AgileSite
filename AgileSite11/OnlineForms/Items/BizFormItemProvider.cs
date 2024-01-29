using System;
using System.Collections;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.OnlineForms
{
    using TypeInfoDictionary = SafeDictionary<string, BizFormTypeInfo>;

    /// <summary>
    /// Class for retrieving BizForm items.
    /// </summary>
    public class BizFormItemProvider : AbstractInfoProvider<BizFormItem, BizFormItemProvider>
    {
        #region "Variables"

        /// <summary>
        /// BizFrom item prefix for object type.
        /// </summary>
        public const string BIZFORM_ITEM_PREFIX = PredefinedObjectType.BIZFORM_ITEM_PREFIX;


        /// <summary>
        /// License limitation "BizForms" table
        /// </summary>
        private static readonly Hashtable licenseBizForm = new Hashtable();

        /// <summary>
        /// BizForms TypeInfo [className.ToLowerInvariant()] -> [TypeInfo]
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
        /// BizForms TypeInfo [className.ToLowerInvariant()] -> [TypeInfo]
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
        public BizFormItemProvider()
            : base(false)
        {
            // Do not initialize base provider, it will be initialized in LoadProviderInternal
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes
        /// </summary>
        /// <param name="className">Class name of the BizForm</param>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public BizFormItemProvider(string className)
            : base(null, new HashtableSettings { ID = true, UseWeakReferences = true })
        {
            TypeInfo = GetTypeInfo(className);
            ClassName = className;
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns BizForm item of specified class name and item ID.
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="className">Class name of the BizForm</param>
        public static BizFormItem GetItem(int itemId, string className)
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
        /// Returns BizForm item of specified type and item ID.
        /// </summary>
        /// <param name="itemId">Item ID</param>
        public static TType GetItem<TType>(int itemId)
            where TType : BizFormItem, new()
        {
            // License checking
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var className = new TType().ClassName;
            var provider = GetProviderObject(className);
            return provider.GetItemInternal(itemId) as TType;
        }


        /// <summary>
        /// Deletes given BizForm item
        /// </summary>
        /// <param name="item">BizForm item</param>
        public static void DeleteItem(BizFormItem item)
        {
            var provider = GetProviderObject(item.ClassName);
            provider.DeleteItemInternal(item);
        }


        /// <summary>
        /// Sets given BizForm item
        /// </summary>
        /// <param name="item">BizForm item</param>
        public static void SetItem(BizFormItem item)
        {
            var provider = GetProviderObject(item.ClassName);
            provider.SetItemInternal(item);
        }


        /// <summary>
        /// Returns query of all data record for given BizForm.
        /// </summary>
        /// <param name="className">Class name of the BizForm</param>
        public static ObjectQuery<BizFormItem> GetItems(string className)
        {
            // License checking
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var provider = GetProviderObject(className);
            return provider.GetItemsInternal();
        }


        /// <summary>
        /// Returns query of all data records for given type.
        /// </summary>
        public static ObjectQuery<TType> GetItems<TType>()
            where TType : BizFormItem, new()
        {
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                return null;
            }

            var className = new TType().ClassName;
            var provider = GetProviderObject(className);
            return provider.GetItemsInternal().As<ObjectQuery<TType>>();
        }


        /// <summary>
        /// Deletes all items for given BizForm.
        /// </summary>
        /// <param name="className">Class name of the BizForm</param>
        /// <param name="where">Where condition to filter the items</param>
        public static void DeleteItems(string className, string where = null)
        {
            var provider = GetProviderObject(className);
            provider.DeleteItemsInternal(@where);

            ClearLicensesCount();
        }


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

            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, FeatureEnum.BizForms, action != ObjectActionEnum.Insert);
            if (versionLimitations == 0)
            {
                return true;
            }

            if (licenseBizForm[domain] == null)
            {
                var siteId = LicenseHelper.GetSiteIDbyDomain(domain);
                if (siteId > 0)
                {
                    licenseBizForm[domain] = BizFormInfoProvider.GetBizForms().OnSite(siteId).GetCount();
                }
            }

            try
            {
                // Try add
                if (action == ObjectActionEnum.Insert)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licenseBizForm[domain], -1) + 1)
                    {
                        return false;
                    }
                }

                // Get status
                if (action == ObjectActionEnum.Edit)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licenseBizForm[domain], 0))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                ClearLicensesCount();
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
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.BizForms);
            }
        }


        /// <summary>
        /// Clear BizForm items hash count values.
        /// </summary>
        public static void ClearLicensesCount()
        {
            licenseBizForm.Clear();
        }


        /// <summary>
        /// Gets BizForm item class name from given object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static string GetClassName(string objectType)
        {
            return objectType.ToLowerInvariant().Replace(BIZFORM_ITEM_PREFIX, "");
        }


        /// <summary>
        /// Gets BizForm item object type from given class name.
        /// </summary>
        /// <param name="className">Class name</param>
        public static string GetObjectType(string className)
        {
            return BIZFORM_ITEM_PREFIX + className.ToLowerInvariant();
        }


        /// <summary>
        /// Indicates if given object type represents BizForm item.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsBizFormItemObjectType(string objectType)
        {
            return objectType.StartsWith(BIZFORM_ITEM_PREFIX, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Gets BizForm item name.
        /// </summary>
        /// <param name="item">BizFrom table item</param>
        /// <param name="classDisplayName">Class display name</param>
        public static string GetItemName(BizFormItem item, string classDisplayName)
        {
            var provider = GetProviderObject(item.ClassName);
            return provider.GetItemNameInternal(item, classDisplayName);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Deletes given BizForm item
        /// </summary>
        /// <param name="item">BizForm item</param>
        protected virtual void DeleteItemInternal(BizFormItem item)
        {
            DeleteInfo(item);

            // Refresh form items count
            BizFormInfoProvider.RefreshDataCount(item.BizFormInfo);
        }


        /// <summary>
        /// Sets given BizForm item
        /// </summary>
        /// <param name="item">BizForm item</param>
        protected virtual void SetItemInternal(BizFormItem item)
        {
            bool isNew = item.ItemID == 0;

            SetInfo(item);

            if (isNew)
            {
                // Refresh form items count
                BizFormInfoProvider.RefreshDataCount(item.BizFormInfo);
            }
        }


        /// <summary>
        /// Returns BizForm item of specified class name and item ID.
        /// </summary>
        /// <param name="itemId">Item ID</param>
        protected virtual BizFormItem GetItemInternal(int itemId)
        {
            // Prepare parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", itemId);

            // Get data
            var className = TypeInfo.ObjectClassName;
            DataSet ds = ConnectionHelper.ExecuteQuery(className + ".select", parameters);
            return !DataHelper.DataSourceIsEmpty(ds) ? BizFormItem.New(className, ds.Tables[0].Rows[0]) : null;
        }


        /// <summary>
        /// Returns query of all data record for given BizForm.
        /// </summary>
        protected virtual ObjectQuery<BizFormItem> GetItemsInternal()
        {
            return GetObjectQuery();
        }


        /// <summary>
        /// Deletes all items for given class.
        /// </summary>
        /// <param name="where">Where condition to filter the items</param>
        protected virtual void DeleteItemsInternal(string where)
        {
            var className = TypeInfo.ObjectClassName;
            ConnectionHelper.ExecuteQuery(className + ".deleteall", null, where);
        }


        /// <summary>
        /// Gets BizForm item name.
        /// </summary>
        /// <param name="item">BizForm item</param>
        /// <param name="classDisplayName">Class display name</param>
        protected virtual string GetItemNameInternal(BizFormItem item, string classDisplayName)
        {
            return String.Format("{0} - {1}", ResHelper.LocalizeString(classDisplayName), item.ItemID.ToString());
        }

        #endregion


        #region "Provider management methods"

        /// <summary>
        /// Gets provider object.
        /// </summary>
        private static BizFormItemProvider GetProviderObject(string className)
        {
            return InfoProviderLoader.GetInfoProvider<BizFormItemProvider>(GetObjectType(className));
        }


        /// <summary>
        /// Loads BizForm item provider for given object type
        /// </summary>
        /// <param name="objectType">BizForm item object type</param>
        internal static IInfoProvider LoadProviderInternal(string objectType)
        {
            var provider = CMSExtensibilitySection.LoadProvider<BizFormItemProvider>();

            // Initialize provider
            provider.ClassName = GetClassName(objectType);
            provider.InfoObject = BizFormItem.New(provider.ClassName);

            var hashtableSettings = new HashtableSettings
            {
                ID = true,
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
        /// Creates new BizForm item instance
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="useGenerator">If true, the process allows using the generator to differentiate between particular info types based on data</param>
        protected override BizFormItem CreateInfo(DataRow dr = null, bool useGenerator = true)
        {
            string className = TypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN ? TypeInfo.ObjectClassName : ClassName;

            return BizFormItem.New(className, dr);
        }

        #endregion


        #region "Cached TypeInfos methods"

        /// <summary>
        /// Returns the TypeInfo for specified class.
        /// </summary>
        /// <param name="className">Class name</param>
        public static ObjectTypeInfo GetTypeInfo(string className)
        {
            // Try to get from hashtable
            className = className.ToLowerInvariant();

            BizFormTypeInfo existingTypeInfo;
            var exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
            if (exists && existingTypeInfo.IsValid)
            {
                return existingTypeInfo;
            }

            lock (TypeInfos.SyncRoot)
            {
                exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
                if (exists && existingTypeInfo.IsValid)
                {
                    return existingTypeInfo;
                }

                var newInfo = CreateTypeInfo(className);

                // Copy events from existing one
                if (existingTypeInfo != null)
                {
                    existingTypeInfo.CopyBizFormTypeInfoEventsTo(newInfo);
                }

                TypeInfos[className] = newInfo;
                return newInfo;
            }
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
                WebFarmHelper.CreateTask(FormTaskType.ClearBizFormTypeInfos);
            }
        }


        /// <summary>
        /// Invalidates typeinfo specified by class name.
        /// </summary>
        internal static void InvalidateTypeInfo(string className, bool logTask)
        {
            className = className.ToLowerInvariant();
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
                WebFarmHelper.CreateTask(FormTaskType.InvalidateBizFormTypeInfo, null, className);
            }
        }


        /// <summary>
        /// Validates given class name if represents existing on-line form.
        /// </summary>
        /// <param name="className">Class name to validate</param>
        private static bool ValidateClass(string className)
        {
            var dataClassInfo = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(className);
            return (dataClassInfo != null) && dataClassInfo.ClassIsForm;
        }


        /// <summary>
        /// Creates new typeinfo.
        /// </summary>
        private static BizFormTypeInfo CreateTypeInfo(string className)
        {
            // Create new class info
            IDataClass dci = DataClassFactory.NewDataClass(className);
            if (dci == null)
            {
                throw new Exception("[BizFormItemProvider.CreateTypeInfo]: Class '" + className + "' not found.");
            }

            if (!ValidateClass(className))
            {
                throw new Exception("[BizFormItemProvider.CreateTypeInfo]: Class '" + className + "' is not BizForm.");
            }

            // Get type info
            // Get timestamp column
            string timestampColumn = null;
            if (dci.ContainsColumn("FormUpdated"))
            {
                timestampColumn = "FormUpdated";
            }

            // Get the DataClass info
            DataClassInfo classInfo = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(className);
            if (classInfo == null)
            {
                throw new Exception("[BizFormItemProvider.GetTypeInfo]: Class '" + className + "' not found.");
            }

            string objectType = GetObjectType(className);
            var result = new BizFormTypeInfo(typeof(BizFormItemProvider), objectType, className, dci.IDColumn, timestampColumn, null, null, null, null, null, null, null)
            {
                SynchronizationSettings =
                {
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                    LogSynchronization = SynchronizationTypeEnum.None
                },
                LogEvents = true,
                TouchCacheDependencies = false,
                SupportsVersioning = false,
                AllowRestore = false,
                ModuleName = "cms.form",
                IsDataObjectType = true,
                ImportExportSettings =
                {
                    IncludeToExportParentDataSet = IncludeToParentEnum.None,
                    LogExport = false,
                    AllowSingleExport = false
                }
            };

            // Initialize new type info object
            ObjectTypeManager.EnsureObjectTypeInfoDynamicList(result);
            return result;
        }

        #endregion
    }
}