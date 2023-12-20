using System.Linq;

using CMS.DataEngine;

namespace CMS.EmailEngine
{
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
            SMTPServerInfoProvider.FlushSMTPServerLookupTable();

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
            SMTPServerInfoProvider.FlushSMTPServerLookupTable();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SMTPServerSiteInfo info)
        {
            base.DeleteInfo(info);
            SMTPServerInfoProvider.FlushSMTPServerLookupTable();
        }

        #endregion
    }
}