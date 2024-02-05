using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface describing common info object provider.
    /// </summary>
    public interface IInfoProvider : ICustomizableProvider, IBulkOperationProvider, IWebFarmProvider
    {
        /// <summary>
        /// Gets the info by given id.
        /// </summary>
        /// <param name="id">ID of the object</param>
        BaseInfo GetInfoById(int id);


        /// <summary>
        /// Gets the info by given GUID.
        /// </summary>
        /// <param name="guid">GUID of the object</param>
        BaseInfo GetInfoByGuid(Guid guid);


        /// <summary>
        /// Gets the info by given combination of GUID and site id.
        /// </summary>
        /// <param name="guid">GUID of the object</param>
        /// <param name="siteId">Site ID</param>
        BaseInfo GetInfoByGuid(Guid guid, int siteId);


        /// <summary>
        /// Gets the info by given code name.
        /// </summary>
        /// <param name="name">Name of the object</param>
        BaseInfo GetInfoByName(string name);


        /// <summary>
        /// Gets the info by given combination of code name and site id.
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="siteId">Site ID</param>
        BaseInfo GetInfoByName(string name, int siteId);


        /// <summary>
        /// Gets the info by given full name.
        /// </summary>
        /// <param name="fullName">Full name of the object</param>
        BaseInfo GetInfoByFullName(string fullName);


        /// <summary>
        /// Gets the infos by ids collection.
        /// </summary>
        /// <param name="ids">IDs of the objects</param>
        SafeDictionary<int, BaseInfo> GetInfosByIds(IEnumerable<int> ids);
    }
}