using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    using TypedDataSet = InfoDataSet<ServerInfo>;

    /// <summary>
    /// Class providing ServerInfo management.
    /// </summary>
    public class ServerInfoProvider : AbstractInfoProvider<ServerInfo, ServerInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerInfoProvider()
            : base(ServerInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns all servers.
        /// </summary>
        public static ObjectQuery<ServerInfo> GetServers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static ServerInfo GetServerInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Returns the ServerInfo structure for the specified server.
        /// </summary>
        /// <param name="serverId">Server id</param>
        public static ServerInfo GetServerInfo(int serverId)
        {
            return ProviderObject.GetInfoById(serverId);
        }


        /// <summary>
        /// Returns the ServerInfo structure for the specified server.
        /// </summary>
        /// <param name="serverName">Server name</param>
        /// <param name="siteId">Site ID</param>
        public static ServerInfo GetServerInfo(string serverName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(serverName, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified server.
        /// </summary>
        /// <param name="serverObj">Server to set</param>
        public static void SetServerInfo(ServerInfo serverObj)
        {
            ProviderObject.SetInfo(serverObj);
        }


        /// <summary>
        /// Deletes specified server.
        /// </summary>
        /// <param name="serverObj">Server object</param>
        public static void DeleteServerInfo(ServerInfo serverObj)
        {
            ProviderObject.DeleteInfo(serverObj);
        }


        /// <summary>
        /// Deletes specified server.
        /// </summary>
        /// <param name="serverId">Server id</param>
        public static void DeleteServerInfo(int serverId)
        {
            ServerInfo serverObj = GetServerInfo(serverId);
            DeleteServerInfo(serverObj);
        }


        /// <summary>
        /// Indicates if there is at least one enabled staging server.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static bool IsEnabledServer(int siteId)
        {
            var query = GetServers()
                .Column("ServerID")
                .WhereTrue("ServerEnabled");

            if (siteId > 0)
            {
                query = query.WhereEquals("ServerSiteID", siteId);
            }

            return query.HasResults();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Removes all site IDs that don't have any enabled server.
        /// </summary>
        /// <param name="siteIDs">Set of site ids to start with.</param>
        internal static List<int> FilterEnabledServers(List<int> siteIDs)
        {
            return GetServers()
                .Column("ServerSiteID").Distinct()
                .WhereIn("ServerSiteID", siteIDs)
                .WhereTrue("ServerEnabled")
                .Select(s => s.ServerSiteID).ToList();
        }

        #endregion
    }
}