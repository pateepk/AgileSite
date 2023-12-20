using System;
using System.Data;

using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.URLRewritingEngine
{
    internal class PreferredCultureEvaluator
    {
        private ViewModeOnDemand ViewMode
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new instance of the <see cref="PreferredCultureEvaluator"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when viewMode is null.</exception>
        /// <param name="viewMode">View mode.</param>
        public PreferredCultureEvaluator(ViewModeOnDemand viewMode)
        {
            if (viewMode == null)
            {
                throw new ArgumentNullException("viewMode");
            }

            ViewMode = viewMode;
        }


        /// <summary>
        /// Returns preferred culture code. Culture is obtained with following priorities:
        ///     - Site domain culture
        ///     - Site default visitor culture
        ///     - User preferred content culture
        ///     - Browser culture
        ///     - Default culture (from application settings)
        /// Evaluated culture is directly set into cookies except of the scenarios with default culture (Default visitor culture, Default culture (from application settings))
        /// </summary>
        public string Evaluate()
        {
            var currentSite = SiteContext.CurrentSite;
            if (currentSite == null)
            {
                return "";
            }

            var domainAliasesCulture = GetDomainAliasesCulture(currentSite);
            if (!string.IsNullOrEmpty(domainAliasesCulture))
            {
                SetPreferredCulture(domainAliasesCulture);
                return domainAliasesCulture;
            }

            var currentSiteDefaultVisitorCulture = GetCurrentSiteDefaultVisitorCulture(currentSite);
            if (!string.IsNullOrEmpty(currentSiteDefaultVisitorCulture))
            {
                return currentSiteDefaultVisitorCulture;
            }

            var userPreferredCulture = GetUserPreferredContentCulture();
            if (!string.IsNullOrEmpty(userPreferredCulture))
            {
                SetPreferredCulture(userPreferredCulture);
                return userPreferredCulture;
            }

            var browserCulture = GetBrowserCulture(currentSite);
            if (!string.IsNullOrEmpty(browserCulture))
            {
                SetPreferredCulture(browserCulture);
                return browserCulture;
            }

            return CultureHelper.GetDefaultCultureCode(currentSite.SiteName);
        }


        private static string GetCurrentSiteDefaultVisitorCulture(SiteInfo currentSite)
        {
            if (!string.IsNullOrEmpty(currentSite.DefaultVisitorCulture))
            {
                return currentSite.DefaultVisitorCulture;
            }

            return null;
        }


        private string GetUserPreferredContentCulture()
        {
            var preferredCulture = MembershipContext.AuthenticatedUser.PreferredCultureCode;
            if (string.IsNullOrEmpty(preferredCulture))
            {
                return null;
            }

            if (AuthenticationMode.IsWindowsAuthentication()
                && AuthenticationHelper.IsAuthenticated()
                && !MembershipContext.AuthenticatedUser.IsPublic())
            {
                return preferredCulture;
            }
            return null;
        }


        private void SetPreferredCulture(string domainAliasesCulture)
        {
            CultureHelper.SetPreferredCulture(domainAliasesCulture, true, !ViewMode.Value.IsLiveSite());
        }


        internal string GetBrowserCulture(SiteInfo currentSite)
        {
            DataSet culturesDS = CultureSiteInfoProvider.GetSiteCultures(currentSite.SiteName);
            string[] languages = CMSHttpContext.Current.Request.UserLanguages;
            if (DataHelper.DataSourceIsEmpty(culturesDS) || (languages == null))
            {
                return null;
            }

            foreach (string language in languages)
            {
                // Get the correct part of the language
                string lang = language;
                int index = lang.IndexOf(';');
                if (index >= 0)
                {
                    lang = lang.Substring(0, index);
                }

                lang = ValidationHelper.GetLanguage(lang, "");

                // Check language presence
                var rows = culturesDS.Tables[0].Select("CultureCode LIKE '" + lang + "%'");
                if (rows.Length > 0)
                {
                    return ValidationHelper.GetString(rows[0]["CultureCode"], "");
                }
            }

            return null;
        }


        internal string GetDomainAliasesCulture(SiteInfo currentSite)
        {
            string domain = RequestContext.FullDomain;

            if (URLHelper.DomainMatch(domain, currentSite.DomainName, true))
            {
                return null;
            }
 
            DataSet ds = SiteDomainAliasInfoProvider.GetDomainAliases(currentSite.SiteID);
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return null;
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                // Check the domain name
                string aliasDomain = ValidationHelper.GetString(dr["SiteDomainAliasName"], "");
                if (URLHelper.DomainMatch(domain, aliasDomain, true))
                {
                    // If domain alias uses specific culture, set the culture
                    string culture = ValidationHelper.GetString(dr["SiteDefaultVisitorCulture"], "");
                    if (culture != "")
                    {
                        return culture;
                    }
                    break;
                }
            }
            return null;
        }
    }
}