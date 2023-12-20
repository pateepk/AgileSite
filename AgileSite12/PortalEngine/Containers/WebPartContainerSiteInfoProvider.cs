using CMS.DataEngine;
using CMS.SiteProvider;
using System.Linq;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing WebPartContainerSiteInfo management.
    /// </summary>
    public class WebPartContainerSiteInfoProvider : AbstractInfoProvider<WebPartContainerSiteInfo, WebPartContainerSiteInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the WebPartContainerSiteInfo structure for the specified webPartContainerSite.
        /// </summary>
        /// <param name="siteId">SiteID</param>
        /// <param name="containerId">ContainerID</param>
        public static WebPartContainerSiteInfo GetWebPartContainerSiteInfo(int siteId, int containerId)
        {
            return ProviderObject.GetWebPartContainerSiteInfoInternal(siteId, containerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified webPartContainerSite.
        /// </summary>
        /// <param name="webPartContainerSite">WebPartContainerSite to set</param>
        public static void SetWebPartContainerSiteInfo(WebPartContainerSiteInfo webPartContainerSite)
        {
            ProviderObject.SetInfo(webPartContainerSite);
        }


        /// <summary>
        /// Deletes specified webPartContainerSite.
        /// </summary>
        /// <param name="infoObj">WebPartContainerSite object</param>
        public static void DeleteWebPartContainerSiteInfo(WebPartContainerSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified webPartContainerSite.
        /// </summary>
        /// <param name="siteId">SiteID</param>
        /// <param name="containerId">ContainerID</param>
        public static void DeleteWebPartContainerSiteInfo(int siteId, int containerId)
        {
            ProviderObject.DeleteWebPartContainerSiteInfoInternal(siteId, containerId);
        }


        /// <summary>
        /// Add specified web part container to the site.
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <param name="siteId">Site ID</param>
        public static void AddContainerToSite(int containerId, int siteId)
        {
            ProviderObject.AddContainerToSiteInternal(containerId, siteId);
        }


        /// <summary>
        /// Add specified web part container to the site.
        /// </summary>
        /// <param name="ci">Web part container info object</param>
        /// <param name="si">Site info object</param>
        public static void AddContainerToSite(WebPartContainerInfo ci, SiteInfo si)
        {
            ProviderObject.AddContainerToSiteInternal(ci, si);
        }


        /// <summary>
        /// Remove specified web part container from site.
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <param name="siteId">Site ID</param>
        public static void RemoveContainerFromSite(int containerId, int siteId)
        {
            ProviderObject.RemoveContainerFromSiteInternal(containerId, siteId);
        }


        /// <summary>
        /// Returns all web part container -- site bindings.
        /// </summary>
        public static ObjectQuery<WebPartContainerSiteInfo> GetWebPartContainerSiteInfos()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WebPartContainerSiteInfo structure for the specified webPartContainerSite.
        /// </summary>
        /// <param name="siteId">SiteID</param>
        /// <param name="containerId">ContainerID</param>
        public virtual WebPartContainerSiteInfo GetWebPartContainerSiteInfoInternal(int siteId, int containerId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("SiteID", siteId)
                .WhereEquals("ContainerID", containerId).FirstOrDefault();
        }

        
        /// <summary>
        /// Deletes specified webPartContainerSite.
        /// </summary>
        /// <param name="siteId">SiteID</param>
        /// <param name="containerId">ContainerID</param>
        public virtual void DeleteWebPartContainerSiteInfoInternal(int siteId, int containerId)
        {
            WebPartContainerSiteInfo infoObj = GetWebPartContainerSiteInfo(siteId, containerId);
            DeleteInfo(infoObj);
        }


        /// <summary>
        /// Add specified web part container to the site.
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <param name="siteId">Site ID</param>
        public virtual void AddContainerToSiteInternal(int containerId, int siteId)
        {
            AddContainerToSiteInternal(WebPartContainerInfoProvider.GetWebPartContainerInfo(containerId), SiteInfoProvider.GetSiteInfo(siteId));
        }


        /// <summary>
        /// Add specified web part container to the site.
        /// </summary>
        /// <param name="ci">Web part container info object</param>
        /// <param name="si">Site info object</param>
        public virtual void AddContainerToSiteInternal(WebPartContainerInfo ci, SiteInfo si)
        {
            // Check whether the container and site object exist
            if ((ci != null) && (si != null))
            {
                // Check whether the Layout is allready added to the site
                if (GetWebPartContainerSiteInfo(si.SiteID, ci.ContainerID) == null)
                {
                    // Create webpart container info object
                    WebPartContainerSiteInfo wpci = new WebPartContainerSiteInfo();
                    wpci.ContainerID = ci.ContainerID;
                    wpci.SiteID = si.SiteID;

                    SetWebPartContainerSiteInfo(wpci);
                }
            }
        }


        /// <summary>
        /// Remove specified web part container from site.
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <param name="siteId">Site ID</param>
        public virtual void RemoveContainerFromSiteInternal(int containerId, int siteId)
        {
            WebPartContainerSiteInfo wpcsi = GetWebPartContainerSiteInfo(siteId, containerId);

            if (wpcsi != null)
            {
                DeleteInfo(wpcsi);
            }
        }

        #endregion
    }
}