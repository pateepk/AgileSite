using System;

using CMS.SiteProvider;
using CMS.Localization;
using CMS.DocumentEngine;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds site-related API examples.
    /// </summary>
    /// <pageTitle>Sites</pageTitle>
    internal class SitesMain
    {
        /// <summary>
        /// Holds site API examples.
        /// </summary>
        /// <groupHeading>Sites</groupHeading>
        private class Sites
        {
            /// <heading>Creating a new site</heading>
            private void CreateSite()
            {
                // Creates a new site object
                SiteInfo newSite = new SiteInfo();

                // Sets the site properties
                newSite.DisplayName = "New site";
                newSite.SiteName = "NewSite";
                newSite.Status = SiteStatusEnum.Stopped;
                newSite.DomainName = "127.0.0.1";

                // Saves the site to the database
                SiteInfoProvider.SetSiteInfo(newSite);
            }


            /// <heading>Updating a site</heading>
            private void GetAndUpdateSite()
            {
                // Gets the site
                SiteInfo updateSite = SiteInfoProvider.GetSiteInfo("NewSite");
                if (updateSite != null)
                {
                    // Updates the site properties
                    updateSite.DisplayName = updateSite.DisplayName.ToLower();

                    // Saves the modified site to the database
                    SiteInfoProvider.SetSiteInfo(updateSite);
                }
            }


            /// <heading>Updating multiple sites</heading>
            private void GetAndBulkUpdateSites()
            {
                // Gets all sites whose code name starts with 'New'
                var sites = SiteInfoProvider.GetSites().WhereStartsWith("SiteName", "New");
                
                // Loops through individual sites
                foreach (SiteInfo site in sites)
                {
                    // Updates the site properties
                    site.DisplayName = site.DisplayName.ToUpper();

                    // Saves the modified site to the database
                    SiteInfoProvider.SetSiteInfo(site);
                }
            }


            /// <heading>Assigning a culture to a site</heading>
            private void AddCultureToSite()
            {
                // Gets the site and culture objects
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");
                CultureInfo culture = CultureInfoProvider.GetCultureInfo("ar-sa");

                if ((site != null) && (culture != null))
                {
                    // Assigns the culture to the site
                    CultureSiteInfoProvider.AddCultureToSite(culture.CultureID, site.SiteID);
                }
            }


            /// <heading>Removing a culture from a site</heading>
            private void RemoveCultureFromSite()
            {
                // Gets the site and culture objects
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");
                CultureInfo culture = CultureInfoProvider.GetCultureInfo("ar-sa");

                if ((site != null) && (culture != null))
                {
                    // Removes the culture from the site
                    CultureSiteInfoProvider.RemoveCultureFromSite(culture.CultureID, site.SiteID);
                }
            }


            /// <heading>Adding a domain alias to a site</heading>
            private void AddDomainAliasToSite()
            {
                // Gets the site object
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");

                if (site != null)
                {
                    // Creates a new site domain alias object
                    SiteDomainAliasInfo newAlias = new SiteDomainAliasInfo();                    
                    newAlias.SiteDomainAliasName = "127.0.0.1";
                    
                    // Assigns the domain alias to the site
                    newAlias.SiteID = site.SiteID;

                    // Saves the site domain alias to the database
                    SiteDomainAliasInfoProvider.SetSiteDomainAliasInfo(newAlias);
                }
            }


            /// <heading>Deleting a site's domain alias</heading>
            private void DeleteSiteDomainAlias()
            {
                // Gets the site object
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");

                if (site != null)
                {
                    // Gets the specified domain alias for the site
                    SiteDomainAliasInfo deleteAlias = SiteDomainAliasInfoProvider.GetSiteDomainAliasInfo("127.0.0.1", site.SiteID);

                    // Deletes the site domain alias
                    SiteDomainAliasInfoProvider.DeleteSiteDomainAliasInfo(deleteAlias);
                }
            }


            /// <heading>Deleting a site</heading>
            private void DeleteSite()
            {
                // Gets the site
                SiteInfo deleteSite = SiteInfoProvider.GetSiteInfo("NewSite");

                if (deleteSite != null)
                {
                    // Deletes the site
                    SiteInfoProvider.DeleteSiteInfo(deleteSite);
                }
            }
        }


        /// <summary>
        /// Holds site action API examples.
        /// </summary>
        /// <groupHeading>Site actions</groupHeading>
        private class SiteActions
        {
            /// <heading>Starting a site</heading>
            private void RunSite()
            {
                // Gets the site
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");
                if (site != null)
                {
                    // Starts the site
                    SiteInfoProvider.RunSite(site.SiteName);
                }
            }


            /// <heading>Stopping a site</heading>
            private void StopSite()
            {
                // Gets the site
                SiteInfo site = SiteInfoProvider.GetSiteInfo("NewSite");
                if (site != null)
                {
                    // Stops the site
                    SiteInfoProvider.StopSite(site.SiteName);
                }
            }
        }
    }
}
