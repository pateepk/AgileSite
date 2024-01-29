using System;
using System.Collections.Generic;

using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding all sites (SiteStates) installed on this instance of Kentico and being used by chat (site has to be requested at least once to be appear in memory).
    /// </summary>
    public class GlobalSites
    {
        #region "Private fields"

        private Dictionary<int, SiteState> siteStates = new Dictionary<int, SiteState>();

        private string uniqueName;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentName">Unique name of a parent class used for caching.</param>
        public GlobalSites(string parentName)
        {
            uniqueName = parentName + "|GlobalSites";
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets SiteState by site ID.
        /// </summary>
        /// <param name="siteID">ID of a site.</param>
        /// <returns>SiteState object</returns>
        public SiteState this[int siteID]
        {
            get
            {
                SiteState siteState;

                if (!siteStates.TryGetValue(siteID, out siteState))
                {
                    lock (siteStates)
                    {
                        // Try getting site one more time
                        if (!siteStates.TryGetValue(siteID, out siteState))
                        {
                            // Check if site exists
                            if (SiteProvider.SiteInfoProvider.GetSiteInfo(siteID) == null)
                            {
                                siteState = null;
                            }
                            else
                            {
                                siteStates[siteID] = siteState = new SiteState(uniqueName, siteID);
                            }


                        }
                    }
                }

                return siteState;
            }
        }


        /// <summary>
        /// Gets current site.
        /// </summary>
        public SiteState Current
        {
            get
            {
                return this[SiteContext.CurrentSiteID];
            }
        }

        #endregion
    }
}