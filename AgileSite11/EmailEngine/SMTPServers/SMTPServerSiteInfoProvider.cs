using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.EmailEngine
{
    using TypedDataSet = InfoDataSet<SMTPServerSiteInfo>;

    /// <summary>
    /// Class providing SMTPServerSiteInfo management.
    /// </summary>
    public class SMTPServerSiteInfoProvider : AbstractInfoProvider<SMTPServerSiteInfo, SMTPServerSiteInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates a new instance of SMTPServerSiteInfoProvider.
        /// </summary>        
        public SMTPServerSiteInfoProvider()
            : base(SMTPServerSiteInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns relationship between specified server and site.
        /// </summary>
        /// <param name="smtpServerId">SMTP Server ID</param>
        /// <param name="siteId">Site ID</param>
        public static SMTPServerSiteInfo GetSMTPServerSiteInfo(int smtpServerId, int siteId)
        {
            return ProviderObject.GetSMTPServerSiteInfoInternal(smtpServerId, siteId);
        }


        /// <summary>
        /// Returns relationships between servers and sites.
        /// </summary>
        public static ObjectQuery<SMTPServerSiteInfo> GetSMTPServerSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all relationships between servers and sites matching the specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        [Obsolete("Use method GetSMTPServerSites() instead")]
        public static TypedDataSet GetSMTPServerSites(string where, string orderBy)
        {
            return GetSMTPServerSites(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns dataset of all relationships between servers and sites matching the specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        [Obsolete("Use method GetSMTPServerSites() instead")]
        public static TypedDataSet GetSMTPServerSites(string where, string orderBy, int topN, string columns)
        {
            return GetSMTPServerSites().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).TypedResult;
        }


        /// <summary>
        /// Sets relationship between specified server and site.
        /// </summary>
        /// <param name="smtpServerSite">Server-site relationship to be set</param>
        public static void SetSMTPServerSiteInfo(SMTPServerSiteInfo smtpServerSite)
        {
            ProviderObject.SetInfo(smtpServerSite);
        }


        /// <summary>
        /// Sets relationship between specified server and site.
        /// </summary>	
        /// <param name="smtpServerId">SMTP Server ID</param>
        /// <param name="siteId">Site ID</param>
        public static void AddSMTPServerToSite(int smtpServerId, int siteId)
        {
            SMTPServerSiteInfo infoObj = ProviderObject.CreateInfo();

            infoObj.ServerID = smtpServerId;
            infoObj.SiteID = siteId;

            SetSMTPServerSiteInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified server and site.
        /// </summary>
        /// <param name="smtpServerSite">Server-site relationship to be deleted</param>
        public static void DeleteSMTPServerSiteInfo(SMTPServerSiteInfo smtpServerSite)
        {
            ProviderObject.DeleteInfo(smtpServerSite);
        }


        /// <summary>
        /// Deletes relationship between specified SMTP server and all sites.
        /// </summary>
        /// <param name="smtpServer">SMTP server</param>
        public static void RemoveSMTPServerFromSites(SMTPServerInfo smtpServer)
        {
            SMTPServerLookupTable.Instance.Flush();

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ServerID", smtpServer.ServerID);

            ConnectionHelper.ExecuteQuery("cms.smtpserversite.deletebindings", parameters);
        }


        /// <summary>
        /// Deletes relationship between specified SMTP server and specified site.
        /// </summary>
        /// <param name="serverId">SMTP Server ID</param>
        /// <param name="siteId">Site ID</param>
        public static void RemoveSMTPServerFromSite(int serverId, int siteId)
        {
            SMTPServerSiteInfo infoObj = GetSMTPServerSiteInfo(serverId, siteId);
            DeleteSMTPServerSiteInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns relationship between specified server and site.
        /// </summary>
        /// <param name="smtpServerId">SMTP Server ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual SMTPServerSiteInfo GetSMTPServerSiteInfoInternal(int smtpServerId, int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals("ServerID", smtpServerId)
                .WhereEquals("SiteID", siteId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SMTPServerSiteInfo info)
        {
            base.SetInfo(info);
            SMTPServerLookupTable.Instance.Flush();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SMTPServerSiteInfo info)
        {
            base.DeleteInfo(info);
            SMTPServerLookupTable.Instance.Flush();
        }

        #endregion
    }
}