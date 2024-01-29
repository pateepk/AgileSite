using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.EmailEngine
{
    using TypedDataSet = InfoDataSet<SMTPServerInfo>;

    /// <summary>
    /// Class providing SMTPServerInfo management.
    /// </summary>
    public class SMTPServerInfoProvider : AbstractInfoProvider<SMTPServerInfo, SMTPServerInfoProvider>
    {
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
        /// Returns a DataSet with SMTP servers matching the specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <returns>DataSet with SMTP servers</returns>
        [Obsolete("Use method GetSMTPServers() instead")]
        public static TypedDataSet GetSMTPServers(string where, string orderBy)
        {
            return GetSMTPServers(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns a DataSet with SMTP servers matching the specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        /// <returns>DataSet with SMTP servers</returns>
        [Obsolete("Use method GetSMTPServers() instead")]
        public static TypedDataSet GetSMTPServers(string where, string orderBy, int topN, string columns)
        {
            return GetSMTPServers().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns a DataSet with SMTP servers matching the specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>DataSet with SMTP servers</returns>
        [Obsolete("Use method GetSMTPServers() instead")]
        public static TypedDataSet GetSMTPServers(string where, string orderBy, int topN, string columns, int siteId)
        {
            return ProviderObject.GetObjectQuery().Where(where).OnSite(siteId, (siteId == ProviderHelper.ALL_SITES))
                                                  .OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
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
                                                ServerPassword = EncryptionHelper.EncryptData(password),
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
            SMTPServerLookupTable.Instance.Flush();
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

                SMTPServerLookupTable.Instance.Flush();
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

        #endregion
    }
}