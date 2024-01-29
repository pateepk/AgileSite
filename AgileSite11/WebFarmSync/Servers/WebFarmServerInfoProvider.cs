using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class providing WebFarmServerInfo management.
    /// </summary>
    public class WebFarmServerInfoProvider : AbstractInfoProvider<WebFarmServerInfo, WebFarmServerInfoProvider>
    {
        #region "Variables"

        private static readonly object mLock = new object();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WebFarmServerInfoProvider()
            : base(WebFarmServerInfo.TYPEINFO, new HashtableSettings
                {
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets task to server.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="serverName">Server name</param>
        public static void SetServerTasks(string serverName, int taskId)
        {
            ProviderObject.SetServerTasksInternal(serverName, taskId);
        }


        /// <summary>
        /// Returns the query for all web farm servers.
        /// </summary>
        public static ObjectQuery<WebFarmServerInfo> GetWebFarmServers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the query for all web farm servers.
        /// </summary>
        /// <param name="checkLicense">Whether to check license.</param>
        internal static ObjectQuery<WebFarmServerInfo> GetWebFarmServers(bool checkLicense)
        {
            return ProviderObject.GetObjectQuery(checkLicense);
        }


        /// <summary>
        /// Returns all the enabled servers records.
        /// </summary>
        public static ObjectQuery<WebFarmServerInfo> GetAllEnabledServers()
        {
            return ProviderObject.GetAllEnabledServersInternal();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WebFarmServerInfo GetWebFarmServerInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the WebFarmServerInfo structure for the specified webFarmServer.
        /// </summary>
        /// <param name="webFarmServerId">WebFarmServer id</param>
        public static WebFarmServerInfo GetWebFarmServerInfo(int webFarmServerId)
        {
            return ProviderObject.GetInfoById(webFarmServerId);
        }


        /// <summary>
        /// Returns the WebFarmServerInfo structure for the specified webFarmServer.
        /// </summary>
        /// <param name="serverName">ServerName</param>
        public static WebFarmServerInfo GetWebFarmServerInfo(string serverName)
        {
            return ProviderObject.GetInfoByCodeName(serverName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified webFarmServer.
        /// </summary>
        /// <param name="infoObj">WebFarmServer to set</param>
        public static void SetWebFarmServerInfo(WebFarmServerInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified webFarmServer.
        /// </summary>
        /// <param name="infoObj">WebFarmServer object</param>
        public static void DeleteWebFarmServerInfo(WebFarmServerInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified webFarmServer.
        /// </summary>
        /// <param name="webFarmServerId">WebFarmServer id</param>
        public static void DeleteWebFarmServerInfo(int webFarmServerId)
        {
            WebFarmServerInfo infoObj = GetWebFarmServerInfo(webFarmServerId);
            if (infoObj != null)
            {
                DeleteWebFarmServerInfo(infoObj);
            }
        }


        /// <summary>
        /// Gets the automatic server name for the web farm
        /// </summary>
        public static string GetAutomaticServerName()
        {
            return ProviderObject.GetAutomaticServerNameInternal();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns all the enabled servers records.
        /// </summary>
        private ObjectQuery<WebFarmServerInfo> GetAllEnabledServersInternal()
        {
            return GetObjectQuery(false).WhereEquals("ServerEnabled", 1);
        }


        /// <summary>
        /// Gets the automatic server name for the web farm
        /// </summary>
        protected virtual string GetAutomaticServerNameInternal()
        {
            return SystemContext.GenerateUniqueInstanceName();
        }


        /// <summary>
        /// Sets task to server.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="serverName">Server name</param>
        protected void SetServerTasksInternal(string serverName, int taskId)
        {
            WebFarmServerInfo wfsi = GetWebFarmServerInfo(serverName);
            if (wfsi != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ServerID", wfsi.ServerID);
                parameters.Add("@TaskID", taskId);

                // Set the tasks
                ConnectionHelper.ExecuteQuery("cms.WebFarmServer.SetServerTasks", parameters);
            }
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WebFarmServerInfo info)
        {
            bool exist = info.ServerID != 0;

            base.SetInfo(info);

            if (!exist)
            {
                // Save first ping
                var monInfo = new WebFarmServerMonitoringInfo
                {
                    ServerID = info.ServerID,
                    ServerPing = WebFarmContext.GetDateTime()
                };
                WebFarmServerMonitoringInfoProvider.SetWebFarmServerMonitoringInfo(monInfo);
            }

            WebFarmContext.Clear();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WebFarmServerInfo info)
        {
            try
            {
                base.DeleteInfo(info);
            }
            catch (Exception ex)
            {
                // Log exception
                CoreServices.EventLog.LogException("WebFarmServerProvider", "DeleteWebFarmServerInfo", ex);
            }

            // Delete old orphaned tasks
            WebFarmTaskInfoProvider.DeleteOrphanedTasks();

            // Clear servers
            WebFarmContext.Clear();
        }


        /// <summary>
        /// Sets up the current web farm server
        /// </summary>
        internal static void EnsureAutomaticServer()
        {
            lock (mLock)
            {
                if (WebFarmContext.InstanceIsHiddenWebFarmServer ||
                    (WebFarmContext.ServerId != 0) ||
                    (WebFarmContext.WebFarmMode != WebFarmModeEnum.Automatic))
                {
                    return;
                }

                // Create web farm server
                SetWebFarmServerInfo(new WebFarmServerInfo
                {
                    ServerEnabled = true,
                    ServerName = WebFarmContext.ServerName,
                    ServerDisplayName = WebFarmContext.ServerName,
                    IsExternalWebAppServer = false
                });
            }
        }


        /// <summary>
        /// Deletes the dynamic web farm server
        /// </summary>
        internal static void DeleteDynamicServer()
        {
            if (WebFarmContext.InstanceIsHiddenWebFarmServer || (WebFarmContext.WebFarmMode != WebFarmModeEnum.Automatic))
            {
                return;
            }

            DeleteWebFarmServerInfo(WebFarmContext.ServerId);
        }

        #endregion
    }
}