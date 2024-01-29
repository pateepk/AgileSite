using System.Linq;

using CMS.DataEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing CssStylesheetSiteInfo management.
    /// </summary>
    public class CssStylesheetSiteInfoProvider : AbstractInfoProvider<CssStylesheetSiteInfo, CssStylesheetSiteInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the CssStylesheetSiteInfo structure for the specified cssStylesheetSite.
        /// </summary>
        /// <param name="stylesheetId">StylesheetID</param>
        /// <param name="siteId">SiteID</param>
        public static CssStylesheetSiteInfo GetCssStylesheetSiteInfo(int stylesheetId, int siteId)
        {
            return ProviderObject.GetCssStylesheetSiteInfoInternal(stylesheetId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified cssStylesheetSite.
        /// </summary>
        /// <param name="cssStylesheetSite">CssStylesheetSite to set</param>
        public static void SetCssStylesheetSiteInfo(CssStylesheetSiteInfo cssStylesheetSite)
        {
            ProviderObject.SetInfo(cssStylesheetSite);
        }


        /// <summary>
        /// Deletes specified cssStylesheetSite.
        /// </summary>
        /// <param name="infoObj">CssStylesheetSite object</param>
        public static void DeleteCssStylesheetSiteInfo(CssStylesheetSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Removes CSS stylesheet from site.
        /// </summary>
        /// <param name="stylesheetId">StylesheetID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveCssStylesheetFromSite(int stylesheetId, int siteId)
        {
            CssStylesheetSiteInfo infoObj = GetCssStylesheetSiteInfo(stylesheetId, siteId);
            DeleteCssStylesheetSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds the class to the specified site.
        /// </summary>
        /// <param name="stylesheetId">StylesheetID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddCssStylesheetToSite(int stylesheetId, int siteId)
        {
            // Create new binding
            CssStylesheetSiteInfo infoObj = new CssStylesheetSiteInfo();
            infoObj.StylesheetID = stylesheetId;
            infoObj.SiteID = siteId;

            // Save to the database
            SetCssStylesheetSiteInfo(infoObj);
        }


        /// <summary>
        /// Returns all CSS stylesheet bindings to site.
        /// </summary>
        public static ObjectQuery<CssStylesheetSiteInfo> GetCssStylesheetSites()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the CssStylesheetSiteInfo structure for the specified cssStylesheetSite.
        /// </summary>
        /// <param name="stylesheetId">StylesheetID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual CssStylesheetSiteInfo GetCssStylesheetSiteInfoInternal(int stylesheetId, int siteId)
        {
            // Get the data
            return GetObjectQuery().TopN(1)
                .WhereEquals("StylesheetID", stylesheetId)
                .WhereEquals("SiteID", siteId).FirstOrDefault();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(CssStylesheetSiteInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }

        #endregion
    }
}