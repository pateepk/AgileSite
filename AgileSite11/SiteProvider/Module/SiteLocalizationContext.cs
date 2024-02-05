using System;

using CMS.Helpers;
using CMS.Localization;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site localization context
    /// </summary>
    internal class SiteLocalizationContext : LocalizationContext
    {
        public static void Init()
        {
            // Apply site to the preferred culture code property
            var culture = mPreferredCultureCode;
            culture.Getter = GetPreferredCultureCode;
            culture.Setter = SetPreferredCultureCode;
        }


        /// <summary>
        /// Sets the preferred culture code and validates it against current site cultures
        /// </summary>
        /// <param name="value">New value</param>
        private static void SetPreferredCultureCode(string value)
        {
            string culture = value;

            // Validate the culture code
            string siteName = SiteContext.CurrentSiteName;
            if (!String.IsNullOrEmpty(siteName))
            {
                culture = CultureSiteInfoProvider.CheckCultureCode(culture, siteName);
            }

            CultureHelper.SetPreferredCulture(culture);
        }


        /// <summary>
        /// Gets the preferred culture code, if not available, initializes the culture with current site culture
        /// </summary>
        private static string GetPreferredCultureCode()
        {
            string culture = CultureHelper.GetPreferredCulture();

            // If no culture found, get default site culture
            if (String.IsNullOrEmpty(culture))
            {
                // Get default culture
                string siteName = SiteContext.CurrentSiteName;
                culture = ((siteName != String.Empty) ? CultureHelper.GetDefaultCultureCode(siteName) : CultureHelper.GetDefaultCultureCode(null));
            }

            return culture;
        }
    }
}
