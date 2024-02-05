using System;

using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing Newsletter management.
    /// </summary>
    public class NewsletterInfoProvider : AbstractInfoProvider<NewsletterInfo, NewsletterInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsletterInfoProvider()
            : base(NewsletterInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns a query for all the NewsletterInfo objects.
        /// </summary>
        public static ObjectQuery<NewsletterInfo> GetNewsletters()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the Newsletter structure for the specified newsletter.
        /// </summary>
        /// <param name="newsletterId">Newsletter id</param>
        public static NewsletterInfo GetNewsletterInfo(int newsletterId)
        {
            return ProviderObject.GetInfoById(newsletterId);
        }


        /// <summary>
        /// Returns the Newsletter structure for the specified newsletter.
        /// </summary>
        /// <param name="newsletterName">NewsletterName</param>
        /// <param name="siteId">Site identifier</param>
        public static NewsletterInfo GetNewsletterInfo(string newsletterName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(newsletterName, siteId);
        }


        /// <summary>
        /// Returns the Newsletter structure for the specified newsletter.
        /// </summary>
        /// <param name="newsletterGuid">GUID of newsletter</param>
        /// <param name="siteId">Site ID</param>
        public static NewsletterInfo GetNewsletterInfo(Guid newsletterGuid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(newsletterGuid, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified newsletter.
        /// </summary>
        /// <param name="newsletter">Newsletter to set</param>
        public static void SetNewsletterInfo(NewsletterInfo newsletter)
        {
            NewsletterHelper.CheckLicense(newsletter);

            ProviderObject.SetInfo(newsletter);
        }


        /// <summary>
        /// Deletes specified newsletter.
        /// </summary>
        /// <param name="newsletterObj">Newsletter object</param>
        public static void DeleteNewsletterInfo(NewsletterInfo newsletterObj)
        {
            ProviderObject.DeleteInfo(newsletterObj);
        }


        /// <summary>
        /// Deletes specified newsletter.
        /// </summary>
        /// <param name="newsletterId">Newsletter id</param>
        public static void DeleteNewsletterInfo(int newsletterId)
        {
            NewsletterInfo newsletterObj = GetNewsletterInfo(newsletterId);
            DeleteNewsletterInfo(newsletterObj);
        }


        /// <summary>
        /// Returns object query with all newsletters of given site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<NewsletterInfo> GetNewslettersForSite(int siteId)
        {
            return ProviderObject.GetNewslettersForSiteInternal(siteId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(NewsletterInfo info)
        {
            base.SetInfo(info);

            NewsletterHelper.ClearLicNewsletter();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(NewsletterInfo info)
        {
            int taskId = 0;
            if (info != null)
            {
                // Get NewsletterDynamicScheduledTaskID
                taskId = info.NewsletterDynamicScheduledTaskID;
            }

            base.DeleteInfo(info);

            // Delete scheduled task if the newsletter was dynamic
            if (taskId > 0)
            {
                TaskInfoProvider.DeleteTaskInfo(taskId);
            }

            NewsletterHelper.ClearLicNewsletter();
        }


        /// <summary>
        /// Returns object query with all newsletters of given site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<NewsletterInfo> GetNewslettersForSiteInternal(int siteId)
        {
            return GetObjectQuery().WhereEquals("NewsletterSiteID", siteId);
        }

        #endregion
    }
}