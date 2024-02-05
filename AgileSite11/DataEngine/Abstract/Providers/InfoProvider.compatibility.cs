using System;
using System.Collections.Generic;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// This class is available only for backward compatibility and should not be used.
    /// </summary>
    /// <exclude />
    [Obsolete("Use AbstractInfoProvider<TInfo, TProvider, TQuery> instead.")]
    public static class AbstractProvider
    {

        /// <summary>
        /// Clears up hashtables of every single provider object present in the list.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        [Obsolete("Use method CMS.DataEngine.ModuleManager.ClearHashtables(bool)")]
        public static void ClearAllHashtables(bool logTasks)
        {
            ModuleManager.ClearHashtables(logTasks);
        }


        /// <summary>
        /// Clears hashtables of the object passed.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        [Obsolete("Use method CMS.DataEngine.ProviderHelper.ClearHashtables(string, bool) instead.")]
        public static void ClearHashtables(string objectType, bool logTasks)
        {
            ProviderHelper.ClearHashtables(objectType, logTasks);
        }
    }


    /// <summary>
    /// This class is available only for backward compatibility and should not be used.
    /// </summary>
    /// <exclude />
    [Obsolete("Use AbstractInfoProvider<TInfo, TProvider, TQuery> instead.")]
    public static class BaseAbstractInfoProvider
    {
        /// <summary>
        /// All sites constant for the data retrieval.
        /// </summary>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.ALL_SITES instead")]
        public const int ALL_SITES = ProviderHelper.ALL_SITES;


        /// <summary>
        /// Gets the info by its type and ID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">ID of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the object type is not found. If false, the null is returned if the object type is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoById(string, int, bool) instead.")]
        public static BaseInfo GetInfoById(string objectType, int id, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoById(objectType, id, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the info by its type and ID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">IDs of the objects</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfosByIds(string, IEnumerable<int>) instead.")]
        public static SafeDictionary<int, BaseInfo> GetInfosByIds(string objectType, IEnumerable<int> ids)
        {
            return ProviderHelper.GetInfosByIds(objectType, ids);
        }


        /// <summary>
        /// Gets the info by its type and code name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="name">Name of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoByName(string, string, bool) instead.")]
        public static BaseInfo GetInfoByName(string objectType, string name, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoByName(objectType, name, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the info by its type and code name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="name">Name of the object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoByName(string, string, int, bool) instead.")]
        public static BaseInfo GetInfoByName(string objectType, string name, int siteId, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoByName(objectType, name, siteId, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the info by its type and full name from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="fullName">Full name of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoByFullName(string, string, bool) instead.")]
        public static BaseInfo GetInfoByFullName(string objectType, string fullName, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoByFullName(objectType, fullName, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the info by its type and GUID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="guid">GUID of the object</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoByGuid(string, Guid, bool) instead.")]
        public static BaseInfo GetInfoByGuid(string objectType, Guid guid, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoByGuid(objectType, guid, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the info by its type and GUID from the appropriate provider.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="guid">GUID of the object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="exceptionIfObjTypeNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetInfoByGuid(string, Guid, int, bool) instead.")]
        public static BaseInfo GetInfoByGuid(string objectType, Guid guid, int siteId, bool exceptionIfObjTypeNotFound = false)
        {
            return ProviderHelper.GetInfoByGuid(objectType, guid, siteId, exceptionIfObjTypeNotFound);
        }


        /// <summary>
        /// Gets the code name by the given ID
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetCodeName(string, int) instead.")]
        public static string GetCodeName(string objectType, int id)
        {
            return ProviderHelper.GetCodeName(objectType, id);
        }


        /// <summary>
        /// Gets the ID by the given code name
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="codeName">Object code name</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.GetId(string, string) instead.")]
        public static int GetId(string objectType, string codeName)
        {
            return ProviderHelper.GetId(objectType, codeName);
        }

    }


    /// <summary>
    /// Base class for the info providers.
    /// </summary>
    [Obsolete("Use AbstractInfoProvider<TInfo, TProvider, TQuery> instead.")]
    public static class AbstractInfoProvider
    {
        /// <summary>
        /// Clears up hashtables of every provider registered by system.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        [Obsolete("Use member CMS.DataEngine.ModuleManager.ClearHashtables(bool)")]
        public static void ClearAllHashtables(bool logTasks)
        {
            ModuleManager.ClearHashtables(logTasks);
        }


        /// <summary>
        /// Clears hashtables for given object type.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        [Obsolete("Use member CMS.DataEngine.ProviderHelper.ClearHashtables(string, bool) instead.")]
        public static void ClearHashtables(string objectType, bool logTasks)
        {
            ProviderHelper.ClearHashtables(objectType, logTasks);
        }
    }

}
