using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing PageTemplateSiteInfo management.
    /// </summary>
    public class PageTemplateSiteInfoProvider : AbstractInfoProvider<PageTemplateSiteInfo, PageTemplateSiteInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the PageTemplateSiteInfo structure for the specified pageTemplateSite.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public static PageTemplateSiteInfo GetPageTemplateSiteInfo(int pageTemplateId, int siteId)
        {
            return ProviderObject.GetPageTemplateSiteInfoInternal(pageTemplateId, siteId);
        }


        /// <summary>
        /// Return all page template -- site binding.
        /// </summary>
        public static ObjectQuery<PageTemplateSiteInfo> GetPageTemplateSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified pageTemplateSite.
        /// </summary>
        /// <param name="pageTemplateSite">PageTemplateSite to set</param>
        public static void SetPageTemplateSiteInfo(PageTemplateSiteInfo pageTemplateSite)
        {
            ProviderObject.SetInfo(pageTemplateSite);
        }


        /// <summary>
        /// Deletes specified pageTemplateSite.
        /// </summary>
        /// <param name="infoObj">PageTemplateSite object</param>
        public static void DeletePageTemplateSiteInfo(PageTemplateSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified pageTemplateSite.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemovePageTemplateFromSite(int pageTemplateId, int siteId)
        {
            ProviderObject.RemovePageTemplateFromSiteInternal(pageTemplateId, siteId);
        }


        /// <summary>
        /// Adds the class to the specified site.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddPageTemplateToSite(int pageTemplateId, int siteId)
        {
            ProviderObject.AddPageTemplateToSiteInternal(pageTemplateId, siteId);
        }


        /// <summary>
        /// Check if exists relation between page template with pagetemplateId and site with siteId.
        /// </summary>
        /// <param name="pageTemplateId">Page template ID</param>
        /// <param name="siteId">Site ID</param>
        public static bool IsPageTemplateOnSite(int pageTemplateId, int siteId)
        {
            PageTemplateSiteInfo infoObj = GetPageTemplateSiteInfo(pageTemplateId, siteId);
            return (infoObj != null);
        }

        #endregion


        #region "Internal methods"
        
        /// <summary>
        /// Returns the PageTemplateSiteInfo structure for the specified pageTemplateSite.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public virtual PageTemplateSiteInfo GetPageTemplateSiteInfoInternal(int pageTemplateId, int siteId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("SiteID", siteId)
                .WhereEquals("PageTemplateID", pageTemplateId).FirstOrDefault();
        }


        /// <summary>
        /// Deletes specified pageTemplateSite.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public virtual void RemovePageTemplateFromSiteInternal(int pageTemplateId, int siteId)
        {
            PageTemplateSiteInfo infoObj = GetPageTemplateSiteInfo(pageTemplateId, siteId);
            if (infoObj != null)
            {
                DeleteInfo(infoObj);
            }
        }


        /// <summary>
        /// Adds the class to the specified site.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplateID</param>
        /// <param name="siteId">SiteID</param>
        public virtual void AddPageTemplateToSiteInternal(int pageTemplateId, int siteId)
        {
            // Create new binding
            PageTemplateSiteInfo infoObj = new PageTemplateSiteInfo();
            infoObj.PageTemplateID = pageTemplateId;
            infoObj.SiteID = siteId;

            // Save to the database
            SetPageTemplateSiteInfo(infoObj);
        }

        #endregion
    }
}