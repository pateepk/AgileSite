using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class providing WebFarmServerMonitoringInfo management.
    /// </summary>
    internal class WebFarmServerMonitoringInfoProvider : AbstractInfoProvider<WebFarmServerMonitoringInfo, WebFarmServerMonitoringInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Monitoring cache key
        /// </summary>
        internal const string CACHE_KEY = "WebFarmMonitoringData";

        #endregion


        #region "Properties"

        /// <summary>
        /// Monitoring data for all web farm servers in the web farm.
        /// </summary>
        public static Dictionary<WebFarmServerInfo, List<DateTime>> MonitoringData
        {
            get
            {
                return MonitoringDataInternal.ToDictionary(x => x.Key, x => x.Value.Select(p => p.ServerPing).ToList());
            }
        }


        /// <summary>
        /// Raw monitoring data for all web farm servers in the web farm.
        /// </summary>
        internal static Dictionary<WebFarmServerInfo, List<WebFarmServerMonitoringInfo>> MonitoringDataInternal
        {
            get
            {
                return CacheHelper.Cache(cs =>
                {
                    var pings = ProviderObject.GetWebFarmServerMonitoringDataInternal().ToList();

                    return WebFarmServerInfoProvider.GetWebFarmServers(false).ToList().ToDictionary(
                        server => server,
                        server => pings.Where(p => p.ServerID == server.ServerID).ToList());
                }, new CacheSettings(0.33, CACHE_KEY)
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        $"{WebFarmServerInfo.OBJECT_TYPE}|all",
                        $"{WebFarmServerLogInfo.OBJECT_TYPE}|all"
                    })
                });
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebFarmServerMonitoringInfoProvider()
            : base(WebFarmServerMonitoringInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns pings for given server.
        /// </summary>
        /// <param name="server">Web farm server.</param>
        public static List<DateTime> GetPings(WebFarmServerInfo server)
        {
            return MonitoringData.ContainsKey(server) ? MonitoringData[server] : new List<DateTime>();
        }


        /// <summary>
        /// Saves new "ping" value and updates monitoring data.
        /// </summary>
        internal static void DoPing()
        {
            var server = WebFarmServerInfoProvider.GetWebFarmServerInfo(WebFarmContext.ServerId);
            if (server == null)
            {
                return;
            }

            // Save new ping value
            var ping = new WebFarmServerMonitoringInfo
            {
                ServerID = server.ServerID,
                ServerPing = WebFarmContext.GetDateTime()
            };
            SetWebFarmServerMonitoringInfo(ping);

            if (MonitoringDataInternal.ContainsKey(server))
            {
                // Delete old pings to clear the table from unnecessary data
                ProviderObject.GetDeleteQuery().WhereIn("WebFarmServerMonitoringID", MonitoringDataInternal[server]
                    .Skip(20)
                    .Select(p => p.WebFarmServerMonitoringID)
                    .ToList()).Execute();
            }
        }


        /// <summary>
        /// Clears whole history except for last row.
        /// </summary>
        /// <param name="serverId">Server of which the history should be cleared.</param>
        internal static void ClearOldMonitoringHistory(int serverId)
        {
            if (serverId == 0)
            {
                return;
            }

            var serverHistory = ProviderObject.GetWebFarmServerMonitoringDataInternal().WhereEquals("ServerID", serverId).FirstObject;
            if (serverHistory == null)
            {
                return;
            }

            ProviderObject
                .GetDeleteQuery()
                .WhereEquals("ServerID", serverId)
                .WhereNotEquals("WebFarmServerMonitoringID", serverHistory.WebFarmServerMonitoringID)
                .Execute();
        }

        #endregion


        #region "Basic methods"

        /// <summary>
        /// Sets (updates or inserts) specified WebFarmServerMonitoringInfo.
        /// </summary>
        /// <param name="infoObj">WebFarmServerMonitoringInfo to be set</param>
        internal static void SetWebFarmServerMonitoringInfo(WebFarmServerMonitoringInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified WebFarmServerMonitoringInfo.
        /// </summary>
        /// <param name="infoObj">WebFarmServerMonitoringInfo to be deleted</param>
        internal static void DeleteWebFarmServerMonitoringInfo(WebFarmServerMonitoringInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns a query for all the WebFarmServerMonitoringInfo objects.
        /// </summary>
        protected virtual ObjectQuery<WebFarmServerMonitoringInfo> GetWebFarmServerMonitoringDataInternal()
        {
            return GetObjectQuery().OrderByDescending("ServerPing");
        }

        #endregion
    }
}