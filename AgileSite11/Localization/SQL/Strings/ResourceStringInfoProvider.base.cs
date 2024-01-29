using System;

using CMS.DataEngine;

namespace CMS.Localization
{
    /// <summary>
    /// Class providing ResourceStringInfo management.
    /// </summary>
    public class ResourceStringInfoProviderBase<TProvider> : AbstractInfoProvider<ResourceStringInfo, TProvider>
        where TProvider : ResourceStringInfoProviderBase<TProvider>, new()
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ResourceStringInfo objects.
        /// </summary>
        public static ObjectQuery<ResourceStringInfo> GetResourceStrings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ResourceStringInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceStringInfo ID</param>
        public static ResourceStringInfo GetResourceStringInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns ResourceStringInfo with specified GUID.
        /// </summary>
        /// <param name="guid">ResourceStringInfo GUID</param>
        public static ResourceStringInfo GetResourceStringInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns ResourceStringInfo with specified name.
        /// </summary>
        /// <param name="name">ResourceStringInfo name</param>
        public static ResourceStringInfo GetResourceStringInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ResourceStringInfo.
        /// </summary>
        /// <param name="infoObj">ResourceStringInfo to be set</param>
        public static void SetResourceStringInfo(ResourceStringInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ResourceStringInfo.
        /// </summary>
        /// <param name="infoObj">ResourceStringInfo to be deleted</param>
        public static void DeleteResourceStringInfo(ResourceStringInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ResourceStringInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceStringInfo ID</param>
        public static void DeleteResourceStringInfo(int id)
        {
            ResourceStringInfo infoObj = GetResourceStringInfo(id);
            DeleteResourceStringInfo(infoObj);
        }

        #endregion
    }
}