using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebFarmSync
{    
    /// <summary>
    /// Class providing WebFarmServerLogInfo management.
    /// </summary>
    internal class WebFarmServerLogInfoProvider : AbstractInfoProvider<WebFarmServerLogInfo, WebFarmServerLogInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Number of logs kept in DB for each server.
        /// </summary>
        private const int HISTORY_COUNT = 100;


        /// <summary>
        /// Server log cache key.
        /// </summary>
        internal const string CACHE_KEY = "WebFarmServerLogsDataAreCached";


        /// <summary>
        /// Server log cache time in minutes.
        /// </summary>
        private const double CACHE_TIME = 0.5;

        #endregion


        #region "Properties"

        /// <summary>
        /// Raw monitoring data for all web farm servers in the web farm.
        /// </summary>
        internal static Dictionary<WebFarmServerInfo, List<WebFarmServerLogInfo>> Logs
        {
            get
            {
                return CacheHelper.Cache(cs =>
                {
                    var pings = ProviderObject.GetWebFarmServerLogsInternal().ToList();

                    return WebFarmServerInfoProvider.GetWebFarmServers(false).ToList().ToDictionary(
                        server => server,
                        server => pings.Where(p => p.ServerID == server.ServerID).ToList());
                }, new CacheSettings(CACHE_TIME, CACHE_KEY)
                {
                    CacheDependency = GetCacheDependency()
                });
            }
            set
            {
                CacheLogs(value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebFarmServerLogInfoProvider()
            : base(WebFarmServerLogInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Checks for new servers in the given status based on given check.
        /// </summary>
        public static void CheckServerStatusChanges()
        {
            // Prolong the cache expiration - this method would update cache if needed
            CacheLogs(Logs);

            // Check and update server status if needed
            foreach (var server in WebFarmServerInfoProvider.GetWebFarmServers())
            {
                var command = StatusCommandFactory.GetStatusCommand(server);
                if (server.Status != command.Status)
                {
                    SetWebFarmServerLogInfo(server, command);
                }
            }
        }


        /// <summary>
        /// Sets (updates or inserts) specified WebFarmServerLogInfo.
        /// </summary>
        /// <param name="server">Server connected with the new log</param>
        /// <param name="statusCommand">Server status command</param>
        internal static void SetWebFarmServerLogInfo(WebFarmServerInfo server, IStatusCommand statusCommand)
        {
            Logs[server] = ProviderObject.GetWebFarmServerLogsInternal().Where("ServerID", QueryOperator.Equals, server.ServerID).ToList();
            List<WebFarmServerLogInfo> serverLogs;

            // Return if Server has any logs and the last log contains the same code
            if (Logs.TryGetValue(server, out serverLogs) 
                && serverLogs.Any()
                && (serverLogs.First().LogCode == statusCommand.Status))
            {
                return;
            }

            var info = new WebFarmServerLogInfo
            {
                LogCode = statusCommand.Status,
                ServerID = server.ServerID
            };

            ProviderObject.SetInfo(info);

            statusCommand.Execute(server);

            // Prevent unnecessary reloading of data
            serverLogs = (serverLogs ?? new List<WebFarmServerLogInfo>());
            serverLogs.Insert(0, info);
            Logs[server] = serverLogs;

            // Delete old data
            if (serverLogs.Count > HISTORY_COUNT)
            {
                var where = 
                    new WhereCondition()
                        .WhereIn(
                            "WebFarmServerLogID", 
                            serverLogs
                                .Skip(HISTORY_COUNT)
                                .Select(l => l.WebFarmServerLogID)
                                .ToList()
                        );

                ProviderObject.BulkDelete(where);
            }
        }


        /// <summary>
        /// Deletes specified WebFarmServerLogInfo.
        /// </summary>
        /// <param name="infoObj">WebFarmServerLogInfo to be deleted</param>
        internal static void DeleteWebFarmServerLogInfo(WebFarmServerLogInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns a query for all the WebFarmServerLogInfo objects.
        /// </summary>
        protected virtual ObjectQuery<WebFarmServerLogInfo> GetWebFarmServerLogsInternal()
        {
            return GetObjectQuery().OrderByDescending("LogTime");
        }


        /// <summary>
        /// Explicitly sets cached server logs.
        /// </summary>
        /// <param name="logs">Logs to be cached</param>
        private static void CacheLogs(Dictionary<WebFarmServerInfo, List<WebFarmServerLogInfo>> logs)
        {
            CacheHelper.Add(CACHE_KEY, logs, GetCacheDependency(), DateTime.Now.AddMinutes(CACHE_TIME), Cache.NoSlidingExpiration);
        }


        /// <summary>
        /// Gets cache dependency for WebFarmServerLogData cache.
        /// </summary>
        private static CMSCacheDependency GetCacheDependency()
        {
            return CacheHelper.GetCacheDependency($"{WebFarmServerLogInfo.OBJECT_TYPE}|all");
        }

        #endregion
    }
}