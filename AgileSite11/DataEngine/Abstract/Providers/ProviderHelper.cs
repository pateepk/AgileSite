using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Helper methods for info providers
    /// </summary>
    public static class ProviderHelper
    {
        /// <summary>
        /// All sites constant for the data retrieval.
        /// </summary>
        public const int ALL_SITES = -1;

        /// <summary>
        /// Codename for column names invalidation
        /// </summary>
        internal const string INVALIDATE_COLUMN_NAMES = "cms.invalidatecolumnnames";

        private const string USE_HASHTABLE_SETTING_KEY = "CMSUseHashtable";
        private const string LOAD_HASHTABLE_SETTING_KEY = "CMSLoadHashtables";
        private const string WEAK_REFERENCE_HASHTABLE_SETTING_KEY = "CMSUseHashtableWeakReferences";

        internal const string HASHTABLE_ID = "ID";
        internal const string HASHTABLE_GUID = "GUID";
        internal const string HASHTABLE_NAME = "Name";
        internal const string HASHTABLE_FULLNAME = "FullName";
        internal const string HASHTABLE_OBJECT_TYPE_ALL = "All";


        // Load hashtables.
        private static bool? mLoadHashTables;


        /// <summary>
        /// Load hashtables.
        /// </summary>
        internal static bool LoadHashTablesSettings
        {
            get
            {
                if (mLoadHashTables == null)
                {
                    mLoadHashTables = SettingsHelper.AppSettings[LOAD_HASHTABLE_SETTING_KEY].ToBoolean(SystemContext.IsWebSite);
                }

                return mLoadHashTables.Value;
            }
            set
            {
                mLoadHashTables = value;
            }
        }


        /// <summary>
        /// Returns true if the given dictionary collection should be loaded.
        /// </summary>
        /// <param name="collection">Collection to check</param>
        public static bool LoadTables(ProviderDictionaryCollection collection)
        {
            return (collection == null);
        }


        /// <summary>
        /// Returns true if the given dictionaries should be loaded.
        /// </summary>
        /// <param name="dictionaries">Dictionaries to check</param>
        public static bool LoadTables(params IProviderDictionary[] dictionaries)
        {
            return dictionaries.Any(dict => (dict == null) || !dict.DataIsValid);
        }


        /// <summary>
        /// Returns true if the hashtables for specified object type should be loaded with the data.
        /// Reflects the AppSettings key "CMSLoadHashtables" + objectType.Replace(".", ""), e.g. CMSLoadHashtablesCMSUser.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="defaultValue">Default value in case the key is not present in the web.config file</param>
        public static LoadHashtableEnum LoadHashTables(string objectType, LoadHashtableEnum defaultValue)
        {
            // If globally not loaded, load none
            if (!LoadHashTablesSettings)
            {
                return LoadHashtableEnum.None;
            }

            // Get value from web.config
            string value = SettingsHelper.AppSettings[LOAD_HASHTABLE_SETTING_KEY + objectType.Replace(".", "")];
            if (value == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetBoolean(value, true)
                ? LoadHashtableEnum.All
                : LoadHashtableEnum.None;
        }


        /// <summary>
        /// Returns true if the hashtable for the specified object type should use weak references.
        /// </summary>
        /// <remarks>
        /// Reflects the AppSettings configuration.
        /// At first AppSettings key "CMSUseHashtableWeakReferencesAll" + tableName is searched.
        /// At second AppSettings key "CMSUseHashtableWeakReferences" + objectType.Replace(".", "") + tableName is searched.
        /// </remarks>
        /// <param name="objectType">Object type</param>
        /// <param name="defaultValue">Default value</param>
        internal static bool UseHashtableWeakReferences(string objectType, bool defaultValue)
        {
            var globalSettingValue = UseHashtableWeakReferencesInternal(HASHTABLE_OBJECT_TYPE_ALL, defaultValue);
            var objectTypeSettingValue = UseHashtableWeakReferencesInternal(objectType, globalSettingValue);

            return objectTypeSettingValue;
        }


        /// <summary>
        /// Returns true if the hashtable for the specified object type should use weak references.
        /// Reflects the AppSettings key "CMSUseHashtableWeakReferences" + objectType.Replace(".", ""), e.g. CMSUseHashtableWeakReferencesCMSUser.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="defaultValue">Default value</param>
        private static bool UseHashtableWeakReferencesInternal(string objectType, bool defaultValue)
        {
            var value = SettingsHelper.AppSettings[WEAK_REFERENCE_HASHTABLE_SETTING_KEY + objectType.Replace(".", "")];

            return ValidationHelper.GetBoolean(value, defaultValue);
        }


        /// <summary>
        /// Returns true if the hashtable for the specified object type should be used.
        /// </summary>
        /// <remarks>
        /// Reflects the AppSettings configuration.
        /// At first AppSettings key "CMSUseHashtableAll" + tableName is searched.
        /// At second AppSettings key "CMSUseHashtable" + objectType.Replace(".", "") + tableName is searched.
        /// </remarks>
        /// <param name="objectType">Object type</param>
        /// <param name="tableName">Table name</param>
        /// <param name="defaultValue">Default value</param>
        internal static bool UseHashtable(string objectType, string tableName, bool defaultValue)
        {
            var globalSettingValue = UseHashtableInternal(HASHTABLE_OBJECT_TYPE_ALL, tableName, defaultValue);
            var objectTypeSettingValue = UseHashtableInternal(objectType, tableName, globalSettingValue);

            return objectTypeSettingValue;
        }


        /// <summary>
        /// Returns true if the hashtable for the specified object type should be used. 
        /// Reflects the AppSettings key "CMSUseHashtable" + objectType.Replace(".", "") + tableName, e.g. CMSUseHashtableCMSUserID.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="tableName">Table name</param>
        /// <param name="defaultValue">Default value</param>
        private static bool UseHashtableInternal(string objectType, string tableName, bool defaultValue)
        {
            var key = string.Concat(USE_HASHTABLE_SETTING_KEY, objectType.Replace(".", ""), tableName);
            var value = SettingsHelper.AppSettings[key];

            return ValidationHelper.GetBoolean(value, defaultValue);
        }


        /// <summary>
        /// Gets the info by its type and ID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">ID of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the object type is not found. If false, the null is returned if the object type is not found</param>
        public static BaseInfo GetInfoById(string objectType, int id, bool exceptionIfObjTypeNotFound = false)
        {
            if (id <= 0)
            {
                return null;
            }

            var provider = InfoProviderLoader.GetInfoProvider(objectType, false);
            if (provider != null)
            {
                // Get from provider
                return provider.GetInfoById(id);
            }

            // Get from info object
            BaseInfo obj = ModuleManager.GetReadOnlyObject(objectType);
            if (obj != null)
            {
                // Get parent object
                return obj.GetObject(id);
            }

            if (exceptionIfObjTypeNotFound)
            {
                throw new Exception("The object type '" + objectType + "' not found.");
            }

            return null;
        }


        /// <summary>
        /// Gets the info by its type and ID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">IDs of the objects</param>
        public static SafeDictionary<int, BaseInfo> GetInfosByIds(string objectType, IEnumerable<int> ids)
        {
            if (ids == null)
            {
                return null;
            }

            var provider = InfoProviderLoader.GetInfoProvider(objectType, false);
            if (provider != null)
            {
                // Get the whole list through provider
                return provider.GetInfosByIds(ids);
            }

            // Try to get one by one if provider not found
            var result = new SafeDictionary<int, BaseInfo>();

            foreach (int id in ids)
            {
                var obj = GetInfoById(objectType, id);
                if (obj != null)
                {
                    result[id] = obj;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the info by its type and code name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="name">Name of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static BaseInfo GetInfoByName(string objectType, string name, bool exceptionIfObjTypeNotFound = false)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            var provider = InfoProviderLoader.GetInfoProvider(objectType, exceptionIfObjTypeNotFound);
            return provider?.GetInfoByName(name);
        }


        /// <summary>
        /// Gets the info by its type and code name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="name">Name of the object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static BaseInfo GetInfoByName(string objectType, string name, int siteId, bool exceptionIfObjTypeNotFound = false)
        {
            var provider = InfoProviderLoader.GetInfoProvider(objectType, exceptionIfObjTypeNotFound);
            return provider?.GetInfoByName(name, siteId);
        }


        /// <summary>
        /// Gets the info by its type and full name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="fullName">Full name of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static BaseInfo GetInfoByFullName(string objectType, string fullName, bool exceptionIfObjTypeNotFound = false)
        {
            if (String.IsNullOrEmpty(fullName))
            {
                return null;
            }

            var provider = InfoProviderLoader.GetInfoProvider(objectType, exceptionIfObjTypeNotFound);
            return provider?.GetInfoByFullName(fullName);
        }


        /// <summary>
        /// Gets the info by its type and GUID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="guid">GUID of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static BaseInfo GetInfoByGuid(string objectType, Guid guid, bool exceptionIfObjTypeNotFound = false)
        {
            if (guid == Guid.Empty)
            {
                return null;
            }

            var provider = InfoProviderLoader.GetInfoProvider(objectType, exceptionIfObjTypeNotFound);
            return provider?.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Gets the info by its type and GUID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="guid">GUID of the object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static BaseInfo GetInfoByGuid(string objectType, Guid guid, int siteId, bool exceptionIfObjTypeNotFound = false)
        {
            var provider = InfoProviderLoader.GetInfoProvider(objectType, exceptionIfObjTypeNotFound);
            return provider?.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Gets the code name by the given ID
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        public static string GetCodeName(string objectType, int id)
        {
            if (id <= 0)
            {
                return null;
            }

            // Get the object
            var obj = GetInfoById(objectType, id);

            return obj?.Generalized.ObjectCodeName;
        }


        /// <summary>
        /// Gets the ID by the given code name
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="codeName">Object code name</param>
        public static int GetId(string objectType, string codeName)
        {
            if (string.IsNullOrEmpty(codeName))
            {
                return 0;
            }

            // Get the object
            var obj = GetInfoByName(objectType, codeName);
            if (obj == null)
            {
                return 0;
            }

            return obj.Generalized.ObjectID;
        }


        /// <summary>
        /// Clears hashtables for given object type.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearHashtables(string objectType, bool logTasks)
        {
            var provider = InfoProviderCache.GetInfoProvider<IInfoCacheProvider>(objectType);
            provider?.ClearHashtables(logTasks);
        }


        /// <summary>
        /// Clears up hashtables of every single provider object present in the list.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal static void ClearAllHashtables(bool logTasks)
        {
            var providers = InfoProviderCache.RegisteredProviders.ToList();
            foreach (var provider in providers)
            {
                var cacheprovider = provider as IInfoCacheProvider;
                cacheprovider?.ClearHashtables(logTasks);
            }
        }


        /// <summary>
        /// Invalidates specific provider.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public static void InvalidateProvider(string objectType)
        {
            // Do not throw exception if no provider found (same as invalidated)
            var provider = InfoProviderCache.GetInfoProvider<IInternalProvider>(objectType);
            provider?.Invalidate();
        }
    }
}
