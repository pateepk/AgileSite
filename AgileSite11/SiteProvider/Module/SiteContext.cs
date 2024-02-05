using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site related context methods and variables.
    /// </summary>
    public class SiteContext : AbstractContext<SiteContext>
    {
        #region "Variables"

        private string mCurrentSiteName;
        private int mCurrentSiteID = -1;
        private SiteInfo mCurrentSite;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current category info object according the URL parameter of the current request. 
        /// It is available when the request contains parameters "category" or "categoryname" with valid value of the category.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public static BaseInfo CurrentCategory
        {
            get
            {
                return (BaseInfo)ModuleManager.GetContextProperty("TaxonomyContext", "CurrentCategory");
            }
        }


        /// <summary>
        /// Returns current site name.
        /// </summary>
        [RegisterColumn]
        public static string CurrentSiteName
        {
            get
            {
                // Try to get from virtual context first
                string virtualSiteName = (string)VirtualContext.GetItem(VirtualContext.PARAM_SITENAME);
                if (virtualSiteName != null)
                {
                    return virtualSiteName;
                }

                // Get from the request
                var c = Current;
                if ((c.mCurrentSiteName == null) && DatabaseHelper.IsDatabaseAvailable)
                {
                    // Get the data
                    DebugHelper.SetContext("CurrentSiteName");

                    // Get the current site object
                    SiteInfo si = GetCurrentSiteByDomain();

                    DebugHelper.ReleaseContext();

                    // Get the site name
                    c.mCurrentSiteName = (si == null) ? "" : si.SiteName;
                }

                return c.mCurrentSiteName ?? "";
            }
            set
            {
                Current.mCurrentSiteName = value;

                // CurrentSiteID and CurrentSite will be derived from SiteName when accessed
                Current.mCurrentSiteID = -1;
                Current.mCurrentSite = null;
            }
        }


        /// <summary>
        /// Returns current site ID.
        /// </summary>
        [RegisterColumn]
        public static int CurrentSiteID
        {
            get
            {
                // Get from the request
                var c = Current;
                if (c.mCurrentSiteID < 0)
                {
                    // Get the data
                    DebugHelper.SetContext("CurrentSiteID");

                    // Get the current site object
                    SiteInfo si = GetCurrentSite();

                    DebugHelper.ReleaseContext();

                    c.mCurrentSiteID = (si == null) ? 0 : si.SiteID;
                }

                return c.mCurrentSiteID;
            }
            set
            {
                Current.mCurrentSiteID = value;

                var site = SiteInfoProvider.GetSiteInfo(value);

                // Propagate values to other properties
                if (site != null)
                {
                    Current.mCurrentSite = site;
                    Current.mCurrentSiteName = site.SiteName;
                }
                else
                {
                    Current.mCurrentSite = null;
                    Current.mCurrentSiteName = null;
                }
            }
        }


        /// <summary>
        /// Returns the current site info.
        /// </summary>
        [RegisterProperty]
        public static SiteInfo CurrentSite
        {
            get
            {
                // Load if not available
                return EnsureValue(ref Current.mCurrentSite, GetCurrentSite);
            }
            set
            {
                Current.mCurrentSite = value;

                // Propagate values to other properties
                if (value != null)
                {
                    Current.mCurrentSiteID = value.SiteID;
                    Current.mCurrentSiteName = value.SiteName;
                }
                else
                {
                    Current.mCurrentSiteID = -1;
                    Current.mCurrentSiteName = null;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the current site.
        /// </summary>
        private static SiteInfo GetCurrentSite()
        {
            return SiteInfoProvider.GetSiteInfo(CurrentSiteName);
        }


        /// <summary>
        /// Gets the current site object based on current context
        /// </summary>
        private static SiteInfo GetCurrentSiteByDomain()
        {
            string appPath = SystemContext.ApplicationPath;
            SiteInfo si = SiteInfoProvider.GetRunningSiteInfo(RequestContext.FullDomain, appPath) ?? SiteInfoProvider.GetRunningSiteInfo(RequestContext.CurrentDomain, appPath);

            return si;
        }

        #endregion
    }
}