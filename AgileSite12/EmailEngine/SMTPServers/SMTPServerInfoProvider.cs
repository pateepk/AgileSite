using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Class providing SMTPServerInfo management.
    /// </summary>
    public class SMTPServerInfoProvider : AbstractInfoProvider<SMTPServerInfo, SMTPServerInfoProvider>
    {
        #region "Constants"

        private const string FLUSH_SMTP_SERVER_LOOKUP_TABLE_ACTION_NAME = "FlushSMTPServerLookupTable";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new instance of SMTPServerInfoProvider.
        /// </summary>
        public SMTPServerInfoProvider()
            : base(SMTPServerInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns all SMTP servers.
        /// </summary>
        public static ObjectQuery<SMTPServerInfo> GetSMTPServers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns SMTP server with the specified ID.
        /// </summary>
        /// <param name="smtpServerId">SMTP server ID</param>
        public static SMTPServerInfo GetSMTPServerInfo(int smtpServerId)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MultipleSMTPServers);
            }
            return ProviderObject.GetInfoById(smtpServerId);
        }


        /// <summary>
        /// Returns a SMTP server with the specified GUID.
        /// </summary>
        /// <param name="smtpServerGuid">SMTP Server GUID</param>
        /// <returns>SMTP server with the specified GUID or null</returns>
        public static SMTPServerInfo GetSMTPServerInfo(Guid smtpServerGuid)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MultipleSMTPServers);
            }
            return ProviderObject.GetInfoByGuid(smtpServerGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SMTP server.
        /// </summary>
        /// <param name="smtpServer">SMTP server to be set</param>
        public static void SetSMTPServerInfo(SMTPServerInfo smtpServer)
        {
            ProviderObject.SetInfo(smtpServer);
        }


        /// <summary>
        /// Deletes specified SMTP server.
        /// </summary>
        /// <param name="smtpServer">SMTP server to be deleted</param>
        public static void DeleteSMTPServerInfo(SMTPServerInfo smtpServer)
        {
            ProviderObject.DeleteInfo(smtpServer);
        }


        /// <summary>
        /// Deletes a SMTP server with the specified ID.
        /// </summary>
        /// <param name="smtpServerId">SMTP server ID</param>
        public static void DeleteSMTPServerInfo(int smtpServerId)
        {
            SMTPServerInfo smtpserverObj = GetSMTPServerInfo(smtpServerId);
            DeleteSMTPServerInfo(smtpserverObj);
        }


        /// <summary>
        /// Enables the specified SMTP server.
        /// </summary>
        /// <param name="smtpServerId">SMTP server ID</param>
        public static void EnableSMTPServer(int smtpServerId)
        {
            SMTPServerInfo smtpServer = GetSMTPServerInfo(smtpServerId);
            if (smtpServer == null)
            {
                return;
            }

            smtpServer.ServerEnabled = true;
            SetSMTPServerInfo(smtpServer);
        }


        /// <summary>
        /// Disables the specified SMTP server.
        /// </summary>
        /// <param name="smtpServerId">SMTP server ID</param>
        public static void DisableSMTPServer(int smtpServerId)
        {
            SMTPServerInfo smtpServer = GetSMTPServerInfo(smtpServerId);
            if (smtpServer == null)
            {
                return;
            }

            smtpServer.ServerEnabled = false;
            SetSMTPServerInfo(smtpServer);
        }


        /// <summary>
        /// Makes the SMTP server available to all sites.
        /// </summary>
        /// <param name="smtpServer">SMTP server</param>
        public static void PromoteSMTPServer(SMTPServerInfo smtpServer)
        {
            if (smtpServer != null)
            {
                SMTPServerSiteInfoProvider.RemoveSMTPServerFromSites(smtpServer);
                smtpServer.ServerIsGlobal = true;
                SetSMTPServerInfo(smtpServer);
            }
        }


        /// <summary>
        /// Makes the SMTP server available to specific sites only.
        /// </summary>
        /// <param name="smtpServer">The SMTP server</param>
        public static void DemoteSMTPServer(SMTPServerInfo smtpServer)
        {
            if (smtpServer != null)
            {
                smtpServer.ServerIsGlobal = false;
                SetSMTPServerInfo(smtpServer);
            }
        }


        /// <summary>
        /// Creates a new SMTP server.
        /// </summary>
        /// <param name="serverName">Name of the server (IP or DNS)</param>
        /// <param name="userName">Username</param>
        /// <param name="password">Password</param>
        /// <param name="useSSL">If true, SSL should be used</param>
        /// <param name="priority">Priority</param>
        public static SMTPServerInfo CreateSMTPServer(string serverName, string userName, string password, bool useSSL, SMTPServerPriorityEnum priority)
        {
            SMTPServerInfo smtpServer = new SMTPServerInfo()
                                            {
                                                ServerName = serverName,
                                                ServerUserName = userName,
#pragma warning disable 618
                                                ServerPassword = EncryptionHelper.EncryptData(password),
#pragma warning restore 618
                                                ServerUseSSL = useSSL,
                                                ServerIsGlobal = false,
                                                ServerEnabled = true,
                                                ServerPriority = priority
                                            };

            SetSMTPServerInfo(smtpServer);

            return smtpServer;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SMTPServerInfo info)
        {
            base.SetInfo(info);
            FlushSMTPServerLookupTable();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SMTPServerInfo info)
        {
            if (info != null)
            {
                SMTPServerSiteInfoProvider.RemoveSMTPServerFromSites(info);
                base.DeleteInfo(info);

                FlushSMTPServerLookupTable();
            }
        }


        /// <summary>
        /// Gets the IDSs of all available enabled global SMTP servers.
        /// </summary>
        /// <returns>Iterator with global SMTP server IDs</returns>
        internal static IEnumerable<int> GetGlobalSMTPServerIDs()
        {
            var serverIds = GetSMTPServers().WhereEquals("ServerEnabled", 1).WhereEquals("ServerIsGlobal", 1).OrderBy("ServerID").Column("ServerID");
            return serverIds.GetListResult<int>();
        }


        /// <summary>
        /// Gets the IDs of all available enabled SMTP servers that are site-specific for a given site ID.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>Iterator with site-specific SMTP server IDs</returns>
        internal static IEnumerable<int> GetSiteSMTPServerIDs(int siteId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);

            DataSet serverIDs = ConnectionHelper.ExecuteQuery("cms.smtpserver.getactiveserverids", parameters);

            return serverIDs.Tables[0].AsEnumerable().Select(row => row[0]).Cast<int>();
        }


        /// <summary>
        /// Flushes the <see cref="SMTPServerLookupTable"/> singleton instance
        /// and logs the corresponding web farm task.
        /// </summary>
        internal static void FlushSMTPServerLookupTable()
        {
            SMTPServerLookupTable.Instance.Flush();

            ProviderObject.CreateWebFarmTask(FLUSH_SMTP_SERVER_LOOKUP_TABLE_ACTION_NAME, null);
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider.
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        protected override void ProcessWebFarmTaskInternal(string actionName, string data, byte[] binary)
        {
            if (String.Equals(actionName, FLUSH_SMTP_SERVER_LOOKUP_TABLE_ACTION_NAME, StringComparison.OrdinalIgnoreCase))
            {
                SMTPServerLookupTable.Instance.Flush();
            }
            else
            {
                base.ProcessWebFarmTaskInternal(actionName, data, binary);
            }
        }

        #endregion
    }
}